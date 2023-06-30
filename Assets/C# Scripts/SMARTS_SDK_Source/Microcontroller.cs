using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;
using System.Collections;

namespace SMARTS_SDK
{
    // THIS IS A SINGLETON:
    // we have one microcontroller that many scripts frequently reference
    // so it is appropriate to use a public static class for our microcontroller.
    // it is also useful to use the property inspector for public variables - 
    // and its also useful to use coroutines for serial connection and read cycles (although we use a thread for that).
    // But, those two useful things come from : MonoBehavior.
    // MonoBehavior classes cannot be static.
    // So, we have an argument to use a singleton: 
    // a regular MonoBehavior class and make one single instance of the whole class.
    // any other script can access public (not public static) variables and funtions like this:
    // Microcontroller.ME.SomeFunction();

    // WE CANNOT USE THESE: 
    // UNITY JIT compiler does not include these functions even though they compile in VB/MONO
    // DataReceivedHandler
    // ReadExisting
    // BytesToRead
    // DiscardNull
    // DiscardInBuffer 
    // DiscardOutBuffer

    /// <summary>
    /// This class is compatible with, and requires the MicrocontrollerWhiteBoxHandshake arduino code develped by:
    /// Andre Bigos
    /// 08.16.2017
    /// 14:30
    /// 
    /// 
    /// EDIT: 10.24.2017
    /// The MicrocontrollerWhiteBoxHandshake arduino code now exists in the SDK as SMARTSMC.
    /// </summary>
    public class Microcontroller : MonoBehaviour
    {

        // --------- Singleton Reference to this script --------------------------------------------------------------------------
        //Static instance of this class which allows it to be accessed by any other script. 
        public static Microcontroller ME;

        [Header("----------- COM Port Initialization ------------------------------------------------------------")]
        [SerializeField]
        private string com_Port;
        public string COM_Port { set { com_Port = value; } } // ATTENTION - Set to default port - 5.

        private SerialPort port;

        [Header("----------- WhiteBox Connection Status------------------------------------------------------------")]
        [SerializeField]
        private bool retryConnection;
        [SerializeField]
        private bool connected;
        [SerializeField]
        private bool whiteboxConnectionAlert;
        public bool RetryConnection
        {
            get
            {
                return retryConnection;
            }
        }
        public bool Connected
        {
            get
            {
                return connected;
            }

        }
        public bool WhiteboxConnectionAlert
        {
            get
            {
                return whiteboxConnectionAlert;
            }

        }
        private Thread streamThread;
        private string inString;

        [Header("----------- Sensor Readings ------------------------------------------------------------")]
        // these dont have to be public - but its handy to see it in the inspector.
        [SerializeField]
        private string incoming;

        [SerializeField]
        private bool tui_Button;
        [SerializeField]
        private int usProbePressureNotchSide;
        [SerializeField]
        private int usProbePressureFlatSide;
        [SerializeField]
        private float valvePressure;
        [SerializeField]
        private bool syringeValvePresent = false;

        public bool TUI_Button
        {
            get
            {
                return tui_Button;
            }
        }
        public int USProbePressureNotchSide
        {
            get
            {
                return usProbePressureNotchSide;
            }
        }
        public int USProbePressureFlatSide
        {
            get
            {
                return usProbePressureFlatSide;
            }
        }
        public float ValvePressure
        {
            get
            {
                return valvePressure;
            }
        }
        public bool SyringeValvePresent
        {
            get
            {
                return syringeValvePresent;
            }
        }

        [SerializeField]
        private bool redLight, blueLight, syringeValveOpen;

        public bool RedLight
        {
            get
            {
                return redLight;
            }
        }
        public bool BlueLight
        {
            get
            {
                return blueLight;
            }
        }
        public bool SyringeValveOpen
        {
            get
            {
                return syringeValveOpen;
            }
        }

