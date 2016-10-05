using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
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


        static void recursiveZip(string zipPath, string exportPath)
        {
            Debug.Log("zippath" + zipPath + " exportpath: " + exportPath);

            Directory.CreateDirectory(exportPath);
           try
            {
                ZipHelper.UnZip(zipPath,exportPath);
            }
            catch (Ionic.Zip.ZipException e)
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
                    Debug.Log("zip files" + f.FullName);
                    recursiveZip(f.FullName, exportPath + '/' + j);
                    j++;

                }
            }
        }

        static List<string> GetAllXDocuments(string unzipPath)
        {
            //TODO pass non parsable xmls to UI Warning message
            var info = new DirectoryInfo(unzipPath);
            var xmlInfo = info.GetFiles("*.xml", SearchOption.AllDirectories);
            Debug.Log("nurmber of xml files" + xmlInfo.Count());
            return xmlInfo.Select(xmlFile =>
            {
                try
                {
                    return XDocument.Load(xmlFile.FullName).ToString();
                }
                catch (XmlException e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }).Where(xdoc => xdoc != null).ToList();
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

        public static List<string> ExtractIMKLXML(byte[] zipData, string imklID)
        {
            string zipPath = Application.temporaryCachePath + "/zip";
            string unzipPath = Application.temporaryCachePath + "/unzip";
            // string zipPath = "/Users/Harald/Downloads" + "/zip";
            // string unzipPath = "/Users/Harald/Downloads" + "/unzip";
            ClearFolder(Application.temporaryCachePath);
            CreateTempDirectories(zipPath, unzipPath);
            var zipFilePath = zipPath + "/" + imklID + ".zip";
            File.WriteAllBytes(zipFilePath, zipData);
            recursiveZip(zipFilePath, unzipPath);
            return GetAllXDocuments(unzipPath);

        }
    }

}
