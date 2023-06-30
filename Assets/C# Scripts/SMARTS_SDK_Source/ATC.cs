using UnityEngine;
using System.Collections;

using System;  // needed for IntPntr
using System.Runtime.InteropServices; // needed to get to the plugin (specifically, needed to reference the unmanaged dll we wrote that calls Ascension's API)
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
# endif

namespace SMARTS_SDK
{
    /*
     THIS IS A SINGLETON:
     This is a script to access the Ascension tracking system plugin.
     There can only be one.

     TO DO:
     Since connecting to the tracking system takes a few seconds, consider threading this script separately someday.

     TRACKING ANATOMY:
     How do we lock 3D printed anatomy to the transmitter? With INTRINSIC BASE TRACKING: THE BASEOBJECT
     This allows each transmitter to be locked to anatomy.
     Transmitters mounted on stands can have individual alignments to account for manufacturing tolerances.
     First, assign the BaseObject to the anatomical model.
     The BaseObject is typically the virtual index plate - which is often not rendered.  
     Anatomical models are children of the index plate.
     The transmitter alignment to the index plate is stored in transmitter plug EEPROM.
     If EnableBaseObject is true, The baseobject (index plate) transform is continously updated to the transmitter.
     When this index plate alignment needs to be adjusted (typically when we build a new stand with a new transmitter):
     - With the simulation running, unclick EnableBaseObject.  This releases the updating of the tracking system transform.
     - Select the Index Plate game object - its helpful to enable the renderer so you can see it.
     - adjust the transform of the index plate in the Scene view,
     - Click the inspector button "BASE OBJECT: Generate a Gameobject Transform String in a Format Suitable for the Transmitter Plug EEPROM"
     - Click the inspector button "Upload Generated String to EEPROM"   
     The BaseObject can be null or unassigned if a project does not use it. 

     TRACKING TOOLS:
     similar to tracking anatomy, except that the inspector is used to adjust the aligment offsets in real-time instead of the scene.


     TYPICAL SENSOR ORDER:
     1 = Camera TUI/Stylus
     2 = Ultrasound Probe
     3 = Needle/Trocar
     4 = Anatomy (if tracked with sensor)

     FUNCITONS NOT CURRENTLY USED but available:
     Text file initialization:
     The tracking system can initialize from a text file.  
     ATCErrorCode = LoadFromConfigFile("c:/CSSALT/ATC.ini");
     The tracking system can save an initialion text file. 
     ATCErrorCode = SaveNewConfigFile("c:/CSSALT/report.ini");
     This API functionality has been useful during debuging.

     Always read the EEPROMS and pass the offsets to ATC every startup.

     NOTE 10-27-2016 TJ:
     Two transforms set for ANATOMY object. 
     Original transform:
        Position: x = 55.58, y = 100.16, z = -118.51
        Rotation: x = 0    , y = 0     , z = 0
        Scale   : x = 1    , y = 1     , z = 1
    Modified transform:
        Position: x = 0    , y = 100   , z = -60
        Rotation: x = 0    , y = -90   , z = 0
        Scale   : x = 1    , y = 1     , z = 1
    */



    public class ATC : MonoBehaviour
    {

        // --------- Singleton Reference to this script --------------------------------------------------------------------------
        public static ATC ME;

        // --------- Reference to Plugin Functions - there are many, many more at the bottom -------------------------------------
        [DllImport("ATC64")]
        private static extern int HelloATC(bool ReadSensorEEPROMS);

        [Header("----------- Base Object Locked to the Transmitter -----------------------------------------------------------")]
        [SerializeField]
        private GameObject baseObject;      // (Optional) Used to intrinsically track the base if the base is locked to the transmitter
        [SerializeField]
        private bool enableBaseObject=true;      // For replaying and resetting other transmitters and co-ordinate systems 
        private Vector3 baseObjectPosition;
        private Quaternion baseObjectRotation;

        public GameObject BaseObject { get { return baseObject; }  set {baseObject=value; } }
        public bool EnableBaseObject
        {
            get
            {
                return enableBaseObject;
            }
            set
            {
                enableBaseObject = value;
            }
        }
        public Vector3 BaseObjectPosition
        {
            get
            {
                return baseObjectPosition;
            }
            set
            {
                baseObjectPosition = value;
            }
        }
        public Quaternion BaseObjectRotation
        {
            get
            {
                return baseObjectRotation;
            }
            set
            {
                baseObjectRotation = value;
            }
        }


        [Header("----------- Gameobject Tracked with Sensor Plug 1 ------------------------------------------------------------")]
        [SerializeField]
        private GameObject trackedObject1;
        [SerializeField]
        private bool enableObject1=true;

        public GameObject TrackedObject1 { get { return trackedObject1; } set { trackedObject1 = value; } }
        public bool EnableObject1 {
            get
            {
                return enableObject1;
            }
            set
            {
                enableObject1 = value;
            }
        }

        [Header("----------- Gameobject Tracked with Sensor Plug 2 ------------------------------------------------------------")]
        [SerializeField]
        private GameObject trackedObject2;
        [SerializeField]
        private bool enableObject2=true;

        public GameObject TrackedObject2 { get { return trackedObject2; } set { trackedObject2 = value; } }

        public bool EnableObject2
        {
            get
            {
                return enableObject2;
            }
            set
            {
                enableObject2 = value;
            }
        }

        [Header("----------- Gameobject Tracked with Sensor Plug 3 ------------------------------------------------------------")]
        [SerializeField]
        private GameObject trackedObject3;
        [SerializeField]
        private bool enableObject3=true;

        public GameObject TrackedObject3  { get { return trackedObject3; } set { trackedObject3 = value; } }
        public bool EnableObject3
        {
            get
            {
                return enableObject3;
            }
            set
            {
                enableObject3 = value;
            }
        }

        [Header("----------- Gameobject Tracked with Sensor Plug 4 ------------------------------------------------------------")]
        [SerializeField]
        private GameObject trackedObject4;
        [SerializeField]
        private bool enableObject4;

        public GameObject TrackedObject4 { get { return trackedObject4; } set { trackedObject4 = value; } }
        public bool EnableObject4
        {
            get
            {
                return enableObject4;
            }
            set
            {
                enableObject4 = value;
            }
        }

        [Header("----------- RealtimeTracking() Toggle Effects These Objects --------------------------------------------------")]

        [SerializeField]
        private bool realtimeToggleBaseObject=true;
        [SerializeField]
        private bool realtimeToggleTrackedObject1=false;
        [SerializeField]
        private bool realtimeToggleTrackedObject2=true;
        [SerializeField]
        private bool realtimeToggleTrackedObject3=true;
        [SerializeField]
        private bool realtimeToggleTrackedObject4;

