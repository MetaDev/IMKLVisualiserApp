using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using Utility;
namespace IMKL_Logic
{
    public class MapHelper
    {

        public static void TurnOfGPS()
        {

        }
        public static void ZoomAndCenterOnElements(IEnumerable<DrawElement> pointsAndlines)
        {
            Debug.Log(string.Join(" ",pointsAndlines.Select(p => p.ToString()).ToArray()));
            Debug.Log("map centering starts");
            //draw all geometry at the center of the scene, defined by all points
            //.Select(p=>p) necessary to avoid InvalidOperationException for some xml files
            var pointsPos = pointsAndlines.OfType<Point>().Select(point => point.GetLatLon());
            Debug.Log(pointsAndlines.OfType<Point>().Count());
             Debug.Log(string.Join(" ", pointsAndlines.OfType<Point>().Select(p => p.ToString()).ToArray()));
            Vector2d min = new Vector2d(pointsPos.Min(v => v.x), pointsPos.Min(v => v.y));
            //set camera of scene to center of geometry
            Vector2d max = new Vector2d(pointsPos.Max(v => v.x), pointsPos.Max(v => v.y));

            var absCenter = (max + min) / 2;

            OnlineMapsLocationService.instance.emulatorPosition = absCenter;
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
