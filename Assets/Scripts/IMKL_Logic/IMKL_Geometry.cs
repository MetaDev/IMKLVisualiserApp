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

        private static void OnChangeZoom()
        {
            // When the zoom changes you will see in the console new zoom.
            Debug.Log(OnlineMaps.instance.zoom);
        }



        public static void DrawPoint(Pos l72, string thema, string pointType, string status)
        {
            //convert L72 to lat lon and lat lon to unity
            Pos latlon = GEO.LambertToLatLong(l72);
          
            GameObject go = GetIconPrefab(thema, pointType, status);
           
            OnlineMapsControlBase3D control = OnlineMaps.instance.GetComponent<OnlineMapsControlBase3D>();

            if (control == null)
            {
                Debug.LogError("You must use the 3D control (Texture or Tileset).");
                return;
            }
            // Create 3D marker
            var marker3D = control.AddMarker3D(latlon, go);
            marker3D.scale = 100;

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
                go.transform.eulerAngles = new Vector3(270, 0, 0);
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

        public static void SetCamera(Vector2 point, float dist = 20)
        {

            Camera.main.transform.transform.position = point;
            Camera.main.transform.transform.position += new Vector3(0, dist, 0);
            Camera.main.transform.transform.LookAt(point);

        }


    }

}
