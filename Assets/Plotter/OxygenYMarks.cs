using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenYMarks : MonoBehaviour 
{
    public Transform lineRenderers;
    public GameObject O2_21;
    public GameObject O2_30;
    private int lastChildCount = 0;



    void Start() {
        lastChildCount = lineRenderers.childCount;
    }

    void Update()
    {


        if (lineRenderers.childCount != lastChildCount)
        { // If there is a change in the # of line renderers, run the code:

            bool O2plotExists = false;
            foreach (Transform plot_child in lineRenderers)
            {
                if (plot_child.gameObject.name.Contains("O<sub>2"))
                {
                    O2plotExists = true;
                    break;
                }
            }
            O2_21.SetActive(O2plotExists);
            O2_30.SetActive(O2plotExists);

            lastChildCount = lineRenderers.childCount;
        }



    }
}