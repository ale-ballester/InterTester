using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SMARTS_SDK;

public class InterTester : MonoBehaviour
{
    public bool align;
    public bool saveDataNow = false;
    public String filename = "";
    public bool timeUniformSampling = false;
    public bool spaceUniformSampling = true;
    public float spaceSamplingRate = 0.1f;
    public float SpaceTolerance = 0.005f;
    bool lastSaveData = false;
    public bool resetArrays = false;
    int sampleNum = 0;

    public Material cloneMaterial;

    // Cameras
    public GameObject MRTCamera;
    public GameObject Sensor1Camera;

    // Sensor live GameObjects
    public GameObject Sensor1;
    public GameObject Sensor2;
    public GameObject Sensor3;

    // Sensor duplicates
    GameObject Sensor1dup;
    GameObject Sensor2dup;
    GameObject Sensor3dup;

    // Sensor Ground Truth GameObjects
    public GameObject GTSensor1;
    public GameObject GTSensor2;
    public GameObject GTSensor3;
    

    // Difference between GT and reported position
    [System.NonSerialized] public Vector3 error1 = Vector3.zero;
    [System.NonSerialized] public Vector3 error2 = Vector3.zero;
    [System.NonSerialized] public Vector3 error3 = Vector3.zero;
    // Difference between GT and reported position
    [System.NonSerialized] public Vector3 RotError1 = Vector3.zero;
    [System.NonSerialized] public Vector3 RotError2 = Vector3.zero;
    [System.NonSerialized] public Vector3 RotError3 = Vector3.zero;
    // Differences between sensors
    [System.NonSerialized] public float error12 = 0;
    [System.NonSerialized] public float error23 = 0;
    [System.NonSerialized] public float error31 = 0;

    // Error arrays
    static int arraySize = 5*60*60; // Save up to 5 minutes at 60 fps
    [System.NonSerialized] public float[] error1array = new float[arraySize];
    [System.NonSerialized] public float[] error2array = new float[arraySize];
    [System.NonSerialized] public float[] error3array = new float[arraySize];
    [System.NonSerialized] public float[] RotError1array = new float[arraySize];
    [System.NonSerialized] public float[] RotError2array = new float[arraySize];
    [System.NonSerialized] public float[] RotError3array = new float[arraySize];
    [System.NonSerialized] public float[] error12array = new float[arraySize];
    [System.NonSerialized] public float[] error23array = new float[arraySize];
    [System.NonSerialized] public float[] error31array = new float[arraySize];
    [System.NonSerialized] public int iterNum = 0;

    // Saving data 
    string data = "";
    string textPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "//";
    float startTime = 0;

