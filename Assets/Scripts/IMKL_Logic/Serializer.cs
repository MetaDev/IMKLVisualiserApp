using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IMKL_Logic;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using System.Linq;
using MoreLinq;
namespace IO
{
    public static class Serializer
    {

        static string serialisationPath = Application.persistentDataPath;
        static TokenInfo _tokenInfo;

        public static void SaveToken(TokenInfo tokenInfo)
        {
            SerialiseObject(tokenInfo, TokenInfoFileName);
            _tokenInfo = tokenInfo;
        }
        public static TokenInfo LoadToken()
        {
            if (_tokenInfo == null)
            {
                _tokenInfo = DeserialiseObject<TokenInfo>(TokenInfoFileName);
            }
            return _tokenInfo;
        }
        static string PackagesFileName =  "IMKLPackages.dat";
        static string TokenInfoFileName =  "TokenInfo.dat";
       public static void DeleteStoredPackages(){
            File.Delete(Path.Combine(serialisationPath,PackagesFileName));
        }
        static IEnumerable<IMKLPackage> _IMKLPackages = new List<IMKLPackage>().ToReactiveCollection();

        public static void SaveIMKLPackages(IEnumerable<IMKLPackage> packages)
        {
            AddPackages(packages);
            Debug.Log(_IMKLPackages.Count());
            SerialiseObject(_IMKLPackages, PackagesFileName);
        }
        public static IEnumerable<IMKLPackage> LoadAllIMKLPackages()
        {
            if (_IMKLPackages.Count() == 0)
            {
                AddPackages(DeserialiseObject<IEnumerable<IMKLPackage>>(PackagesFileName));
            }
            return _IMKLPackages;
        }
        static void AddPackages(IEnumerable<IMKLPackage> packages)
        {
            if (packages != null)
            {
                _IMKLPackages = _IMKLPackages.Concat(packages).DistinctBy(package=>package.ID);
            }
        }
        public static IObservable<IEnumerable<IMKLPackage>> PackagesChanged()
        {
            return Serializer._IMKLPackages.ObserveEveryValueChanged(_ => Serializer._IMKLPackages);
        }

        static T DeserialiseObject<T>(string fileName)
        {
            if (!File.Exists(Path.Combine(serialisationPath, fileName)))
            {
                return default(T);
            }
            using (StreamReader sr = new StreamReader(Path.Combine(serialisationPath, fileName)))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return (JsonSerializer.Create(new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                                                    .Deserialize<T>(reader));
            }
        }
        static void SerialiseObject<T>(T obj, string fileName)
        {
            if (!File.Exists(serialisationPath + fileName))
            {
                File.Create(serialisationPath + fileName);
            }
            using (StreamWriter sw = new StreamWriter(Path.Combine(serialisationPath, fileName)))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                JsonSerializer.Create(new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                                                    .Serialize(writer, obj);
            }
        }

        //return all serialized IMKL packages


    }

}
