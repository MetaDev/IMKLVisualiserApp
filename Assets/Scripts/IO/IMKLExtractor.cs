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
    }

}
