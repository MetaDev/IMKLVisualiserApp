using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using UnityEditor;
using Utility;
namespace IMKL_logic
{
    public class IMKL_Geometry
    {
        static System.Random rnd = new System.Random();

        static List<Tuple<GameObject, Pos>> geometryPos = new List<Tuple<GameObject, Pos>>();
        private static void OnChangePosition()
        {
            // When the position changes you will see in the console new map coordinates.
            foreach (var gp in geometryPos)
            {
                gp.Item1.transform.position = OnlineMapsTileSetControl.instance.GetWorldPosition(gp.Item2);
            }
        }

        private static void OnChangeZoom()
        {
            // When the zoom changes you will see in the console new zoom.
            Debug.Log(OnlineMaps.instance.zoom);
        }
        static bool hookToMap = false;
        static void HookToMap()
        {
            if (!hookToMap)
            {
                // Subscribe to change position event.
                OnlineMaps.instance.OnChangePosition += OnChangePosition;

                // Subscribe to change zoom event.
                OnlineMaps.instance.OnChangeZoom += OnChangeZoom;
                hookToMap=true;
            }

        }

       
        public static void DrawPoint(Pos l72, string thema, string pointType, string status)
        {
            HookToMap();
            //convert L72 to lat lon and lat lon to unity
            var temp = GEO.LBToLL.LambertToLatLong(l72);
            Pos latlon = new Pos(temp.y,temp.x);
            Vector3 pos = OnlineMapsTileSetControl.instance.GetWorldPosition(latlon);
            //create object in the scene
            GameObject go = new GameObject();
            go.name = "point";
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            go.transform.eulerAngles = new Vector3(270, 0, 0);
            go.transform.position = pos;
            Texture2D tex = GetTexture(thema, pointType, status);
            renderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            geometryPos.Add(Tuple.Create(go, latlon));

        }
        public enum LineStyle
        {
            FULL, DASH, DASHDOT
        }
        static IDictionary<LineStyle, string> lineTileMap = new Dictionary<LineStyle, string>(){
            {LineStyle.DASHDOT,"dot_dash"},
            {LineStyle.DASH,"square_dash"}
        };
        public static void DrawLineString(IEnumerable<Pos> posList, Color col, LineStyle style)
        {
            HookToMap();
            var linestring = new GameObject();
            linestring.name = "line";
            LineRenderer lineRenderer = linestring.AddComponent<LineRenderer>();
            Vector3[] points = posList.Select(s => new Vector3((float)s.x, 0, (float)s.y)).ToArray();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startColor = col;
            lineRenderer.endColor = col;
            lineRenderer.startWidth = 0.2F;
            lineRenderer.endWidth = 0.2F;
            lineRenderer.numPositions = (posList.Count());
            if (style != LineStyle.FULL)
            {
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.material.mainTexture = Resources.Load("linestyles/" + lineTileMap[style]) as Texture2D;
                lineRenderer.material.SetColor("_TintColor", col);

            }
            lineRenderer.SetPositions(points);
        }
        static IDictionary<string, Texture2D> prefabIcons = new Dictionary<string, Texture2D>();
        private static Texture2D GetTexture(string thema, string pointType, string status)
        {
            string name = (thema == "oilGasChemical" ? thema + "s" : thema).ToLowerInvariant()
                 + "_" + pointType
                 + (status == "functional" ? "" : "_" + status).ToLowerInvariant();
            if (!prefabIcons.ContainsKey(name))
            {
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
                prefabIcons.Add(name, tex);
            }
            return prefabIcons[name];

        }

        public static void SetCamera(Vector2 point, float dist = 20)
        {

            Camera.main.transform.transform.position = point;
            Camera.main.transform.transform.position += new Vector3(0, dist, 0);
            Camera.main.transform.transform.LookAt(point);

        }


    }

}
