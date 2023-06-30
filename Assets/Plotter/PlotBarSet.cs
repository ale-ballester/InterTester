using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlotBarSet : MonoBehaviour
{

    public GameObject PlotBarSet_1_0; // Thick labeled line every minute, no thin minor lines.
    public GameObject PlotBarSet_5_1; // Thick labeled line every five minutes, thin minor line every minute.
    public GameObject PlotBarSet_15_5;// Thick labeled line every 15 minutes, thin minor line every 5 minutes.

    public GameObject VerticalPlotBarWithLabel; // Thick labeled line. It has a text object below the line for the time. 
    public GameObject VerticalPlotBar;          // Thin labeled line. It has no text. 

    // Start is called before the first frame update
    void Start()
    {

        Build_PlotBarSet_1_0(); // Thick labeled line every minute, no thin minor lines.
        Build_PlotBarSet_5_1(); // Thick labeled line every five minutes, thin minor line every minute.
        Build_PlotBarSet_15_5();// Thick labeled line every 15 minutes, thin minor line every 5 minutes.

    }

    void Update()
    {
        // this plot bar set is good for the 3 hour plot window.
        PlotBarSet_15_5.SetActive(Plotter.ME.Window == Plotter.PlotWidth._3Hrs 
            | Plotter.ME.Window == Plotter.PlotWidth._90Min);

        // this plot bar set is good for the 15 or 45 minute plot windows.
        PlotBarSet_5_1.SetActive(Plotter.ME.Window == Plotter.PlotWidth._45Min
            | Plotter.ME.Window == Plotter.PlotWidth._15Min); 


        // if neither of the two above are active, then this is the one to use.
        PlotBarSet_1_0.SetActive(!PlotBarSet_5_1.activeInHierarchy & !PlotBarSet_15_5.activeInHierarchy);

    }

    // Thick labeled line every minute, no thin minor lines.
    void Build_PlotBarSet_1_0()
    {
        for (int i = 0; i <= 10800; i += 60) // one bar for each minute
        {
            GameObject newBar = Instantiate(VerticalPlotBarWithLabel, PlotBarSet_1_0.transform);
            newBar.GetComponent<VerticalPlotterBar>().TimeIndex = i;

            TextMesh label = newBar.GetComponentInChildren<TextMesh>();
            if (i < 3600)
            {
                label.text = DateTime.FromBinary(599266080000000000).AddSeconds(i).ToString("mm:ss");
            }
            else
            {
                label.text = DateTime.FromBinary(599266080000000000).AddSeconds(i).ToString("h:mm:ss");
            }

        }
    }


    // Thick labeled line every five minutes, thin minor line every minute.
    void Build_PlotBarSet_5_1()
    {
        for (int i = 0; i <= 10800; i += 60)
        {

            // the five minute marks get labels and thicker lines.
            if (i % 300 == 0) // this is every five minutes (300 seconds)
            {
                GameObject newBar = Instantiate(VerticalPlotBarWithLabel, PlotBarSet_5_1.transform);
                newBar.GetComponent<VerticalPlotterBar>().TimeIndex = i;

                TextMesh label = newBar.GetComponentInChildren<TextMesh>();
                if (i < 3600)
                {
                    label.text = DateTime.FromBinary(599266080000000000).AddSeconds(i).ToString("mm:ss");
                }
                else
                {
                    label.text = DateTime.FromBinary(599266080000000000).AddSeconds(i).ToString("h:mm:ss");
                }

            }
            // the other minute marks get thin lines with no labels.
            else
            {
                GameObject newBar = Instantiate(VerticalPlotBar, PlotBarSet_5_1.transform);
                newBar.GetComponent<VerticalPlotterBar>().TimeIndex = i;
            }

        }
    }


    // Thick labeled line every 15 minutes, thin minor line every 5 minutes.
    void Build_PlotBarSet_15_5()
    {
        for (int i = 0; i <= 10800; i += 300) // there is a line every five minutes (300 seconds)
        {

            // the 15 minute marks get labels and thicker lines.
            if (i % 900 == 0) // this is every 15 minutes (900 seconds)
            {
                GameObject newBar = Instantiate(VerticalPlotBarWithLabel, PlotBarSet_15_5.transform);
                newBar.GetComponent<VerticalPlotterBar>().TimeIndex = i;

                TextMesh label = newBar.GetComponentInChildren<TextMesh>();
                if (i < 3600)
                    label.text = DateTime.FromBinary(599266080000000000).AddSeconds(i).ToString("0:mm");
                else
                    label.text = DateTime.FromBinary(599266080000000000).AddSeconds(i).ToString("h:mm");

                if (i == 3600) label.text = "1 hr";
                if (i == 7200) label.text = "2 hr";
                if (i == 10800) label.text = "3 hr";

            }
            // the other five minute marks get thin lines with no labels.
            else
            {
                GameObject newBar = Instantiate(VerticalPlotBar, PlotBarSet_15_5.transform);
                newBar.GetComponent<VerticalPlotterBar>().TimeIndex = i;
            }

        }
    }

}
