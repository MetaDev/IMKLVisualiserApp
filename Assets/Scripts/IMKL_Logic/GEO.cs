using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjNet;
using Utility;
public static class GEO
{


    static double ConvertDegreeAngleToDouble(double degrees, double minutes, double seconds)
    {
        //Decimal degrees = 
        //   whole number of degrees, 
        //   plus minutes divided by 60, 
        //   plus seconds divided by 3600

        return degrees + (minutes / 60) + (seconds / 3600);
    }
    public static double ToRadians(this double val)
    {
        return (Math.PI / 180.0) * val;
    }
    public static double Pow(this double r, double p)
    {
        return Math.Pow(r, p);
    }
    public static double Sin(this double r)
    {
        return Math.Sin(r);
    }
    public static double Cos(this double r)
    {
        return Math.Cos(r);
    }
    public static double Tan(this double r)
    {
        return Math.Tan(r);
    }
    private static double ToDegree(this double angle)
    {
        return angle * (180.0 / Math.PI);
    }
    public static void Log(params object[] objs)
    {
        string log = "";
        foreach (object obj in objs)
        {
            log += " " + obj.ToString();
        }
        Debug.Log(log);
    }
    public static double Ln(this double d)
    {
        return Math.Log(d);
    }
    static double PI = Math.PI;
    //convert EPSG:4326 WGS 84 to EPSG:31370 Belge 1972 / Belgian Lambert 72
    public static class LBToLL
    {
        static double a = 6378388;
        static double f = 1d / 297.0;
        static double phi1 = ConvertDegreeAngleToDouble(49, 50, 0.00204).ToRadians();
        static double phi2 = ConvertDegreeAngleToDouble(51, 10, 0.00204).ToRadians();
        static double phi0 = 90.0.ToRadians();
        static double lambda0 = ConvertDegreeAngleToDouble(4, 22, 2.952).ToRadians();
        static double x0 = 150000.013;
        static double y0 = 5400088.438;
        static double eTo2 = 2 * f - f.Pow(2);
        static double e = Math.Sqrt(eTo2);
        static double m1 = Cos(phi1) / (1 - eTo2 * Sin(phi1).Pow(2)).Pow(0.5);
        static double m2 = Cos(phi2) / (1 - eTo2 * Sin(phi2).Pow(2)).Pow(0.5);

        static double t1 = Tan(PI / 4 - phi1 / 2) / ((1 - e * Sin(phi1)) / (1 + e * Sin(phi1))).Pow(e / 2);
        static double t2 = Tan(PI / 4 - phi2 / 2) / ((1 - e * Sin(phi2)) / (1 + e * Sin(phi2))).Pow(e / 2);
        static double t0 = Tan(PI / 4 - phi0 / 2) / ((1 - e * Sin(phi0)) / (1 + e * Sin(phi0))).Pow(e / 2);

        static double n = (m1.Ln() - m2.Ln()) / (t1.Ln() - t2.Ln());

        static double g = m1 / (n * t1.Pow(n));
        static double r0 = a * g * t0.Pow(n);
        //precision for 1 cm correctness
        static double min_precision = 0.00000001;
        public static Pos LambertToLatLong(Pos lb)
        {

            var x = lb.x;
            var y = lb.y;
            var r = ((x - x0).Pow(2) + (r0 - (y - y0)).Pow(2)).Pow(0.5);
            var t = (r / a * g).Pow(1 / n);
            var theta = Math.Atan((x - x0) / (r0 - (y - y0)));

            var lambda = (theta / n) + lambda0;

            var phi_i = PI / 2 - 2 * Math.Atan(t);
            var phi = 0.0;
            var diff = 1.0;
            Log(f);
            do
            {
                phi = PI / 2 - 2 * Math.Atan(t * ((1 - e * Sin(phi_i)) / (1 + e * Sin(phi_i))).Pow(e / 2));
                diff = Math.Abs(phi - phi_i);
                phi_i = phi;
            } while (diff > min_precision);

            return new Pos((phi+2*PI).ToDegree(), lambda.ToDegree());
        }


        public static Pos LatLongToLambert(Pos LatLong)
        {

            var phi = LatLong.x;
            var lambda = LatLong.y;
            var t = Tan(PI / 4 - phi / 2) / ((1 - e * Sin(phi)) / (1 + e * Sin(phi))).Pow(e / 2);
            var r = a * g * t.Pow(n);
            var theta = n * (lambda - lambda0);
            var x = x0 + r * Sin(theta);
            var y = y0 + r0 - r * Cos(theta);

            return new Pos(x, y);
        }
    }




}
