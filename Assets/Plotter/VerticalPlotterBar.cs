using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalPlotterBar : MonoBehaviour
{


    public int TimeIndex;



    // Update is called once per frame
    void Update()
    {
        if (Plotter.ME.Redraw) // comment out for dev in realtime
        {
            Redraw();
        }
    }


    void Redraw()
    {

        // render current time pointer - the little blue carrot at the bottom of the SAA plotter
        // first find where the caret should be placed on the screen as a fraction of the plot width currently displayed
        //float lerpFraction = Mathf.InverseLerp(Plotter.ME.PlotTimeStart, Plotter.ME.PlotTimeEnd, Timestamp);
        float x = LinearScale(Plotter.ME.PlotTimeStart, Plotter.ME.PlotTimeEnd, Plotter.ME.LeftBoarderLine.transform.position.x, Plotter.ME.RightBoarderLine.transform.position.x, TimeIndex);

        // render the time pointer on the plot based on the plot boarders. 
        Vector3 a = transform.position;
        a.x = x;

        a.y = -3; //
        if ( TimeIndex < Plotter.ME.PlotTimeStart | TimeIndex > Plotter.ME.PlotTimeEnd)
        {
            a.y = 10; // this moves the event above the UI camera, but still visible in the Unity Editor where its important for us to see it.
        }

        transform.position = a;

        // hide the caret if its off the range of the plotter, because its confusing.
        // TimePointer.SetActive(lerpFraction != 1 & lerpFraction != 0);

    }

    // LinearScale was a utility in the original SAA, which did not have Mathf.
    float LinearScale(float A, float B, float C, float D, float x)
    {
        // return Mathf.Lerp(C, D, Mathf.InverseLerp(A, B, x)); // if A=B, returns C.
        // Mathf.InverseLerp is also clamped, and we do not what it clamped. 
        float y = 0;
        if (A != B)
        {
            float m = (D - C) / (float)(B - A);
            float b = -((m * A) - C);
            y = m * x + b;
            return y; // if A=B, returns NaN. The original SAA threw an alert message and returned zero. 
        }
        return y;
    }

    void Delete()
    {
        Destroy(this.gameObject);
    }


}

