using System.Collections;
using System.Collections.Generic;
using System.IO;
using MoreLinq;
using UnityEngine;

namespace IO
{
    public static class IMKLExtractor
    {
        public static string GetSafeFileName(string name){
             return string.Join("_", name.Split(Path.GetInvalidFileNameChars())); 
        }
        static string zipPath = Application.temporaryCachePath + "/zip";
        static string unzipPath = Application.temporaryCachePath + "/unzip";
        static void unzipAllFiles(string zipPath, string exportPath)
        {
            ZipFile.UnZip(exportPath, File.ReadAllBytes(zipPath));
            var info = new DirectoryInfo(exportPath);
            var zipInfo = info.GetFiles("*.zip");
            int j = 0;
            if (zipInfo.Length > 0)
            {
                foreach (FileInfo f in zipInfo)
                {
                    //create new folder for each subzip file (because the sub zip are from different companies)
                    //and name uniqueness cannot be guaranteed
                    //unzipfiles in the same path but add id to unsure unique filename
                    //TODO only unzip if file doesn't already exist
                    unzipAllFiles(f.FullName, exportPath + '/' + j);
                    j++;

                }
            }
        }
        static void MoveAllExtractedXML(string exportPath, string xmlpath,string imklName)
        {
            var info = new DirectoryInfo(exportPath);
            //parse all xml
            //find all xmls in subfolders
            var xmlInfo = info.GetFiles("*.xml", SearchOption.AllDirectories);
            Debug.Log("moving " + xmlInfo.Length + " xml files.");
            //move to seperate folder
            xmlInfo.ForEach((f) =>
            {
                try
                {
                    File.Move(f.FullName, Path.Combine(xmlpath, imklName));
                }
                catch (IOException)
                {
                    //alert user
                    Debug.Log("XML already exists");
                }
            });
        }
        static void CheckDirectories(){
            Directory.CreateDirectory(zipPath);
            Directory.CreateDirectory(unzipPath);
        }
        public static void ExtractIMKL(byte[] zipData, string zipName,string imklName)
        {
            CheckDirectories();
            var zipFilePath = zipPath + "/" + zipName + ".zip";
            File.WriteAllBytes(zipFilePath, zipData);
            unzipAllFiles(zipFilePath, unzipPath);
            MoveAllExtractedXML(unzipPath, IMKLParser.xmlpath,imklName);
        }
    }

}