        //List of Microcontroller commands
        [HideInInspector]
        public enum MicrocontrollerCommands
        {
            Alive,                      //Unity is Alive and Well
            LPop,                       //Large Tactile Pop
            SPop,                       //Small Tactile Pop
            LORO,                       //Open Syringe Valve
            LORC,                       //Close Syringe Valve
            RedLight,                   //Red Syringe LED
            BlueLight,                  //Blue Syringe LED
            LightsOff,                  //Syringe LED OFF
            ZeroSyringePressure,        //Calibrate Syringe Pressure
            ResetSyringe               //Reset Syring hardware
        }
        // Testing variables
        // private int lastthing;
        // public GameObject CamCube;


        private int awakeTimer;
        private int awakeTimerPeriod = 350; //Hardcoded values

        ArrayList commandQueue = new ArrayList();
        bool waitingForData = false;
        bool canSendAgain = true;
        bool dataReceived = false;
        // --------- Awake Singleton Constructor --------------------------------//
        //  Awake is always called before any Start functions 
        //  Check if instance already exists
        //  If so, then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of this class.
        void Awake()
        {

            if (ME != null)
                GameObject.Destroy(ME);
            else
                ME = this;

            DontDestroyOnLoad(this);

        }

        // --------- Start Connection --------------------------------------//
        //Wait for 50ms and initiate connection
        void Start()
        {
            Thread.Sleep(50);   // 50 ms
            Connect();
        }

        // --------- Connection Sequence--------------------------------------//
        //Connect to Microcontroller from new thread
        public void Connect()
        {
            try
            {
                connected = false;
                streamThread.Abort();
                port.Close();
                port.Dispose();
                Thread.Sleep(50);   // 50 ms

            }
            catch (Exception e)
            {
                //Debug.Log(e);
            }
            streamThread = new Thread(ThreadListener); //Start a new thread and listen to Microcontroller
            streamThread.Start();
        }

