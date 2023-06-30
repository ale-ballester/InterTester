using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // note you need this to manipulate TMP objects - dont forget!!!

// Why do we do plot function calls this way? Why not a canvas, with canvas objects like buttons?
// Because we need to render plots using line renderers, and they are not compatible with canvas objects.
// Because I want everything to do with the plot window to render together, as a cohesive unit, not half on a UI camera and the other half on a UI canvas.
// and because Text Mesh Pro utilities do not play nice with WebGL.

public class PlotMenuClicks : MonoBehaviour
{

    public string PlotName;

    public Color PlotColor;

    public float Min, Max;

    public Color initialLabelColor;
    public TextMeshPro label;
    public TextMesh oldLabel;

    public GameObject InterTester;
    InterTester IT;

    void Start()
    {

        label = GetComponent<TextMeshPro>();
        if (label != null)
        {
            initialLabelColor = label.color;
            if (PlotName == "")
                PlotName = label.text;
        }
        else
        {
            oldLabel = GetComponent<TextMesh>();
            if (oldLabel != null)
            {
                initialLabelColor = oldLabel.color;
                if (PlotName == "")
                    PlotName = oldLabel.text;
            }
        }

        IT = InterTester.GetComponent<InterTester>();
    }

    // Designed to be used by LFA State control to activate selected plots.
    public void Activate()
    {
        if (label.color == initialLabelColor) // we use the label color as a flag to see if the plot is off.
        {
            OnMouseDown();
        }
        else // The label color is not the off color, so we assume the plot is already on.
        {
            // do nothing, plot is already active.
        }
    }

    // Designed to be used by LFA State control to deactivate plots that are not needed.
    public void DeActivate()
    {
        if (label.color == initialLabelColor)
        {
            // do nothing, the plot is already off.
        } else
        {
            OnMouseDown();
        }
    }


    public void OnMouseDown()
    {
        // Debug.Log("PlotMenuClicks OnMouseDown sees PlotName of " + PlotName);

        switch (PlotName)
        {

            /////// Plot time width options now have their own script, not controlled here anymore.

            /////// These are plot variables displayed in the LFA.

            // TogglePlot used to be called CreatePlot().
            // If an existing plot is found, TogglePlot() destroys the plot. If not, it creates the plot.

            case "S1 PE":
                if (Plotter.ME.TogglePlot(IT.error1array, "S1 PE", Min, Max, PlotColor, 0)) label.color = PlotColor;
                else label.color = initialLabelColor;
                break;

            case "F<sub>I</sub>O<sub>2</sub>":
                /*
                if (Plotter.ME.TogglePlot(QNMTinterface.ME.FIO2, "F<sub>I</sub>O<sub>2</sub>", Min, Max, PlotColor, 0)) label.color = PlotColor;
                else label.color = initialLabelColor;
                */
                break;


            case "F<sub>E</sub>O<sub>2</sub>":
                /*
                if (Plotter.ME.TogglePlot(QNMTinterface.ME.FEO2, "F<sub>E</sub>O<sub>2</sub>", Min, Max, PlotColor, 0)) label.color = PlotColor;
                else label.color = initialLabelColor;
                */
                break;

                // we do some special things with Agent: we detect max values for autoscale to 2, 4, or 6 percent. its based on FDA, the largest agent variable. 

            case "F<sub>D</sub>Agent":
                /*
                Max = QNMTinterface.ME.AgentMaxScale;
                if (Plotter.ME.TogglePlot(QNMTinterface.ME.FDA, "F<sub>D</sub>Agent", Min, Max, PlotColor, 0)) label.color = PlotColor;
                else label.color = initialLabelColor;
                Plotter.ME.TogglePlot(QNMTinterface.ME.FDA_snapshotA, "F<sub>D</sub>Agent Snapshot A", Min, Max, PlotColor, 1);
                Plotter.ME.TogglePlot(QNMTinterface.ME.FDA_snapshotB, "F<sub>D</sub>Agent Snapshot B", Min, Max, PlotColor, 2);
                */
                break;

            case "F<sub>I</sub>Agent":
                /*
                Max = QNMTinterface.ME.AgentMaxScale;
                if (Plotter.ME.TogglePlot(QNMTinterface.ME.FIA, "F<sub>I</sub>Agent", Min, Max, PlotColor, 0)) label.color = PlotColor;
                else label.color = initialLabelColor;
                Plotter.ME.TogglePlot(QNMTinterface.ME.FIA_snapshotA, "F<sub>I</sub>Agent Snapshot A", Min, Max, PlotColor, 1);
                Plotter.ME.TogglePlot(QNMTinterface.ME.FIA_snapshotB, "F<sub>I</sub>Agent Snapshot B", Min, Max, PlotColor, 2);
                */
                break;

            case "F<sub>E</sub>Agent":
                /*
                Max = QNMTinterface.ME.AgentMaxScale;
                if (Plotter.ME.TogglePlot(QNMTinterface.ME.FEA, "F<sub>E</sub>Agent", Min, Max, PlotColor, 0)) label.color = PlotColor;
                else label.color = initialLabelColor;
                Plotter.ME.TogglePlot(QNMTinterface.ME.FEA_snapshotA, "F<sub>E</sub>Agent Snapshot A", Min, Max, PlotColor, 1);
                Plotter.ME.TogglePlot(QNMTinterface.ME.FEA_snapshotB, "F<sub>E</sub>Agent Snapshot B", Min, Max, PlotColor, 2);
                */
                break;

            default:
                string s = "Unrecognized click in PlotMenuClicks.cs attached to gameobject " + gameObject.name + " under " + gameObject.transform.parent.gameObject.name;
                Debug.Log(s);
                break;
        }

    }

}
