using UnityEngine;
using System.Xml.Linq;

//Needed for XML functionality


//Needed for XML Functionality

using System.Linq;

using IMKL_logic;
using Utility;
using System.Collections.Generic;
using System;
using UniRx;

namespace IO
{
    public static class IMKLParser
    {
        public static OnlineMaps map= OnlineMaps.instance;
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
        public static void Parse(IEnumerable<string> imklXMLFileNames)
        {
            var pointsDrawInfo = new List<Tuple<Vector2, Texture2D>>();
            var linesDrawInfo = new List<Tuple<IEnumerable<Vector2>, Color, IMKL_Geometry.LineStyle>>();
            foreach (String fileName in imklXMLFileNames)
            {
                XDocument doc = XDocument.Load(fileName);
                pointsDrawInfo.AddRange(ParsePoints(doc));
                linesDrawInfo.AddRange(ParseLines(doc));

            }
            //draw all geometry at the center of the scene
            var pointsPos = pointsDrawInfo.Select(point => point.Item1);
            Vector2 min = new Vector2(pointsPos.Min(v => v.x), pointsPos.Min(v => v.y));
            pointsDrawInfo.ForEach(pInfo => IMKL_Geometry.DrawPoint(pInfo.Item1 - min, pInfo.Item2));
            linesDrawInfo.ForEach(lInfo => IMKL_Geometry.DrawLineString(lInfo.Item1.Select(pos => pos - min), lInfo.Item2, lInfo.Item3));

            //set camera of scene to center of geometry
            Vector2 max = new Vector2(pointsPos.Max(v => v.x), pointsPos.Max(v => v.y));
            var relCenter = (max - min) / 2;

            //IMKL_Geometry.SetCamera(center, 50);
            var absCenter = (max+min)/2;
            Debug.Log(absCenter);
            Debug.Log(GEO.Lambert72_To_LatLon(absCenter));
            OnlineMapsLocationService.instance.position= GEO.Lambert72_To_LatLon(absCenter);
            
            double t;
            double k;
            map.GetPosition(out t,out k);
        }
        static IEnumerable<Tuple<Vector2, Texture2D>> ParsePoints(XDocument doc)
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
            //draw points
            List<Texture2D> textures = new List<Texture2D>();
            foreach (var point in points)
            {
                Texture2D tex = Resources.Load("icons/" + ((point.thema == "oilGasChemical" ? point.thema + "s" : point.thema).ToLowerInvariant()
                 + "_" + point.pointType
                 + (point.status == "functional" ? "" : "_" + point.status)).ToLowerInvariant(), typeof(Texture2D)) as Texture2D;
                //appurtenance is the default icon if not found
                if (tex == null)
                {
                    tex = Resources.Load("icons/" + ((point.thema == "oilGasChemical" ? point.thema + "s" : point.thema).ToLowerInvariant()
                + "_" + "appurtenance"
                + (point.status == "functional" ? "" : "_" + point.status)).ToLowerInvariant(), typeof(Texture2D)) as Texture2D;
                }
                //TODO properly handle unfound icons
                if (tex == null)
                {
                    Debug.Log("icon not found");
                }
                textures.Add(tex);
                //draw center pos
            }
            return points.Zip(textures, (point, tex) => Tuple.Create(point.pos, tex));
        }

        static IDictionary<string, string> lineColorMap = new Dictionary<string, string>(){
            {"electricity","#D73027"},
            {"oilgaschemical","#D957F9"},
            {"sewer","#8C510A"},
            {"telecommunications","#68BB1F"},
            {"thermal","#FFC000"},
            {"water","#2166AC"},
            {"crossTheme","#FEE08B"}
        };
        static IDictionary<string, IMKL_Geometry.LineStyle> lineStyleMap = new Dictionary<string, IMKL_Geometry.LineStyle>(){
            {"functional",IMKL_Geometry.LineStyle.FULL},
            {"projected",IMKL_Geometry.LineStyle.DASH},
            {"disused",IMKL_Geometry.LineStyle.DASHDOT}
        };
        static IEnumerable<Tuple<IEnumerable<Vector2>, Color, IMKL_Geometry.LineStyle>> ParseLines(XDocument doc)
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
            //color map
            return lines.Select(line => Tuple.Create(
                line.posList,
                Conversion.hexToColor(lineColorMap[line.thema]),
                lineStyleMap[line.status]));
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