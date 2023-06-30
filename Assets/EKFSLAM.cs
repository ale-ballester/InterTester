using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Differentiation;

public class EKFSLAM : MonoBehaviour
{
    private static MatrixBuilder<double> M = Matrix.Build;
    private static VectorBuilder<double> V = Vector.Build;

    private static int N = 3+4+3+3+7*7;
    private float dt = 0.1f;

    private Vector<double> state = V.Dense(N);
    private Matrix<double> processCovariance = M.Dense(N, N);
    private MatrixNormal processNoise;
    private Matrix<double> measurementCovariance = M.Dense(7, 7);
    private MatrixNormal measurementNoise;

    private Matrix<double> KalmanGain = M.Dense(N, 7);
    private Matrix<double> P_covariance = M.Dense(N, N);

    private Matrix<double> gradF = M.Dense(N, N);
    private Matrix<double> gradH = M.Dense(N, N);


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Transform initialSensorTransform, double[,] initialProcessCovariance, double[,] initialMeasurementCovariance) {
        Vector3 position = initialSensorTransform.position;
        Quaternion rotation = initialSensorTransform.rotation;

        Matrix<double> initialPosition = M.DenseOfArray(new double[,] {
            {position.x},
            {position.y},
            {position.z}
        });
        Matrix<double> initialQuaternion = M.DenseOfArray(new double[,] {
            {rotation.x},
            {rotation.y},
            {rotation.z},
            {rotation.w}
        });

        state.SetSubVector(0, 3, V.DenseOfArray(initialPosition.ToColumnMajorArray()));
        state.SetSubVector(3, 4, V.DenseOfArray(initialQuaternion.ToColumnMajorArray()));
        processCovariance = M.DenseOfArray(initialProcessCovariance);
        measurementCovariance = M.DenseOfArray(initialMeasurementCovariance);

        processNoise = new MatrixNormal(M.Dense(N,1), processCovariance, M.Dense(1,1,1.0));
        // Measurement noise should have position-dependent covariance!!!
        measurementNoise = new MatrixNormal(M.Dense(N,1), measurementCovariance, M.Dense(1,1,1.0));
    }

    private void EKFupdate(Vector<double> measurement) {
        Vector<double> s_predicted = dynamics(state);

        Func<Vector<double>,Vector<double>> eval_dyns = s => dynamics(s);
        Func<Vector<double>,Vector<double>> eval_obs = s => observation(s);
        gradF = VectorFieldJacobian(eval_dyns, state);
        gradH = VectorFieldJacobian(eval_obs, s_predicted);

        Matrix<double> P_predicted = gradF*P_covariance*gradF.Transpose() + processCovariance;

        Vector<double> y_innovation = measurement - observation(s_predicted);
        Matrix<double> S = gradH*P_predicted*gradH.Transpose() + measurementCovariance;
        KalmanGain = P_predicted*gradH.Transpose()*S.Inverse();


        state = s_predicted + KalmanGain*y_innovation;
        P_covariance = (M.DenseIdentity(N,N) - KalmanGain*gradH)*P_predicted;
    }

    public Vector<double> Vector3AndQuaternionToVector(Vector3 position, Quaternion rotation) {
        Vector<double> pose = V.DenseOfArray(new double[] {
            {position.x},
            {position.y},
            {position.z},
            {rotation.x},
            {rotation.y},
            {rotation.z},
            {rotation.w}
        });

        return pose;
    }

    public (Vector3,Quaternion) VectorToVector3AndQuaternion(Vector<double> pose) {
        Vector3 position = new Vector3((float)pose[0], (float)pose[1], (float)pose[2]);
        Quaternion rotation = new Quaternion((float)pose[3], (float)pose[4], (float)pose[5], (float)pose[6]);

        return (position, rotation);
    }

    private Matrix<double> VectorFieldJacobian(Func<Vector<double>, Vector<double>> f, Vector<double> s) {
        NumericalJacobian df = new NumericalJacobian();

        Matrix<double> jacobian = M.Dense(s.Count, s.Count);

        for (int i = 0; i < s.Count; i++) {
            int j = i;
            Vector<double> grad_i = V.DenseOfArray(df.Evaluate(s_array => f(V.DenseOfArray(s_array))[j], s.ToArray()));
            jacobian.SetColumn(i, grad_i);
        }

        return jacobian;
    }

    private Vector<double> dynamics(Vector<double> s, bool makeNoise = false) //, Vector<double> control)
    {
        Vector<double> position = s.SubVector(0, 3);
        Vector<double> quaternion = s.SubVector(3, 4);
        Vector<double> velocity = s.SubVector(7, 3);
        Vector<double> angularVelocity = s.SubVector(10, 3);
        Vector<double> landmarks = s.SubVector(13, N-13);

        Vector<double> newPosition = position + velocity * dt;
        Matrix<double> crossProductMatrix = M.DenseOfArray(new double[,] {
            {0, -angularVelocity[2], angularVelocity[1]},
            {angularVelocity[2], 0, -angularVelocity[0]},
            {-angularVelocity[1], angularVelocity[0], 0},
        });
        Matrix<double> W = M.Dense(N, N);
        W = crossProductMatrix.Append(angularVelocity.ToColumnMatrix());
        W = W.Stack(-angularVelocity.ToColumnMatrix().Append(M.Dense(1,1)));
        Vector<double> newQuaternion = Matrix<double>.Exp(dt*W) * quaternion;
        
        Matrix<double> newStateMatrix = M.Dense(N,1);
        newStateMatrix.SetSubMatrix(0, 3, newPosition.ToColumnMatrix());
        newStateMatrix.SetSubMatrix(3, 4, newQuaternion.ToColumnMatrix());
        newStateMatrix.SetSubMatrix(7, 3, velocity.ToColumnMatrix());
        newStateMatrix.SetSubMatrix(10, 3, angularVelocity.ToColumnMatrix());
        newStateMatrix.SetSubMatrix(13, N-13, landmarks.ToColumnMatrix());

        Vector<double> newState = V.DenseOfArray(newStateMatrix.ToColumnMajorArray());

        if (makeNoise) {
            Vector<double> noise = V.DenseOfArray(processNoise.Sample().ToColumnMajorArray());
            newState += noise;
        }

        return newState;
    }

    private Vector<double> observation(Vector<double> s, bool makeNoise = false) {
        Vector<double> position = s.SubVector(0, 3);
        Vector<double> quaternion = s.SubVector(3, 4);
        Vector<double> velocity = s.SubVector(7, 3);
        Vector<double> angularVelocity = s.SubVector(10, 3);
        Vector<double> landmarks = s.SubVector(13, N-13);

        Matrix<double> newObservationMatrix = M.Dense(7,1);
        newObservationMatrix.SetSubMatrix(0, 3, position.ToColumnMatrix());
        newObservationMatrix.SetSubMatrix(3, 4, quaternion.ToColumnMatrix());

        Vector<double> newObservation = V.DenseOfArray(newObservationMatrix.ToColumnMajorArray());

        if (makeNoise) {
            Vector<double> noise = V.DenseOfArray(measurementNoise.Sample().ToColumnMajorArray());
            newObservation += noise;
        }

        return newObservation;
    }

    private Vector<double> distortion(Vector<double> sensorPosition, Vector<double> landmarks) {

        Vector<double> result = V.Dense(7);
        for (int n = 0; n < N-13; N++) {
            double distortion = landmarks[7*n] + Math.Exp(-Math.Pow(((sensorPosition - landmarks.SubVector(1+7*n,3)).PointwiseDivide(landmarks.SubVector(3+7*n,3))).L2Norm(),2));
            result[n] = distortion;
        }

        return result;
    }
}
