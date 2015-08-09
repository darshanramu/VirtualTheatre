using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Reflection;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;

namespace VirtualTheatre
{
    public partial class Form1 : Form
    {
        #region Variable declarations
        private LibVlc.LibVlc vlc;
        private LibVlc.LibVlc.Error playErrorCode = LibVlc.LibVlc.Error.Generic;
        
        private static int currTime = 0;
        
        private enum playbackStatusCodes { Play, Pause, Stop } ;
        playbackStatusCodes playbackStatus = playbackStatusCodes.Stop;
        
        private bool remoteSession = false;
        private bool localSession = false;
        private bool streamFlag = false;
        private bool authenticationDone = false;

        private string serverName;
        private string fileName;

        /// <summary>
        /// UdpClient to listen for requests after creation of new session
        /// </summary>
        private UdpClient sessionListener;
        /// <summary>
        /// UdpClient to listen for chat in a session
        /// </summary>
        private UdpClient chatListener;

        /// <summary>
        /// UdpClient to listen for acknowledgement after joining a new session
        /// </summary>
        private UdpClient sessionClient;
        /// <summary>
        /// UdpClient to send chat in a session
        /// </summary>
        private UdpClient chatClient;

        /// <summary>
        /// UdpClient to initialize the environment and listen for any session related information
        /// being broadcasted
        /// </summary>
        private UdpClient initListener;
        /// <summary>
        /// UdpClient to initialize the environment and broadcast any session related information
        /// </summary>
        private UdpClient initClient;

        /// <summary>
        /// UdpClient to broadcast any "urgent" information that needs to be complied
        /// </summary>
        private UdpClient forceInformationClient;

        private Thread initListenerThread = null;
        private Thread initClientThread = null;
        private Thread seekBarThread = null;
        private Thread sessionListenerThread = null;
        private Thread sessionClientThread = null;
        private Thread chatThread = null;
        
        delegate void updateSeekBarCallback();
        delegate void updateLogBoxCallback(string line);
        delegate void forceUpdateSessionCreationCallback(string line);
        delegate void updateChatDisplayCallback(string line, bool broadcastToAll, bool localUpdate);
        delegate void buttonStatusCallback(bool createButton, bool joinButton);

        #endregion

        public Form1()
        {
            InitializeComponent();
            vlc = new LibVlc.LibVlc();
            initConditions();

            initListenerThread = new Thread(new ThreadStart(initListenerThreadProc));
            initListenerThread.IsBackground = true;
            initListenerThread.Start();

            initClientThread = new Thread(new ThreadStart(initClientThreadProc));
            initClientThread.IsBackground = true;
            initClientThread.Start();
        }

        #region Methods to setup the PATH variable
        /// <summary>
        /// Search path for libvlc and plugins directory using registry, 
        /// and environement PATH variable
        /// </summary>
        /// <returns>VLC path, or current app path</returns>
        public static string SearchVlcPath()
        {
            //Search VLC - Registry
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VideoLAN\VLC");
            if (regKey != null)
            {
                string path = (string)regKey.GetValue("InstallDir", null);
                if (!string.IsNullOrEmpty(path))
                {
                    if (CheckVlcDirectory(path))
                    {
                        return path;
                    }
                }
            }

            //Search VLC - Path
            string[] envpaths = Environment.GetEnvironmentVariable("PATH").Split(';');
            foreach (string env in envpaths)
            {
                if (CheckVlcDirectory(env))
                {
                    return env;
                }
            }

            //Set program path as VLC Path
            Assembly asm = Assembly.GetEntryAssembly();
            string loc = asm.Location;
            loc = Path.GetDirectoryName(asm.Location);
            return loc;
        }

