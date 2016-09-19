using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;
using System.Net;
using System;
using System.IO;

public class Test : MonoBehaviour
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
    void method1()
    {
        string url = CombineUri(_serviceUriAuth, "/authorization/ws/oauth/v2/token").ToString();

        WebRequest request = WebRequest.Create(url);
        _codeAuthorization = HttpUtility.UrlEncode(_codeAuthorization);
        _clientSecret = HttpUtility.UrlEncode(_clientSecret);
        _redirectUri = HttpUtility.UrlEncode(_redirectUri);

        request.Method = "POST";
        string body = String.Format("grant_type={0}&code={1}&client_id={2}&client_secret={3}&redirect_uri={4}",
                                           "authorization_code", _codeAuthorization, _clientId, _clientSecret,
                                           _redirectUri);

        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(body);
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteArray.Length;
        Stream dataStream = request.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);
        dataStream.Close();
        var response = request.GetResponse();
        StreamReader stream = new StreamReader(response.GetResponseStream());
        String json = stream.ReadToEnd();
        Debug.Log(json);
    }
    void method2()
    {
        WWWForm form = new WWWForm();
        form.AddField("client_id", "1030");
        form.AddField("redirect_uri", "https://vianova.com");
        form.AddField("client_secret", "+csxuC35huCwJokbdJBTWu9hrO0nX5G3");
        form.AddField("grant_type", "authorization_code");
        form.AddField("code", "sVfsaEG4gf7fHXa7B1I+Rg==");


        Debug.Log(System.Text.Encoding.UTF8.GetString(form.data));

        string url = "https://oauth.beta.agiv.be/authorization/ws/oauth/v2/token";



        // Add a custom header to the request.
        // In this case a basic authentication to access a password protected resource.

        UnityWebRequest www = UnityWebRequest.Post(url, form);
       // www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        //www.SetRequestHeader("Host", "oauth.beta.agiv.be");

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
    IEnumerator WaitForAccess(WWW www)
    {
        Debug.Log("waiting for access\n");
        yield return www;
        Debug.Log("Past the Yield \n");
        // check for errors
        if (www.error == null)
        {
            Debug.Log("no error \n");
            Debug.Log("wwwText: " + www.text);
            //Debug.Log("WWW Ok!: " + www.text);
            // _accessToken = www.responseHeaders["access_token"];
        }
        if (www.error != null)
        {
            Debug.Log("\n Error" + www.error);
            Debug.Log(www.error);
        }
        Debug.Log("end of WaitForAccess \n");
    }
    void Start()
    {
        //method1();
        //method2();
    }

    // Update is called once per frame

}
