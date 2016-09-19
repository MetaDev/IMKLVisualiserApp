using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GEO
{
	
    public static Vector2 LatLon_To_Lambert72(Vector2 latlon)
    {
        double lat = latlon.x;
        double lng = latlon.y;

        double LongRef = 0.076042943;
        //=4°21'24"983
        double bLamb = 6378388 * (1 - (1 / 297));
        double aCarre = Math.Pow(6378388, 2);
        double eCarre = (aCarre - Math.Pow(bLamb, 2)) / aCarre;
        double KLamb = 11565915.812935;
        double nLamb = 0.7716421928;

        double eLamb = Math.Sqrt(eCarre);
        double eSur2 = eLamb / 2;

        //conversion to radians
        lat = (Math.PI / 180) * lat;
        lng = (Math.PI / 180) * lng;

        double eSinLatitude = eLamb * Math.Sin(lat);
        double TanZDemi = (Math.Tan((Math.PI / 4) - (lat / 2))) * (Math.Pow(((1 + (eSinLatitude)) / (1 - (eSinLatitude))), (eSur2)));

        double RLamb = KLamb * (Math.Pow((TanZDemi), nLamb));

        double Teta = nLamb * (lng - LongRef);

        double x = 0;
        double y = 0;

        x = 150000 + 0.01256 + RLamb * Math.Sin(Teta - 0.000142043);
        y = 5400000 + 88.4378 - RLamb * Math.Cos(Teta - 0.000142043);

        return new Vector2((float)x,(float) y);
    }

    public static Vector2 Lambert72_To_LatLon(Vector2 lb72)
    {
        double X = lb72.x;
        double Y = lb72.y;

        double LongRef = 0.076042943;
        //=4°21'24"983
        double nLamb = 0.7716421928;
        double aCarre = Math.Pow(6378388, 2);
        double bLamb = 6378388 * (1 - (1 / 297));
        double eCarre = (aCarre - Math.Pow(bLamb, 2)) / aCarre;
        double KLamb = 11565915.812935;

        double eLamb = Math.Sqrt(eCarre);
        double eSur2 = eLamb / 2;

        double Tan1 = (X - 150000.01256) / (5400088.4378 - Y);
        double Lambda = LongRef + (1 / nLamb) * (0.000142043 + Math.Atan(Tan1));
        double RLamb = Math.Sqrt(Math.Pow((X - 150000.01256), 2) + Math.Pow((5400088.4378 - Y), 2));

        double TanZDemi = Math.Pow((RLamb / KLamb), (1 / nLamb));
        double Lati1 = 2 * Math.Atan(TanZDemi);

        double eSin = 0;
        double Mult1 = 0;
        double Mult2 = 0;
        double Mult = 0;
        double LatiN = 0;
        double Diff = 0;

        double lat = 0;
        double lng = 0;

        do
        {
            eSin = eLamb * Math.Sin(Lati1);
            Mult1 = 1 - eSin;
            Mult2 = 1 + eSin;
            Mult = Math.Pow((Mult1 / Mult2), (eLamb / 2));
            LatiN = (Math.PI / 2) - (2 * (Math.Atan(TanZDemi * Mult)));
            Diff = LatiN - Lati1;
            Lati1 = LatiN;
        } while (Math.Abs(Diff) > 2.77777E-08);

        lat = (LatiN * 180) / Math.PI;
        lng = (Lambda * 180) / Math.PI;

        return new Vector2((float)lat,(float) lng);
    }
}
