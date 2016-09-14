using UnityEngine;
using System.Xml.Linq;

//Needed for XML functionality


//Needed for XML Functionality

using System.Linq;

using IMKL_logic;
using Utility;
using System.Collections.Generic;
using System;


namespace IO
{

    public static class IMKLParser
    {
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
        public static void Parse(string fileName)
        {
            XDocument doc = XDocument.Load(fileName);
            //TODO show to chris
            //load all namespaces, works but gives errors when used in method with LINQ 
            // var imklNamespaces = doc.Root.Attributes().
            //     Where(a => a.IsNamespaceDeclaration).
            //     GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
            //         a => XNamespace.Get(a.Value)).
            //     ToDictionary(g => g.Key,
            //         g => g.First());

            //draw only the selected elements

            //TODO catch parsing errors
            var pointsPos = from point in doc.Descendants().Where(ePoint => pointsToDraw.Contains(ePoint.Name.LocalName))
                            select StringParser.parsePos(point.DescendantsByLocalName("pos").Single().Value);
            //translate point to origin of scene
            Vector2 min = new Vector2(pointsPos.Min(v => v.x), pointsPos.Min(v => v.y));
            pointsPos.Select(pos => pos - min).ForEach(pos => IMKL_Geometry.DrawPoint(pos, 0));

            IMKL_Geometry.SetCamera(doc.Descendants().Where(e => pointsToDraw.Contains(e.Name.LocalName))
                 .Select(e =>
                   StringParser.parsePos(e.DescendantsByLocalName("pos").First().Value)
                 ));
            //TODO show to Chris Debug.Log(imklNamespaces["gml"]) -> error but correct output
            
            var linesPoslists = from linkInLine in doc.Descendants().Where(eLine => linesToDraw.Contains(eLine.Name.LocalName))
            																						.DescendantsByLocalName("link")
                                join link in doc.DescendantsByLocalName("UtilityLink")
                                on linkInLine.AttributeByLocalName("href").Value.Split(':').Last() equals
                                link.DescendantsByLocalName("localId").Single().Value
                                select StringParser.parsePosList(link.DescendantsByLocalName("posList").Single().Value);
            linesPoslists.Select(posList => posList.Select(pos => pos - min)).ForEach(poslist => IMKL_Geometry.DrawLineString(poslist, 0));


        }
        static IEnumerable<XElement> DescendantsByLocalName(this XContainer root, string localName)
        {
            return root.Descendants().Where(e => e.Name.LocalName == localName);
        }
        public static IEnumerable<XElement> DescendantsByLocalName<T>(this IEnumerable<T> root, string localName) where T : XContainer
        {
            return root.Descendants().Where(e => e.Name.LocalName == localName);
        }
		public static XAttribute AttributeByLocalName(this XElement el, string localName){
			return el.Attributes().Where(a => a.Name.LocalName == localName).First();
		}

    }

}