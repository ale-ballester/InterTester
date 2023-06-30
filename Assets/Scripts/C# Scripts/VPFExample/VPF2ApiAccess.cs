using UnityEngine;
using SimpleJSON;
using System.Collections;


//  Script to open communication with VPF
public class VPF2ApiAccess : MonoBehaviour {

    //Requires client ID and secret - which can be received on request from vpf2.cise.ufl.edu or contact people at www.verg.cise.ufl.edu
    public string clientID, clientSecret, ScenarioID;

    private string  accessTokenUrl =  "https://vpf2.cise.ufl.edu:35000/oauth2/token";
    private string baseURL;
    private string accessToken;
	// Use this for initialization
    //  Get access token from server
    //Need to be renewed after certain time! - which is not handled here
	void Start () {        
        baseURL = "https://vpf2.cise.ufl.edu:35000/api/Interaction/FindResponse?ScenarioID=" + ScenarioID + "&userinput=";
        StartCoroutine(GetAccessToken());        
    }
	
	// Update is called once per frame
	void Update () {
    }

    //API call to VPF
    public IEnumerator FindResponse(string speechText, System.Action<JSONNode> callback)
    {
        var url = baseURL + WWW.EscapeURL(speechText) + "&access_token=" + accessToken;

        var www = new WWW(url);
        yield return www;        

        var result = JSON.Parse(www.text);

        callback(result);        
    }

    //Get access token to use API
    IEnumerator GetAccessToken()
    {        
        var form = new WWWForm();

        form.AddField("grant_type", "client_credentials");

        var headers = form.headers;
        var rawData = form.data;

        headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(clientID + ":" + clientSecret));

        var www = new WWW(accessTokenUrl, rawData, headers);

        yield return www;

        var obj = JSON.Parse(www.text);

        Debug.Log(www.text);

        accessToken = obj["access_token"];

        Invoke("GetAccessToken", 3600);
        
    }
}
