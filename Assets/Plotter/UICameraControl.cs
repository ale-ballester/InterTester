using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICameraControl : MonoBehaviour
{

    // core architectural classes are singletons. This is one of them. 
    // Reason: this is the one and only controller that determines if we see the plots. 
    // (By the way, its named "UICameraControl" but it evolved into just controlling the rendering of the plots. Anyway,)
    // The plots themselves (and their mathematical models) are designed to run independently from the rendering camera, which this script controls.
    // Since this is the one and only controller that determines if we see the plots, and other scripts like LFA State Control (and maybe others in different scenes) 
    // need to know how to render themselves around the plots; therefore this singleton is for them to reference, wherever they are.
    // --------- Singleton Reference to this script --------------------------------------------------------------------------
    public static UICameraControl ME;


    public Transform ShowPlotsPosition;
    public Transform HidePlotsPosition;
    public bool LookAtPlotter;
    public bool FinishedMoving; // self-explanatory. coupled with LookAtPlotter, other scripts should have everything they need to render themselves around the plots.

    public float MoveSpeed;

    //Added by TJ 6/1/22. Didn't know where to put it so just put it here.
    // This script is called Travis' idea to solve the layering issue was for the plot renderer. I ended up making the LFA states smarter instead.
    // public Canvas[] ToggleCanvasArray;


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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        
        if (LookAtPlotter)
        {
            //transform.position = ShowPlotsPosition.position;

            //float step =  // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, ShowPlotsPosition.position, (MoveSpeed * Time.deltaTime));
            FinishedMoving = (transform.position == ShowPlotsPosition.position);
        }
        else
        {
            //transform.position = HidePlotsPosition.position;
            transform.position = Vector3.MoveTowards(transform.position, HidePlotsPosition.position, (MoveSpeed * Time.deltaTime));
            FinishedMoving = (transform.position == HidePlotsPosition.position);
        }
        

    }


    // TJ's code for solving layering issue. hiding/showing elements. It does work, and his implementation is exactly what we talked about. 
    // However, it feels like spaghetti. I just feel like the individual LFA states should know what to do on their own.
    // But note how we can turn off canvas arrays from a public list.
    //public void HideCanvas()
    //{
    //    foreach (Canvas cElement in ToggleCanvasArray)
    //    {
    //        cElement.enabled = false;
    //    }
    //}
    //
    //public void ShowCanvas()
    //{
    //    foreach (Canvas cElement in ToggleCanvasArray)
    //    {
    //        cElement.enabled = true;
    //    }
    //}
    // end TJ's code for solving layering issue. 


    public void TogglePlotter()
    {
        LookAtPlotter = !LookAtPlotter;
    }

    public void HidePlotter()
    {
        LookAtPlotter = false;
    }

    public void ShowPlotter()
    {
        LookAtPlotter = true;
    }

}