        // --------- Listen to MicroController via comport--------------------------------------//
        // Listen to port for whitebox parameters
        // ATTENTION - dont do string comparisons, compare with ints or enums
        // ATTENTION - dont do string + string
        public void ThreadListener()
        {
            retryConnection = false;
            try
            {
                if (String.IsNullOrEmpty(com_Port))
                {
                    Debug.Log("Handshake is being done");
                    string[] ports = SerialPort.GetPortNames();
                    Debug.Log("Number of available COM Ports:" + ports.Length);
                    foreach (string portname in ports)
                    {
                        float portOpenPeriod = 0;

                        Debug.Log("Attempting to connect to " + portname);
                        port = null;
                        port = new SerialPort(portname, 19200, Parity.None, 8, StopBits.One);
                        port.Open();
                        port.ReadTimeout = 150;
                        port.Write("-");

                        while (port.IsOpen)
                        {

                            byte handShake = (byte)port.ReadByte();
                            if (handShake == '>')
                            {
                                Debug.Log("CSSALT compatible microcontroller found!");
                                canSendAgain = true;
                                break;
                            }
                            else if (portOpenPeriod > 150)
                            {
                                port.Close();
                                port.Dispose();
                                port = null;
                                Debug.Log("Not a CSSALT compatible microcontroller!");
                                break;
                            }
                            portOpenPeriod++;
                        }
                        if (port != null)
                            break;


                    }
                    if (port != null && port.IsOpen)
                    {
                        connected = true;
                        retryConnection = false;
                    }
                    else
                    {
                        connected = false;
                    }
                }
                else
                {
                    float portOpenPeriod = 0;
                    string portName = "COM" + com_Port;
                    //Debug.Log("Attempting to connect to " + portName);
                    port = null;
                    port = new SerialPort(portName, 19200, Parity.None, 8, StopBits.One);
                    port.Open();
                    port.ReadTimeout = 150;
                    port.Write("-");

                    while (port.IsOpen)
                    {

                        byte handShake = (byte)port.ReadByte();
                        if (handShake == '>')
                        {
                            Debug.Log("CSSALT compatible microcontroller found!");
                            canSendAgain = true;
                            break;
                        }
                        else if (portOpenPeriod > 150)
                        {
                            port.Close();
                            port.Dispose();
                            port = null;
                            Debug.Log("Not a CSSALT compatible microcontroller!");
                            break;
                        }
                        portOpenPeriod++;
                    }
                    if (port != null && port.IsOpen)
                    {
                        connected = true;
                        retryConnection = false;
                    }
                    else
                    {
                        connected = false;
                    }
                }
            }


            catch (Exception)
            {
                retryConnection = true;
                connected = false;
                streamThread.Abort();
            }

            while (port.IsOpen & connected)
            {
                if (waitingForData)
                {
                    try
                    {
                        inString = "";
                        inString = port.ReadLine();
                        inString = inString.Trim();
                        if (!String.IsNullOrEmpty(inString) && (inString.Substring(0, 1)).CompareTo("$") == 0 && (inString.Substring(inString.Length - 1)).CompareTo("#") == 0)
                        {

                            try
                            {
                                // public variable for property inspector visibility
                                incoming = inString.Substring(1, inString.Length - 2);

                                string[] SplitArray = incoming.Split(',');

                                if (SplitArray.Length == 3)
                                {
                                    tui_Button = (int.Parse(SplitArray[0]) > 0);
                                    usProbePressureNotchSide = int.Parse(SplitArray[1]);
                                    usProbePressureFlatSide = int.Parse(SplitArray[2]);
                                    syringeValvePresent = false;
                                    dataReceived = true;
                                }
                                if (SplitArray.Length == 4) // this newer whitebox has a valve pressure sensor.
                                {
                                    tui_Button = (int.Parse(SplitArray[0]) > 0);
                                    usProbePressureNotchSide = int.Parse(SplitArray[1]);
                                    usProbePressureFlatSide = int.Parse(SplitArray[2]);
                                    valvePressure = float.Parse(SplitArray[3]);
                                    syringeValvePresent = true;
                                    dataReceived = true;
                                }
                            }
                            catch (Exception e)
                            {
                                dataReceived = true;
                                Debug.Log(e);
                                // this catches mostly parseInt errors from bad data that the arduino sometimes spits out after reads.
                                // Note the problem is most likely the poor .NET implementation of Serial.IO.ports
                                // read this blog - its written by a popular embedded systems engineer who posts on stackoverflow alot: 
                                // www.sparxeng.com/blog/software/muse-use-net-system-io-ports-serialport
                                //
                                //
                                /// This problem was fixed by Andre Bigos 8.16.2017 by splitting data communication between the white box
                                /// and this code into two cycles. During one cycle, the computer sends data to the microcontroller. The 
                                /// next part of the cycles has the microcontroller send back data to the computer. Because there is now
                                /// only communication in a single direction, the loss of data that used to occur is no longer present.
                            }
                        }
                        else
                        {
                            dataReceived = true;
                        }
                    }
                    catch (Exception e)
                    {
                        retryConnection = true;
                        connected = false;
                        Debug.Log(e);

                    }
                    if (dataReceived)
                    {
                        waitingForData = false;
                        dataReceived = false;
                        canSendAgain = true;
                    }
                }
            }
            waitingForData = false;
            dataReceived = false;
            connected = false;
            retryConnection = true;
        }

        // --------- Every Rendering Frame --------------------------------------//
        void Update()
        {
            if (retryConnection)
            {
                // strobing through the COM ports like this introduces instability.
                // COM_portNumber++;
                // if (COM_portNumber > 20) COM_portNumber = 1;
                Connect();
            }

            whiteboxConnectionAlert = retryConnection;

            awakeTimer += (int)(Time.deltaTime * 1000);
            if (awakeTimer > awakeTimerPeriod)
            {
                awakeTimer = 0;
                Send(MicrocontrollerCommands.Alive); // this lets the arduino know that unity is actively talking to it.
            }

        }


        // ---------  Send commands to the microcontroller --------------------------------------//
        // The commands are simple and few, therefore we can limit them to single bytes for quick parsing.
        public void Send(MicrocontrollerCommands theCommand)
        {
            commandQueue.Add(theCommand);
        }

        void LateUpdate()
        {
            //if (!commandQueue.Contains(MicrocontrollerCommands.SendData))
            //	commandQueue.Add(MicrocontrollerCommands.SendData);
            if (canSendAgain)
                SendData();
        }

