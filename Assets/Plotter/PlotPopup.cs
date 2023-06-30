using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;




public class PlotPopup : MonoBehaviour
{
    public Camera plotterCamera;
    public Transform plotLineRenderer;
    public GameObject textMesh;
    private TextMeshPro textMeshPro;
    private RectTransform textMeshTransform;
    public Vector3 mouseLocation;
    public Vector3 mouseLocalLocation;
    // due to how text is created it is difficult to get line height so use padding to avoid clipping at top and bottom
    public float topPadding = 4.02f;
    public bool renderSnapShots = false;
    public bool hideTextOnCursorLeave = true;
    public bool constrainTextToCanvas = true;
    public float lineRendererWidth;
    public float mouseRatio;
    public int nearestIndex;
    public string outputString = "";

    private UICameraControl uiCamera;
    //private Color clearColor = new Color(0, 0, 0, 0);
    private Vector2 textSize;
    private float maxXcoordinate;
    private float minXcoordinate;
    private float minYcoordinate;
    private float maxYcoordinate;
    // pivot is a ratio between 0 and 1 we need the invert of this for constrainig the text object to the graph

    //public GameObject HowToUseMessage;

    // Start is called before the first frame update
    void OnEnable()
    {
        textMeshPro = textMesh.GetComponent<TextMeshPro>();
        textMeshTransform = textMesh.GetComponent<RectTransform>();
        textMeshTransform.pivot = new Vector2(0, 0);
        uiCamera = plotterCamera.gameObject.GetComponent<UICameraControl>();
    }

    // when needed you can pass in a resolutionIndex which will equal the resolution-1
    void GetMouseLocation()
    {
        // get world coordinates of mouse
        mouseLocation = plotterCamera.ScreenToWorldPoint(Input.mousePosition);
        // get local coordinates of mouse 
        mouseLocalLocation = mouseLocation - plotLineRenderer.position;
    }
    void GetNearestIndex(float resolutionIndex)
    {
        // get width of graph
        //lineRendererWidth = 10799 * Plotter.ME.RenderPlotWidth;
        lineRendererWidth = resolutionIndex * Plotter.ME.RenderPlotWidth;
        // gets the percentage the mouse is between the left and right boundaries 0 for fully left 1 for fully right
        mouseRatio = Mathf.InverseLerp(0, lineRendererWidth, mouseLocalLocation.x);
        // uses that ratio to select the closest index using the lerp equation (offset the timerange by the starting number to get a range of 0 to width
        // then multiply width by the a percentage to get then add round for the index then add the offset back to get a number in the original range with the same relative location.) 
        nearestIndex = Mathf.RoundToInt(((Plotter.ME.PlotTimeEnd - 1) - Plotter.ME.PlotTimeStart) * mouseRatio) + Plotter.ME.PlotTimeStart;
    }



    void AddPlotString(
    Color colorInput, string abbreviationInput, float plotNumberInput,
    int decimalAccuracyInput, float multiplierInput, string symbolInput)
    {
        if (decimalAccuracyInput == 1 & symbolInput != "%")
        {
        outputString +=
            $@"<color=#{ColorUtility.ToHtmlStringRGBA(colorInput)}> {abbreviationInput}: {
            (System.Math.Round(plotNumberInput * multiplierInput, decimalAccuracyInput)).ToString("0.0")
            }{symbolInput}</color>" + "\n";
        }

