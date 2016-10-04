using UnityEngine;
using System.Xml.Linq;

//Needed for XML functionality


//Needed for XML Functionality

using System.Linq;

using IMKL_Logic;
using Utility;
using System.Collections.Generic;
using System;
using UniRx;
using System.IO;
using MoreLinq;
using System.Text;
using System.Xml;

namespace IO
{
    public static class IMKLParser
    {
        //    public static string xmlpath = "/Users/Harald/Downloads" + "/imkl_xmls";

        // public static IEnumerable<FileInfo> GetAllXMLFiles()
        // {
        //     var info = new DirectoryInfo(xmlpath);

        //     return info.GetFiles("*.xml", SearchOption.AllDirectories);

        // }
        public static OnlineMaps map = OnlineMaps.instance;
        static HashSet<string> linesToDraw = new HashSet<string>(new string[] {
            "ElectricityCable",
            "TelecommunicationsCable",
            "OilGasChemicalsPipe",
            "SewerPipe",
            "WaterPipe",
            "ThermalPipe",
            "Duct",
            "Pipe"
        });
        static HashSet<string> pointsToDraw = new HashSet<string>(new string[] {
            "Appurtenance",
            "Tower",
            "Pole",
            "Manhole",
            "Cabinet"
        });
        //private static Dictionary<string, XNamespace> imklNamespaces;
        public static IEnumerable<DrawElement> ParseDrawElements(IEnumerable<XDocument> KLBResponses)
        {
            try
            {
                Debug.Log(KLBResponses.Count());
                //ToList is necessary because the lists are lazely evaluated 
                return KLBResponses.Where(xdoc=>xdoc!=null).SelectMany(xdoc =>
                {
                    return ParsePoints(xdoc).Concat(ParseLines(xdoc));
                }).ToList();
            }
            //TODO properly catch xml parse exceptions
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }

        }

        static IEnumerable<DrawElement> ParsePoints(XDocument doc)
        {

            //parse all point with their position and necessary properties
            //Appurtenance is a special case, it has a specific subtype
            var points = from point in doc.Descendants().Where(ePoint => pointsToDraw.Contains(ePoint.Name.LocalName))
                         join network in doc.DescendantsByLocalName("UtilityNetwork")
                         on point.DescendantsByLocalName("inNetwork").Single().AttributeByLocalName("href").Value.Split(':').Last() equals
                         network.DescendantsByLocalName("localId").Single().Value
                         select new
                         {
                             pointType = point.Name.LocalName == "Appurtenance" ?
                             point.DescendantsByLocalName("appurtenanceType").Single().AttributeByLocalName("href").Value.Split('/').Last() :
                             point.Name.LocalName,
                             pos = StringParser.parsePos(point.DescendantsByLocalName("pos").Single().Value),
                             thema = network.DescendantsByLocalName("utilityNetworkType").Single().AttributeByLocalName("href")
                                                                        .Value.Split('/').Last(),
                             status = point.DescendantsByLocalName("currentStatus").Single().AttributeByLocalName("href").Value.Split('/').Last()

                         };
            return points.Select(point => (DrawElement)new Point(point.pos,
                        new Dictionary<Point.Properties, string>(){
                            {Point.Properties.THEMA,point.thema},
                            {Point.Properties.POINTTYPE,point.pointType},
                            {Point.Properties.STATUS,point.status}
                        })
                        );
        }
        public static string GetKLBResponseID(XDocument KLBResponse)
        {
            return KLBResponse.DescendantsByLocalName("FeatureCollection").First().AttributeByLocalName("id").Value;
        }


        static IEnumerable<DrawElement> ParseLines(XDocument doc)
        {
            var lines = from line in doc.Descendants().Where(eLine => linesToDraw.Contains(eLine.Name.LocalName))
                        from linkInLine in line.DescendantsByLocalName("link")
                        join link in doc.DescendantsByLocalName("UtilityLink")
                        on linkInLine.AttributeByLocalName("href").Value.Split(':').Last()
                        equals link.DescendantsByLocalName("localId").Single().Value

                        join network in doc.DescendantsByLocalName("UtilityNetwork")
                        on link.DescendantsByLocalName("inNetwork").Single().AttributeByLocalName("href").Value.Split(':').Last()
                        equals network.DescendantsByLocalName("localId").Single().Value
                        select new
                        {
                            posList = StringParser.parsePosList(link.DescendantsByLocalName("posList").Single().Value),
                            thema = network.DescendantsByLocalName("utilityNetworkType").Single().AttributeByLocalName("href")
                                                                        .Value.Split('/').Last().ToLowerInvariant(),
                            status = line.DescendantsByLocalName("currentStatus").Single().AttributeByLocalName("href").Value.Split('/').Last()

                        };


            return lines.Select(line => (DrawElement)new Line(line.posList,
                            new Dictionary<Line.Properties, string>(){
                                        {Line.Properties.THEMA,line.thema},
                                        {Line.Properties.STATUS,line.status}
                            }
                            ));
        }

        static IEnumerable<XElement> DescendantsByLocalName(this XContainer root, string localName)
        {
            return root.Descendants().Where(e => e.Name.LocalName == localName);
        }
        public static IEnumerable<XElement> DescendantsByLocalName<T>(this IEnumerable<T> root, string localName) where T : XContainer
        {
            return root.Descendants().Where(e => e.Name.LocalName == localName);
        }
        public static XAttribute AttributeByLocalName(this XElement el, string localName)
        {
            return el.Attributes().Where(a => a.Name.LocalName == localName).First();
        }

    }

}