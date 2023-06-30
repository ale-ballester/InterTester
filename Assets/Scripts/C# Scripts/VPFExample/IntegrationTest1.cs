using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;


//  This script integrates SDK with Virtual People Factory (VPF) -> www.vpf2.cise.ufl.edu
//  VPF provides an GUI interface to create a conversation script with machine.
//  It depends on script VPF2ApiAccess.cs for communicating with VPF
//  For more details contact people from www.verg.cise.ufl.edu

public class IntegrationTest1 : MonoBehaviour
{

    private VPF2ApiAccess apiAcces;
    string url = "https://s3.amazonaws.com/vpf2cise/Uploads/Audio/Speeches/";  //URL for audio recordings of dialogues
    string scenarioID; //VPF scenario ID from vpf2.cise.ufl.edu
    string characterID; //VPF characterID,  required to find audio recordings

    void Start()
    {
        //initiates communication with  VPF
        apiAcces = this.GetComponent<VPF2ApiAccess>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //  Function to send dialogue input to VPF
    //  Returns result from VPF 
    public void SendUserInputToVPF(string textQuery)
    {

        if (!string.IsNullOrEmpty(textQuery))
        {
            
            StartCoroutine(apiAcces.FindResponse(textQuery, (result) =>
            {
                HandleVPFResponse(result);
            }));
        }
    }

    // Hack to initiate conversation from vpf2
    // For ex - you might want system to ask user question like - where does sun rise? 
    // VPF cannot initiate question on it's own
    // VPF is designed to repond to intitiated converstion from user.
    // So hack is done using Act Support from VPF
    // Hack - VPF expects text input from user to respond. This scripts sends particular tags as text to VPF
    // VPF then responds back based on what text is received - which can be a question to user.
    // For more details contact people from VPF
    public void InitiateQuestionFromVPF(string textQuery)
    {
        textQuery = "Video0tag";
        if (!string.IsNullOrEmpty(textQuery))
        {

            StartCoroutine(apiAcces.FindResponse(textQuery, (result) =>
            {
                HandleVPFResponse(result);
            }));
        }

    }

    //  Function to handle VPF response.
    //  VPF responds with error if it cannot find response for input.
    //  Size of returned JSON object is different in success and error case.
    //  They are handled separately
    void HandleVPFResponse(SimpleJSON.JSONNode vpfReturnObject)
    {
        string errorString = "Sorry! Can you please say it in other way.";
        if (vpfReturnObject.Count == 21)
        {
            //string stringObject = vpfReturnObject.ToString();
            //JSONObject obj = new JSONObject(stringObject);

            //string responseString = obj["SpeechText"].str;
            //Debug.Log("ResponseText:" + responseString);

            //scenarioID = obj["ScenarioID"].ToString();
            //characterID = obj["CharacterID"].ToString();
            //string audioURL = obj["AudioFileName"].str;
            //StartCoroutine(PlayAudio(audioURL));


            //JSONObject topic = obj["Topics"];
            //ArrayList topicList = topic.list;

            //if (topicList.Count != 0)
            //{
            //    foreach (var topicName in topicList)
            //    {
            //        Debug.Log("Topic:" + topicName.ToString());
            //        StartCoroutine(HandleUserRequest(topicName.ToString()));

            //    }
            //}

            //textHandler.GetComponent<Text>().text = textHandler.GetComponent<Text>().text + "\nTutor Said - " + responseString;

        }
        else
        {
            //textHandler.GetComponent<Text>().text = textHandler.GetComponent<Text>().text + "\nTutor Said - " + errorString;
        }

    }

    //  Function to handle user request
    //  Ex - User might usk show me how to do ultrasound procedure.
    //  Based on user request, vpf can repond with certain tag and system can initiate action based on that tag
    //  Here user is askeed to choose between two methods. Based on user selection, VPF responds with tags and system plays a video based on that tag
    IEnumerator HandleUserRequest(string tag)
    {
        yield return new WaitForSeconds(2);
        //if (tag == "\"Learn Epidural Type 1\"")
        //{
        //    RA_Simulation.ME.ShowIT();
        //    ITMaster.ME.LoadScenario(13);

        //}
        //else if(tag == "\"Learn Epidural Type 2\"")
        //{
        //    RA_Simulation.ME.ShowIT();
        //    ITMaster.ME.LoadScenario(16);

        //}

    }

    // Download audio for particular dialogue from server and play.
    IEnumerator PlayAudio(string audioName)
    {
        string audioURL = url + scenarioID + "/" + characterID + "/" + audioName;
        Debug.Log("audioURL:" + audioURL);
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioURL, AudioType.WAV);
        yield return www.Send();

        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

        this.GetComponent<AudioSource>().clip = clip;
        this.GetComponent<AudioSource>().Play();
    }

}


