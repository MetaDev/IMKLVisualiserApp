using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;
using UniRx;
using MoreLinq;
//using More
namespace IMKL_Logic
{
    public class Line : DrawElement
    {

        IEnumerable<Pos> latLonPos;
        float width = 3F;
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

        //range of zoom levels for which the elements are visible
        OnlineMapsRange range = new OnlineMapsRange(15, 20);
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
        LineRenderer lineRenderer;

        public override void Draw()
        {
            //first point is position
            linestring = new GameObject();
            linestring.name = "line";
            lineRenderer = linestring.AddComponent<LineRenderer>();
            Vector3[] points = latLonPos.Select(s => OnlineMapsTileSetControl.instance.GetWorldPosition(s)).ToArray();

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
            // if (linestring == null)
            //     return;
            //     //lag due to method nlineMapsTileSetControl.instance.GetWorldPosition(
            // // OnlineMapsTileSetControl.instance.GetWorldPosition(latLonPos.Last());
            // //draw line if for each endpoints (or it's predecessor) is in mapview 

            // //the list is zipped with itself where the first is skipped and the last element is appended
            // //which is required for the last not be omitted 
            // var posInMap = latLonPos.Zip(latLonPos.Skip(1).Pad(latLonPos.Count(), latLonPos.Last()),
            //        (prev, curr) => (InMapView(prev, range) || InMapView(curr, range)) ? prev : null)
            //                        .Where(pos => pos != null);

            // if (posInMap.Count() > 0)
            // {
            //     var posOnMap = posInMap.Select(pos => OnlineMapsTileSetControl.instance.GetWorldPosition(pos)).ToArray();
            //     if (!linestring.activeInHierarchy)
            //         linestring.SetActive(true);

            //     lineRenderer.SetPositions(posOnMap);
            //     lineRenderer.numPositions = (posOnMap.Count());
            // }
            // else
            // {
            //     linestring.SetActive(false);
            // }
        }

    }

}
