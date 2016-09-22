using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using UnityEditor;
using Utility;
namespace IMKL_Logic
{
    public class IMKL_Geometry
    {


        public static void Draw(IEnumerable<DrawElement> pointsAndlines)
        {
            Debug.Log("map centering starts");
            //draw all geometry at the center of the scene, defined by all points
            var pointsPos = pointsAndlines.OfType<Point>().Select(point => point.GetLatLon());
            Pos min = new Pos(pointsPos.Min(v => v.x), pointsPos.Min(v => v.y));
            //set camera of scene to center of geometry
            Pos max = new Pos(pointsPos.Max(v => v.x), pointsPos.Max(v => v.y));

            var absCenter = (max + min) / 2;

            OnlineMapsLocationService.instance.emulatorPosition=absCenter;
            OnlineMaps.instance.zoom = 17;
            OnlineMaps.instance.Redraw();



        }


        public static void SetCamera(Vector2 point, float dist = 20)
        {

            Camera.main.transform.transform.position = point;
            Camera.main.transform.transform.position += new Vector3(0, dist, 0);
            Camera.main.transform.transform.LookAt(point);

        }


    }

}
