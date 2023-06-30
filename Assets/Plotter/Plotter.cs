using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// #if UNITY_EDITOR
// using UnityEditor;
// # endif

public class Plotter : MonoBehaviour
{
    // core architectural classes are singletons. This is one of them.
    // --------- Singleton Reference to this script --------------------------------------------------------------------------
    public static Plotter ME;

    public Transform PlotRootTransform;

    public enum PlotWidth { _15Min, _45Min, _90Min, _3Hrs, _30Sec, _2Min, _10Min}
    public PlotWidth Window;

    [Range(0, 10800)]
    public int PlotTimeStart, PlotTimeEnd;

    public GameObject PlotRendererPrefab;
    public Transform PlotRendererLocation;

    // TimePointer deleted
    public GameObject LeftBoarderLine, RightBoarderLine;

    [Range(0, 0.1F)]
    public float RenderPlotWidth;

    [Range(0, 200F)]
    public float RenderPlotHeight;

    PlotWidth lastPlotWidth;
    int lastTimeStart;

    // this flag is intended to be read externally, not written. consider making a read wrapper for it.
    public bool Redraw;     // dont let anyone else write to this.

    public float CurrentTime_LineWidth, Snapshot1_LineWidth, Snapshot2_LineWidth;
    public float CurrentTime_Alpha, Snapshot1_Alpha, Snapshot2_Alpha;

    private bool updatePlotsRequested = false;

    /*
    public GameObject dotPrototype;
    public GameObject squarePrototype;
    //public int dotSpacing;
    public float stepSize;
    public float dotDistance;
    public float squareDistance;
    */

    // Line renderer material for plots
    //public Material PlotMaterial;
    // Line renderer material for Snapshots
    //public Material SnapshotMaterial;

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
        //Window = PlotWidth._45Min;
    }


    // temporary overloaded function for plots that have no snapshot code.
    public bool TogglePlot(float[] plotdata, string plotdescription, float min, float max, Color colorcode)
    {
        // overload function. if no snapshot is specified, assign snapshot to zero, meaning its current time, not a snapshot
        return TogglePlot(plotdata, plotdescription, min, max, colorcode, 0);
    }

    public bool TogglePlot(float[] plotdata, string plotdescription, float min, float max, Color colorcode, int snapshot)
    {

        // the button that calls CreatePlot is actually a toggle. 
        // if an existing plot is found with that name, we pull it down. if not, then we create it. 
        // the return is so that the calling function can do some user interface things like change colors of a text menu to show a plot is active.
        foreach (Transform plot_child in PlotRendererLocation) // Don't use transform.find here! It wont fine plots with complex names!
        {
            if (plot_child.gameObject.name == plotdescription) // we brute force compare the string name against the gameobject name.
            {
                // we found an existing plot with the same name.
                // since the button that called this function is a toggle,
                // and the plot already exists, mark the plot for death with evil laugh!
                Destroy(plot_child.gameObject);
                // the Plotterline script also has a DeletePlot function that does the same thing but we are not using it. 
                // the calling function knows what to do with this bool.
                return false;
            }
        }

        // if you made it down here, no plot was found by that name and we can make a new one.
        GameObject newPlot = Instantiate(PlotRendererPrefab, PlotRootTransform.position, PlotRootTransform.rotation, this.transform);
        newPlot.GetComponent<PlotterLine>().Load(plotdata, plotdescription, min, max, colorcode, snapshot);
        newPlot.transform.parent = PlotRootTransform;
        newPlot.transform.name = plotdescription;

        // Agent plots get assigned the AgentScale boolean, so they know to track the max agent value in CFM Interface. 
        // I'm using "</sub>Agent" because that applies to FDA, FIA, FEA and their snapshots, but not Cumulative Agent Delivered or Agent Waste.
        if (plotdescription.Contains("</sub>Agent")) newPlot.GetComponent<PlotterLine>().AgentScale = true;

        // the calling function knows what to do with this bool.
        return true;

    }


    public void PlotScrolling(float scrollFraction)
    {
        if (Window == PlotWidth._30Sec) PlotTimeStart = PlotTimeStart + (int)(scrollFraction * 0.5f);
        else if (Window == PlotWidth._2Min) PlotTimeStart = PlotTimeStart + (int)scrollFraction;
        else if (Window == PlotWidth._10Min) PlotTimeStart = PlotTimeStart + (int)scrollFraction;
        else if (Window == PlotWidth._15Min) PlotTimeStart = PlotTimeStart + (int)scrollFraction;
        else if (Window == PlotWidth._45Min) PlotTimeStart = PlotTimeStart + (int)(scrollFraction * 3);
        else if (Window == PlotWidth._90Min) PlotTimeStart = PlotTimeStart + (int)(scrollFraction * 3);
        else if (Window == PlotWidth._3Hrs) PlotTimeStart = 0;


    }

    // I dont think anyone calls this but here it remains. The Plotterline contains this function.
    public void RemoveAllPlots()
    {
        BroadcastMessage("DeletePlot", SendMessageOptions.DontRequireReceiver);
    }

    // Update is called once per frame
    void Update()
    {

        // The code below doesn't have to be updated every frame. It needs to be updated when the user drags the plot window.
        // however its easier to just run in Update(). Its little code and won't slow things down too much.

        int plotWidthInSeconds = 10800;

        switch (Window)
        {
            case PlotWidth._15Min:  plotWidthInSeconds = 900;   break;
            case PlotWidth._45Min:  plotWidthInSeconds = 2700;  break;
            case PlotWidth._90Min:  plotWidthInSeconds = 5400;  break;
            case PlotWidth._30Sec: plotWidthInSeconds = 30; break;
            case PlotWidth._2Min: plotWidthInSeconds = 120; break;
            case PlotWidth._10Min: plotWidthInSeconds = 600; break;
        }

        if (PlotTimeStart > (10800 - plotWidthInSeconds)) PlotTimeStart = 10800 - plotWidthInSeconds;
        PlotTimeEnd = PlotTimeStart + plotWidthInSeconds;


        if (PlotTimeStart < 0) PlotTimeStart = 0;

        /*
        // render current time pointer - the little blue carrot at the bottom of the SAA plotter
        // first find where the caret should be placed on the screen as a fraction of the plot width currently displayed
        float lerpFraction = Mathf.InverseLerp(PlotTimeStart, PlotTimeEnd, Clock.ME.SimTime);

        // render the time pointer on the plot based on the plot boarders. 
        TimePointer.transform.position = Vector3.Lerp(LeftBoarderLine.transform.position, RightBoarderLine.transform.position, lerpFraction);

        // hide the caret if its off the range of the plotter, because its confusing.
        TimePointer.SetActive(lerpFraction != 1 & lerpFraction != 0);
        */

    }

    // should we spend the time and effort redrawing the line renderer plots? Lets only do that if we need to, when something changes:
    private void LateUpdate()
    {
        Redraw = false;
        if (updatePlotsRequested) Redraw = true;
        if (lastTimeStart != Plotter.ME.PlotTimeStart) Redraw = true;
        if (lastPlotWidth != Plotter.ME.Window) Redraw = true;

        lastTimeStart = Plotter.ME.PlotTimeStart;
        lastPlotWidth = Plotter.ME.Window;
        updatePlotsRequested = false;
    }


    public void UpdatePlots()
    {
        updatePlotsRequested = true;
    }

}