        public bool RealtimeToggleBaseObject
        {
            get
            {
                return realtimeToggleBaseObject;
            }
            set
            {
                realtimeToggleBaseObject = value;
            }
        }
        public bool RealtimeToggleTrackedObject1
        {
            get
            {
                return realtimeToggleTrackedObject1;
            }
            set
            {
                realtimeToggleTrackedObject1 = value;
            }
        }
        public bool RealtimeToggleTrackedObject2
        {
            get
            {
                return realtimeToggleTrackedObject2;
            }
            set
            {
                realtimeToggleTrackedObject2 = value;
            }
        }
        public bool RealtimeToggleTrackedObject3
        {
            get
            {
                return realtimeToggleTrackedObject3;
            }
            set
            {
                realtimeToggleTrackedObject3 = value;
            }
        }
        public bool RealtimeToggleTrackedObject4
        {
            get
            {
                return realtimeToggleTrackedObject4;
            }
            set
            {
                realtimeToggleTrackedObject4 = value;
            }
        }


        [Header("----------- System Status and Error Reports ------------------------------------------------------------------")]
        [SerializeField]
        private bool connected;
        public State ConnectionStatus;
        public enum State { NotConnected, AttemptingToConnect, WaitingToReconnect, Connected };
        [SerializeField]
        private string errorReport;
        private int atcErrorCode; // most ATC functions return an integer error code, this is the raw one.
        private float restartWaitSeconds = 5; // if connection is lost, retry to connect every so often, not every frame.
        private float restartWaitTimer = 0;

        public bool Connected
        {
            get
            {
                return connected;
            }
            set
            {
                connected = value;
            }
        }
        public string ErrorReport
        {
            get
            {
                return errorReport;
            }
            set
            {
                errorReport = value;
            }
        }
        public int AtcErrorCode
        {
            get
            {
                return atcErrorCode;
            }
            set
            {
                atcErrorCode = value;
            }
        }
        public float RestartWaitSeconds
        {
            get
            {
                return restartWaitSeconds;
            }
            set
            {
                restartWaitSeconds = value;
            }
        }
        public float RestartWaitTimer
        {
            get
            {
                return restartWaitTimer;
            }
            set
            {
                restartWaitTimer = value;
            }
        }

        //Error reporting for BSOD
        private bool isMainPowerDown = false;
        public bool IsMainPowerDown
        {
            get
            {
                return isMainPowerDown;
            }
        }

        [Header("----------- Transmitter Settings and Reference Frame ---------------------------------------------------------")]
        public Zone Hemisphere=Zone.BOTTOM; // this is an ATC property of each sensor, refers to transmitter hemisphere, we assume one for all sensors.
        public enum Zone { FRONT = 1, BACK, TOP, BOTTOM, LEFT, RIGHT }; // handy enum for readability
        [SerializeField]
        private Vector3 transmitterRotation; // this preset based on typical configurations of the modlular stand is applied in Start()
        [SerializeField]
        private bool useModularStandPresetRotation=true; // use this for different transmitter rotations

        public Vector3 TransmitterRotation
        {
            get
            {
                return transmitterRotation;
            }
            set
            {
                transmitterRotation = value;
            }
        }
        public bool UseModularStandPresetRotation
        {
            get
            {
                return useModularStandPresetRotation;
            }
            set
            {
                useModularStandPresetRotation = value;
            }
        }


        [Header("----------- Filter Settings (Applied to all Sensors) ---------------------------------------------------------")]
        [SerializeField]
        private bool usePresetsForSRT=true; // This will set all filter settings and the Volume Correction Factor to Dave's best result from 4/24/2015
        [SerializeField]
        private int samplingFrequency=200; // this is the internal hardware setting in the Ascension box - has to do with how well the filters work
        [SerializeField]
        private bool applyCustomFilters; // if this is true, we upload sensor filters, all sensors get the same filters for now.
        [SerializeField]
        private bool ignoreLargeChanges, ac_WideNotchFilter, ac_NarrowNotchFilter, dc_AdaptiveFilter; // ATC sensor filter settings
        public int[] Vm = new int[7]; // DC Adaptive filter settings
        [SerializeField]
        private int alphaMin, alphaMax; // DC Alpha filter settings

        public bool UsePresetsForSRT
        {
            get
            {
                return usePresetsForSRT;
            }
            set
            {
                usePresetsForSRT = value;
            }
        }
        public int SamplingFrequency { get{ return samplingFrequency; } set { samplingFrequency = value; } }

        public bool ApplyCustomFilters
        {
            get
            {
                return applyCustomFilters;
            }
            set
            {
                applyCustomFilters = value;
            }
        }
        public bool IgnoreLargeChanges
        {
            get
            {
                return ignoreLargeChanges;
            }
            set
            {
                ignoreLargeChanges = value;
            }
        }
        public bool Ac_WideNotchFilter
        {
            get
            {
                return ac_WideNotchFilter;
            }
            set
            {
                ac_WideNotchFilter = value;
            }
        }
        public bool Ac_NarrowNotchFilter
        {
            get
            {
                return ac_NarrowNotchFilter;
            }
            set
            {
                ac_NarrowNotchFilter = value;
            }
        }
        public bool Dc_AdaptiveFilter
        {
            get
            {
                return dc_AdaptiveFilter;
            }
            set
            {
                dc_AdaptiveFilter = value;
            }
        }
        public int AlphaMin
        {
            get
            {
                return alphaMin;
            }
            set
            {
                alphaMin = value;
            }
        }
        public int AlphaMax
        {
            get
            {
                return alphaMax;
            }
            set
            {
                alphaMax = value;
            }
        }

        // This was needed for the SRT filters on the first SRT we developed on only and is under investigation.
        [Header("----------- Tracking Volume Scale Correction Factor ----------------------------------------------------------")]
        private float volumeCorrectionFactor;

        public float VolumeCorrectionFactor
        {
            get
            {
                return volumeCorrectionFactor;
            }
            set
            {
                volumeCorrectionFactor = value;
            }
        }


        [Header("----------- Edit the Sensor Alignments -----------------------------------------------------------------------")]
        public AlignSensor EditSensor = AlignSensor.NONE;
        public enum AlignSensor { NONE = 0, Sensor1 = 1, Sensor2 = 2, Sensor3 = 3, Sensor4 = 4 };
        private AlignSensor LastEditedSensor = AlignSensor.NONE;
        [SerializeField]
        private Vector3 offsetPosition;
        [SerializeField]
        private Vector3 offsetRotation;
        [SerializeField]
        private Vector4 utilities;


        public Vector3 OffsetPosition
        {
            get
            {
                return offsetPosition;
            }
            set
            {
                offsetPosition = value;
            }
        }
        public Vector3 OffsetRotation
        {
            get
            {
                return offsetRotation;
            }
            set
            {
                offsetRotation = value;
            }
        }
        public Vector4 Utilities
        {
            get
            {
                return utilities;
            }
            set
            {
                utilities = value;
            }
        }

        // These are filled by reading the EEPROMS at startup and are used for real-time tuning of the offsets
        private Vector3 sensor1OffsetPosition = Vector3.zero;
        private Vector3 sensor2OffsetPosition = Vector3.zero;
        private Vector3 sensor3OffsetPosition = Vector3.zero;
        private Vector3 sensor4OffsetPosition = Vector3.zero;
        private Vector3 sensor1OffsetRotation = Vector3.zero;
        private Vector3 sensor2OffsetRotation = Vector3.zero;
        private Vector3 sensor3OffsetRotation = Vector3.zero;
        private Vector3 sensor4OffsetRotation = Vector3.zero;

