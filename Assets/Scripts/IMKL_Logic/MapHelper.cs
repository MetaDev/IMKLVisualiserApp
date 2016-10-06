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
           
            //draw all geometry at the center of the scene, defined by all points
            //TODO throws error on empty list, center using package info
            Debug.Log( pointsAndlines.OfType<Point>().Count());
            var pointsPos = pointsAndlines.OfType<Point>().Select(point => point.GetLatLon());
            Vector2d min = new Vector2d(pointsPos.Min(v => v.x), pointsPos.Min(v => v.y));
            //set camera of scene to center of geometry
            Vector2d max = new Vector2d(pointsPos.Max(v => v.x), pointsPos.Max(v => v.y));

            var absCenter = (max + min) / 2;
            //turn off gps and relocate map vies
            OnlineMapsLocationService.instance.updatePosition=false;
            OnlineMaps.instance.position = absCenter;
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