        else if (decimalAccuracyInput == 2)
        {
            outputString +=
                $@"<color=#{ColorUtility.ToHtmlStringRGBA(colorInput)}> {abbreviationInput}: {
                (System.Math.Round(plotNumberInput * multiplierInput, decimalAccuracyInput)).ToString("0.00")
                }{symbolInput}</color>" + "\n";
        }

        else
        {
            outputString +=
                $@"<color=#{ColorUtility.ToHtmlStringRGBA(colorInput)}> {abbreviationInput}: {
                (System.Math.Round(plotNumberInput * multiplierInput, decimalAccuracyInput)).ToString()
                }{symbolInput}</color>" + "\n";
        }

    }

    void TextToMouse()
    {
        // if position of mouse is outside of bounds then dont move the text 
        //set text parent empty to mouse position
        // plotLineRenderer.position.z = y coordinate


        // get rendered values gives you the width and height in a vector
        transform.position = mouseLocation;
        if (constrainTextToCanvas)
        {
            textSize = textMeshPro.GetRenderedValues(false);

            maxXcoordinate = lineRendererWidth +
                plotLineRenderer.position.x -
                textMeshTransform.anchoredPosition.x -
                textSize.x;
            maxYcoordinate = Plotter.ME.RenderPlotHeight +
                plotLineRenderer.position.z -
                textMeshTransform.anchoredPosition.y - topPadding -
                textSize.y * 0.5f;

            minXcoordinate =
                plotLineRenderer.position.x - textMeshTransform.anchoredPosition.x;
            minYcoordinate =
                plotLineRenderer.position.z - textMeshTransform.anchoredPosition.y
                + textSize.y * 0.5f;


            //right
            if (mouseLocation.x > maxXcoordinate)
            {
                transform.position = new Vector3(maxXcoordinate, transform.position.y, transform.position.z);
            }
            //top
            if (mouseLocation.z > maxYcoordinate)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, maxYcoordinate);
            }
            //Left
            if (mouseLocation.x < minXcoordinate)
            {
                transform.position = new Vector3(minXcoordinate, transform.position.y, transform.position.z);
            }
            //bottom
            if (mouseLocation.z < minYcoordinate)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, minYcoordinate);
            }
        }

    }
    void SetText(string text)
    {
        // if moue pos relative to the linerenderer origin which is in the bottom left 
        // is greater than the plots total width or less than zero then dont render else render


        textMeshPro.SetText(text);
        textMeshPro.ForceMeshUpdate();

        if (
            mouseLocalLocation.x > lineRendererWidth ||
            mouseLocalLocation.x < 0 ||
            mouseLocalLocation.z > Plotter.ME.RenderPlotHeight ||
            mouseLocalLocation.z < 0
        )
        {
            if (hideTextOnCursorLeave)
            {
                textMeshPro.color = Color.clear;
                textMeshPro.ForceMeshUpdate();
            }
        }
        else
        {
            textMeshPro.color = Color.white;
            textMeshPro.ForceMeshUpdate();
        }



    }

    // Update is called once per frame
    void LateUpdate()
    {
        outputString = "";
        GetMouseLocation();

        // if there are no plots, turn the thingy on that says how to use. 
        //HowToUseMessage.SetActive(PlotCacheContainer.enabledPlots.Count == 0);

        // each plot in 
        foreach (var plot in PlotCacheContainer.enabledPlots)
        {
            

            GetNearestIndex(plot.plotterLine.PlotNumbers.Length);

            // fix objects showing up when plot is not in view
            if (uiCamera.LookAtPlotter)
            {
                // multi line interpolated string literal same as a long list of string concatenations
                // for a normal plot, not a snapshot. 
                if (plot.plotterLine.Snapshot == 0)
                {
                    // create a string for the number associated the plot and the mouse position
                    AddPlotString(plot.plotterLine.Colorcode, plot.abbreviation,
                        plot.plotterLine.PlotNumbers[nearestIndex], plot.decimalAccuracy, plot.multiplier, plot.extraSymbol);
                    //set marker color and move it to linerenderer position
                    plot.markerRenderer.material.color = plot.plotterLine.Colorcode;
                    plot.markerTransform.position = plot.lineRenderer.GetPosition(nearestIndex - Plotter.ME.PlotTimeStart);
                }

                /*
                // we create a string for the number associated the snapshot 1 and the mouse position - only if Snapshot 1 is turned on.
                if (plot.plotterLine.Snapshot == 1 & QNMTinterface.ME.SnapshotA)
                {
                    if (plot.lineRenderer.positionCount == 0) break; // this happens when the snapshots are first enabled but the values have not had a chance to load.
                    // create a string for the number associated the plot and the mouse position
                    AddPlotString(plot.plotterLine.Colorcode, plot.abbreviation,
                        plot.plotterLine.PlotNumbers[nearestIndex], plot.decimalAccuracy, plot.multiplier, plot.extraSymbol);
                    //set marker color and move it to linerenderer position
                    plot.markerRenderer.material.color = plot.plotterLine.Colorcode;
                    plot.markerTransform.position = plot.lineRenderer.GetPosition(nearestIndex - Plotter.ME.PlotTimeStart);
                }

                // we create a string for the number associated the snapshot 2 and the mouse position - only if Snapshot 2 is turned on.
                if (plot.plotterLine.Snapshot == 2 & QNMTinterface.ME.SnapshotB)
                {
                    if (plot.lineRenderer.positionCount == 0) break; // this happens when the snapshots are first enabled but the values have not had a chance to load.
                    // create a string for the number associated the plot and the mouse position
                    AddPlotString(plot.plotterLine.Colorcode, plot.abbreviation,
                        plot.plotterLine.PlotNumbers[nearestIndex], plot.decimalAccuracy, plot.multiplier, plot.extraSymbol);
                    //set marker color and move it to linerenderer position
                    plot.markerRenderer.material.color = plot.plotterLine.Colorcode;
                    plot.markerTransform.position = plot.lineRenderer.GetPosition(nearestIndex - Plotter.ME.PlotTimeStart);
                }
                */




            }

        }
        TextToMouse();
        SetText(outputString);
    }
}