        // These are filled by reading the EEPROMS at startup and are used for utility functions
        private Vector4 sensor1Utilities = Vector4.zero;
        private Vector4 sensor2Utilities = Vector4.zero;
        private Vector4 sensor3Utilities = Vector4.zero;
        private Vector4 sensor4Utilities = Vector4.zero;

        // used for noticing changes for uploading to the ATC 
        private Vector3 lastTransmitterRotation;
        private Vector3 lastSensorOffsetPosition;
        private Vector3 lastSensorOffsetRotation;


        [Header("----------- What's Currently on the EEPROMs ------------------------------------------------------------------")]
        [SerializeField]
        private string memorySensorPlug1 = ""; // Sensor memory - we write alignments on the plug
        [SerializeField]
        private string memorySensorPlug2 = ""; // Sensor memory - we write alignments on the plug
        [SerializeField]
        private string memorySensorPlug3 = ""; // Sensor memory - we write alignments on the plug
        [SerializeField]
        private string memorySensorPlug4 = ""; // Sensor memory - we write alignments on the plug
        [SerializeField]
        private string memoryTransmitterPlug = ""; // Transmitter memory - we write alignments on the plug
        [SerializeField]
        private string memoryTrackingUnit = ""; // Board memory - we write alignments on a chip in the board
        private bool ReadSensorEEPROMS = true; // Startup flag.  Reading EEPROMS takes extra time and must done every time the ascension tracking system is powered.  Do this every time; ATC cannot tell the difference between USB disconnected and power lost. 

        public string MemorySensorPlug1
        {
            get
            {
                return memorySensorPlug1;
            }
            set
            {
                memorySensorPlug1 = value;
            }
        }
        public string MemorySensorPlug2
        {
            get
            {
                return memorySensorPlug2;
            }
            set
            {
                memorySensorPlug2 = value;
            }
        }
        public string MemorySensorPlug3
        {
            get
            {
                return memorySensorPlug3;
            }
            set
            {
                memorySensorPlug3 = value;
            }
        }
        public string MemorySensorPlug4
        {
            get
            {
                return memorySensorPlug4;
            }
            set
            {
                memorySensorPlug4 = value;
            }
        }
        public string MemoryTransmitterPlug
        {
            get
            {
                return memoryTransmitterPlug;
            }
            set
            {
                memoryTransmitterPlug = value;
            }
        }
        public string MemoryTrackingUnit
        {
            get
            {
                return memoryTrackingUnit;
            }
            set
            {
                memoryTrackingUnit = value;
            }
        }

        [Header("----------- Edit the EEPROMs ---------------------------------------------------------------------------------")]
        public EEPROM UploadTarget = EEPROM.NONE;
        public enum EEPROM { NONE = 0, Sensor1 = 1, Sensor2 = 2, Sensor3 = 3, Sensor4 = 4, Transmitter = 5, Board = 6 }; // the plugin expects this order
        [SerializeField]
        private string uploadText;

        public string UploadText
        {
            get
            {
                return uploadText;
            }
            set
            {
                uploadText = value;
            }
        }

        // not used yet, perhaps someday?
        // private enum XRT { MRT, SRT, WRT };
        // private XRT TransmitterType;

        //Thread declaration
        private Thread connectThread;

        // --------- Awake Singleton Constructor --------------------------------//
        void Awake()
        {

            if (ME != null)
                GameObject.Destroy(ME);
            else
                ME = this;

            DontDestroyOnLoad(this);

        }
        // --------- Awake Singleton Constructor --------------------------------//


        // --------- Start Connection Sequence ----------------------------------//
        void Start()
        {

            connectThread = new Thread(ConnectToATC) { Name = "ATC Thread" };

            // update will read this status and try to reconnect.
            ConnectionStatus = State.WaitingToReconnect;

            // We will wait only one second on startup to let the screen refresh.
            restartWaitTimer = restartWaitSeconds - 1;

            // Suggested Transmitter co-ordinate system rotations
            // These presets make the most sense in the Unity editor based on the MRT in the 2014 modular stand.
            if (useModularStandPresetRotation)
            {
                if (Hemisphere == Zone.FRONT) transmitterRotation = new Vector3(0, 180F, 0);
                if (Hemisphere == Zone.BOTTOM) transmitterRotation = new Vector3(0, 180F, 180F);
                if (Hemisphere == Zone.BACK) transmitterRotation = new Vector3(0, 0, 0);
                if (Hemisphere == Zone.TOP) transmitterRotation = new Vector3(0, 180F, 0);
            }

            // Enable tracking on all tracked objects by default if they are named.
            /*
            enableBaseObject = (baseObject != null);
            enableObject1 = (trackedObject1 != null);
            enableObject2 = (trackedObject2 != null);
            enableObject3 = (trackedObject3 != null);
            enableObject4 = (trackedObject4 != null);
            */

            // This will set all filter settings and the Volume Correction Factor to Dave's best result from 4/24/2015
            if (usePresetsForSRT)
            {
                samplingFrequency = 255;
                applyCustomFilters = true;
                ignoreLargeChanges = false;
                ac_WideNotchFilter = true;
                ac_NarrowNotchFilter = false;
                dc_AdaptiveFilter = true;
                Vm[0] = 1;
                Vm[1] = 1;
                Vm[2] = 1;
                Vm[3] = 1;
                Vm[4] = 1;
                Vm[5] = 10;
                Vm[6] = 200;
                alphaMin = 50;
                alphaMax = 15000;
            }

            // VolumeCorrectionFactor is zero if we forgot to set it
            if (volumeCorrectionFactor == 0) volumeCorrectionFactor = 1;


        }
        // --------- Start Connection Sequence ----------------------------------//


