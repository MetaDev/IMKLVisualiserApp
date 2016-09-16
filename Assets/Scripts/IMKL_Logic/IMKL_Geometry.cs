using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;


namespace IMKL_logic
{
    public class IMKL_Geometry
    {
        static System.Random rnd = new System.Random();


        public static void DrawPoint(Vector2 pos, Texture2D tex)
        {


            GameObject go = new GameObject();
            go.name="point";
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            //go.transform.localScale = new Vector3(1, 1, 1);
            go.transform.position = new Vector3(pos.x, 0, pos.y);
            go.transform.eulerAngles= new Vector3(270,0,0);

        }
        public enum LineStyle
        {
            FULL, DASH, DASHDOT
        }
        static IDictionary<LineStyle, string> lineTileMap = new Dictionary<LineStyle, string>(){
            {LineStyle.DASHDOT,"dot_dash"},
            {LineStyle.DASH,"square_dash"}
        };
        public static void DrawLineString(IEnumerable<Vector2> posList, Color col, LineStyle style)
        {

            var linestring = new GameObject();
            linestring.name="line";
            LineRenderer lineRenderer = linestring.AddComponent<LineRenderer>();
            Vector3[] points = posList.Select(s => new Vector3(s.x, 0, s.y)).ToArray();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startColor = col;
            lineRenderer.endColor = col;
            lineRenderer.startWidth = 0.2F;
            lineRenderer.endWidth = 0.2F;
            lineRenderer.numPositions = (posList.Count());
            if (style != LineStyle.FULL)
            {
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.material=new Material(Shader.Find("Sprites/Default"));
                lineRenderer.material.mainTexture = Resources.Load("linestyles/"+lineTileMap[style]) as Texture2D;
                lineRenderer.material.SetColor("_TintColor",col);
                
            }
            lineRenderer.SetPositions(points);
        }


        public static void SetCamera(Vector2 point, float dist = 20)
        {

            Camera.main.transform.transform.position = point;
            Camera.main.transform.transform.position += new Vector3(0, dist, 0);
            Camera.main.transform.transform.LookAt(point);

        }


    }

}
