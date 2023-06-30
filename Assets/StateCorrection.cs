using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateCorrection : MonoBehaviour
{
    public Transform sensor;

    private EKFSLAM ekfslam;

    // Start is called before the first frame update
    void Start()
    {
        ekfslam.Initialize(sensor, array2DfromDiagonal(0.1,ekfslam.N), array2DfromDiagonal(0.1,7));
    }

    // Update is called once per frame
    void Update()
    {   
        if (ATC.ME.Connected) {
            ekfslam.EKFUpdate(ekfslam.Vector3AndQuaternionToVector(sensor.position, sensor.rotation));

            Vector3 position;
            Quaternion rotation;
            (position, rotation) = ekfslam.VectorToVector3AndQuaternion(ekfslam.state);

            transform.position = position;
            transform.rotation = rotation;
        }
    }

    private double[,] array2DfromDiagonal(double[] diagonal) {
        double[,] array2D = new double[diagonal.Length, diagonal.Length];
        for (int i = 0; i < diagonal.Length; i++) {
            array2D[i,i] = diagonal[i];
        }

        return array2D;
    }

    private double[,] array2DfromDiagonal(double diagonal, int size) {
        double[] diagonalArray = new double[size];
        for (int i = 0; i < size; i++) {
            diagonalArray[i] = diagonal;
        }

        return array2DfromDiagonal(diagonalArray);
    }
}
