using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


public class PlotterLine : MonoBehaviour
{

    public float[] PlotNumbers;// = new float [10800];
    public string Description;
    public Color Colorcode;

    //[Range(0, 0.1F)]
    //public float RenderPlotWidth;

    //[Range(0, 200F)]
    //public float RenderPlotHeight;

   // [Range(0, 1)]
   // public float PlotWidth;

   // [Range(0, 1)]
   // public float PlotHeight;

    LineRenderer Line;

    Vector3 OriginIn3DSpace;

    public float min, max;

    public TextMesh MaxText;
    public TextMesh DescriptionText;

    // Is this plot a snapshot? that makes a difference in how its rendered. 
    public int Snapshot; // 0 is not a snapshot; its the current values. 1 is snapshot #1, 2 is snapshot #2.

    // Set flags to follow special dynamic scales here.
    public bool AgentScale; // 

    public Transform DotBin;

    // Start is called before the first frame update
    void Awake()
    {

    }

    public void Load(float[] plotData, string description, float a_min, float a_max, Color colorcode, int a_snapshot)
    {
        Line = GetComponent<LineRenderer>();
        OriginIn3DSpace = transform.position;

        PlotNumbers = plotData;
        Description = description;
        
        Colorcode = colorcode;

        Snapshot = a_snapshot;


        // pull data from physiokinetics models
        // find min and max values for the plot range, but only if min and max are not specified.
        min = a_min;
        max = a_max;
        if (min >= max) // could have just said max = 0 and that would probably always work. 
        {
            for (int i = 0; i < PlotNumbers.Length; i++)
            {
                float x = PlotNumbers[i];
                if (x > max) max = x;
                if (x < min) min = x;
            }
            
        }
        //Debug.Log("min = " + min + "   max = " + max);

        MaxText.text = Description;
        MaxText.color = colorcode;

        Redraw();

    }

    // Update is called once per frame
    void Update()
    {
        if (Plotter.ME.Redraw)
        {
            Redraw();
        }
    }

    void Redraw()
    {   
        /*
        // define max scales. if not defined here, the default values will be used.
        if (AgentScale) max = QNMTinterface.ME.AgentMaxScale;

        // if I am a snapshot 1 plot, and all the snapshot 1s have been turned off,  
        if (Snapshot == 1 & QNMTinterface.ME.SnapshotA == false) 
        {
            // Delete all the dots
            foreach (Transform child in DotBin) {
                GameObject.Destroy(child.gameObject);
            }
            // just make me zero. So my line renderer will be empty.
            Line.positionCount = 0;
        }
        // if I am a snapshot 2 plot, and all the snapshot 2s have been turned off, 
        else if (Snapshot == 2 & QNMTinterface.ME.SnapshotB == false)
        {
            // Delete all the dots
            foreach (Transform child in DotBin) {
                GameObject.Destroy(child.gameObject);
            }
            // just make me zero. So my line renderer will be empty.
            Line.positionCount = 0;
        }
        // else, I'm an active plot and my line renderers need an update.
        else */
        if (true)
        {
            //Debug.Log("I'm a PlotterLine with this many PlotNumbers: " + PlotNumbers.Length + " and my RenderPlotWidth is " + Plotter.ME.RenderPlotWidth);

            // build the vector3[] for the line renderer using data from the Compartment Flow Model interface.
            if (Plotter.ME.Window == Plotter.PlotWidth._2Min)
            {

                Vector3[] lineVector = new Vector3[120];
                for (int i = 0; i < 120; i++)
                {
                    float x = i * Plotter.ME.RenderPlotWidth * 90;

                    float lerpfraction = Mathf.InverseLerp(min, max, PlotNumbers[i + Plotter.ME.PlotTimeStart]);
                    float y = Mathf.Lerp(0, Plotter.ME.RenderPlotHeight, lerpfraction);

                    float z = 0;
                    lineVector[i] = new Vector3(x, z, y) + OriginIn3DSpace;
                }

                Line.positionCount = 120;
                Line.SetPositions(lineVector);
            }

            if (Plotter.ME.Window == Plotter.PlotWidth._10Min)
            {

                Vector3[] lineVector = new Vector3[600];
                for (int i = 0; i < 600; i++)
                {
                    float x = i * Plotter.ME.RenderPlotWidth * 18;

                    float lerpfraction = Mathf.InverseLerp(min, max, PlotNumbers[i + Plotter.ME.PlotTimeStart]);
                    float y = Mathf.Lerp(0, Plotter.ME.RenderPlotHeight, lerpfraction);

                    float z = 0;
                    lineVector[i] = new Vector3(x, z, y) + OriginIn3DSpace;
                }

                Line.positionCount = 600;
                Line.SetPositions(lineVector);
            }

            if (Plotter.ME.Window == Plotter.PlotWidth._15Min)
            {

                Vector3[] lineVector = new Vector3[900];
                for (int i = 0; i < 900; i++)
                {
                    float x = i * Plotter.ME.RenderPlotWidth * 12;

                    float lerpfraction = Mathf.InverseLerp(min, max, PlotNumbers[i + Plotter.ME.PlotTimeStart]);
                    float y = Mathf.Lerp(0, Plotter.ME.RenderPlotHeight, lerpfraction);

                    float z = 0;
                    lineVector[i] = new Vector3(x, z, y) + OriginIn3DSpace;
                }

                Line.positionCount = 900;
                Line.SetPositions(lineVector);
            }

            if (Plotter.ME.Window == Plotter.PlotWidth._45Min)
            {

                Vector3[] lineVector = new Vector3[2700];
                for (int i = 0; i < 2700; i++)
                {
                    float x = i * Plotter.ME.RenderPlotWidth * 4;

                    float lerpfraction = Mathf.InverseLerp(min, max, PlotNumbers[i + Plotter.ME.PlotTimeStart]);
                    float y = Mathf.Lerp(0, Plotter.ME.RenderPlotHeight, lerpfraction);

                    float z = 0;
                    lineVector[i] = new Vector3(x, z, y) + OriginIn3DSpace;
                }

                Line.positionCount = 2700;
                Line.SetPositions(lineVector);
            }

            if (Plotter.ME.Window == Plotter.PlotWidth._90Min)
            {

                Vector3[] lineVector = new Vector3[5400];
                for (int i = 0; i < 5400; i++)
                {
                    float x = i * Plotter.ME.RenderPlotWidth * 2;

                    float lerpfraction = Mathf.InverseLerp(min, max, PlotNumbers[i + Plotter.ME.PlotTimeStart]);
                    float y = Mathf.Lerp(0, Plotter.ME.RenderPlotHeight, lerpfraction);

                    float z = 0;
                    lineVector[i] = new Vector3(x, z, y) + OriginIn3DSpace;
                }

                Line.positionCount = 5400;
                Line.SetPositions(lineVector);
            }

            if (Plotter.ME.Window == Plotter.PlotWidth._3Hrs)
            {

                Vector3[] lineVector = new Vector3[PlotNumbers.Length];
                for (int i = 0; i < PlotNumbers.Length; i++)
                {
                    float x = i * Plotter.ME.RenderPlotWidth;

                    float lerpfraction = Mathf.InverseLerp(min, max, PlotNumbers[i]);
                    float y = Mathf.Lerp(0, Plotter.ME.RenderPlotHeight, lerpfraction);

                    float z = 0;
                    lineVector[i] = new Vector3(x, z, y) + OriginIn3DSpace;

                }

                Line.positionCount = PlotNumbers.Length;
                Line.SetPositions(lineVector);
            }
        }

    }

    void DeletePlot()
    {
        Destroy(this.gameObject);
    }
}
