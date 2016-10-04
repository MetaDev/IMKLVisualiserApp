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
using IMKL_Logic;
using System.Xml.Linq;

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


        static string allMapRequestAPIURL = "https://klip.beta.agiv.be/api/ws/klip/v1/MapRequest/Mri";
        public static IObservable<UnityWebRequest> LoginWithAuthCode(string authCode)
        {
            return GetAccesTokenFromAuthCode(authCode);
        }

        static public IObservable<UnityWebRequest> CallAPIAndLogin(string APIURL, string httpAcceptHeader = "application/json")
        {
            IObservable<UnityWebRequest> obs;
            //if token expired or no token available
            if (GetTokenInfo() == null || GetTokenInfo().refreshToken == null)
            {
                //TODO show message that user should login through menu (use observable error)
                var auth_code = "uZBRFQh19cCmRrTG+upQEA==";
                obs = GetAccesTokenFromAuthCode(auth_code);
            }
            else if (GetTokenInfo().accesToken == null || GetTokenInfo().expireDate < DateTime.Now)
            {
                obs = SaveAccesTokenFromRefreshToken(GetTokenInfo().refreshToken);
                //TODO subscribe to observable and notify user if response code is not 200
            }
            else
            {
                obs = Observable.Return<UnityWebRequest>(null);
            }
            return obs.SelectMany(_ => CallAPI(APIURL, GetTokenInfo().accesToken, httpAcceptHeader));

        }


        //method returns json body API call
        static IObservable<UnityWebRequest> CallAPI(string APIURL, string acces_code, string httpAcceptHeader)
        {
            var progressNotifier = new ScheduledNotifier<float>();
            progressNotifier.Subscribe(x => Debug.Log(x));
            UnityWebRequest www = UnityWebRequest.Get(APIURL);
            www.SetRequestHeader("Authorization", "Bearer " + acces_code);
            www.SetRequestHeader("Accept", httpAcceptHeader);
            return UniRXExtensions.GetWWW(www, progressNotifier).Select((webRequest) =>
             {
                 return webRequest;
             });
        }
        public static string BytesToString(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        public static IObservable<IList<IMKLPackage>> GetAllIMKLPackage()
        {
            return CallAPIAndLogin(WebService.allMapRequestAPIURL).Select(requests =>
                        JArray.Parse(BytesToString(requests.downloadHandler.data)))
                        .SelectMany(urls => urls.Select(urlToken => urlToken["MapRequest"].Value<string>()))
                        .SelectMany(url =>
                        {
                            return CallAPIAndLogin(url).Select(webrequest =>
                            {
                                var jsontext = WebService.BytesToString(webrequest.downloadHandler.data);
                                var jObj = Newtonsoft.Json.Linq.JObject.Parse(jsontext);
                                return new IMKLPackage(
                                jObj["LocalId"].ToObject<string>(),
                                jObj["Reference"].ToObject<string>(),
                                jObj["Status"].ToObject<string>(),
                                ParseMRZoneFromJObj(jObj),
                                EditIMKLURLForZIP(url)
                                );
                            });
                        }).ToList();
        }


        static IEnumerable<Vector2d> ParseMRZoneFromJObj(JObject mapRequest)
        {
            return mapRequest["MapRequestZone"]["coordinates"][0].Select(coords => coords.ToObject<double[]>())
            .Select(coord => new Vector2d(coord[0], coord[1]));

        }
        public static IObservable<List<XDocument>> DownloadXMLForIMKLPackage(IMKLPackage package)
        {
            Debug.Log("method called");
            //also return packages which are unavailable but are handled differently in gui
            if (package.DownloadIMKL)
            {
                return CallAPIAndLogin(package.ZIPUrl, "application/zip").Select(webrequest =>
                            {
                                //save KLBresponse
                                return IMKLExtractor.ExtractIMKLXML(webrequest.downloadHandler.data,
                                                package.ID).ToList();
                            });
            }
            return Observable.Return<List<XDocument>>(null);


        }
        public static string EditIMKLURLForZIP(string url)
        {
            string pattern = "v1/";
            string replacement = "v1/imkl/";
            Regex rgx = new Regex(pattern);
            return rgx.Replace(url, replacement);
        }




        //use refresh_token to ask for new acces token
        static IObservable<UnityWebRequest> SaveAccesTokenFromRefreshToken(string refreshToken)
        {
            Debug.Log("acces token request");
            WWWForm form = new WWWForm();
            form.AddField("client_id", "1030");
            form.AddField("client_secret", "+csxuC35huCwJokbdJBTWu9hrO0nX5G3");
            form.AddField("grant_type", "refresh_token");
            form.AddField("refresh_token", refreshToken);

            string url = "https://oauth.beta.agiv.be/authorization/ws/oauth/v2/token";
            UnityWebRequest www = UnityWebRequest.Post(url, form);


            return UniRXExtensions.GetWWW(www).Select((webrequest) =>
            {
                SaveAccesTokenFromBytes(webrequest.downloadHandler.data);
                return webrequest;
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
        static IObservable<UnityWebRequest> GetAccesTokenFromAuthCode(string code_authorization)
        {
            WWWForm form = new WWWForm();
            form.AddField("client_id", "1030");
            form.AddField("redirect_uri", "https://vianova.com");
            form.AddField("client_secret", "+csxuC35huCwJokbdJBTWu9hrO0nX5G3");
            form.AddField("grant_type", "authorization_code");
            form.AddField("code", code_authorization);

            string url = "https://oauth.beta.agiv.be/authorization/ws/oauth/v2/token";

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            return UniRXExtensions.GetWWW(www).Select((webrequest) =>
            {
                SaveAccesTokenFromBytes(webrequest.downloadHandler.data);
                return webrequest;
            });
        }
    }

}
