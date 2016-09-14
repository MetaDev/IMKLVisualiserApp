using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Security.Policy;
using UniRx;
using Utility;

namespace IO
{
	public class Unzip : MonoBehaviour
	{
		public bool online = false;
		//doc
		//https://github.com/tsubaki/UnityZip/tree/Sample

//		string exportPath = "/Users/Harald/Cloud Workspace/Professional/Vianova"+ "/unzip";

		public void UnzipAndRead ()
		{
			string exportPath = Application.persistentDataPath + "/unzip";
			clear (exportPath);
			Debug.Log ("start");
			string zipPath;
			if (online) {
				//WARNING: the url should be a direct download link dl=1 in dropbox
				//TODO add progress notifier to downloader
				string url = "https://www.dropbox.com/s/iyhwc008sbb7pq3/test.zip?dl=1";
				zipPath = Application.temporaryCachePath + "/tempZip.zip";
				IObservable<byte[]> download = UniRXExtensions.GetWWW (url);
				download.Do ((data) => {
					File.WriteAllBytes (zipPath, data);
					unzipAllFiles (zipPath, exportPath);
					parseAllXML (exportPath);
				}).ObserveOnMainThread ().Subscribe ();

			} else {
				Observable.Start (() => {
					zipPath = "/Users/Harald/Cloud Workspace/Professional/Vianova/IMKLVisualiserApp/Assets/Resources/test.zip";
					Debug.Log ("zipping");
					unzipAllFiles (zipPath, exportPath);

				}).ObserveOnMainThread ().Subscribe ((t) => parseAllXML (exportPath));
			}
		}

		void clear(string path){
			if (Directory.Exists (path)) {
				var info = new DirectoryInfo (path);
				foreach (FileInfo f in info.GetFiles ()) {
					File.Delete (f.FullName);
				}
			}

				
		}
		void parseAllXML (string exportPath)
		{
			var info = new DirectoryInfo (exportPath);

			//parse all xml
			//find all xmls in subfolders
			var xmlInfo = info.GetFiles ("*.xml", SearchOption.AllDirectories);

			Debug.Log ("parsing " + xmlInfo.Length + " xml files.");
			if (xmlInfo.Length > 0) {
				foreach (FileInfo f in xmlInfo) {
					IMKLParser.Parse (f.FullName);
				}
			}

		}

		void unzipAllFiles (string zipPath, string exportPath)
		{
			ZipUtil.Unzip (zipPath, exportPath);
			var info = new DirectoryInfo (exportPath);
			var zipInfo = info.GetFiles ("*.zip");
			int j = 0;
			if (zipInfo.Length > 0) {
				
				foreach (FileInfo f in zipInfo) {					
					//create new folder for each subzip file (because the sub zip are from different companies)
					//and name uniqueness cannot be guaranteed
					//unzipfiles in the same path but add id to unsure unique filename
					unzipAllFiles (f.FullName, exportPath + '/' + j);

					j++;

				}
			}


		}



	}
}
