using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // note you need this to manipulate TMP objects - dont forget!!!

public class PlotWidthMenus : MonoBehaviour
{
    public Plotter.PlotWidth PlotWidth;


    Color initialLabelColor;
    public Color ActiveColor;

    private TextMeshPro label;

    // Start is called before the first frame update
    void Start()
    {
        label = GetComponent<TextMeshPro>();
        initialLabelColor = label.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlotWidth == Plotter.ME.Window)
            label.color = ActiveColor;
        else
            label.color = initialLabelColor;
    }

    private void OnMouseDown()
    {

        switch (PlotWidth)
        {
            // some of these are for dev only - not intended for publishing
            case Plotter.PlotWidth._30Sec: 
                Plotter.ME.Window = Plotter.PlotWidth._30Sec;
                break;
            case Plotter.PlotWidth._2Min: 
                Plotter.ME.Window = Plotter.PlotWidth._2Min;
                break;
            case Plotter.PlotWidth._10Min:
                Plotter.ME.Window = Plotter.PlotWidth._10Min;
                break;
            case Plotter.PlotWidth._15Min:
                Plotter.ME.Window = Plotter.PlotWidth._15Min;
                break;
            case Plotter.PlotWidth._45Min:
                Plotter.ME.Window = Plotter.PlotWidth._45Min;
                break;
            case Plotter.PlotWidth._90Min:
                Plotter.ME.Window = Plotter.PlotWidth._90Min;
                break;
            case Plotter.PlotWidth._3Hrs:
                Plotter.ME.Window = Plotter.PlotWidth._3Hrs;
                break;
        }
    }

}

