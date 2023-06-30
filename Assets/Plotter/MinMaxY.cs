using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MinMaxY : MonoBehaviour
{
    public GameObject max;
    public GameObject min;
    private TextMeshPro maxText;
    private TextMeshPro minText;
    private int lastCacheSize;
    private string maxTempString;
    private string minTempString;
    public string maxOutputString;
    public string minOutputString;

    
    // Start is called before the first frame update
    void setMaxLocation(){
        max.transform.position = new Vector3(transform.position.x,transform.position.y,transform.position.z+Plotter.ME.RenderPlotHeight);
    }
    void OnEnable()
    {
       // you have to wait a bit or else you wont be able to retrieve the data
       Invoke("setMaxLocation", 0);
       
        
       maxText =max.GetComponentInChildren(typeof(TextMeshPro)) as TextMeshPro;
       minText =min.GetComponentInChildren(typeof(TextMeshPro)) as TextMeshPro;
    }

    // Update is called once per frame
    string AddPlotString(
    Color colorInput,float plotNumberInput,
    int decimalAccuracyInput, float multiplierInput, string symbolInput)
    {
        return $@"<color=#{ColorUtility.ToHtmlStringRGBA(colorInput)}>{
            (System.Math.Round(plotNumberInput * multiplierInput, decimalAccuracyInput)).ToString()
            }{symbolInput}</color>" + "\n";
    }
 
    void Update()
    {
        
    
        maxOutputString = "";
        minOutputString = "";
        foreach (var plot in PlotCacheContainer.enabledPlots){
            if (plot.plotterLine.Snapshot == 0){
                maxTempString=
                AddPlotString(plot.plotterLine.Colorcode,
                plot.plotterLine.max, plot.decimalAccuracy, plot.multiplier, plot.extraSymbol);
                
                minTempString=
                AddPlotString(plot.plotterLine.Colorcode,
                plot.plotterLine.min, plot.decimalAccuracy, plot.multiplier, plot.extraSymbol);
                
                if(!(maxOutputString.Contains(maxTempString))){
                maxOutputString+=maxTempString;
                }
                if(!(minOutputString.Contains(minTempString))){
                    minOutputString+=minTempString;
                }
                
            }
            
            /*
             if (plot.plotterLine.Snapshot == 1 & QNMTinterface.ME.SnapshotA){
                maxTempString=
                AddPlotString(plot.plotterLine.Colorcode,
                plot.plotterLine.max, plot.decimalAccuracy, plot.multiplier, plot.extraSymbol);
                
                minTempString=
                AddPlotString(plot.plotterLine.Colorcode,
                plot.plotterLine.min, plot.decimalAccuracy, plot.multiplier, plot.extraSymbol);
                
                if(!(maxOutputString.Contains(maxTempString))){
                maxOutputString+=maxTempString;
                }
                if(!(minOutputString.Contains(minTempString))){
                    minOutputString+=minTempString;
                }
             }
            
             if (plot.plotterLine.Snapshot == 2 & QNMTinterface.ME.SnapshotB){
                maxTempString=
                AddPlotString(plot.plotterLine.Colorcode,
                plot.plotterLine.max, plot.decimalAccuracy, plot.multiplier, plot.extraSymbol);
                
                minTempString=
                AddPlotString(plot.plotterLine.Colorcode,
                plot.plotterLine.min, plot.decimalAccuracy, plot.multiplier, plot.extraSymbol);
                
                if(!(maxOutputString.Contains(maxTempString))){
                maxOutputString+=maxTempString;
                }
                if(!(minOutputString.Contains(minTempString))){
                    minOutputString+=minTempString;
                }
             }
             */
            
            
            
            
        
        }
        maxText.SetText(maxOutputString);
        maxText.ForceMeshUpdate();
        minText.SetText(minOutputString);
        minText.ForceMeshUpdate();
           
        
    }
}