        void SendData()
        {
            canSendAgain = false;
            if (port != null && port.IsOpen & connected)
            {
                foreach (MicrocontrollerCommands theCommand in commandQueue)
                {

                    try
                    {

                        switch (theCommand)
                        {

                            // ------ Unity is Alive and Well --------------------------------- 
                            case (MicrocontrollerCommands.Alive):
                                port.Write("A");    // writes ASCII character 65 "A",
                                awakeTimer = 0;
                                break;

                            // ------ Large Tactile Pop ---------------------------------------
                            case (MicrocontrollerCommands.LPop):
                                port.Write("1");    // writes 0110001 ("1" is ASCII character 49)
                                awakeTimer = 0;
                                break;

                            // ------ Small Tactile Pop ---------------------------------------
                            case (MicrocontrollerCommands.SPop):
                                port.Write("2");    // writes 0110010 ("2" is ASCII character 50)
                                awakeTimer = 0;
                                break;

                            // ------ Open Syringe Valve ---------------------------------------
                            case (MicrocontrollerCommands.LORO):
                                if (syringeValveOpen)
                                {
                                    // nothing - the syringe valve is already open.
                                }
                                else
                                {
                                    port.Write("3");  // this writes 0110011 ("3" is ASCII character 51)
                                    syringeValveOpen = true;
                                    awakeTimer = 0;
                                }
                                break;

                            // ------ Close Syringe Valve --------------------------------------
                            case (MicrocontrollerCommands.LORC):
                                if (syringeValveOpen)
                                {
                                    port.Write("4");  // this writes 0110011 ("3" is ASCII character 51)
                                    syringeValveOpen = false;
                                    awakeTimer = 0;
                                }
                                else
                                {
                                    // nothing - the valve is already closed
                                }
                                break;

                            // ------ Red Syringe LED ------------------------------------------
                            case (MicrocontrollerCommands.RedLight):
                                if (!redLight)
                                {
                                    port.Write("8");
                                    redLight = true;
                                    awakeTimer = 0;
                                }
                                break;

                            // ------ Blue Syringe LED ------------------------------------------
                            case (MicrocontrollerCommands.BlueLight):
                                if (!blueLight)
                                {
                                    port.Write("7");
                                    blueLight = true;
                                    awakeTimer = 0;
                                }
                                break;

                            // ------ Syringe LED OFF ------------------------------------------
                            case (MicrocontrollerCommands.LightsOff):
                                if (redLight || blueLight)
                                {
                                    port.Write("9");
                                    redLight = false;
                                    blueLight = false;
                                    awakeTimer = 0;
                                }
                                break;

                            // ------ Calibrate Syringe Pressure -------------------------------
                            case (MicrocontrollerCommands.ZeroSyringePressure):
                                port.Write("P");
                                awakeTimer = 0;
                                syringeValveOpen = true;
                                break;

                            // ------ Reset Syring hardware -------------------------------
                            case (MicrocontrollerCommands.ResetSyringe):
                                port.Write("9");
                                redLight = false;
                                blueLight = false;
                                awakeTimer = 0;
                                port.Write("3");  // this writes 0110011 ("3" is ASCII character 51)
                                syringeValveOpen = true;
                                break;
                            //case (MicrocontrollerCommands.SendData):
                            //	port.Write("S");
                            //	awakeTimer = 0;
                            //	break;
                            default:
                                break;
                        }

                    }
                    catch (System.Exception)
                    {
                        connected = false;
                        retryConnection = true;
                        //Debug.Log("there is a problem with the arduino.");
                        port.Close();
                        //throw;
                    }
                }
                try
                {
                    port.Write("S");
                    awakeTimer = 0;
                }
                catch (Exception)
                {

                }
            }
            commandQueue = new ArrayList();
            waitingForData = true;
        }
        // ------ Close Connection Sequence ------------------------------------------
        public void OnApplicationQuit()
        {
            streamThread.Abort();
            if (port != null && port.IsOpen)
                port.Close();
            if (port != null)
                port.Dispose();

            connected = false;
            Thread.Sleep(10);


        }

    }
}