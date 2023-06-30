using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ErrorUpdate : MonoBehaviour
{
    public GameObject InterTester;
    InterTester IT;

    public int MA = 100;

    // Start is called before the first frame update
    void Start()
    {
        IT = InterTester.GetComponent<InterTester>();
    }

    // Update is called once per frame
    void Update()
    {
        int text_num = 1;
        foreach (Transform child in transform) {
            float mse = 0f;
            switch (text_num)
            {
                case 1:
                    mse = MovingAverage(IT.error1array,IT.iterNum);
                    break;
                case 2:
                    mse = MovingAverage(IT.error2array,IT.iterNum);
                    break;
                case 3:
                    mse = MovingAverage(IT.error3array,IT.iterNum);
                    break;
                case 4:
                    mse = MovingAverage(IT.RotError1array,IT.iterNum);
                    break;
                case 5:
                    mse = MovingAverage(IT.RotError2array,IT.iterNum);
                    break;
                case 6:
                    mse = MovingAverage(IT.RotError3array,IT.iterNum);
                    break;
                case 7:
                    mse = MovingAverage(IT.error12array,IT.iterNum);
                    break;
                case 8:
                    mse = MovingAverage(IT.error23array,IT.iterNum);
                    break;
                case 9:
                    mse = MovingAverage(IT.error31array,IT.iterNum);
                    break;
                default:
                    break;
            }
            child.gameObject.GetComponent<TextMeshProUGUI>().text = mse.ToString();
            text_num++;
        }
    }

    float MovingAverage(float[] arr, int n) {
        float sum = 0;
        int L = 0;
        if (n >= MA) {
            for (int i = n-MA-1; i<n; i++) {
                sum += arr[i];
            }
            L = MA;
        }
        else {
            for (int i = 0; i<n; i++) {
                sum += arr[i];
            }
            L = n;
        }

        return sum/L;
    }
}
