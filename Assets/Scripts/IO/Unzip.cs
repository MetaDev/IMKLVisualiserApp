using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Linq;

using UniRx;
using Utility;
using System.Collections.Generic;
using IMKL_Logic;
using UnityEditor;

namespace IO
{
    public class Unzip : MonoBehaviour
    {
       

        public void UnzipAndRead()
        {
            string exportPath = Application.persistentDataPath + "/unzip";
           
			//create Directory if it doesn't exist yet
			
            clear(exportPath);
            Debug.Log("start");
            string zipPath;
            string url = "https://www.dropbox.com/s/iyhwc008sbb7pq3/test.zip?dl=1";


            zipPath = Application.temporaryCachePath + "/tempZip.zip";
            var progressNotifier = new ScheduledNotifier<float>();
            progressNotifier.Subscribe(x => UpdatePogressBar(x));
            IObservable<byte[]> download = ObservableWWW.GetAndGetBytes(url, progress: progressNotifier);
            // download.Select((data) =>
            // {
            //     File.WriteAllBytes(zipPath, data);
            //     unzipAllFiles(zipPath, exportPath);
            //     MoveAllExtractedXML(exportPath, IMKLParser.xmlpath);
            //     return parseAllXML(IMKLParser.xmlpath);
            // }).ObserveOnMainThread().Subscribe((drawInfo) => IMKL_Geometry.Draw(drawInfo));
            // //start download
            // download.Subscribe();


        }
        //TODO make UI to display download progress
        void UpdatePogressBar(float progress)
        {

        }
        void clear(string path)
        {
            if (Directory.Exists(path))
            {
                var info = new DirectoryInfo(path);
                foreach (FileInfo f in info.GetFiles())
                {
                    File.Delete(f.FullName);
                }
            }
        }
        void MoveAllExtractedXML(string exportPath, string xmlpath)
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
                    File.Move(f.FullName, Path.Combine(xmlpath, f.Name));
                }
                catch (IOException)
                {
                    //alert user
                    Debug.Log("XML already exists");
                }
            });
        }
        // public static IEnumerable<DrawElement> parseAllXML(string xmlpath)
        // {
        //     var info = new DirectoryInfo(xmlpath);
        //     //parse all xml
        //     //find all xmls in subfolders
        //     var xmlInfo = info.GetFiles("*.xml", SearchOption.AllDirectories);
        //     Debug.Log("parsing " + xmlInfo.Length + " xml files.");

        //     return IMKLParser.Parse(xmlInfo.Select(f => f.FullName));
        // }

        void unzipAllFiles(string zipPath, string exportPath)
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



    }
}
