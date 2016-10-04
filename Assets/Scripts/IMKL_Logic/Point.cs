﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;
using System;

namespace IMKL_Logic
{

    public class Point : DrawElement
    {
        public enum Properties
        {
            THEMA, POINTTYPE, STATUS
        }
        float scale = 30;
        public Vector2d latlon
        {
            get;
            private set;
        }

        Dictionary<Properties, string> properties;
        // Use this for initialization
        public Point(Vector2d pos, Dictionary<Properties, string> properties)
        {
            //check properties
            if (!(properties.ContainsKey(Properties.THEMA) &&
            properties.ContainsKey(Properties.POINTTYPE) &&
            properties.ContainsKey(Properties.STATUS)))
            {
                Debug.Log("Point is missing poperties.");
                Debug.Log(string.Join(" ", properties.Select(kvp => kvp.ToString() + ":" + kvp.Value).ToArray()));
            }

            this.properties = properties;
            this.latlon = GEO.LambertToLatLong(pos);

        }
        public override string ToString()
        {
            return "properties: " + string.Join(" ", properties.Select(kvp => kvp.ToString()).ToArray()) + Environment.NewLine
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
        //draw is a seperate method because the creation of a point and it's actual drawing should be done on a seperate thread
        public override void Init()
        {
            OnlineMapsControlBase3D control = OnlineMaps.instance.GetComponent<OnlineMapsControlBase3D>();
            GameObject prefab = null;
            try
            {
                prefab = GetIconPrefab(properties[Properties.THEMA],
          properties[Properties.POINTTYPE], properties[Properties.STATUS]);
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
        }

    }

}
