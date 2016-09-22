using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;
namespace IMKL_Logic
{
    public class Line : DrawElement
    {

        IEnumerable<Pos> latLonPos;
        LineRenderer lineRenderer;
        float width = 0.2F;
        public enum Properties
        {
            THEMA, STATUS
        }
        Dictionary<Properties, string> properties;

        static IDictionary<string, string> lineColorMap = new Dictionary<string, string>(){
            {"electricity","#D73027"},
            {"oilgaschemical","#D957F9"},
            {"sewer","#8C510A"},
            {"telecommunications","#68BB1F"},
            {"thermal","#FFC000"},
            {"water","#2166AC"},
            {"crossTheme","#FEE08B"}
        };
        static IDictionary<string, LineStyle> lineStyleMap = new Dictionary<string, LineStyle>(){
            {"functional",LineStyle.FULL},
            {"projected",LineStyle.DASH},
            {"disused",LineStyle.DASHDOT}
        };
        static IDictionary<LineStyle, string> styleTextureMap = new Dictionary<LineStyle, string>(){
            {LineStyle.DASHDOT,"dot_dash"},
            {LineStyle.DASH,"square_dash"}
        };

        public enum LineStyle
        {
            FULL, DASH, DASHDOT
        }
        Color color;
        LineStyle style;
        public Line(IEnumerable<Pos> lb72Pos, Dictionary<Properties, string> properties) 
        {
            latLonPos = lb72Pos.Select(pos => GEO.LambertToLatLong(pos));
            this.properties = properties;
            try
            {
                this.color = Conversion.hexToColor(lineColorMap[properties[Properties.THEMA]]);
                this.style = lineStyleMap[properties[Properties.STATUS]];
            }
            catch (KeyNotFoundException e)
            {
                Debug.Log("The initiated point is missing some properties.");
            }
            OnlineMaps.instance.OnChangePosition += UpdateLine;
            OnlineMaps.instance.OnChangeZoom += UpdateLine;
        }
        GameObject linestring;
        public override void Draw()
        {
            //first point is position
            linestring = new GameObject();
            linestring.name = "line";
            lineRenderer = linestring.AddComponent<LineRenderer>();
            Vector3[] points = latLonPos.Select(s => new Vector3((float)s.x, 0, (float)s.y)).ToArray();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.numPositions = (points.Count());
            if (style != LineStyle.FULL)
            {
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.material.mainTexture = Resources.Load("linestyles/" + styleTextureMap[style]) as Texture2D;
                lineRenderer.material.SetColor("_TintColor", color);

            }
            lineRenderer.SetPositions(points);
        }
        void UpdateLine()
        {
            // //check if line in scene
            // //            OnlineMaps.instance.GetTopLeftPosition
            // double lat;
            // double lon;
            // OnlineMaps.instance.GetTopLeftPosition(out lon,out lat);
            // var topLeft=new Vector2((float)lat,(float)lon);
            // if (latLonPos.All(pos => pos> )){

            // }
            // IEnumerable<Vector3> drawPos = latLonPos.Select(pos => OnlineMapsTileSetControl.instance.GetWorldPosition(pos));
            // lineRenderer.SetPositions(drawPos.ToArray());
        }

    }

}
