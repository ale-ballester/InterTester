using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PlotCacheContainer : MonoBehaviour
{

    void CreateCache()
    {
        PlotterLine plotterLine = this.GetComponent<PlotterLine>();
        // only create cache if its not a snapshot or if it is a snapshot and rendersnapshots is enabled
        if (plotterLine.Snapshot == 0 || FindObjectOfType<PlotPopup>().renderSnapShots && plotterLine.Snapshot > 0)
        {
            plotCache = new PlotCache(gameObject.name,
                    gameObject, plotterLine,
                    this.GetComponent<LineRenderer>());

            enabledPlots.Add(plotCache);
            
            enabledPlots.Sort((x, y) => {
            // stabalizes sort
            if(x.sortOrder!=y.sortOrder) return x.sortOrder.CompareTo(y.sortOrder);
            else return x.index.CompareTo(y.index);
            });
            //enabledPlots.Sort((x, y) => x.sortOrder.CompareTo(y.sortOrder));// unstable sort
            plotCache.markerTransform = transform.Find("Marker");
            plotCache.markerRenderer = plotCache.markerTransform.gameObject.GetComponent<Renderer>();

        }
    }
    void OnEnable()
    {
        // has to delay slightly to let objects properties load or else it will imediatley retrive prefab defaults and not the actual object information
        Invoke("CreateCache", 0);
    }
    void OnDisable()
    {
        enabledPlots.Remove(plotCache);
    }

    //PLOT CACH DEFINITION 
    public PlotCache plotCache;
    // static list opted to use this over dictionary for the ability to sort also its faster for anything under few hundred elements
    public static List<PlotCache> enabledPlots = new List<PlotCache>();
    
    public class PlotCache
    {
        public GameObject plotObject;
        public PlotterLine plotterLine;
        public LineRenderer lineRenderer;
        public Transform markerTransform;
        public Renderer markerRenderer;
        public int sortOrder;
        public string abbreviation;
        public int decimalAccuracy;
        public float multiplier;
        public string extraSymbol;
        public int index;


        public PlotCache(string plotName, GameObject plotObject, PlotterLine plotterLine, LineRenderer lineRenderer)
        {
            // default values are set in constructor
            // arbitary large number just to make sure all plots are sorted the same if no sort order is given
            // change those v
            SetDefaults(plotName);
            SetPlotSettings(plotName);
            this.plotObject = plotObject;
            this.plotterLine = plotterLine;
            this.lineRenderer = lineRenderer;
        }
        public void SetDefaults(string plotName)
        {
            SetAbbreviation(plotName);
            SetOrder(5000);
            SetDecimalAccuracy(2); // default should be 2, changed here for dev. dont leave it at 4.
            SetMultiplier(1);
            SetExtraSymbol("");
        }
        public void SetOrder(int orderInput)
        {
            sortOrder = orderInput;
        }
        public void SetAbbreviation(string abbreviationInput)
        {
            abbreviation = abbreviationInput;
        }
        //change decimal accuracy
        public void SetDecimalAccuracy(int decimalAccuracyInput)
        {
            decimalAccuracy = decimalAccuracyInput;
        }
        // pass in 100 to display as a percent rather than 
        public void SetMultiplier(float multiplierInput)
        {
            multiplier = multiplierInput;
        }
        // use to append a percent to the end of the string
        public void SetExtraSymbol(string symbolInput)
        {
            extraSymbol = symbolInput;
        }

        public void SetPlotSettings(string plotName)
        {
            switch (plotName) // names are set in PlotMenuClicks.
            {
                // if no explicit setting for a plot is defined it will go with default
                // ive done the first three you can comment this out if you want no ordering
                //case "Fraction Delivered O2 [%]":
                //    SetOrder(1); SetAbbreviation("F<sub>D</sub>O<sub>2</sub>"); break;
                //
                //case "FD O2 Snapshot 1": SetOrder(2); break;
                //case "FD O2 Snapshot 2": SetOrder(3); break;

                case "F<sub>D</sub>O<sub>2</sub>":            SetDecimalAccuracy(0); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(1); break;
                case "F<sub>D</sub>O<sub>2</sub> Snapshot A": SetDecimalAccuracy(0); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(2); break;
                case "F<sub>D</sub>O<sub>2</sub> Snapshot B": SetDecimalAccuracy(0); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(3); break;

                case "F<sub>I</sub>O<sub>2</sub>":            SetDecimalAccuracy(0); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(4); break;
                case "F<sub>I</sub>O<sub>2</sub> Snapshot A": SetDecimalAccuracy(0); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(5); break;
                case "F<sub>I</sub>O<sub>2</sub> Snapshot B": SetDecimalAccuracy(0); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(6); break;

                case "F<sub>E</sub>O<sub>2</sub>":            SetDecimalAccuracy(0); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(7); break;
                case "F<sub>E</sub>O<sub>2</sub> Snapshot A": SetDecimalAccuracy(0); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(8); break;
                case "F<sub>E</sub>O<sub>2</sub> Snapshot B": SetDecimalAccuracy(0); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(9); break;

                case "F<sub>D</sub>Agent":             SetDecimalAccuracy(2); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(10); break;
                case "F<sub>D</sub>Agent Snapshot A":  SetDecimalAccuracy(2); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(11); break;
                case "F<sub>D</sub>Agent Snapshot B":  SetDecimalAccuracy(2); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(12); break;

                case "F<sub>I</sub>Agent":             SetDecimalAccuracy(2); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(13); break;
                case "F<sub>I</sub>Agent Snapshot A":  SetDecimalAccuracy(2); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(14); break;
                case "F<sub>I</sub>Agent Snapshot B":  SetDecimalAccuracy(2); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(15); break;

                case "F<sub>E</sub>Agent":             SetDecimalAccuracy(2); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(16); break;
                case "F<sub>E</sub>Agent Snapshot A":  SetDecimalAccuracy(2); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(17); break;
                case "F<sub>E</sub>Agent Snapshot B":  SetDecimalAccuracy(2); SetMultiplier(100); SetExtraSymbol("%"); SetOrder(18); break;

                case "Liquid Agent Used":            SetDecimalAccuracy(1);  SetExtraSymbol(" mL"); SetOrder(19); break;
                case "Liquid Agent Used Snapshot A": SetDecimalAccuracy(1);  SetExtraSymbol(" mL"); SetOrder(20); break;
                case "Liquid Agent Used Snapshot B": SetDecimalAccuracy(1);  SetExtraSymbol(" mL"); SetOrder(21); break;

                case "Agent Waste":            SetDecimalAccuracy(1); SetExtraSymbol(" mL liquid/hr"); SetOrder(22); break;
                case "Agent Waste Snapshot A": SetDecimalAccuracy(1); SetExtraSymbol(" mL liquid/hr"); SetOrder(23); break;
                case "Agent Waste Snapshot B": SetDecimalAccuracy(1); SetExtraSymbol(" mL liquid/hr"); SetOrder(24); break;

                case "Waste Gas":            SetDecimalAccuracy(1); SetExtraSymbol(" L/min"); SetOrder(25); break;
                case "Waste Gas Snapshot A": SetDecimalAccuracy(1); SetExtraSymbol(" L/min"); SetOrder(26); break;
                case "Waste Gas Snapshot B": SetDecimalAccuracy(1); SetExtraSymbol(" L/min"); SetOrder(27); break;

                case "FGF":             SetDecimalAccuracy(1); SetExtraSymbol(" L/min"); SetOrder(28); break;
                case "FGF Snapshot A":  SetDecimalAccuracy(1); SetExtraSymbol(" L/min"); SetOrder(29); break;
                case "FGF Snapshot B":  SetDecimalAccuracy(1); SetExtraSymbol(" L/min"); SetOrder(30); break;

                case "FGF:MV Ratio":            SetDecimalAccuracy(2); SetOrder(31); break;
                case "FGF:MV Ratio Snapshot A": SetDecimalAccuracy(2); SetOrder(32); break;
                case "FGF:MV Ratio Snapshot B": SetDecimalAccuracy(2); SetOrder(33); break;

                case "Rebreathing Percent":            SetDecimalAccuracy(1); SetExtraSymbol("%"); SetOrder(34); break;
                case "Rebreathing Percent Snapshot A": SetDecimalAccuracy(1); SetExtraSymbol("%"); SetOrder(35); break;
                case "Rebreathing Percent Snapshot B": SetDecimalAccuracy(1); SetExtraSymbol("%"); SetOrder(36); break;
            }
        }


    }
    






}