        // --------- Begin Connection Sequence ----------------------------------//
        void ConnectToATC()
        {
            // Set an internal flag 
            ConnectionStatus = State.AttemptingToConnect;

            // Initializes connection to Ascension board, sets metric, sensor data formats, reads the EEPROMS (if asked) and turns on the transmitter
            atcErrorCode = HelloATC(ReadSensorEEPROMS);

            // Debug.Log("CONNECT TO ATC, read EEPROMS:" + ReadSensorEEPROMS.ToString());

            if (atcErrorCode == 0)
            {

                isMainPowerDown = false;
                // Set Measurement Rate.  This is a system parameter in the API.  NOTE, This may strongly effect how well the filters work.
                if (samplingFrequency >= 20 && samplingFrequency <= 255)
                {
                    if (atcErrorCode == 0) atcErrorCode = SetMeasurementRate(samplingFrequency);
                }
                else
                {
                    print("The Sampling Frequency needs to be between 20 and 255 Hz.  Check the ATC settings!");
                }

                // Set Hemisphere. This is a sensor parameter in the API.  We usually assume the same hemisphere for all sensors.
                if (atcErrorCode == 0) atcErrorCode = SetHemisphere(1, (int)Hemisphere);
                if (atcErrorCode == 0) atcErrorCode = SetHemisphere(2, (int)Hemisphere);
                if (atcErrorCode == 0) atcErrorCode = SetHemisphere(3, (int)Hemisphere);
                if (atcErrorCode == 0) atcErrorCode = SetHemisphere(4, (int)Hemisphere);

                // Rotate Coordinate System. This is a transmitter parameter in the API and will apply to all sensors.
                if (atcErrorCode == 0) atcErrorCode = RotateTransmitterReferenceFrame(transmitterRotation.x, transmitterRotation.y, transmitterRotation.z);

                // Set Signal Processing Filter Parameters. These are sensor parameters in the API.  We usually assume the same filters for all.
                // note the API will convert AlphaMin and Max to whatever you set here divided by 32765
                if (applyCustomFilters)
                {
                    // Set Filters for sensor plug 1
                    if (atcErrorCode == 0)
                        atcErrorCode = SetSensorFilterParameters(1, ignoreLargeChanges, ac_WideNotchFilter, ac_NarrowNotchFilter, dc_AdaptiveFilter, Vm[0], Vm[1], Vm[2], Vm[3], Vm[4], Vm[5], Vm[6], alphaMin, alphaMax);

                    // Set Filters for sensor plug 2
                    if (atcErrorCode == 0)
                        atcErrorCode = SetSensorFilterParameters(2, ignoreLargeChanges, ac_WideNotchFilter, ac_NarrowNotchFilter, dc_AdaptiveFilter, Vm[0], Vm[1], Vm[2], Vm[3], Vm[4], Vm[5], Vm[6], alphaMin, alphaMax);

                    // Set Filters for sensor plug 3
                    if (atcErrorCode == 0)
                        atcErrorCode = SetSensorFilterParameters(3, ignoreLargeChanges, ac_WideNotchFilter, ac_NarrowNotchFilter, dc_AdaptiveFilter, Vm[0], Vm[1], Vm[2], Vm[3], Vm[4], Vm[5], Vm[6], alphaMin, alphaMax);

                    // Set Filters for sensor plug 4
                    if (atcErrorCode == 0)
                        atcErrorCode = SetSensorFilterParameters(4, ignoreLargeChanges, ac_WideNotchFilter, ac_NarrowNotchFilter, dc_AdaptiveFilter, Vm[0], Vm[1], Vm[2], Vm[3], Vm[4], Vm[5], Vm[6], alphaMin, alphaMax);

                }

                // Clears any old Error message from the property inspector or - hopefully not - reports an app-killing error.
                LookupErrorDescription(atcErrorCode);

                ConnectionStatus = State.Connected;




            }
            else // HelloATC() returned an error code probably because its not plugged in. Whatever it is, try to reconnect after a few seconds.
            {

                ConnectionStatus = State.WaitingToReconnect;

                LookupErrorDescription(atcErrorCode);
                if (atcErrorCode == -2147483634 || atcErrorCode == -2013265855)
                {
                    Connected = false;
                }
                if (atcErrorCode == -1610612685)
                {
                    isMainPowerDown = true;
                }
            }

            // Reset the restart wait timer to zero - if connections failed, don't retry every single frame.
            restartWaitTimer = 0;

        }
        // --------- Begin Connection Sequence ----------------------------------//


        // --------- Organize Data Read off of the EEPROMS ----------------------//
        void ParseDataFromEEPROMs()
        {
            string[] data;

            // Base Object - reading MemoryTransmitterPlug
            if (baseObject != null)
            {
                if (!String.IsNullOrEmpty(memoryTransmitterPlug))
                {
                    data = memoryTransmitterPlug.Split(' ');
                    if (data.Length > 8)
                    {
                        Vector3 pos = Vector3.zero;
                        pos.x = (float)Convert.ToDouble(data[3].Trim());
                        pos.y = (float)Convert.ToDouble(data[4].Trim());
                        pos.z = (float)Convert.ToDouble(data[5].Trim());

                        Quaternion rot = Quaternion.identity;
                        rot.w = (float)Convert.ToDouble(data[6].Trim());
                        rot.x = (float)Convert.ToDouble(data[7].Trim());
                        rot.y = (float)Convert.ToDouble(data[8].Trim());
                        rot.z = (float)Convert.ToDouble(data[9].Trim());

                        baseObjectPosition = pos;
                        baseObjectRotation = rot;

                        // Unlike the sensors, the baseobject doesn't change; its transform data is saved once instead.
                        baseObject.transform.position = baseObjectPosition;
                        baseObject.transform.rotation = baseObjectRotation;
                    }
                }
            }

            // Sensor 1 - reading MemorySensorPlug1
            if (!String.IsNullOrEmpty(memorySensorPlug1))
            {
                data = memorySensorPlug1.Split(' ');
                if (data.Length > 8)
                {
                    sensor1OffsetPosition.x = (float)Convert.ToDouble(data[3].Trim());
                    sensor1OffsetPosition.y = (float)Convert.ToDouble(data[4].Trim());
                    sensor1OffsetPosition.z = (float)Convert.ToDouble(data[5].Trim());
                    sensor1OffsetRotation.x = (float)Convert.ToDouble(data[6].Trim());
                    sensor1OffsetRotation.y = (float)Convert.ToDouble(data[7].Trim());
                    sensor1OffsetRotation.z = (float)Convert.ToDouble(data[8].Trim());
                    SetSensorPositionOffset(1, sensor1OffsetPosition.x, sensor1OffsetPosition.y, sensor1OffsetPosition.z);
                    SetSensorRotationOffset(1, sensor1OffsetRotation.x, sensor1OffsetRotation.y, sensor1OffsetRotation.z);
                }
                if (data.Length > 12)
                {
                    sensor1Utilities.x = (float)Convert.ToDouble(data[9].Trim());
                    sensor1Utilities.y = (float)Convert.ToDouble(data[10].Trim());
                    sensor1Utilities.z = (float)Convert.ToDouble(data[11].Trim());
                    sensor1Utilities.w = (float)Convert.ToDouble(data[12].Trim());
                }
            }


            // Sensor 2 - reading MemorySensorPlug2
            if (!String.IsNullOrEmpty(memorySensorPlug2))
            {
                data = memorySensorPlug2.Split(' ');
                if (data.Length > 8)
                {
                    sensor2OffsetPosition.x = (float)Convert.ToDouble(data[3].Trim());
                    sensor2OffsetPosition.y = (float)Convert.ToDouble(data[4].Trim());
                    sensor2OffsetPosition.z = (float)Convert.ToDouble(data[5].Trim());
                    sensor2OffsetRotation.x = (float)Convert.ToDouble(data[6].Trim());
                    sensor2OffsetRotation.y = (float)Convert.ToDouble(data[7].Trim());
                    sensor2OffsetRotation.z = (float)Convert.ToDouble(data[8].Trim());
                    SetSensorPositionOffset(2, sensor2OffsetPosition.x, sensor2OffsetPosition.y, sensor2OffsetPosition.z);
                    SetSensorRotationOffset(2, sensor2OffsetRotation.x, sensor2OffsetRotation.y, sensor2OffsetRotation.z);
                }
                if (data.Length > 12)
                {
                    sensor2Utilities.x = (float)Convert.ToDouble(data[9].Trim());
                    sensor2Utilities.y = (float)Convert.ToDouble(data[10].Trim());
                    sensor2Utilities.z = (float)Convert.ToDouble(data[11].Trim());
                    sensor2Utilities.w = (float)Convert.ToDouble(data[12].Trim());
                }
            }

            // Sensor 3 - reading MemorySensorPlug3
            if (!String.IsNullOrEmpty(memorySensorPlug3))
            {
                data = memorySensorPlug3.Split(' ');
                if (data.Length > 8)
                {
                    sensor3OffsetPosition.x = (float)Convert.ToDouble(data[3].Trim());
                    sensor3OffsetPosition.y = (float)Convert.ToDouble(data[4].Trim());
                    sensor3OffsetPosition.z = (float)Convert.ToDouble(data[5].Trim());
                    sensor3OffsetRotation.x = (float)Convert.ToDouble(data[6].Trim());
                    sensor3OffsetRotation.y = (float)Convert.ToDouble(data[7].Trim());
                    sensor3OffsetRotation.z = (float)Convert.ToDouble(data[8].Trim());
                    SetSensorPositionOffset(3, sensor3OffsetPosition.x, sensor3OffsetPosition.y, sensor3OffsetPosition.z);
                    SetSensorRotationOffset(3, sensor3OffsetRotation.x, sensor3OffsetRotation.y, sensor3OffsetRotation.z);
                }
                if (data.Length > 12)
                {
                    sensor3Utilities.x = (float)Convert.ToDouble(data[9].Trim());
                    sensor3Utilities.y = (float)Convert.ToDouble(data[10].Trim());
                    sensor3Utilities.z = (float)Convert.ToDouble(data[11].Trim());
                    sensor3Utilities.w = (float)Convert.ToDouble(data[12].Trim());
                }
            }

            // Sensor 4 - reading MemorySensorPlug4
            if (!String.IsNullOrEmpty(memorySensorPlug4))
            {
                data = memorySensorPlug4.Split(' ');
                if (data.Length > 8)
                {
                    sensor4OffsetPosition.x = (float)Convert.ToDouble(data[3].Trim());
                    sensor4OffsetPosition.y = (float)Convert.ToDouble(data[4].Trim());
                    sensor4OffsetPosition.z = (float)Convert.ToDouble(data[5].Trim());
                    sensor4OffsetRotation.x = (float)Convert.ToDouble(data[6].Trim());
                    sensor4OffsetRotation.y = (float)Convert.ToDouble(data[7].Trim());
                    sensor4OffsetRotation.z = (float)Convert.ToDouble(data[8].Trim());
                    SetSensorPositionOffset(4, sensor4OffsetPosition.x, sensor4OffsetPosition.y, sensor4OffsetPosition.z);
                    SetSensorRotationOffset(4, sensor4OffsetRotation.x, sensor4OffsetRotation.y, sensor4OffsetRotation.z);
                }
                if (data.Length > 12)
                {
                    sensor4Utilities.x = (float)Convert.ToDouble(data[9].Trim());
                    sensor4Utilities.y = (float)Convert.ToDouble(data[10].Trim());
                    sensor4Utilities.z = (float)Convert.ToDouble(data[11].Trim());
                    sensor4Utilities.w = (float)Convert.ToDouble(data[12].Trim());
                }
            }

        }
        // --------- Organize Data Read off of the EEPROMS ----------------------//


