using UnityEngine;
using System.Collections;
using System.Xml.Linq;
using System.Xml;

//Needed for XML functionality

using System.Xml.Serialization;

//Needed for XML Functionality

using System.IO;
using System.Linq;

using IMKL_logic;
using Utility;
using System.Collections.Generic;
using System;


namespace IO
{

	public static class KLIPParser
	{
		static HashSet<string> drawLines = new HashSet<string> (new string[] {
			"ElectricityCable",
			"TelecommunicationsCable",
			"OilGasChemicalsPipe",
			"SewerPipe",
			"WaterPipe",
			"ThermalPipe",
			"Duct",
			"Pipe"

		});
		static HashSet<string> drawPoints = new HashSet <string> (new string[] {
			"Appurtenance",
			"Tower",
			"Pole",
			"Manhole",
			"Cabinet"
		});
		static XNamespace NS_GML = "http://www.opengis.net/gml/3.2";
		static XNamespace NS_IMKL = "http://mir.agiv.be/cl/AGIV/v1/xmlns/IMKL2.2";

		public static void Parse (string fileName)
		{ 
			XDocument doc = XDocument.Load (fileName);

			//draw only the render elements
			
			DrawPoint (doc.Descendants ().Where (e => drawPoints.Contains (e.Name.LocalName)));

			IMKL_Geometry.SetCamera (doc.Descendants ().Where (e => drawPoints.Contains (e.Name.LocalName))
				.Select ( e => 
					StringParser.parsePos (e.Descendants (NS_GML + "pos").First ().Value)
				));

		}
		static void DrawLine(IEnumerable<XElement> elements){
			//				var poslist = e.Descendants (NS_GML + "poslist").DefaultIfEmpty (null).Single ();
			//else if (poslist!= null){
//			IMKL_Geometry.DrawLineString (StringParser.parsePosList (poslist.Value), 0);
		
		}
		static void DrawPoint (IEnumerable<XElement> elements)
		{
			elements.ForEach (e => e.Descendants (NS_GML + "pos").ForEach (pos =>
				IMKL_Geometry.DrawPoint (StringParser.parsePos (pos.Value), 0)));
		}
	}

}