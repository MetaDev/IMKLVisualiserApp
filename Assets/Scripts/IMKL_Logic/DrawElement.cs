using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using MoreLinq;

namespace IMKL_Logic
{
    public abstract class DrawElement
    {
        static OnlineMapsRange drawRange = new OnlineMapsRange(15, 20);
        public static OnlineMapsRange DrawRange
        {
            get{
                return drawRange;
            }
        }

        //BROL throw away at earliest convenience
        public bool InMapView(Pos latlon, OnlineMapsRange range)
        {

            float latitude = (float)latlon.y;
            float longitude = (float)latlon.x;
            OnlineMaps api = OnlineMaps.instance;

            if (!range.InRange(api.zoom)) return false;

            double tlx, tly, brx, bry;
            api.GetTopLeftPosition(out tlx, out tly);
            api.GetBottomRightPosition(out brx, out bry);

            if (longitude >= tlx && longitude <= brx && latitude >= bry && latitude <= tly) return true;
            return false;
        }
        public abstract void Init();

    }
}

