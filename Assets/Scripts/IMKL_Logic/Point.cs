using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;
using System;

namespace IMKL_Logic
{

    public class Point : DrawElement
    {

        float scale = 30;
        public Vector2d latlon
        {
            get;
            private set;
        }
        public enum VisualisedProperties
        {
            THEMA, POINTTYPE, STATUS
        }
        public static Dictionary<VisualisedProperties, string> VisualisedPropertyMap = new Dictionary<VisualisedProperties, string>(){
            {VisualisedProperties.THEMA,VisualisedProperties.THEMA.ToString().ToLowerInvariant()},
            {VisualisedProperties.POINTTYPE,VisualisedProperties.POINTTYPE.ToString().ToLowerInvariant()},
            {VisualisedProperties.STATUS,VisualisedProperties.STATUS.ToString().ToLowerInvariant()}
        };
        string PointType;
        string Thema;
        string Status;
        // Use this for initialization
        public Point(Vector2d pos, string pointType, string thema, string status, Dictionary<string, string> properties) : base(properties)
        {
            //check properties
            PointType = pointType;
            Thema = thema;
            Status = status;
            this.latlon = GEO.LambertToLatLong(pos);

        }
        public override string ToString()
        {
            return "properties: " + string.Join(" ", Properties.Select(kvp => kvp.ToString()).ToArray()) + Environment.NewLine
                + "Position: " + latlon.ToString();
        }
        public Vector2d GetLatLon()
        {
            return this.latlon;
        }
        // public Point(Pos latlon, GameOb)

        static IDictionary<string, GameObject> prefabIcons = new Dictionary<string, GameObject>();
        private static GameObject GetIconPrefab(string thema, string pointType, string status)
        {

            //go.transform.localScale=new Vector3(10,10,1);
            string name = (thema == "oilGasChemical" ? thema + "s" : thema).ToLowerInvariant()
                 + "_" + pointType
                 + (status == "functional" ? "" : "_" + status).ToLowerInvariant();
            if (!prefabIcons.ContainsKey(name))
            {
                GameObject go = new GameObject();
                go.name = "point";
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                go.transform.eulerAngles = new Vector3(180, 0, 0);
                go.transform.localScale = new Vector3(100, 100, 0);
                Texture2D tex = Resources.Load("icons/" + name, typeof(Texture2D)) as Texture2D;
                //appurtenance is the default icon if not found
                if (tex == null)
                {
                    var default_name = ((thema == "oilGasChemical" ? thema + "s" : thema).ToLowerInvariant()
                + "_" + "appurtenance"
                + (status == "functional" ? "" : "_" + status)).ToLowerInvariant();
                    tex = Resources.Load("icons/" + default_name, typeof(Texture2D)) as Texture2D;
                }
                //TODO properly handle unfound icons
                if (tex == null)
                {
                    Debug.Log("icon not found");
                }
                renderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

                prefabIcons.Add(name, go);
            }
            return prefabIcons[name];

        }
        protected override bool ClickWithinDistance(Vector3 mouseWorldPos, float maxDist)
        {
            if (GO != null)
            {
                return false;
            }
            return Vector3.Distance(mouseWorldPos, GO.transform.position) < maxDist;
        }
        //draw is a seperate method because the creation of a point and it's actual drawing should be done on a seperate thread
        public override void Init()
        {
            OnlineMapsControlBase3D control = OnlineMaps.instance.GetComponent<OnlineMapsControlBase3D>();
            GameObject prefab = null;
            try
            {
                prefab = GetIconPrefab(Thema, PointType, Status);

            }
            catch (KeyNotFoundException e)
            {
                Debug.Log("The initiated point is missing some properties.");
            }


            if (control == null)
            {
                Debug.LogError("You must use the 3D control (Texture or Tileset).");
                return;
            }
            // Create 3D marker, x lon y lat
            //the game object is a child of map in the scene
            var marker3D = control.AddMarker3D(latlon, prefab);
            marker3D.scale = scale;
            marker3D.range = DrawElement.DrawRange;
            GO = marker3D.instance;
        }

        public override string GetTextForPropertiesPanel()
        {
            return Thema;
        }
    }

}
