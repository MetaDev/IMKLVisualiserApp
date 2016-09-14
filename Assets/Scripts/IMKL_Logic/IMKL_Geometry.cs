using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;


namespace IMKL_logic
{
    public class IMKL_Geometry
    {
        static System.Random rnd = new System.Random();


        public static void DrawPoint(Vector2 pos, float height)
        {

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = new Vector3(1, 1, 1);
            sphere.transform.position = new Vector3(pos.x, pos.y, height);
        }

        public static void DrawLineString(IEnumerable<Vector2> posList, float height)
        {
            Color c1 = Color.yellow;
            Color c2 = Color.red;
            var linestring = new GameObject();
            LineRenderer lineRenderer = linestring.AddComponent<LineRenderer>();
            Vector3[] points = posList.Select(s => new Vector3(s.x, s.y, height)).ToArray();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startColor = c1;
            lineRenderer.endColor = c2;
            lineRenderer.startWidth = 0.2F;
            lineRenderer.endWidth = 0.2F;
            lineRenderer.numPositions=(posList.Count());

            lineRenderer.SetPositions(points);

        }


        public static void SetCamera(IEnumerable<Vector2> points)
        {

            Camera.main.transform.transform.position = points.ElementAt(rnd.Next(points.Count()));
            Camera.main.transform.transform.position += new Vector3(0, 0, 20);
            Camera.main.transform.transform.LookAt(points.ElementAt(rnd.Next(points.Count())));

        }


    }

}