    Vector3 lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        MRTCamera.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (ATC.ME.Connected) {
            if (iterNum == arraySize || resetArrays) {
                ResetArrays();
                resetArrays = false;
            }

            if (align) {
                GTSensor1.transform.position = Sensor1.transform.position;
                GTSensor2.transform.position = Sensor2.transform.position;
                GTSensor3.transform.position = Sensor3.transform.position;
                GTSensor1.transform.rotation = Sensor1.transform.rotation;
                GTSensor2.transform.rotation = Sensor2.transform.rotation;
                GTSensor3.transform.rotation = Sensor3.transform.rotation;

                Destroy(Sensor1dup);
                Destroy(Sensor2dup);
                Destroy(Sensor3dup);

                Sensor1dup = Instantiate(Sensor1,Sensor1.transform.parent);
                Sensor2dup = Instantiate(Sensor2,Sensor2.transform.parent);
                Sensor3dup = Instantiate(Sensor3,Sensor3.transform.parent);

                Sensor1dup.GetComponent<Renderer>().material = cloneMaterial;
                Sensor2dup.GetComponent<Renderer>().material = cloneMaterial;
                Sensor3dup.GetComponent<Renderer>().material = cloneMaterial;

                align = false;
            }

            error1 = Sensor1.transform.position - GTSensor1.transform.position;
            error2 = Sensor2.transform.position - GTSensor2.transform.position;
            error3 = Sensor3.transform.position - GTSensor3.transform.position;
            RotError1 = (Sensor1.transform.rotation * Quaternion.Inverse(GTSensor1.transform.rotation)).eulerAngles;
            RotError2 = (Sensor2.transform.rotation * Quaternion.Inverse(GTSensor2.transform.rotation)).eulerAngles;
            RotError3 = (Sensor3.transform.rotation * Quaternion.Inverse(GTSensor3.transform.rotation)).eulerAngles;
            error12 = Vector3.Magnitude(Sensor1.transform.position-Sensor2.transform.position)-Vector3.Magnitude(GTSensor1.transform.position-GTSensor2.transform.position);
            error23 = Vector3.Magnitude(Sensor2.transform.position-Sensor3.transform.position)-Vector3.Magnitude(GTSensor2.transform.position-GTSensor3.transform.position);
            error31 = Vector3.Magnitude(Sensor3.transform.position-Sensor1.transform.position)-Vector3.Magnitude(GTSensor3.transform.position-GTSensor1.transform.position);

            error1array[iterNum] = Vector3.Magnitude(error1);
            error2array[iterNum] = Vector3.Magnitude(error2);
            error3array[iterNum] = Vector3.Magnitude(error3);
            RotError1array[iterNum] = Vector3.Magnitude(RotError1);
            RotError2array[iterNum] = Vector3.Magnitude(RotError2);
            RotError3array[iterNum] = Vector3.Magnitude(RotError3);
            error12array[iterNum] = error12;
            error23array[iterNum] = error23;
            error31array[iterNum] = error31;

            if (spaceUniformSampling) {
                timeUniformSampling = false;
                if (saveDataNow == true && lastSaveData == false) {
                    startTime = Time.time;
                    lastSaveData = saveDataNow;
                    lastPosition = Sensor1.transform.position;
                }
                if (saveDataNow == true && lastSaveData == true) {
                    Vector3 position = Sensor1.transform.position;
                    Quaternion rotation = Sensor1.transform.rotation;
                    float timestamp = Time.time - startTime;
                    if (Math.Abs(Vector3.Distance(position,lastPosition)-spaceSamplingRate) < SpaceTolerance) {
                        AddLine(sampleNum, position, rotation, timestamp);
                        lastPosition = position;
                    }
                }
                if (saveDataNow == false && lastSaveData == true) {
                    SaveData();
                    lastSaveData = saveDataNow;
                }
                sampleNum++;
            }

            iterNum++;
        }
    }

    void FixedUpdate() {
        if (ATC.ME.Connected) {
            if (timeUniformSampling) {
                spaceUniformSampling = false;
                if (saveDataNow == true && lastSaveData == false) {
                    startTime = Time.time;
                    lastSaveData = saveDataNow;
                }
                if (saveDataNow == true && lastSaveData == true) {
                    Vector3 position = Sensor1.transform.position;
                    Quaternion rotation = Sensor1.transform.rotation;
                    float timestamp = Time.time - startTime;
                    AddLine(sampleNum,position, rotation, timestamp);
                }
                if (saveDataNow == false && lastSaveData == true) {
                    SaveData();
                    lastSaveData = saveDataNow;
                }
            }    
        }
    }

    void AddLine(int sampleNum, Vector3 position, Quaternion rotation, float timestamp)
    {
        string line = String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\n", sampleNum, timestamp, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w);
        data += line;
    }

    void SaveData()
    {
        File.AppendAllText(textPath + filename + ".txt", data);
        data = "";
        sampleNum = 0;
        ResetArrays();
    }

    void ResetArrays() {
        error1array = new float[arraySize];
        error2array = new float[arraySize];
        error3array = new float[arraySize];
        RotError1array = new float[arraySize];
        RotError2array = new float[arraySize];
        RotError3array = new float[arraySize];
        error12array = new float[arraySize];
        error23array = new float[arraySize];
        error31array = new float[arraySize];
        sampleNum = 0;
        data = "";
    }

    int TotalLines(string path)
    {
        if (File.Exists(path)) 
        {
            using (StreamReader r = new StreamReader(path))
            {
                int i = 0;
                while (r.ReadLine() != null) { i++; }
                return i;
            }
        }
        else
        {
            return 0;
        }
    }
}
