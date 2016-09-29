using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using IMKL_Logic;
using UnityEngine;
namespace IO
{
    public static class Serializer
    {
        //return all serialized IMKL packages
        static IEnumerable<IMKLPackage> packages;
        public static IEnumerable<IMKLPackage> LoadAllIMKLPackages()
        {
            if (packages == null)
            {
                if (File.Exists(IMKLPackagesPath))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream saveFile = File.Open(IMKLPackagesPath, FileMode.Open);
                    try
                    {
                        packages = (IEnumerable<IMKLPackage>)formatter.Deserialize(saveFile);
                    }
                    catch (TypeLoadException e)
                    {
                        Debug.Log("packages not found");
                    }

                    saveFile.Close();
                }
            }
            return packages;
        }
        static string IMKLPackagesPath = Application.persistentDataPath + "/IMKLPackages.dat";
        public static void SaveAllIMKLPackages(IEnumerable<IMKLPackage> packages)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            //creates or overwrites file
            FileStream saveFile = File.Create(IMKLPackagesPath);

            formatter.Serialize(saveFile, packages);
            saveFile.Close();
        }

    }

}
