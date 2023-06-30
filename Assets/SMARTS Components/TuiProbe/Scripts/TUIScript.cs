using UnityEngine;
using SMARTS_SDK;
using System.Collections;

// SINGLETON
// 

public class TUIScript : MonoBehaviour, ITUIProbe
{
    // SINGLETON
    public static TUIScript ME;

    [Header("----------- Initializations --------------------------------------------------------------")]
    public string Name;
    public bool ShowCrosshairs;
    public GameObject DefaultParent;

    [Header("----------- External Follow Object  ------------------------------------------------------")]
    public bool EnableFollowParent;
    public GameObject FollowParent;
    public float StickyDistance;


    //[Header("----------- Tangible User Interface Stylus Settings --------------------------------------")]
    //public bool EnableSUIButtons;
    //public float MouseDownDepth;
    //public float MouseEnterDepth;
    //private int SUIButtonLayerMask;
    //private bool SUIButtonIsClicked = false;
    //private GameObject LastButtonClicked;
    //private bool SUIButtonIsOver = false;
    //private GameObject LastButtonOver;

    private GameObject Crosshairs;

    // this is a flag that the system uses to test if the camera is intended to follow another object.
    // for example, it can follow the ultrasound probe if the shutter button is clicked if the camera is near a probe.
    // TODO: make a user interface that makes this explicit.
    // TODO: make a property inspector interface that allows developers to add or remove tools that can be followed.
    private bool FollowParentLatch;

    //to disable camera switching in skill scripts
    public bool actingAsStylusPointer_notCamera { get; set; }

    // Use this for initialization
    void Start()
    {
        actingAsStylusPointer_notCamera = false;
        if (ME != null) GameObject.Destroy(ME);
        else ME = this;
        DontDestroyOnLoad(this);

        // SUIButtonLayerMask = 1 << LayerMask.NameToLayer("SUI Buttons");
        // MainCamera = GameObject.Find("Main Camera");

        Crosshairs = GameObject.Find("Crosshairs");
        //if (!ShowCrosshairs) Crosshairs.SetActive(false);

        ResetFree();

    }

    //public bool ActingAsStylusPointer_notCamera { get; set; } // Commented out by Andre. This boolean was not being used anywhere and was causing great
    // confusion because it's the only thing an outside script can change but it does nothing.
    // I added a get and a set for the other lowercase actingAsStylusPointer_notCamera boolean.
    public void ResetFree()
    {
        Name = "Free";
        Camera.main.transform.SetParent(DefaultParent.transform);
        Camera.main.GetComponent<Camera>().orthographic = false;
        FollowParentLatch = false;
    }

    // Update is called once per frame
    void Update()
    {


        /* // This code enabled the tangible user interface -  real function buttons aligned in space.  re-enable and rework as needed.
        if (EnableSUIButtons)
        {
            RaycastHit hit;

            // Stylus Mousedown - like a click.  you have to get close.
            if (Physics.Raycast(transform.position, transform.forward, out hit, MouseDownDepth, SUIButtonLayerMask))
            {
                actingAsStylusPointer_notCamera = true;  // block the camera trigger.  someone is messing with the icons
                if (SUIButtonIsClicked == false)
                {
                    SUIButtonIsClicked = true;
                    LastButtonClicked = hit.collider.gameObject;
                    LastButtonClicked.GetComponent<SUIButtonScripts>().OutsideMouseDown();
                }
            }
            else
            {
                if (SUIButtonIsClicked)
                {
                    SUIButtonIsClicked = false;
                }
            }

            // Stylus Mouseover- test ActiveDepth * some factor..
            if (Physics.Raycast(transform.position, transform.forward, out hit, MouseEnterDepth, SUIButtonLayerMask))
            {
                actingAsStylusPointer_notCamera = true;  // block the camera trigger.  someone is messing with the icons
                if (SUIButtonIsOver == false)
                {
                    SUIButtonIsOver = true;
                    LastButtonOver = hit.collider.gameObject;
                    LastButtonOver.GetComponent<SUIButtonScripts>().OutsideMouseEnter();
                }
            }
            else
            {
                if (SUIButtonIsOver)
                {
                    SUIButtonIsOver = false;
                    LastButtonOver.GetComponent<SUIButtonScripts>().OutsideMouseExit();
                }
            }

        }
        */
        //Debug.Log("actingAsStylusPointer_NotCamera "+actingAsStylusPointer_notCamera);
        //Debug.Log(actingAsStylusPointer_notCamera + " == false result: " + (actingAsStylusPointer_notCamera == false));
        if (actingAsStylusPointer_notCamera == false) // acting as a normal Camera, not a Stylus Pointer
        {

            if (Microcontroller.ME.Connected)
            {

                bool cameraShutter = Microcontroller.ME.TUI_Button;

                if (cameraShutter)
                {

                    Camera.main.orthographic = false;
                    Camera.main.transform.position = transform.position;
                    Camera.main.transform.rotation = transform.rotation;


                    if (EnableFollowParent & !FollowParentLatch)
                    {
                        float d = Vector3.Distance(transform.position, FollowParent.transform.position);
                        if (d < StickyDistance) FollowParentLatch = true;
                    }

                    if (!FollowParentLatch)
                    {
                        ResetFree();
                    }
                    else // standard camera 
                    {
                        Camera.main.transform.SetParent(FollowParent.transform);
                        Name = FollowParent.name;
                    }
                }
                else // no camera shutter.
                {
                    FollowParentLatch = false;
                }

                // Crosshairs
                if (ShowCrosshairs)
                {
                    if (cameraShutter)
                    {
                        if (!Crosshairs.activeSelf)
                            Crosshairs.SetActive(true);

                    }
                    else
                    {
                        if (Crosshairs.activeSelf)
                            Crosshairs.SetActive(false);
                    }
                }

            } // Microcontroller_sdk connected
        }



    }

}
