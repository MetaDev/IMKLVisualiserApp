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

        }
        GameObject linestring;
        IEnumerable<Vector2> relativePos;
        //init and draw are both methods accesing unity API and as such should always be called from main thread
        public override void Init()
        {
            GameObject container = new GameObject("Line");
            meshFilter = container.AddComponent<MeshFilter>();
            meshRenderer = container.AddComponent<MeshRenderer>();

            mesh = meshFilter.sharedMesh = new Mesh();
            mesh.name = "Line";
            mat = new Material(Shader.Find("Mobile/Particles/Multiply"));

            meshRenderer.material = mat;

            //first point is position
            linestring = new GameObject();
            linestring.name = "line";

            switch (style)
            {
                case LineStyle.DASH:
                    //tiling 0.2 0.4, width 2*
                    uvScale = new Vector2(0.2f, 0.4f);
                    width = width * 2;
                    width = width * 2;
                    break;
                case LineStyle.DASHDOT:
                    //tileing 0.1 0.1
                    uvScale = new Vector2(0.1f, 0.1f);
                    break;
                case LineStyle.FULL:
                    uvScale = new Vector2(1f, 1f);
                    break;


            }
            lineRenderer.textureMode = LineTextureMode.Tile;
            mat.mainTexture = styleTextureMap[style];
            lineRenderer.material.SetColor("_TintColor", color);
            if (style == LineStyle.DASH)
            {


            }
            else if (style == LineStyle.DASHDOT)
            {
                //tileing 0.1 0.1
                uvScale = new Vector2(0.1f, 0.1f);
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
                UpdateLine(newWorldPosInMap);
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
                    UpdateLine(worldPos);
                }
            }
        }

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;
        Vector2 uvScale;
        void UpdateLine(Vector3[] worldPos)
        {

            float totalDistance = 0;
            Vector3 lastPosition = Vector3.zero;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            List<Vector3> positions = new List<Vector3>();
            //skip first position for angle calculation
            foreach (Vector3 position in worldPos.Skip(1))
            {
                // Calculate angle between coordinates.
                float a = OnlineMapsUtils.Angle2DRad(lastPosition, position, 90);

                // Calculate offset
                Vector3 off = new Vector3(Mathf.Cos(a) * width, 0, Mathf.Sin(a) * width);

                // Init verticles, normals and triangles.
                int vCount = vertices.Count;

                vertices.Add(lastPosition + off);
                vertices.Add(lastPosition - off);
                vertices.Add(position + off);
                vertices.Add(position - off);

                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);

                triangles.Add(vCount);
                triangles.Add(vCount + 3);
                triangles.Add(vCount + 1);
                triangles.Add(vCount);
                triangles.Add(vCount + 2);
                triangles.Add(vCount + 3);

                totalDistance += (lastPosition - position).magnitude;


                lastPosition = position;
            }

            float tDistance = 0;

            for (int i = 1; i < positions.Count; i++)
            {
                float distance = (positions[i - 1] - positions[i]).magnitude;

                // Updates UV
                uvs.Add(new Vector2(tDistance / totalDistance, 0));
                uvs.Add(new Vector2(tDistance / totalDistance, 1));

                tDistance += distance;

                uvs.Add(new Vector2(tDistance / totalDistance, 0));
                uvs.Add(new Vector2(tDistance / totalDistance, 1));
            }

            // Update mesh
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangles.ToArray();

            // Scale texture
            Vector2 scale = new Vector2(totalDistance / width, 1);
            scale.Scale(uvScale);
            meshRenderer.material.mainTextureScale = scale;
        }

    }
}