        // --------- Every Rendering Frame --------------------------------------//
        void Update()
        {
            // Connected is a public variable for other scripts to see. We don't use it here
            connected = (ConnectionStatus == State.Connected);

            // if not connected, wait a bit before trying again.
            if (ConnectionStatus == State.WaitingToReconnect)
            {
                restartWaitTimer = restartWaitTimer + Time.deltaTime;
                if (restartWaitTimer > restartWaitSeconds)
                {
                    if (!connectThread.IsAlive)
                    {
                        try { connectThread.Start(); }
                        catch (Exception)
                        {
                            connectThread.Abort();
                            connectThread = new Thread(ConnectToATC) { Name = "ATC Thread" };
                            connectThread.Start();
                            //Debug.Log("Reconnecting...");
                        }
                    }
                }
            }

            // if we have been connected since last frame, then proceed
            if (ConnectionStatus == State.Connected)
            {
                // we read strings from the C++ dll using int pointers and memory marshaling.
                if (ReadSensorEEPROMS)
                {
                    IntPtr echoedStringPtr1 = ReadSensor1EEPROM();
                    memorySensorPlug1 = Marshal.PtrToStringAnsi(echoedStringPtr1);

                    IntPtr echoedStringPtr2 = ReadSensor2EEPROM();
                    memorySensorPlug2 = Marshal.PtrToStringAnsi(echoedStringPtr2);

                    IntPtr echoedStringPtr3 = ReadSensor3EEPROM();
                    //memorySensorPlug3 = Marshal.PtrToStringAnsi(echoedStringPtr3);

                    IntPtr echoedStringPtr4 = ReadSensor4EEPROM();
                    //memorySensorPlug4 = Marshal.PtrToStringAnsi(echoedStringPtr4);

                    IntPtr echoedStringPtr5 = ReadTransmitterEEPROM();
                    memoryTransmitterPlug = Marshal.PtrToStringAnsi(echoedStringPtr5);

                    IntPtr echoedStringPtr6 = ReadBoardEEPROM();
                    memoryTrackingUnit = Marshal.PtrToStringAuto(echoedStringPtr6);

                    ReadSensorEEPROMS = true; // ALWAYS - never set to FALSE.  

                    ParseDataFromEEPROMs();

                }

                // attempt to generate a new set of sensor locations.  
                atcErrorCode = MakeRecordsATC();

                // If everything is OK our records should be good to use.
                if (atcErrorCode == 0)
                {

                    // if Base Object is enabled, update the base object to the transmitter position and rotation.  
                    // This need not be done every frame.  It is done here to keep the code simple and straightforward.
                    if (enableBaseObject)
                    {
                        baseObject.transform.position = baseObjectPosition;
                        baseObject.transform.rotation = baseObjectRotation;
                    }

                    // If Tracking1 is enabled, then update sensor 1 position and quaternion.
                    if (enableObject1)
                    {
                        trackedObject1.transform.position = new Vector3(GetRecordATC_1x(), GetRecordATC_1y(), GetRecordATC_1z()) * volumeCorrectionFactor;
                        trackedObject1.transform.rotation = new Quaternion(GetRecordATC_1q0(), GetRecordATC_1q1(), GetRecordATC_1q2(), GetRecordATC_1q3());
                        if (TrackedObject1.transform.position == new Vector3(0, 0, 0))
                        {
                            ConnectionStatus = State.WaitingToReconnect;
                            isMainPowerDown = true;
                        }
                    }

                    // If Tracking2 is enabled, then update sensor 2 position and quaternion.
                    if (enableObject2)
                    {
                        trackedObject2.transform.position = new Vector3(GetRecordATC_2x(), GetRecordATC_2y(), GetRecordATC_2z()) * volumeCorrectionFactor;
                        trackedObject2.transform.rotation = new Quaternion(GetRecordATC_2q0(), GetRecordATC_2q1(), GetRecordATC_2q2(), GetRecordATC_2q3());
                        if (TrackedObject2.transform.position == new Vector3(0, 0, 0))
                        {
                            ConnectionStatus = State.WaitingToReconnect;
                            isMainPowerDown = true;
                        }
                    }

                    // If Tracking3 is enabled, then update sensor 3 position and quaternion.
                    if (enableObject3)
                    {
                        trackedObject3.transform.position = new Vector3(GetRecordATC_3x(), GetRecordATC_3y(), GetRecordATC_3z()) * volumeCorrectionFactor;
                        trackedObject3.transform.rotation = new Quaternion(GetRecordATC_3q0(), GetRecordATC_3q1(), GetRecordATC_3q2(), GetRecordATC_3q3());
                        if (TrackedObject3.transform.position == new Vector3(0, 0, 0))
                        {
                            ConnectionStatus = State.WaitingToReconnect;
                            isMainPowerDown = true;
                        }
                    }

                    // If Tracking4 is enabled, then update sensor 3 position and quaternion.
                    if (enableObject4)
                    {
                        trackedObject4.transform.position = new Vector3(GetRecordATC_4x(), GetRecordATC_4y(), GetRecordATC_4z()) * volumeCorrectionFactor;
                        trackedObject4.transform.rotation = new Quaternion(GetRecordATC_4q0(), GetRecordATC_4q1(), GetRecordATC_4q2(), GetRecordATC_4q3());
                        if (TrackedObject4.transform.position == new Vector3(0, 0, 0))
                        {
                            ConnectionStatus = State.WaitingToReconnect;
                            isMainPowerDown = true;
                        }
                    }


                }
                else // MakeRecordsATC() returned an error code. Something is wrong.
                {
                    ConnectionStatus = State.WaitingToReconnect;
                    LookupErrorDescription(atcErrorCode);
                    // if (ATCErrorCode == -2013265855) Debug.Log("We lost the ATC.  Check if anything is unplugged or unpowered.");
                }

            }

        }
        // --------- Every Rendering Frame --------------------------------------//


