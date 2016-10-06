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
        public static IEnumerable<string> ExtractIMKLXML(byte[] zipData)
        {
            try
            {
                return ZipHelper.ExtractFilesFromRecursiveZipData(zipData, ".xml").Select(bytes =>
                 {
                     using (MemoryStream ms = new MemoryStream(bytes))
                     {
                         using (var sr = new StreamReader(ms, true))
                         {
                             return sr.ReadToEnd();
                         }
                     }
                 }).ToList();
            }
            catch (Ionic.Zip.ZipException e)
            {
                GUIFactory.instance.MyModalWindow.Show("Something whent wrong when unzipping package: " + e.Message, true);
                Debug.Log("Something whent wrong when unzipping: " + e.Message);
                return null;
            }

        }
    }

}
