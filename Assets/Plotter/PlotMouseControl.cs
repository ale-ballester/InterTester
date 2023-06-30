using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotMouseControl : MonoBehaviour
{

    public Vector3 InitialMouseClick;
    public Camera PlotterCamera;

    [Range(0, 15000F)]
    public float ScrollMultiplier;

    public float scrollFraction;

    // Start is called before the first frame update
    void Start()
    {
    
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        InitialMouseClick = PlotterCamera.ScreenToViewportPoint(Input.mousePosition);
    }

    private void OnMouseDrag()
    {
        Vector3 currentMouse = PlotterCamera.ScreenToViewportPoint(Input.mousePosition);
        scrollFraction = ((currentMouse.x - InitialMouseClick.x) * ScrollMultiplier * Time.deltaTime);

        Plotter.ME.PlotScrolling(scrollFraction);

    }
}
