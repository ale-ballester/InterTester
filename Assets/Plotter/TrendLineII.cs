using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrendLineII : MonoBehaviour
{

    public float[] PlotNumbers;// = new float [10800];

    public Color Colorcode;


    LineRenderer Line;

    Vector3 OriginIn3DSpace;

    float min, max;
    public float RenderPlotWidth, RenderPlotHeight;
    int PlotTimeStart;

    public bool FiFeO2, FiFeIso, FDO2, FDIso;


    int lastSimTime;

    /*
    // Start is called before the first frame update
    void Awake()
    {
        Invoke("delayload", 1);

        OriginIn3DSpace = transform.position;

    }

    void delayload()
    {
        if (FiFeO2)
            Load(QNMTinterface.ME.YO2, "FiFeO2", 0, 1, Colorcode);
        if (FDO2)
            Load(QNMTinterface.ME.FDO2, "FDO2", 0, 1, Colorcode);
        if (FiFeIso)
            Load(QNMTinterface.ME.YAgent, "FiFeIso", 0, 1, Colorcode);
        if (FDIso)
            Load(QNMTinterface.ME.FDA, "FDAgent", 0, 1, Colorcode);

    }

    public void Load(float[] plotData, string description, float a_min, float a_max, Color colorcode)
    {
        Line = GetComponent<LineRenderer>();

        PlotNumbers = plotData;
        Colorcode = colorcode;

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

    }


    // Update is called once per frame
    void Update()
    {
        // obviously dont do this every frame but for now...
        if (Clock.ME.SimTime != lastSimTime & PlotNumbers.Length != 0) // i.e. if something changed and we've been initialized,
        {
            Redraw();
            lastSimTime = Clock.ME.SimTime; // this allows us to update in real-time during time scrolling.
        }
    }

    void Redraw()
    {

        // build the vector3[] for the line renderer using data already loaded from the CFM model
        // 15 minutes is 900 seconds.

        PlotTimeStart = Clock.ME.SimTime - 900;
        if (PlotTimeStart < 0) PlotTimeStart = 0;

        Vector3[] lineVector = new Vector3[900];

        for (int i = 0; i < 900; i++)
        {
            if (i > Clock.ME.SimTime)
            {
                lineVector[i] = lineVector[i - 1];
            }
            else
            {
            float x = i * RenderPlotWidth * 90;

            float lerpfraction = Mathf.InverseLerp(min, max, PlotNumbers[i + PlotTimeStart]);
            float y = Mathf.Lerp(0, RenderPlotHeight, lerpfraction);

            float z = 0;
                lineVector[i] = new Vector3(x, y, z) + OriginIn3DSpace;
            }
        }

        Line.positionCount = 900;
        Line.SetPositions(lineVector);

        Line.startColor = Colorcode;
        Line.endColor = Colorcode;

    }

    void DeletePlot()
    {
        Destroy(this.gameObject);
    }
    */

}
