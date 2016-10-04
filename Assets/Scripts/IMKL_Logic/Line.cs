using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;
using UniRx;
using MoreLinq;
using System;
//using More
namespace IMKL_Logic
{
    public class Line : DrawElement
    {

        public IEnumerable<Vector2d> latLonPos
        {
            get;
            private set;
        }
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
            {"crosstheme","#FEE08B"}
        };
        static IDictionary<string, LineStyle> lineStyleMap = new Dictionary<string, LineStyle>(){
            {"functional",LineStyle.FULL},
            {"projected",LineStyle.DASH},
            {"disused",LineStyle.DASHDOT}
        };
        static IDictionary<LineStyle, Texture2D> styleTextureMap;
        static Material mat;
        public enum LineStyle
        {
            FULL, DASH, DASHDOT
        }
        Color color;
        LineStyle style;

        //range of zoom levels for which the elements are visible
        public Line(IEnumerable<Vector2d> lb72Pos, Dictionary<Properties, string> properties)
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
                Debug.Log("The initiated Line is missing or has unidentified properties." + e.Message);
                Debug.Log(string.Join(" ", properties.Select(kvp => kvp.ToString()).ToArray()));
            }
            Debug.Log(this.ToString());
            Debug.Log(this.style);

        }
        GameObject linestring;
        LineRenderer lineRenderer;
        IEnumerable<Vector2> relativePos;
        //init and draw are both methods accesing unity API and as such should always be called from main thread
        public override void Init()
        {
            //load textures
            if (styleTextureMap == null)
            {
                styleTextureMap = new Dictionary<LineStyle, Texture2D>(){
            {LineStyle.DASHDOT,Resources.Load("linestyles/dot_dash") as Texture2D},
            {LineStyle.DASH,Resources.Load("linestyles/square_dash") as Texture2D}};
            }
            if (mat == null)
            {
                mat = new Material(Shader.Find("Mobile/Particles/Multiply"));
            }
            //first point is position
            linestring = new GameObject();
            linestring.name = "line";
            lineRenderer = linestring.AddComponent<LineRenderer>();
            lineRenderer.material = mat;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.numPositions = (latLonPos.Count());
            if (style != LineStyle.FULL)
            {
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.material.mainTexture = styleTextureMap[style];
                lineRenderer.material.SetColor("_TintColor", color);
                if (style == LineStyle.DASH)
                {
                    //tiling 0.2 0.4, width 2*
                    lineRenderer.material.mainTextureScale = new Vector2(0.2f, 0.4f);
                    lineRenderer.startWidth = width * 2;
                    lineRenderer.endWidth = width * 2;

                }
                else if (style == LineStyle.DASHDOT)
                {
                    //tileing 0.1 0.1
                    lineRenderer.material.mainTextureScale = new Vector2(0.1f, 0.1f);
                }
            }

            OnlineMaps.instance.OnChangePosition += UpdateAbsPosition;
            OnlineMaps.instance.OnChangeZoom += UpdateRelPos;
            OnlineMaps.instance.OnChangeZoom += UpdateAbsPosition;
            originPos = latLonPos.First();
            CacheWorldPos();
            UpdateRelPos();
        }
        public override string ToString()
        {
            return "properties: " + string.Join(" ", properties.Select(kvp => kvp.ToString()).ToArray()) + Environment.NewLine
                + "Position" + string.Join(" ", latLonPos.Select(p => p.ToString()).ToArray());
        }

        void UpdateAbsPosition()
        {
            if (originPos != null && prevWorldOriginPos != null && WorldPosCache.ContainsKey(OnlineMaps.instance.zoom))
            {
                var worldOriginPos = OnlineMapsTileSetControl.instance.GetWorldPosition(originPos.x, originPos.y);
                var delta = worldOriginPos - prevWorldOriginPos;
                var newWorldPosInMap = WorldPosCache[OnlineMaps.instance.zoom].Select(pos => pos + delta).ToArray();
                lineRenderer.SetPositions(newWorldPosInMap);
            }

        }
        public Vector3 prevWorldOriginPos;
        public Vector2d originPos;
        public Dictionary<int, Vector3[]> WorldPosCache;
        void CacheWorldPos()
        {
            WorldPosCache = new Dictionary<int, Vector3[]>();
            var prev_zoom = OnlineMaps.instance.zoom;
            foreach (int zoom in Enumerable.Range(DrawElement.DrawRange.min, DrawElement.DrawRange.max))
            {
                //set the map to appropriate zoom levels
                OnlineMaps.instance.zoom = zoom;
                WorldPosCache[zoom] = latLonPos.Select(pos => OnlineMapsTileSetControl.instance.GetWorldPosition(pos.x, pos.y)).ToArray();
            }
            OnlineMaps.instance.zoom = prev_zoom;
        }
        void UpdateRelPos()
        {
            if (DrawElement.DrawRange.InRange(OnlineMaps.instance.zoom))
            {
                //draw the line from previously cached zoom for relatively correct points
                if (OnlineMapsTileSetControl.instance != null && WorldPosCache.ContainsKey(OnlineMaps.instance.zoom))
                {
                    //reset GameObject position
                    //for the relative draw to work
                    linestring.transform.position = Vector3.zero;
                    var worldPos = WorldPosCache[OnlineMaps.instance.zoom];
                    //save origins for relative draw
                    prevWorldOriginPos = worldPos[0];
                    //update line
                    lineRenderer.SetPositions(worldPos);
                }
            }


        }

    }

}