        // --------- Toggle Realtime Tracking of Selected Objects ---------------//
        public void RealtimeTracking(bool activeState)
        {

            if (baseObject != null) { if (realtimeToggleBaseObject) enableBaseObject = activeState; }
            if (trackedObject1 != null) { if (realtimeToggleTrackedObject1) enableObject1 = activeState; }
            if (trackedObject2 != null) { if (realtimeToggleTrackedObject2) enableObject2 = activeState; }
            if (trackedObject3 != null) { if (realtimeToggleTrackedObject3) enableObject3 = activeState; }
            if (trackedObject4 != null) { if (realtimeToggleTrackedObject4) enableObject4 = activeState; }

        }
        // --------- Toggle Realtime Tracking of Selected Objects ---------------//

        // --------- Quit Gracefully --------------------------------------------//
        void OnApplicationQuit()
        {
            atcErrorCode = GoodbyeATC();
        }
        // --------- Quit Gracefully --------------------------------------------//


        // --------- Turn ErrorCodes into Something Useful ----------------------//
        private string noError = "No Error Codes to lookup.";
        void LookupErrorDescription(int errorCode)
        {
            if (errorCode != 0)
            {
                IntPtr echoedStringErr = GetErrorsReport(errorCode);
                errorReport = Marshal.PtrToStringAuto(echoedStringErr);
                //Debug.Log("ATC Error Report: " + errorReport);
            }
            else
            {
                errorReport = noError;
            }
        }
        // --------- Turn ErrorCodes into Something Useful ----------------------//

        public Vector4 LookupTrackedObjectUtilities(string trackedObjectName)
        {

            Vector4 results = Vector4.zero;
            if (trackedObject1 != null && trackedObjectName == trackedObject1.name)
                results = sensor1Utilities;
            if (trackedObject2 != null && trackedObjectName == trackedObject2.name)
                results = sensor2Utilities;
            if (trackedObject3 != null && trackedObjectName == trackedObject3.name)
                results = sensor3Utilities;
            if (trackedObject4 != null && trackedObjectName == trackedObject4.name)
                results = sensor4Utilities;

            return results;
        }



        ///////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR /////////////////////////////////////////////////////////

    // --------- Special Alignment Stuff - EDITOR ONLY ----------------------//
    void LateUpdate()
    {

        // if we have been connected since last frame, then proceed
        if (ConnectionStatus == State.Connected)
        {

            // Want to play with the Transmitter Reference Frame Rotation in realtime?  Set UsePresetTransmitterRotation to true.
            if (useModularStandPresetRotation == false)
            {
                if (transmitterRotation != lastTransmitterRotation)
                {
                    atcErrorCode = RotateTransmitterReferenceFrame(transmitterRotation.x, transmitterRotation.y, transmitterRotation.z);
                    lastTransmitterRotation = transmitterRotation;
                    LookupErrorDescription(atcErrorCode);
                }
            }

            // Adjust the Sensor Position and Rotation Offsets in realtime
            if (EditSensor != AlignSensor.NONE)
            {
                // if a new sensor was just selected, then load the OffsetPosition and Rotation from what was previously read from EEPROM.
                if (LastEditedSensor != EditSensor)
                {
                    if (EditSensor == AlignSensor.Sensor1) offsetPosition = sensor1OffsetPosition;
                    if (EditSensor == AlignSensor.Sensor1) offsetRotation = sensor1OffsetRotation;
                    if (EditSensor == AlignSensor.Sensor1) utilities = sensor1Utilities;
                    if (EditSensor == AlignSensor.Sensor2) offsetPosition = sensor2OffsetPosition;
                    if (EditSensor == AlignSensor.Sensor2) offsetRotation = sensor2OffsetRotation;
                    if (EditSensor == AlignSensor.Sensor2) utilities = sensor2Utilities;
                    if (EditSensor == AlignSensor.Sensor3) offsetPosition = sensor3OffsetPosition;
                    if (EditSensor == AlignSensor.Sensor3) offsetRotation = sensor3OffsetRotation;
                    if (EditSensor == AlignSensor.Sensor3) utilities = sensor3Utilities;
                    if (EditSensor == AlignSensor.Sensor4) offsetPosition = sensor4OffsetPosition;
                    if (EditSensor == AlignSensor.Sensor4) offsetRotation = sensor4OffsetRotation;
                    if (EditSensor == AlignSensor.Sensor4) utilities = sensor4Utilities;
                }
                LastEditedSensor = EditSensor;

                // Upload any changes to the Sensor Position.  These are temporary and will be overwritten unless saved to EEPROM.
                if (lastSensorOffsetPosition != offsetPosition)
                {
                    SetSensorPositionOffset((int)EditSensor, offsetPosition.x, offsetPosition.y, offsetPosition.z);
                    lastSensorOffsetPosition = offsetPosition;
                }

                // Upload any changes to the Sensor Rotation.  These are temporary and will be overwritten unless saved to EEPROM.
                if (lastSensorOffsetRotation != offsetRotation)
                {
                    SetSensorRotationOffset((int)EditSensor, offsetRotation.x, offsetRotation.y, offsetRotation.z);
                    lastSensorOffsetRotation = offsetRotation;
                }


            }
            else
            {
                // Don't leave confuse the user interface by leaving expired information laying around
                offsetPosition = Vector3.zero;
                offsetRotation = Vector3.zero;
                utilities = Vector4.zero;
            }


        }
    }
    // --------- Special Alignment Stuff - EDITOR ONLY ----------------------//

