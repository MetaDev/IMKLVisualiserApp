using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;
using UniRx;
using MoreLinq;
using System;
using UnityEditor;
//using More
namespace IMKL_Logic
{
    public class Line : DrawElement
    {

        public static Dictionary<VisualisedProperties, string> VisualisedPropertyMap = new Dictionary<VisualisedProperties, string>(){
            {VisualisedProperties.THEMA,VisualisedProperties.THEMA.ToString().ToLowerInvariant()},
            {VisualisedProperties.STATUS,VisualisedProperties.STATUS.ToString().ToLowerInvariant()}
        };
        public enum VisualisedProperties
        {
            THEMA, STATUS
        }
        public IEnumerable<Vector2d> latLonPos
        {
            get;
            private set;
        }
        public float width = 3F;


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
        static IDictionary<LineStyle, Texture2D> styleTextureMap = new Dictionary<LineStyle, Texture2D>(){
            {LineStyle.DASHDOT,Resources.Load("linestyles/dot_dash") as Texture2D},
            {LineStyle.DASH,Resources.Load("linestyles/square_dash") as Texture2D},
            {LineStyle.FULL,Resources.Load("linestyles/full") as Texture2D}};
        static Material mat;
        public enum LineStyle
        {
            FULL, DASH, DASHDOT
        }
        Color color;
        LineStyle style;

        GameObject linestring;
        LineRenderer lineRenderer;
        IEnumerable<Vector2> relativePos;

        //range of zoom levels for which the elements are visible
        public Line(IEnumerable<Vector2d> lb72Pos, string thema, string status, Dictionary<string, string> properties) : base(properties)
        {
            latLonPos = lb72Pos.Select(pos => GEO.LambertToLatLong(pos));

            this.color = Conversion.hexToColor(lineColorMap[thema]);
            this.style = lineStyleMap[status];


        }
        //init and draw are both methods accesing unity API and as such should always be called from main thread
        public override void Init()
        {

            //check if not clicked
            //TODO distinguish between drag and click, only work on click
            Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0)).Subscribe(_ =>
            {
                float clickLineSensitivity = 150.0f;
                var maxDist = (clickLineSensitivity / OnlineMaps.instance.zoom);
                //works for mobile devices as well
                //find closest point to line
                var mousScreenPos = Input.mousePosition;
                mousScreenPos.z = 0;
                var mousePos = Camera.main.ScreenToWorldPoint(mousScreenPos);
                //mousePos.z = 0;
                var closestDist = CurrentWorldPos.Pairwise((prev, curr) =>
                {

                    return HandleUtility.DistancePointToLineSegment(prev, curr, mousePos);
                }).Min();
                Debug.Log(closestDist + " " + mousePos);
            
                if (closestDist < maxDist)
                {
                    lineRenderer.endColor = Color.cyan;
                    lineRenderer.startColor = Color.cyan;
                }
            });
            linestring = new GameObject("Line");
            lineRenderer = linestring.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Mobile/Particles/Multiply"));
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.numPositions = (latLonPos.Count());
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.material.mainTexture = styleTextureMap[style];
            lineRenderer.material.SetColor("_TintColor", color);
            switch (style)
            {
                case LineStyle.DASH:
                    //tiling 0.2 0.4, width 2*
                    lineRenderer.material.mainTextureScale = new Vector2(0.2f, 0.4f);
                    lineRenderer.startWidth = width * 2;
                    lineRenderer.endWidth = width * 2;
                    break;
                case LineStyle.DASHDOT:
                    //tileing 0.1 0.1
                    lineRenderer.material.mainTextureScale = new Vector2(0.1f, 0.1f);
                    break;
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
            return "properties: " + string.Join(" ", Properties.Select(kvp => kvp.ToString()).ToArray()) + Environment.NewLine
                + "Position" + string.Join(" ", latLonPos.Select(p => p.ToString()).ToArray());
        }
        Vector3[] CurrentWorldPos;
        void UpdateAbsPosition()
        {
            if (originPos != null && prevWorldOriginPos != null && WorldPosAndMeshCache.ContainsKey(OnlineMaps.instance.zoom))
            {
                var worldOriginPos = OnlineMapsTileSetControl.instance.GetWorldPosition(originPos.x, originPos.y);
                var delta = worldOriginPos - prevWorldOriginPos;
                var newWorldPosInMap = WorldPosAndMeshCache[OnlineMaps.instance.zoom].Select(pos => pos + delta).ToArray();
                CurrentWorldPos = newWorldPosInMap;
                lineRenderer.SetPositions(newWorldPosInMap);
            }

        }
        public Vector3 prevWorldOriginPos;
        public Vector2d originPos;
        public Dictionary<int, Vector3[]> WorldPosAndMeshCache;
        void CacheWorldPos()
        {
            WorldPosAndMeshCache = new Dictionary<int, Vector3[]>();
            var prev_zoom = OnlineMaps.instance.zoom;
            foreach (int zoom in Enumerable.Range(DrawElement.DrawRange.min, DrawElement.DrawRange.max))
            {
                //set the map to appropriate zoom levels
                OnlineMaps.instance.zoom = zoom;
                WorldPosAndMeshCache[zoom] = latLonPos.Select(pos => OnlineMapsTileSetControl.instance.GetWorldPosition(pos.x, pos.y)).ToArray();
            }
            //also cache meshes

            OnlineMaps.instance.zoom = prev_zoom;
        }
        void UpdateRelPos()
        {
            if (DrawElement.DrawRange.InRange(OnlineMaps.instance.zoom))
            {
                //draw the line from previously cached zoom for relatively correct points
                if (OnlineMapsTileSetControl.instance != null && WorldPosAndMeshCache.ContainsKey(OnlineMaps.instance.zoom))
                {
                    var worldPos = WorldPosAndMeshCache[OnlineMaps.instance.zoom];
                    //save origins for relative draw
                    prevWorldOriginPos = worldPos[0];
                }
            }
        }



    }
}

