using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using UniRx;
using System.Runtime.Serialization.Formatters.Binary;
using MoreLinq;
using System.Text.RegularExpressions;

namespace IO
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
    public static class WebService
    {


        public static Uri CombineUri(string baseUri, string relativeOrAbsoluteUri)
        {
            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri);
        }
        static string _clientId = "1030";
        static string _clientSecret = "+csxuC35huCwJokbdJBTWu9hrO0nX5G3";
        static string _redirectUri = "https://vianova.com";
        static TokenInfo _tokenInfo;
        static TokenInfo GetTokenInfo()
        {
            if (_tokenInfo == null)
            {
                LoadTokenInfo();
            }
            return _tokenInfo;
        }
        static void SetTokenInfo(TokenInfo tokeInfo)
        {
            _tokenInfo = tokeInfo;
            SaveData(tokeInfo);

        }
        static void SaveData(TokenInfo tokenInfo)
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

        static public void LoadTokenInfo()
        {

            BinaryFormatter formatter = new BinaryFormatter();
            if (File.Exists(Application.persistentDataPath + "/settings.dat"))
            {
                FileStream saveFile = File.Open(Application.persistentDataPath + "/settings.dat", FileMode.Open);
                try
                {
                    _tokenInfo = (TokenInfo)formatter.Deserialize(saveFile);
                    Debug.Log(GetTokenInfo().accesToken);
                }
                catch (TypeLoadException e)
                {
                    Debug.Log("token not found");
                }

                saveFile.Close();
            }
            Debug.Log("token loaded");
        }



        /*
        {"access_token":"I-M-_ZdcGpabykvrhRLrvQ==","scope":"MapRequestInitiator UtilityNetworkAuth MapRequestReader 
        UnaOperator UnaReader","expires_in":57600,"refresh_token":"-or4Br2IeTenTBpbuqIfpA=="}
        */

        static string allMapRequestAPIURL = "https://klip.beta.agiv.be/api/ws/klip/v1/MapRequest/Mri";


        static public IObservable<byte[]> CallAPIAndLogin(string APIURL, string httpAcceptHeader = "application/json")
        {
            IObservable<Unit> obs;
            //if token expired or no token available
            if (GetTokenInfo() == null || GetTokenInfo().refreshToken == null)
            {
                //TODO ask user for authorization code through UI
                var auth_code = "uZBRFQh19cCmRrTG+upQEA==";
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
            return obs.SelectMany(_ => CallAPI(APIURL, GetTokenInfo().accesToken, httpAcceptHeader));

        }

        //method returns json body API call
        static IObservable<byte[]> CallAPI(string APIURL, string acces_code, string httpAcceptHeader)
        {
            Debug.Log("API called");
            UnityWebRequest www = UnityWebRequest.Get(APIURL);
            www.SetRequestHeader("Authorization", "Bearer " + acces_code);
            www.SetRequestHeader("Accept", httpAcceptHeader);
            return UniRXExtensions.GetWWW(www).Select((bytesAndresponseCode) =>
            {
                return bytesAndresponseCode.Item1;
            });
        }
        static string BytesToString(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        static string EditIMKLURLForZIP(string url)
        {
            string pattern = "v1/";
            string replacement = "v1/imkl/";
            Regex rgx = new Regex(pattern);
            return rgx.Replace(url, replacement);
        }
        static string GetIMKLIDFromURL(string url)
        {
            return url.Split('/').Reverse().Skip(1).First();
        }
        static public void test()
        {
            CallAPIAndLogin(allMapRequestAPIURL).Subscribe(requests =>
            {
                var JArr = JArray.Parse(BytesToString(requests));
                //show box with all Mrs
                var panel = GUIFactory.CreateMultiSelectPanel(new Vector2(50, 50));
                //modify url to api url which downloads zips

                //the request id is in the second to last part of the url
                panel.AddItems(JArr.Select(url => url["MapRequest"].Value<string>())
                    .Select(urlstring =>
                    Tuple.Create(GetIMKLIDFromURL(urlstring),
                    urlstring)));

                //download selected
                var drawElementsObs = panel.OnSelectedItemsAsObservable().Subscribe(items =>
                    {
                        items.ForEach(item =>
                        {
                            var url = item.GetText().Item2;
                            var imklID = item.GetText().Item1;
                            //Call API for json zip info and zip file data
                            var obsIMKLRef = CallAPIAndLogin(url).Select(bytes =>
                                                   {
                                                       var jsontext = BytesToString(bytes);
                                                       var jObj = JObject.Parse(jsontext);

                                                       return Tuple.Create(jObj["Reference"].ToObject<string>(),
                                                       jObj["Status"].ToObject<string>());
                                                   });
                            obsIMKLRef.Subscribe(info =>
                            {
                                if (info.Item2.EndsWith("available"))
                                {
                                    CallAPIAndLogin(EditIMKLURLForZIP(url), "application/zip").Do(bytes =>
                                                                    {
                                                                        Debug.Log(info.Item1 + "_" + imklID);
                                                                        IMKLExtractor.ExtractIMKL(bytes,imklID,
                                                                        IMKLExtractor.GetSafeFileName(info.Item1 + "_" + imklID));
                                                                        //show window of all available xml
                                                                    }).Subscribe(_ => GUIFactory.ShowAllIMKLPanel());
                                }
                                //else show message that request is not available yet  TODO                               
                                else{
                                    Debug.Log("package not available yet");
                                }
                            });
                        });

                    });
            });
        }


        //use refresh_token to ask for new acces token
        static IObservable<Unit> SaveAccesTokenFromRefreshToken(string refreshToken)
        {
            Debug.Log("acces token request");
            WWWForm form = new WWWForm();
            form.AddField("client_id", "1030");
            form.AddField("client_secret", "+csxuC35huCwJokbdJBTWu9hrO0nX5G3");
            form.AddField("grant_type", "refresh_token");
            form.AddField("refresh_token", refreshToken);

            string url = "https://oauth.beta.agiv.be/authorization/ws/oauth/v2/token";
            UnityWebRequest www = UnityWebRequest.Post(url, form);


            return UniRXExtensions.GetWWW(www).Select((bytesAndresponseCode) =>
            {
                SaveAccesTokenFromBytes(bytesAndresponseCode.Item1);
                return Unit.Default;
            });
        }
        static void SaveAccesTokenFromBytes(byte[] bytes)
        {
            var jsontext = BytesToString(bytes);
            var jObj = JObject.Parse(jsontext);
            var expireDate = DateTime.Now.AddSeconds(jObj["expires_in"].ToObject<int>());
            var access_token = jObj["access_token"].ToObject<string>();
            var refresh_token = jObj["refresh_token"].ToObject<string>();
            SetTokenInfo(new TokenInfo(access_token, expireDate, refresh_token));
        }
        //return empty string when complete because the authorization process cannot be done stateless
        static IObservable<Unit> GetAccesTokenFromAuthCode(string code_authorization)
        {
            WWWForm form = new WWWForm();
            form.AddField("client_id", "1030");
            form.AddField("redirect_uri", "https://vianova.com");
            form.AddField("client_secret", "+csxuC35huCwJokbdJBTWu9hrO0nX5G3");
            form.AddField("grant_type", "authorization_code");
            form.AddField("code", code_authorization);

            string url = "https://oauth.beta.agiv.be/authorization/ws/oauth/v2/token";

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            return UniRXExtensions.GetWWW(www).Select((bytesAndresponseCode) =>
            {
                SaveAccesTokenFromBytes(bytesAndresponseCode.Item1);
                return Unit.Default;
            });
        }
    }

}