    // --------- Upload to EEPROM - EDITOR ONLY -----------------------------//
    public void UploadToEEPROM()
    {
        int identifier = (int)UploadTarget;
        if (identifier != 0)
        {

            WriteToEEPROM(identifier, uploadText);
            UploadText = "";
            UploadTarget = EEPROM.NONE;
            EditSensor = AlignSensor.NONE;
            offsetPosition = Vector3.zero;
            offsetRotation = Vector3.zero;
            utilities = Vector4.zero;

            // Flag for the restart sequence - also read the EEPROMS
            ReadSensorEEPROMS = true;

            // update will read this status and try to reconnect.
            ConnectionStatus = State.WaitingToReconnect;

            // We will wait only one second on startup to let the screen refresh.
            restartWaitTimer = restartWaitSeconds - 1;
        }
        else
        {
            Debug.Log("Can't upload. Select an upload target. Be sure to first review your edited/generated string.");
        }
    }
    // --------- Upload to EEPROM - EDITOR ONLY -----------------------------//

    // --------- Make a Formatted Alignment STRING - EDITOR ONLY ------------//
    public void GenerateSensorEEPROMAlignmentString()
    {
        if (EditSensor != AlignSensor.NONE)
        {
            string makerID = "UFCSSALT";
            string date = System.DateTime.Today.Date.ToShortDateString();

            string objectName = "";
            if (EditSensor == AlignSensor.Sensor1) objectName = trackedObject1.name;
            if (EditSensor == AlignSensor.Sensor2) objectName = trackedObject2.name;
            if (EditSensor == AlignSensor.Sensor3) objectName = trackedObject3.name;
            if (EditSensor == AlignSensor.Sensor4) objectName = trackedObject4.name;

            string px = offsetPosition.x.ToString("0.000");
            string py = offsetPosition.y.ToString("0.000");
            string pz = offsetPosition.z.ToString("0.000");
            string rx = offsetRotation.x.ToString("0.000");
            string ry = offsetRotation.y.ToString("0.000");
            string rz = offsetRotation.z.ToString("0.000");
            string ux = utilities.x.ToString("0.000");
            string uy = utilities.y.ToString("0.000");
            string uz = utilities.z.ToString("0.000");
            string uw = utilities.w.ToString("0.000");

            if (EditSensor == AlignSensor.Sensor1) UploadTarget = EEPROM.Sensor1;
            if (EditSensor == AlignSensor.Sensor2) UploadTarget = EEPROM.Sensor2;
            if (EditSensor == AlignSensor.Sensor3) UploadTarget = EEPROM.Sensor3;
            if (EditSensor == AlignSensor.Sensor4) UploadTarget = EEPROM.Sensor4;

            makerID = makerID.Replace(" ", "");
            objectName = objectName.Replace(" ", "");
            date = date.Replace(" ", "");
            uploadText = objectName + " " + makerID + " " + date + " " + px + " " + py + " " + pz + " " + rx + " " + ry + " " + rz + " " + ux + " " + uy + " " + uz + " " + uw;

            print("Review the string before uploading to EEPROM:");
            print(uploadText);
        }
        else
        {
            Debug.Log("Select a sensor to edit from the dropdown list.");
        }

    }
    // --------- Make a Formatted Alignment STRING - EDITOR ONLY ------------//

    // --------- Make a Formatted Alignment STRING - EDITOR ONLY ------------//
    public void GenerateTransmitterEEPROMAlignmentString()
    {
        if (baseObject != null)
        {

            UploadTarget = EEPROM.Transmitter;

            Debug.Log("Generating a Gameobject alignment string for the transmitter...");
            Debug.Log("Be sure the anatomy or alignment objects are children of the Base Object.");
            Debug.Log("This allows this transmitter to be registered to an anatomical base.");

            string makerID = "UFCSSALT";
            string date = System.DateTime.Today.Date.ToShortDateString();

            string objectName = "SRT-MRT";

            string px = baseObject.transform.position.x.ToString("0.000");
            string py = baseObject.transform.position.y.ToString("0.000");
            string pz = baseObject.transform.position.z.ToString("0.000");
            string rw = baseObject.transform.rotation.w.ToString("0.000");
            string rx = baseObject.transform.rotation.x.ToString("0.000");
            string ry = baseObject.transform.rotation.y.ToString("0.000");
            string rz = baseObject.transform.rotation.z.ToString("0.000");
            string ux = utilities.x.ToString("0.000");
            string uy = utilities.y.ToString("0.000");
            string uz = utilities.z.ToString("0.000");
            string uw = utilities.w.ToString("0.000");

            // remove unnecessary spaces
            makerID = makerID.Replace(" ", "");
            objectName = objectName.Replace(" ", "");
            date = date.Replace(" ", "");
            uploadText = objectName + " " + makerID + " " + date + " " + px + " " + py + " " + pz + " " + rw + " " + rx + " " + ry + " " + rz + " " + ux + " " + uy + " " + uz + " " + uw;

            print("Review the string before uploading to EEPROM:");
            print(uploadText);
        }
        else
        {
            Debug.Log("Select a base object first.");
        }

    }
    // --------- Make a Formatted Alignment STRING - EDITOR ONLY ------------//

#endif ///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////

