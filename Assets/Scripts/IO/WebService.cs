using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;
using System.Net;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using UniRx;
using System.Runtime.Serialization.Formatters.Binary;
using MoreLinq;
using System.Text.RegularExpressions;

public class WebService : MonoBehaviour
{
    [Serializable]
    class TokenInfo
    {
        public TokenInfo(string access_token, DateTime expireDate, string refresh_token)
        {
            this.accesToken = access_token;
            this.expireDate = expireDate;
            this.refreshToken = refresh_token;
        }
        public string refreshToken;
        public string accesToken;
        public DateTime expireDate;
    }
    public static Uri CombineUri(string baseUri, string relativeOrAbsoluteUri)
    {
        return new Uri(new Uri(baseUri), relativeOrAbsoluteUri);
    }
    string _clientId = "1030";
    static string _clientSecret = "+csxuC35huCwJokbdJBTWu9hrO0nX5G3";
    static string _redirectUri = "https://vianova.com";

    TokenInfo _tokenInfo;
    TokenInfo GetTokenInfo()
    {
        if (_tokenInfo == null)
        {
            LoadTokenInfo();
        }
        return _tokenInfo;
    }
    void SetTokenInfo(TokenInfo tokeInfo)
    {
        _tokenInfo = tokeInfo;
        SaveData(tokeInfo);

    }
    void SaveData(TokenInfo tokenInfo)
    {
        if (tokenInfo != null)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            //creates or overwrites file
            FileStream saveFile = File.Create(Application.persistentDataPath + "/settings.dat");

            formatter.Serialize(saveFile, tokenInfo);

            saveFile.Close();
        }

    }

    public void LoadTokenInfo()
    {

        BinaryFormatter formatter = new BinaryFormatter();
        if (File.Exists(Application.persistentDataPath + "/settings.dat"))
        {
            FileStream saveFile = File.Open(Application.persistentDataPath + "/settings.dat", FileMode.Open);
            _tokenInfo = (TokenInfo)formatter.Deserialize(saveFile);
            Debug.Log(GetTokenInfo().accesToken);
            saveFile.Close();
        }
        Debug.Log("token loaded");
    }



    /*
    {"access_token":"I-M-_ZdcGpabykvrhRLrvQ==","scope":"MapRequestInitiator UtilityNetworkAuth MapRequestReader 
    UnaOperator UnaReader","expires_in":57600,"refresh_token":"-or4Br2IeTenTBpbuqIfpA=="}
    */

    string allMapRequestAPIURL = "https://klip.beta.agiv.be/api/ws/klip/v1/MapRequest/Mri";
   

    public IObservable<string> CallAPIAndLogin(string APIURL)
    {
        IObservable<Unit> obs;
        //if token expired or no token available
        if (GetTokenInfo() == null || GetTokenInfo().refreshToken == null)
        {
            //TODO ask user for authorization code through UI
            var auth_code = "GoUt7Bph9nf9jrsaSQRtmw==";
            obs = GetAccesTokenFromAuthCode(auth_code).Select(_ => _);
        }
        else if (GetTokenInfo().accesToken == null || GetTokenInfo().expireDate < DateTime.Now)
        {

            Debug.Log("get acces token");
            obs = SaveAccesTokenFromRefreshToken(GetTokenInfo().refreshToken).Select(_ => _);
        }
        else
        {
            Debug.Log("acces token available");
            obs = Observable.Return<Unit>(Unit.Default);
        }
        return obs.SelectMany(_ => CallAPI(APIURL, GetTokenInfo().accesToken));

    }
    //method returns json body API call
    IObservable<string> CallAPI(string APIURL, string acces_code)
    {
        Debug.Log("API called");
        UnityWebRequest www = UnityWebRequest.Get(APIURL);
        www.SetRequestHeader("Authorization", "Bearer " + acces_code);
        www.SetRequestHeader("Accept", "application/json");
        return UniRXExtensions.GetWWW(www).Select((bytes) =>
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        });
    }


    public void test()
    {
        CallAPIAndLogin(allMapRequestAPIURL).Subscribe(requests =>
        {
            var JArr = JArray.Parse(requests);
            Debug.Log(requests);
            //show box with all Mrs
            var panel = GUIFactory.CreateMultiSelectPanel(new Vector2(50, 50));
            //modify url to api url which downloads zips
            string pattern = "v1/";
            string replacement = "v1/imkl/";
            Regex rgx = new Regex(pattern);
            //the request id is in the second to last part of the url
            panel.AddItems(JArr.Select(url => url["MapRequest"].Value<string>())
            .Select(urlstring =>
            Tuple.Create(urlstring.Split('/').Reverse().Skip(1).First(),
            rgx.Replace(urlstring, replacement))));

            //download selected
            var drawElementsObs = panel.OnSelectedItemsAsObservable().Subscribe(items =>
            {
                items.ForEach(item =>
                {
                    Debug.Log(item.GetText().Item2);
                    UnityWebRequest www = UnityWebRequest.Get(item.GetText().Item2);
                    UniRXExtensions.GetWWW(www).Subscribe((bytes) =>
                                            {
                                                Debug.Log(bytes.Length);
                                                Debug.Log( System.Text.Encoding.UTF8.GetString(bytes));
                                            });
                });
            });
        });

        //
    }

    //use refresh_token to ask for new acces token
    public IObservable<Unit> SaveAccesTokenFromRefreshToken(string refreshToken)
    {
        Debug.Log("acces token request");
        WWWForm form = new WWWForm();
        form.AddField("client_id", "1030");
        form.AddField("client_secret", "+csxuC35huCwJokbdJBTWu9hrO0nX5G3");
        form.AddField("grant_type", "refresh_token");
        form.AddField("refresh_token", refreshToken);

        string url = "https://oauth.beta.agiv.be/authorization/ws/oauth/v2/token";
        UnityWebRequest www = UnityWebRequest.Post(url, form);


        return UniRXExtensions.GetWWW(www).Select((bytes) =>
        {
            SaveAccesTokenBytes(bytes);
            return Unit.Default;
        });
    }
    void SaveAccesTokenBytes(byte[] bytes)
    {
        var jsontext = System.Text.Encoding.UTF8.GetString(bytes);
        var jObj = JObject.Parse(jsontext);
        var expireDate = DateTime.Now.AddSeconds(jObj["expires_in"].ToObject<int>());
        var access_token = jObj["access_token"].ToObject<string>();
        var refresh_token = jObj["refresh_token"].ToObject<string>();
        SetTokenInfo(new TokenInfo(access_token, expireDate, refresh_token));
    }
    //return empty string when complete because the authorization process cannot be done stateless
    public IObservable<Unit> GetAccesTokenFromAuthCode(string code_authorization)
    {
        WWWForm form = new WWWForm();
        form.AddField("client_id", "1030");
        form.AddField("redirect_uri", "https://vianova.com");
        form.AddField("client_secret", "+csxuC35huCwJokbdJBTWu9hrO0nX5G3");
        form.AddField("grant_type", "authorization_code");
        form.AddField("code", code_authorization);

        string url = "https://oauth.beta.agiv.be/authorization/ws/oauth/v2/token";

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        return UniRXExtensions.GetWWW(www).Select((bytes) =>
        {
            SaveAccesTokenBytes(bytes);
            return Unit.Default;
        });
    }



}
