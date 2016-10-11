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
        public static void ZoomAndCenterOnElements(IEnumerable<Vector2d> MapRequestZone)
        {

            Vector2d min = new Vector2d(MapRequestZone.Min(v => v.x), MapRequestZone.Min(v => v.y));
            //set camera of scene to center of geometry
            Vector2d max = new Vector2d(MapRequestZone.Max(v => v.x), MapRequestZone.Max(v => v.y));

            var absCenter = (max + min) / 2;
            //turn off gps and relocate map vies
            ZoomAndCenter(absCenter, 17);


        }
        public static void ZoomAndCenter(Vector2 latLng, int zoom)
        {
            OnlineMapsLocationService.instance.updatePosition = false;
            OnlineMaps.instance.position = latLng;
            OnlineMaps.instance.zoom = zoom;
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