        // --------- References to Plugin Functions -------------------//
        [DllImport("ATC64")]
        private static extern IntPtr GetTransmitterType();
        [DllImport("ATC64")]
        private static extern int SetHemisphere(int sensorPlug, int zone);
        [DllImport("ATC64")]
        private static extern int SetSensorPositionOffset(int sensorPlug, float x, float y, float z);
        [DllImport("ATC64")]
        private static extern int SetSensorRotationOffset(int sensorPlug, float x, float y, float z);
        [DllImport("ATC64")]
        private static extern int RotateTransmitterReferenceFrame(float azimuth, float elevation, float roll);
        [DllImport("ATC64")]
        private static extern int SetMeasurementRate(int freq);
        [DllImport("ATC64")]
        private static extern int SetSensorFilterParameters(int sensorPlug, bool largeChange, bool ACWNF, bool ACNNF, bool DCAF, int Vm0, int Vm1, int Vm2, int Vm3, int Vm4, int Vm5, int Vm6, int alphaMin, int alphaMax);
        [DllImport("ATC64")]
        private static extern IntPtr ReadSensor1EEPROM();
        [DllImport("ATC64")]
        private static extern IntPtr ReadSensor2EEPROM();
        [DllImport("ATC64")]
        private static extern IntPtr ReadSensor3EEPROM();
        [DllImport("ATC64")]
        private static extern IntPtr ReadSensor4EEPROM();
        [DllImport("ATC64")]
        private static extern IntPtr ReadTransmitterEEPROM();
        [DllImport("ATC64")]
        private static extern IntPtr ReadBoardEEPROM();
        [DllImport("ATC64")]
        private static extern void WriteToEEPROM(int Identifier, string Information);
        [DllImport("ATC64")]
        private static extern int MakeRecordsATC();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_1x();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_1y();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_1z();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_1q0();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_1q1();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_1q2();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_1q3();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_2x();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_2y();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_2z();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_2q0();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_2q1();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_2q2();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_2q3();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_3x();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_3y();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_3z();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_3q0();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_3q1();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_3q2();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_3q3();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_4x();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_4y();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_4z();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_4q0();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_4q1();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_4q2();
        [DllImport("ATC64")]
        private static extern float GetRecordATC_4q3();
        [DllImport("ATC64")]
        private static extern IntPtr GetErrorsReport(int errorCode);
        [DllImport("ATC64")]
        private static extern int LoadFromConfigFile(string filename);
        [DllImport("ATC64")]
        private static extern int SaveNewConfigFile(string filename);
        [DllImport("ATC64")]
        private static extern int GoodbyeATC();
        // --------- References to Plugin Functions -------------------//
    }



    ///////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR /////////////////////////////////////////////////////////
// This is a custom editor class that facilitates alignment and management of sensors, transmitters, and board EEPROM
[CustomEditor(typeof(ATC))]
class ATC_Editor : Editor
{
    // IF we are using the TRANSMITTER for alignment, Write the system/antomical registration to the TRANSMITTER'S plug.
    // Unity reads this on startup, parses it, and applies it to GameObject BaseObject's transform.  
    // The anatomy should be a child of GameObject BaseObject.
    private string generateTransmitterLabel = '\n' + "BASE OBJECT: Generate a Gameobject Transform String" + '\n' + "in a Format Suitable for the Transmitter Plug EEPROM" + '\n';

    // Write the registration offsets of the needles/TUI/ultrasound probe to the sensor plugs.  
    // Ascention trakSTAR and driveBAY applies this before it gets to Unity.
    private string generateSensorLabel = '\n' + "SENSOR OBJECT: Generate an Alignment String" + '\n' + "in a Format Suitable for a Sensor Plug EEPROM" + '\n';

    private string uploadLabel = '\n' + "Upload Generated String to EEPROM" + '\n';

    public override void OnInspectorGUI()
    {
        // OnInspectorGUI() overrides the default inspector panel; we want that, so explicity request it. 
        base.OnInspectorGUI();

        GUILayout.Label(""); // blank space

        // Make an Alignment String for the Transmitter using the Base Object.
        if (GUILayout.Button(generateTransmitterLabel)) ATC.ME.GenerateTransmitterEEPROMAlignmentString();

        GUILayout.Label(""); // blank space

        // Make an Alignment String for a Sensor
        if (GUILayout.Button(generateSensorLabel)) ATC.ME.GenerateSensorEEPROMAlignmentString();

        GUILayout.Label(""); // blank space

        // Trigger an EEPROM upload
        if (GUILayout.Button(uploadLabel))
        {
            // If a target exists, ask to double check it through a dialog window.
            if (ATC.ME.UploadTarget != ATC.EEPROM.NONE)
            {
                string alertBoxTitle = "Confirm EEPROM Upload to " + ATC.ME.UploadTarget + " ?";
                string uploadText = ATC.ME.UploadText;

                if (uploadText.Length >= 112)
                {
                    int overLength = uploadText.Length - 112;
                    alertBoxTitle = "Too Many Characters To Upload ";
                    uploadText = "Delete " + overLength + " characters from " + uploadText;
                    if (EditorUtility.DisplayDialog(alertBoxTitle, uploadText, "           OK           "))
                    {

                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog(alertBoxTitle, uploadText, "Make it so.", "      Cancel      "))
                    {
                        ATC.ME.UploadToEEPROM();
                    }
                }


            }
        }
        GUILayout.Label(""); // blank space
    }

}
#endif ///////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////


    /*
     * ------------------ API Literature search terms ---------------------------
     * This plugin uses an API called "ATC3DG.dll"
     * Its for NDI Ascension Technology Corporation's 3D Guidance (Rev D) tracking systems (driveBAY & trackSTAR)
     * 
     * Literature reference:
     * 3DGuidance_trakSTAR_Installation_and_Operation_Guide.pdf available at www.ascension-tech.com
     * 
     * HelloATC uses:
     * InitializeBIRDSystem, SetSystemParameter METRIC and SELECT_TRANSMITTER, SetSensorParameter DATA_FORMAT
     * 
     * SetMeasurementRate uses:
     * SetSystemParameter and MEASUREMENT_RATE
     * 
     * SetHemisphere uses:
     * SetSensorParameter and HEMISPHERE
     * 
     * LookupErrorDescription uses:
     * GetErrorText SIMPLE_MESSAGE
     * 
     * ReadSensor(x)EEPROM uses:
     * GetSensorParameter VITAL_PRODUCT_DATA_RX, GetTransmitterParameter VITAL_PRODUCT_DATA_TX, and GetBoardParameter VITAL_PRODUCT_DATA_PCB
     * 
     * WriteToEEPROM uses:
     * SetSensorParameter VITAL_PRODUCT_DATA_RX, SetTransmitterParameter VITAL_PRODUCT_DATA_TX, and SetBoardParameter VITAL_PRODUCT_DATA_PCB
     * 
     * MakeRecordsATC uses:
     * GetAsynchronousRecord and DOUBLE_POSITION_QUATERNION_RECORD
     * 
     * GoodByeATC uses:
     * CloseBIRDSystem
     * 
     */

    /*
    Change log:

        December 17 2015: never set the ReadSensorEEPROMS to false.
        Barys witnessed misaglinment that looked like needle offsets not being applied.
        Dave replicated error by unplugging ascension, plugging it back in, and letting ATC reconnect.
        Solution: set ReadSensorEEPROMS always true.
        We encode alignment offsets for the sensors in the EEPROMs.
        The Ascension hardware motherboard applies the aligment offsets, not Unity (which was the old way, used GameObject heirarchy).
        When Ascension hardware starts up, this script tells the Ascension hardware what those offsets are.
        Ascension hardware compensates for hemisphere, and our Unity scenes' Gameobject heirarchy doesn't need an awkward and unwieldy solution for offsets.
        If the power to the tracking box was lost, we will have to re-load those offsets to the Ascension motherboard.
        If connection to ATC was lost due to unplugging the USB cable, we don't have to do this again, but:
        we have NOT implemented a way to tell why the connection was lost.  Therefore, 
        Always upload the offsets to the Ascension motherboard.  it only takes a few more seconds to reconnect.

    */
}