        /// <summary>
        /// Check if directory is a valid VLC directory : contains libvlc.* (dll or so) and sub-directory 'plugins'
        /// </summary>
        /// <param name="dir">Directory to check</param>
        /// <returns>True if directory is a VLC</returns>
        public static bool CheckVlcDirectory(string dir)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(dir);
            if (dirinfo.Exists)
            {
                if (dirinfo.GetDirectories("plugins").Length == 1)
                {
                    if (dirinfo.GetFiles("libvlc.*").Length > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Set current process environement variable PATH to have the system find the libvlc.dll
        /// </summary>
        /// <param name="vlcpath">Path of libvlc.dll</param>
        public static void SetEnvironement(string vlcpath)
        {
            string path = Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Process);
            if (!path.Contains(vlcpath))
            {
                path += ";" + vlcpath;
                Environment.SetEnvironmentVariable("path", path, EnvironmentVariableTarget.Process);
            }
        }
        #endregion

        #region Thread-Safe calls for seekBarThread
        /// <summary>
        /// Update the seekBar
        /// </summary>
        public void updateseekBar()
        {
            try
            {
                if (seekBar.InvokeRequired)
                {
                    updateSeekBarCallback d = new updateSeekBarCallback(updateseekBar);
                    if (this.IsDisposed == false)
                        Invoke(d);
                }
                else
                {
                    if (authenticationDone == false)
                        return;
                    if (vlc.LengthGet > 0)
                    {
                        seekBar.Maximum = vlc.LengthGet;
                        seekBar.LargeChange = 1;
                        if (vlc.TimeGet < vlc.LengthGet)
                        {
                            if (vlc.TimeGet > currTime)
                            {
                                currTime = vlc.TimeGet;
                                seekBar.Value = currTime;
                            }
                        }
                        else
                        {
                            currTime = 0;
                            seekBar.Value = currTime;
                            seekBarThread.Abort(seekBarThread.ThreadState);
                            playbackStatus = playbackStatusCodes.Stop;
                            vlc.PlaylistClear();
                            fileName = null;

                            if (localSession == true && remoteSession == true)
                            {
                                forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " is exiting. Session is closed ");
                            }
                            else
                                if (remoteSession)
                                {
                                    forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " is exiting.");
                                }

                            performCleanup(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }

        /// <summary>
        /// The method to create a thread and start it
        /// </summary>
        private void startseekBar()
        {
            seekBarThread = new Thread(new ThreadStart(seekBarThreadProc));
            seekBarThread.Start();
        }

        /// <summary>
        /// The method that starts the thread
        /// </summary>
        private void seekBarThreadProc()
        {
            while (true)
            {
                updateseekBar();
            }
        }
        #endregion

        #region Thread-Safe calls for initThread
        /// <summary>
        /// Used to change the state of session handling buttons
        /// </summary>
        /// <param name="createButton">Determines whether or not a new session can be created</param>
        /// <param name="joinButton">Determines whether a session can be joined</param>
        private void changeState(bool createButton, bool joinButton)
        {
            if (buttonCreateSession.InvokeRequired)
            {
                buttonStatusCallback d = new buttonStatusCallback(changeState);
                if (this.IsDisposed == false)
                    Invoke(d, createButton, joinButton);
            }
            else
            {
                buttonCreateSession.Enabled = createButton;
                buttonJoinSession.Enabled = joinButton;

                //Prevent any playback if any of the two buttons is highlighted
                if (createButton || joinButton)
                    initConditions();
            }
        }

        /// <summary>
        /// Listener thread that runs for the lifetime of application
        /// </summary>
        private void initListenerThreadProc()
        {
            try
            {
                initListener = new UdpClient(2030);
                IPAddress addr = IPAddress.Parse("224.100.0.1");
                initListener.JoinMulticastGroup(addr);
                IPEndPoint ep = null;

                while (true)
                {
                    if (initListener != null)
                    {
                        byte[] rdata = initListener.Receive(ref ep);
                        string line = Encoding.ASCII.GetString(rdata);
                        string[] separator = new string[1];
                        separator[0] = "@";
                        logBoxAdd(line);
                        
                        if (line.StartsWith("Force Update@"))
                        {
                            string information = line.Split(separator, StringSplitOptions.None)[1];
                            chatDisplayAdd(information, false, true);
                            if (information.Contains("exiting"))
                            {

                                if (information.Contains(Dns.GetHostName()))
                                {
                                    remoteSession = false;
                                    localSession = false;
                                }
                                else if (information.Contains("closed") == true)
                                {
                                    remoteSession = false;
                                    vlc.Stop(); // to stop the playback of the leech
                                    Thread.Sleep(5000);//wait for other to die
                                }
                            }
                            else if (information.Contains("pause"))
                                pause(true);
                            else if (information.Contains("play"))
                                pause(false);
                        }

                        if (line == "EXISTS")   //Session exists
                        {
                            if (remoteSession != true)
                                changeState(false, true);
                            remoteSession = true;
                        }
                        else
                        {
                            if (remoteSession == false)
                                changeState(true, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }

        /// <summary>
        /// Thread that runs for the lifetime of the application and is respponsible 
        /// for broadcasting session status
        /// </summary>
        private void initClientThreadProc()
        {
            try
            {
                initClient = new UdpClient("224.100.0.1", 2030);
                while (true)
                {
                    byte[] sdata;
                    if (remoteSession)
                        sdata = Encoding.ASCII.GetBytes("EXISTS");
                    else
                        sdata = Encoding.ASCII.GetBytes("OPEN");
                    initClient.Send(sdata, sdata.Length);
                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }
        #endregion

        #region Thread-Safe calls for sessionThread
        /// <summary>
        /// Method to add a new message to the logBox
        /// </summary>
        /// <param name="line">The log message to be appended</param>
        private void logBoxAdd(string line)
        {
            if (logBox.InvokeRequired)
            {
                updateLogBoxCallback d = new updateLogBoxCallback(logBoxAdd);
                if (this.IsDisposed == false)
                    Invoke(d, line);
            }
            else
            {
                
                string[] lines = logBox.Lines;
                int i;

                /*
                for (i = 0; i < lines.Length; i++)
                {
                    string s = lines[i];
                    if (s.Equals(line))
                        break;
                }
                if (i == lines.Length)
                  */  logBox.AppendText(line + "\n");
            }
        }

        /// <summary>
        /// Listener thread that comes into existence when a new session is created
        /// </summary>
        private void newSessionListenerThreadProc()
        {
            UdpClient sessionWriter;
            try
            {
                authenticationDone = true;
                sessionWriter = new UdpClient(2000);
                sessionListener = new UdpClient(2001);
                IPAddress addr = IPAddress.Parse("224.100.0.1");
                sessionListener.JoinMulticastGroup(addr);
                sessionWriter.JoinMulticastGroup(addr);
                
                IPEndPoint ep = null;
                while (true)
                {
                    byte[] rdata = sessionListener.Receive(ref ep);
                    string line = Encoding.ASCII.GetString(rdata);
                    string address;
                    string[] separator = new string[1];
                    separator[0] = ": ";
                    if (line == null)
                        break;
                    logBoxAdd(line);

                    //Check if a request has come
                    if (line.IndexOf(':') == -1)
                        continue;

                    if (line.StartsWith("Session available: "))
                        continue;

                    //Requests have the pattern "Allow: x.x.x.x"
                    //Thus extract the address
                    if (line.StartsWith("Allow: "))
                    {
                        address = line.Split(separator, StringSplitOptions.None)[1];
                        //Now send an ack
                        //Assigning a uid and saving state info is left

                        logBoxAdd("Request from " + Dns.GetHostByAddress(address).HostName.ToString());
                        DialogResult result=MessageBox.Show("Do you want to authenticate" + Dns.GetHostByAddress(address).HostName.ToString(),"Authentication",MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            byte[] sdata = Encoding.ASCII.GetBytes("+OK " + address);
                            sessionWriter.Send(sdata, sdata.Length, "224.100.0.1", 2000);
                        }
                        else
                        {
                            byte[] sdata = Encoding.ASCII.GetBytes("-ERR " + address);
                            sessionWriter.Send(sdata, sdata.Length, "224.100.0.1", 2000);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }

        /// <summary>
        /// Client thread to broadcast presence of new session
        /// </summary>
        private void newSessionClientThreadProc()
        {
            try
            {
                sessionClient = new UdpClient("224.100.0.1", 2001);
                forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " created a new session");
                chatDisplayAdd(Dns.GetHostName() + " joined the session", true, true);
                serverName = Dns.GetHostName();
                sessionClient.Close();
                //Send heartbeat every 5 seconds
                sessionClient = new UdpClient("224.100.0.1", 2002);
                while (true)
                {
                    System.Threading.Thread.Sleep(5000);
                    byte[] sdata = Encoding.ASCII.GetBytes("Session available: " + Dns.GetHostAddresses(Dns.GetHostName())[0].ToString());
                    sessionClient.Send(sdata, sdata.Length);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }

        /// <summary>
        /// Force update of special session status messages
        /// </summary>
        /// <param name="line">The message to be passed</param>
        private void forceUpdateSessionInformation(string line)
        {
            if (logBox.InvokeRequired)
            {
                forceUpdateSessionCreationCallback d = new forceUpdateSessionCreationCallback(forceUpdateSessionInformation);
                if (this.IsDisposed == false)
                    Invoke(d, line);
            }
            else
            {
                forceInformationClient = new UdpClient("224.100.0.1", 2030);
                byte[] sdata = Encoding.ASCII.GetBytes(line);
                forceInformationClient.Send(sdata, sdata.Length);
                forceInformationClient.Close();
            }
        }

        /// <summary>
        /// Listener thread that comes into existence when an existing session is joined
        /// </summary>
        private void joinSessionListenerThreadProc()
        {
            try
            {
                UdpClient sessionListener2;
                sessionListener = new UdpClient(2002);
                sessionListener2 = new UdpClient(2000);
                IPAddress addr = IPAddress.Parse("224.100.0.1");
                sessionListener.JoinMulticastGroup(addr);
                sessionListener2.JoinMulticastGroup(addr);
                IPEndPoint ep = null;
                IPAddress[] addresses;
                bool done = false;
                while (true)
                {
                    byte[] rdata = sessionListener.Receive(ref ep);
                    string line = Encoding.ASCII.GetString(rdata);
                    string[] separator = new string[1];
                    separator[0] = ": ";
                    logBoxAdd(line);
                    if (line.StartsWith("Session available: "))
                    {
                        joinSessionClientThreadProc();
                        Thread.Sleep(5000); //To prevent duplicate requests being served
                    }
                    ep = null;
                    rdata = sessionListener2.Receive(ref ep);
                    line = Encoding.ASCII.GetString(rdata);
                    separator[0] = ": ";
                    logBoxAdd(line);
                    addresses = Dns.GetHostAddresses(Dns.GetHostName());
                    foreach (IPAddress ip in addresses)
                        if (line == ("+OK " + ip.ToString()))
                        {
                            logBoxAdd("Successfully Authenticated");
                            done = true;
                            authenticationDone = true;
                            buttonPlay.Enabled = true;
                            serverName = Dns.GetHostByAddress(ep.Address.ToString()).HostName.ToString();
                            chatDisplayAdd(Dns.GetHostName() + " joined the session", true, true);
                            break;
                        }
                        else
                            if (line == ("-ERR " + ip.ToString()))
                            {
                                logBoxAdd("Access Denied");
                                done = true;
                                authenticationDone = false;
                                buttonPlay.Enabled = false;
                                MessageBox.Show("Access denied. The application will now close");
                                this.Close();
                                break;
                            }
                    if (done)
                        break;
                }
                //Close the session listener and start chat listener
                chatThread = new Thread(new ThreadStart(this.chatThreadProc));
                chatThread.Start();
                sessionListener.Close();
                sessionListener2.Close();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }

        /// <summary>
        /// Client thread that advertises the presence of a new host
        /// </summary>
        private void joinSessionClientThreadProc()
        {
            try
            {
                sessionClient = new UdpClient("224.100.0.1", 2001);
                {
                    byte[] sdata = Encoding.ASCII.GetBytes("Allow: " + Dns.GetHostAddresses(Dns.GetHostName())[0].ToString());
                    sessionClient.Send(sdata, sdata.Length);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }
        #endregion

        #region Thread-Safe calls for chatThread
        /// <summary>
        /// Method to add a new chat message to the chat window
        /// </summary>
        /// <param name="line">The message to be appended</param>
        /// <param name="broadcastToAll">If message is updated on remote window</param>
        /// <param name="localUpdate">If message is updated on local window</param>
        private void chatDisplayAdd(string line, bool broadcastToAll, bool localUpdate)
        {
            if (logBox.InvokeRequired)
            {
                updateChatDisplayCallback d = new updateChatDisplayCallback(chatDisplayAdd);
                if (this.IsDisposed == false)
                    Invoke(d, line, broadcastToAll, localUpdate);
            }
            else
            {
                if (localUpdate)
                    chatDisplay.AppendText(line + "\n");
                if (broadcastToAll)
                {
                    chatClient = new UdpClient("224.100.0.1", 2020);
                    byte[] sdata = Encoding.ASCII.GetBytes(line);
                    chatClient.Send(sdata, sdata.Length);
                    chatClient.Close();
                }
            }
        }

        /// <summary>
        /// Thread to run chat. Runs for the lifetime of the session
        /// </summary>
        private void chatThreadProc()
        {
            try
            {
                chatListener = new UdpClient(2020);
                IPAddress addr = IPAddress.Parse("224.100.0.1");
                chatListener.JoinMulticastGroup(addr);
                IPEndPoint ep = null;

                while (true)
                {
                    byte[] rdata = chatListener.Receive(ref ep);
                    string line = Encoding.ASCII.GetString(rdata);
                    string[] separator = new string[1];
                    separator[0] = ": ";
                    if (line.StartsWith(Dns.GetHostName()) == false)
                        chatDisplayAdd(line, false, true);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Reset the application to starting conditions
        /// </summary>
        private void initConditions()
        {
            playbackStatus = playbackStatusCodes.Stop;
            buttonPlay.Enabled = false;
            buttonPause.Enabled = false;
            buttonStop.Enabled = false;
            remoteSession = false;
            localSession = false;
            streamFlag = false;
            authenticationDone = false;
            logBox.Clear();
        }

        /// <summary>
        /// Used to play or pause video. Invoked from within other event handlers
        /// </summary>
        /// <param name="playPause"> true = pause; false = play</param>
        /// 
        private void pause(bool playPause)
        {
            try
            {

                if (playPause == true)
                {
                    vlc.Pause();
                    if (streamFlag)
                        if (seekBarThread != null)
                            seekBarThread.Suspend();
                    playbackStatus = playbackStatusCodes.Pause;
                    buttonPause.Enabled = false;
                    buttonPlay.Enabled = true;
                }
                else
                {

                    vlc.Pause();
                    if (streamFlag)
                        if (seekBarThread != null)
                            seekBarThread.Resume();
                    playbackStatus = playbackStatusCodes.Play;
                    buttonPause.Enabled = true;
                    buttonPlay.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }

        /// <summary>
        /// Performs teardown of a session and does necessary cleanup
        /// </summary>
        private void performCleanup(bool resetListener)
        {
            try
            {
                if (sessionListenerThread != null)
                {
                    sessionListener.Close();
                }

                if (sessionClientThread != null)
                {
                    sessionClient.Close();
                }
                if (chatThread != null)
                {
                    chatListener.Close();
                    chatClient.Close();
                }
                authenticationDone = false;
                streamFlag = false;
                initConditions();
                
                if (resetListener)
                {
                    changeState(true, false);
                    initListener.Close();
                    initListener = null;
                    
                    initListener = new UdpClient(2030);
                    IPAddress addr = IPAddress.Parse("224.100.0.1");
                    initListener.JoinMulticastGroup(addr);
                    IPEndPoint ep = null;

                    Thread.Sleep(1000);
                    initClient = new UdpClient("224.100.0.1", 2030);

                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }

        /// <summary>
        /// Method to start streaming of media
        /// </summary>
        private void playStream()
        {
            try
            {
                if (playbackStatus == playbackStatusCodes.Stop)
                {
                    SetEnvironement(SearchVlcPath());

                    vlc.Initialize();
                    vlc.VideoOutput = pictureBox1;
                    vlc.PlaylistClear();
                    //Multicast address
                    string[] Options = new string[] { ":sout=#duplicate{dst=display,dst=std {access=udp,mux=ts,dst=224.100.0.1:1234}}" };

                    if (fileName == null)
                    {
                        MessageBox.Show("Please select a file by double clicking in the space for video");
                        return;
                    }
                    vlc.PlaylistClear();
                    vlc.AddTarget(fileName, Options);

                    playErrorCode = vlc.Play();
                    this.Text = "VirtualTheater - " + fileName;
                    if (playErrorCode == LibVlc.LibVlc.Error.Success)
                    {
                        //For the seekBar
                        seekBarThread = new Thread(new ThreadStart(this.seekBarThreadProc));
                        seekBarThread.Start();
                        playbackStatus = playbackStatusCodes.Play;
                        buttonPlay.Enabled = false;
                        buttonPause.Enabled = true;
                        buttonStop.Enabled = true;
                    }
                }
                else
                {
                    forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " requests a play");
                }
            }
            catch (Exception e1)
            {
                System.Console.WriteLine("Exception occurred: {0}", e1);
            }
        }

        /// <summary>
        /// Method to start leeching of media
        /// </summary>
        private void receiveStream()
        {
            try
            {
                if (authenticationDone == false)
                {
                    MessageBox.Show("Please wait for authentication response");
                    return;
                }

                if (playbackStatus == playbackStatusCodes.Stop)
                {
                    SetEnvironement(SearchVlcPath());
                    vlc.Initialize();
                    vlc.VideoOutput = pictureBox1;
                    vlc.PlaylistClear();
                    string[] options = { ":sout=#duplicate{dst=display}" };

                    //Muticast address
                    vlc.AddTarget("udp://@224.100.0.1:1234", options);

                    playErrorCode = vlc.Play();

                    if (playErrorCode == LibVlc.LibVlc.Error.Success)
                    {
                        playbackStatus = playbackStatusCodes.Play;
                        buttonPlay.Enabled = false;
                        buttonPause.Enabled = true;
                        buttonStop.Enabled = true;
                    }
                }
                else
                {
                    forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " requests a play");
                }
            }
            catch (Exception e1)
            {
                System.Console.WriteLine("Exception occurred: {0}", e1);
            }
        }
        #endregion

        #region Event Handlers
        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (streamFlag == true)
                receiveStream();
            else
                playStream(); 
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (seekBarThread != null)
                    while (seekBarThread.ThreadState != ThreadState.Stopped)
                        seekBarThread.Abort(seekBarThread.ThreadState);
                vlc.Stop();
                currTime = 0;
                seekBar.Value = 0;
                
                playErrorCode = LibVlc.LibVlc.Error.Exit;
                playbackStatus = playbackStatusCodes.Stop;
                vlc.PlaylistClear();

                fileName = null;
                
                if (localSession == true && remoteSession == true)
                {
                    forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " is exiting. Session is closed ");
                    initClient.Close();
                    initClient = null; 
                }
                else
                    if (remoteSession)
                    {
                        forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " is exiting.");
                    }

                performCleanup(true);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception occurred: {0}", ex);
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
              forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " requests a pause");
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = (trackBar1.Value * 10).ToString();
            vlc.VolumeSet(trackBar1.Value * 10);
        }

        private void seekBar_Scroll(object sender, EventArgs e)
        {
            if (playbackStatus == playbackStatusCodes.Play)
                seekBarThread.Suspend();
            vlc.TimeSet(seekBar.Value, false);
            currTime = seekBar.Value;
            if (playbackStatus == playbackStatusCodes.Play)
                seekBarThread.Resume();
        }

        private void buttonCreateSession_Click(object sender, EventArgs e)
        {
            remoteSession = true;
            localSession = true;

            sessionListenerThread = new Thread(new ThreadStart(this.newSessionListenerThreadProc));
            sessionListenerThread.IsBackground = true;
            sessionListenerThread.Start();

            sessionClientThread = new Thread(new ThreadStart(this.newSessionClientThreadProc));
            sessionClientThread.IsBackground = true;
            sessionClientThread.Start();

            chatThread = new Thread(new ThreadStart(this.chatThreadProc));
            chatThread.IsBackground = true;
            chatThread.Start();

            changeState(false, false);
            buttonPlay.Enabled = true;
        }

        private void buttonJoinSession_Click(object sender, EventArgs e)
        {
            remoteSession = true;
            sessionListenerThread = new Thread(new ThreadStart(this.joinSessionListenerThreadProc));
            sessionListenerThread.IsBackground = true;
            sessionListenerThread.Start();

            changeState(false, false);
            streamFlag = true;
        }

        private void chatType_TextChanged(object sender, EventArgs e)
        {
            if (chatType.Text.EndsWith("\n"))
            {
                chatDisplayAdd(Dns.GetHostName() + ": " + chatType.Text, true, true);
                chatType.Clear();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            currTime = 0;
            if (seekBarThread != null)
                while (seekBarThread.ThreadState != ThreadState.Stopped)
                    seekBarThread.Abort(seekBarThread.ThreadState);
            vlc.Stop();
            playbackStatus = playbackStatusCodes.Stop;
            
            if (localSession == true && remoteSession == true)
            {
                forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " is exiting. Session is closed ");
            }
            else
            if (remoteSession)
            {
                forceUpdateSessionInformation("Force Update@" + Dns.GetHostName() + " is exiting.");
            }

            performCleanup(false);
        }
        
        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            if ((localSession == false))
            {
                return;
            }
            openFileDialog1.AddExtension = true;
            openFileDialog1.AutoUpgradeEnabled = true;
            openFileDialog1.Multiselect = false;
            openFileDialog1.DefaultExt = "avi";
            openFileDialog1.Filter = "Movie file (mpeg) (*.mpeg, *.mpg)|*.mpg|MP4 Video File (*.mp4)|*.mp4|Windows Video File (avi)|*.avi|Matroska Video File (*.mkv)|*.mkv|All files (*.*)|*.*";
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            fileName = openFileDialog1.FileName;
        }
        
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Press \"New Session\" button to create a new session and host a media file\n\nPress \"Join Session\" to join an existing session and stream media from host\n\nYou can use play/pause/stop buttons to control media playing\n\nChat box can be used to chat with others in the current session","Help",MessageBoxButtons.OK);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This project is developed by Kalpesh,Darshan and Hersh of PESIT,Bangalore \n in partial fulfillment for the award of Bachelor of Engineering in Computer Science \n and Engineering of Visvesvaraya Technological University, Belgaum during the year 2008-09 ", "About", MessageBoxButtons.OK);
        }
        #endregion
    }
}
