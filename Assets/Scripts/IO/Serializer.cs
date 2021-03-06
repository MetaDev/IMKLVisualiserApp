﻿using System;
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
        static TokenInfo _TokenInfo;

        public static void SaveToken(TokenInfo tokenInfo)
        {
            SerialiseObject(tokenInfo, TokenInfoFileName);
            _TokenInfo = tokenInfo;
        }
        public static TokenInfo LoadToken()
        {
            if (_TokenInfo == null)
            {
                _TokenInfo = DeserialiseObject<TokenInfo>(TokenInfoFileName);
            }
            return _TokenInfo;
        }
        static string PackagesFileName = "IMKLPackages.dat";
        static string TokenInfoFileName = "TokenInfo.dat";
        public static void DeletePackages(IEnumerable<IMKLPackage> toDeletePackages)
        {
            var packageIDs = toDeletePackages.Select(delPackage => delPackage.ID);
            SetPackages(_IMKLPackages.Where(package => !packageIDs.Contains(package.ID)));
        }
        static IEnumerable<IMKLPackage> _IMKLPackages = new List<IMKLPackage>();

        public static void SaveAdditionalIMKLPackages(IEnumerable<IMKLPackage> packages)
        {
            AddPackages(packages);
            SerialiseObject(_IMKLPackages, PackagesFileName);
        }
        static void SetPackages(IEnumerable<IMKLPackage> packages){
            _IMKLPackages =packages;
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
                _IMKLPackages = _IMKLPackages.Concat(packages).DistinctBy(package => package.ID);
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
