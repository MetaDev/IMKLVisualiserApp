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

        LineRenderer lineRenderer;
        IEnumerable<Vector2> relativePos;
        string Thema;
        //range of zoom levels for which the elements are visible
        public Line(IEnumerable<Vector2d> lb72Pos, string thema, string status, Dictionary<string, string> properties) : base(properties)
        {
            latLonPos = lb72Pos.Select(pos => GEO.LambertToLatLong(pos));
            this.Thema = thema;
            this.color = Conversion.hexToColor(lineColorMap[thema]);
            this.style = lineStyleMap[status];


        }
        //init and draw are both methods accesing unity API and as such should always be called from main thread
        public override void Init()
        {
            GO = new GameObject("Line");
            GO.tag = "DrawElement";

            lineRenderer = GO.AddComponent<LineRenderer>();
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
        protected override bool ClickWithinDistance(Vector3 worldMousePos, float maxDist)
        {
            //find closest point to line
            return CurrentWorldPos
            .Pairwise((prev, curr) => Vector3.Distance(ProjectPointOnLineSegment(prev, curr, worldMousePos), worldMousePos))
            .Any(dist => dist < maxDist);

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
        public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {

            Vector3 vector = linePoint2 - linePoint1;

            Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

            int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);

            //The projected point is on the line segment
            if (side == 0)
            {
                return projectedPoint;
            }

            if (side == 1)
            {
                return linePoint1;
            }

            if (side == 2)
            {
                return linePoint2;
            }

            //output is invalid
            return Vector3.zero;
        }
        public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
        {

            //get vector from point on line to point in space
            Vector3 linePointToPoint = point - linePoint;

            float t = Vector3.Dot(linePointToPoint, lineVec);

            return linePoint + lineVec * t;
        }
        public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {

            Vector3 lineVec = linePoint2 - linePoint1;
            Vector3 pointVec = point - linePoint1;

            float dot = Vector3.Dot(pointVec, lineVec);

            //point is on side of linePoint2, compared to linePoint1
            if (dot > 0)
            {

                //point is on the line segment
                if (pointVec.magnitude <= lineVec.magnitude)
                {

                    return 0;
                }

                //point is not on the line segment and it is on the side of linePoint2
                else
                {

                    return 2;
                }
            }

            //Point is not on side of linePoint2, compared to linePoint1.
            //Point is not on the line segment and it is on the side of linePoint1.
            else
            {

                return 1;
            }
        }

        public override string GetTextForPropertiesPanel()
        {
            return "Line: " + Thema;
        }
    }
}

