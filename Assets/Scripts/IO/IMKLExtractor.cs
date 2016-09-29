using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using MoreLinq;
using UnityEngine;

namespace IO
{
    public static class IMKLExtractor
    {
        public static string GetSafeFileName(string name)
        {
            return string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
        }

       
        static void unzipAllFiles(string zipPath, string exportPath)
        {
            //make sure the export path exists
            Directory.CreateDirectory(exportPath);
            try
            {
                ZipFile.UnZip(exportPath, File.ReadAllBytes(zipPath));
            }
            catch (ZipException e)
            {
                Debug.Log("Something whent wrong when unzipping: " + e.Message);
                Debug.Log("zipPath " + zipPath);
                Debug.Log("exportpath" + exportPath);
            }
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
        // static void MoveAllExtractedXML(string exportPath, string xmlpath, string imklName)
        // {
        //     var info = new DirectoryInfo(exportPath);
        //     //parse all xml
        //     //find all xmls in subfolders
        //     var xmlInfo = info.GetFiles("*.xml", SearchOption.AllDirectories);
        //     Debug.Log("moving " + xmlInfo.Length + " xml files.");
        //     //move to seperate folder
        //     int i = 0;
        //     xmlInfo.ForEach((f) =>
        //     {
        //         try
        //         {
        //             File.Move(f.FullName, Path.Combine(xmlpath, imklName + "_" + i + ".xml"));
        //             i++;
        //         }
        //         catch (IOException)
        //         {
        //             //alert user
        //             Debug.Log("XML already exists, file name: " + Path.Combine(xmlpath, imklName + "_" + f.Name));

        //         }
        //     });
        // }
        static IEnumerable<XDocument> GetAllXDocuments(string unzipPath)
        {
            //TODO pass non parsable xmls to UI Warning message
            var info = new DirectoryInfo(unzipPath);
            var xmlInfo = info.GetFiles("*.xml", SearchOption.AllDirectories);
            return xmlInfo.Select(xmlFile =>
            {
                try
                {
                    return XDocument.Load(xmlFile.FullName);
                }
                catch (XmlException e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }).Where(xdoc => xdoc != null);
        }
        static void CreateTempDirectories(string zipPath, string unzipPath)
        {
            Directory.CreateDirectory(zipPath);
            Directory.CreateDirectory(unzipPath);
        }
        public static void ClearFolder(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static IEnumerable<XDocument> ExtractIMKLXML(byte[] zipData, string imklID)
        {
            string zipPath = Application.temporaryCachePath + "/zip";
            string unzipPath = Application.temporaryCachePath + "/unzip";
            ClearFolder(Application.temporaryCachePath);
            CreateTempDirectories(zipPath,unzipPath);
            var zipFilePath = zipPath + "/" + imklID + ".zip";
            File.WriteAllBytes(zipFilePath, zipData);
            unzipAllFiles(zipFilePath, unzipPath);
            return GetAllXDocuments(unzipPath);

        }
    }

}
