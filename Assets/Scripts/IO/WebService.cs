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
    public class TokenInfo
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




        static string allMapRequestAPIURL = "https://klip.beta.agiv.be/api/ws/klip/v1/MapRequest/Mri";
        public static IObservable<UnityWebRequest> LoginWithAuthCode(string authCode)
        {
            return LoadAccesTokenFromAuthCode(authCode);
        }

        static public IObservable<UnityWebRequest> CallAPIAndLogin(string APIURL, string httpAcceptHeader = "application/json")
        {
            IObservable<UnityWebRequest> obs;
            //if token expired or no token available
            if (Serializer.LoadToken() == null || Serializer.LoadToken().refreshToken == null)
            {
                obs = Observable.Throw<UnityWebRequest>(new Exception("Application not logged in."));
            }
            else if (Serializer.LoadToken().accesToken == null || Serializer.LoadToken().expireDate < DateTime.Now)
            {
                obs = LoadAccesTokenFromRefreshToken(Serializer.LoadToken().refreshToken);
            }
            else
            {
                obs = Observable.Return<UnityWebRequest>(null);
            }
            return obs.SelectMany(request => CallAPI(APIURL, Serializer.LoadToken().accesToken, httpAcceptHeader))
            .Do(request =>
            {
                if (request != null && request.responseCode != 200)
                {

                    Observable.Throw<UnityWebRequest>(
                         new Exception("api call failed, error" + request.error + " " + request.downloadHandler.text +
                         ", response code " + request.responseCode));
                }
            });

        }


        //method returns json body API call
        static IObservable<UnityWebRequest> CallAPI(string APIURL, string acces_code, string httpAcceptHeader)
        {
            UnityWebRequest www = UnityWebRequest.Get(APIURL);
            www.SetRequestHeader("Authorization", "Bearer " + acces_code);
            www.SetRequestHeader("Accept", httpAcceptHeader);
            return UniRXExtensions.GetWWW(www).Select((webRequest) =>
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
        public static IObservable<List<string>> DownloadXMLForIMKLPackage(IMKLPackage package)
        {
            //also return packages which are unavailable but are handled differently in gui
            if (package.DownloadIMKL)
            {
                return CallAPIAndLogin(package.ZIPUrl, "application/zip").Select(webrequest =>
                            {
                                //save KLBresponse
                                try
                                {
                                    return IMKLExtractor.ExtractIMKLXML(webrequest.downloadHandler.data).ToList();
                                }catch(Exception e) {
                                    Observable.Throw<List<string>>(e);
                                    return null;
                                }
                            });
            }
            return Observable.Return<List<string>>(null);


        }
        public static string EditIMKLURLForZIP(string url)
        {
            string pattern = "v1/";
            string replacement = "v1/imkl/";
            Regex rgx = new Regex(pattern);
            return rgx.Replace(url, replacement);
        }




        //use refresh_token to ask for new acces token
        static IObservable<UnityWebRequest> LoadAccesTokenFromRefreshToken(string refreshToken)
        {
            WWWForm form = new WWWForm();
            form.AddField("client_id", _clientId);
            form.AddField("client_secret", _clientSecret);
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
            Serializer.SaveToken(new TokenInfo(access_token, expireDate, refresh_token));
        }
        //return empty string when complete because the authorization process cannot be done stateless
        static IObservable<UnityWebRequest> LoadAccesTokenFromAuthCode(string code_authorization)
        {
            WWWForm form = new WWWForm();
            form.AddField("client_id", _clientId);
            form.AddField("redirect_uri", _redirectUri);
            form.AddField("client_secret", _clientSecret);
            form.AddField("grant_type", "authorization_code");
            form.AddField("code", code_authorization);

            string url = "https://oauth.beta.agiv.be/authorization/ws/oauth/v2/token";
            UnityWebRequest www = UnityWebRequest.Post(url, form);
            return UniRXExtensions.GetWWW(www).Select((webrequest) =>
            {
                if (www.responseCode == 200)
                {
                    SaveAccesTokenFromBytes(webrequest.downloadHandler.data);
                }
                else
                {
                    var message = @"Something went wrong when trying to log in with the authorization code.
                     got error: " + www.error + " " + webrequest.downloadHandler.text + ", with code " + www.responseCode;
                    throw new Exception(message);
                }
                return webrequest;
            });
        }
    }

}
