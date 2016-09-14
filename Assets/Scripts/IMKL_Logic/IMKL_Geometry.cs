using UnityEngine;
using System.Collections;
using Utility;
using System.Linq;
using System.Security.Policy;
using UniRx;
using System.Collections.Generic;
using System;

namespace IMKL_logic
{
	public class IMKL_Geometry
	{
		static System.Random rnd = new System.Random();


		public static void DrawPoint (Vector2 pos, float height)
		{
			
			var sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			sphere.transform.localScale = new Vector3 (1, 1, 1);
			sphere.transform.position = new Vector3 (pos.x, pos.y, height);
		}

		public static void DrawLineString (Vector2[] posList, float height)
		{
			var linestring = new GameObject ();
			LineRenderer lineRenderer = linestring.AddComponent<LineRenderer> ();
			Vector3[] points = posList.Select (s => new Vector3 (s.x, s.y, height)).ToArray ();
			lineRenderer.SetPositions (points);

		}

		public static void DrawPolygon (Vector2[] posList, float height)
		{
			// Use the triangulator to get indices for creating triangles
			Triangulator tr = new Triangulator (posList);
			int[] indices = tr.Triangulate ();

			// Create the Vector3 vertices
			Vector3[] vertices = new Vector3[posList.Length];
			for (int i = 0; i < vertices.Length; i++) {
				vertices [i] = new Vector3 (posList [i].x, posList [i].y, height);
			}

			// Create the mesh
			Mesh msh = new Mesh ();
			msh.vertices = vertices;
			msh.triangles = indices;
			msh.RecalculateNormals ();
			msh.RecalculateBounds ();

			// Set up game object with mesh;
			var polygon = new GameObject ();
			polygon.AddComponent (typeof(MeshRenderer));
			MeshFilter filter = polygon.AddComponent (typeof(MeshFilter)) as MeshFilter;
			filter.mesh = msh;
		}
		public static void SetCamera(IEnumerable<Vector2> points){
			
			Camera.main.transform.transform.position = points.ElementAt (rnd.Next (points.Count ()));
			Camera.main.transform.transform.position += new Vector3 (0,0,20);
			Camera.main.transform.transform.LookAt (points.ElementAt (rnd.Next (points.Count ())) );

		}


	}

}
