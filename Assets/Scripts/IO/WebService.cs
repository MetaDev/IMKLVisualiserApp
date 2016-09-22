using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;
using System.Net;
using System;
using System.IO;

public class WebService : MonoBehaviour
{
    public static Uri CombineUri(string baseUri, string relativeOrAbsoluteUri)
    {
        return new Uri(new Uri(baseUri), relativeOrAbsoluteUri);
    }
    static string _clientId = "1030";
    static string _clientSecret = "+csxuC35huCwJokbdJBTWu9hrO0nX5G3";
    static string _redirectUri = "https://vianova.com";
    static string _codeAuthorization = "sVfsaEG4gf7fHXa7B1I+Rg==";

    static string _serviceUriAuth = "https://oauth.beta.agiv.be";
    static string _serviceUriKlip = "https://klip.beta.agiv.be";
    //TODO 
    /*
    {"access_token":"EZO7MvfyinuXPsFER0NKHg==","scope":"MapRequestInitiator UtilityNetworkAuth MapRequestReader UnaOperator UnaReader",
    "expires_in":57598,"refresh_token":"QGVkHGo5zuN2KtJhduO5iQ=="}
UnityEngine.Debug:Log(Object)
<request>c__Iterator0:MoveNext() (at Assets/Scripts/IO/WebService.cs:62)
UnityEngine.SetupCoroutine:InvokeMoveNext(IEnumerator, IntPtr)

    */
    public void download()
    {
        WWWForm form = new WWWForm();
        form.AddField("client_id", "1030");
        form.AddField("redirect_uri", "https://vianova.com");
        form.AddField("client_secret", "+csxuC35huCwJokbdJBTWu9hrO0nX5G3");
        form.AddField("grant_type", "authorization_code");
        form.AddField("code", "EFWkczgnrJahElNx85pbhw==");

        string url = "https://oauth.beta.agiv.be/authorization/ws/oauth/v2/token";



        //TODO save refreshtoken if first time
        //check vb code

        UnityWebRequest www = UnityWebRequest.Post(url, form);
       

        StartCoroutine(request(www));
    }
    IEnumerator request(UnityWebRequest www)
    {
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
            Debug.Log(www.responseCode);

        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }
   
    

    // Update is called once per frame

}
