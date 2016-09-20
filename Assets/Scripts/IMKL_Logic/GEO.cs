
using Utility;
using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using DotSpatial.Projections;

public static class GEO
{
    static string EPSG4326 = "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\"," +
            "SPHEROID[\"WGS 84\",6378137,298.257223563," +
            "AUTHORITY[\"EPSG\",\"7030\"]]," +
            "AUTHORITY[\"EPSG\",\"6326\"]]," +
            "PRIMEM[\"Greenwich\",0," +
            "AUTHORITY[\"EPSG\",\"8901\"]]," +
            "UNIT[\"degree\",0.0174532925199433," +
            "AUTHORITY[\"EPSG\",\"9122\"]]," +
            "AUTHORITY[\"EPSG\",\"4326\"]]";
    static string EPSG31370 = "GEOGCS[\"WGS 84\"," +
        "DATUM[\"WGS_1984\"," +
        "SPHEROID[\"WGS 84\",6378137,298.257223563," +
        "AUTHORITY[\"EPSG\",\"7030\"]]," +
        "AUTHORITY[\"EPSG\",\"6326\"]]," +
        "PRIMEM[\"Greenwich\",0," +
"AUTHORITY[\"EPSG\",\"8901\"]]," +
"UNIT[\"degree\",0.0174532925199433," +
"AUTHORITY[\"EPSG\",\"9122\"]]," +
"AUTHORITY[\"EPSG\",\"4326\"]]";


    public static Pos LambertToLatLong(Pos lb)
    {
        var source = ProjectionInfo.FromProj4String("+proj=lcc +lat_1=51.16666723333333 +lat_2=49.8333339 +lat_0=90 +lon_0=4.367486666666666 +x_0=150000.013 +y_0=5400088.438 +ellps=intl +towgs84=-106.869,52.2978,-103.724,0.3366,-0.457,1.8422,-1.2747 +units=m +no_defs");
		var target = ProjectionInfo.FromProj4String("+proj=longlat +datum=WGS84 +no_defs ");
		double[] xy = new double[2];
        xy[0] = lb.x;
        xy[1] = lb.y;
        //An array for the z coordinate
        double[] z = new double[1];
        z[0] = 1;
		Reproject.ReprojectPoints(xy, z, source, target, 0, 1);
        return new Pos(xy[0],xy[1]);
    }


    public static Pos LatLongToLambert(Pos LatLong)
    {
       var source = ProjectionInfo.FromProj4String("+proj=lcc +lat_1=51.16666723333333 +lat_2=49.8333339 +lat_0=90 +lon_0=4.367486666666666 +x_0=150000.013 +y_0=5400088.438 +ellps=intl +towgs84=-106.869,52.2978,-103.724,0.3366,-0.457,1.8422,-1.2747 +units=m +no_defs");
		var target = ProjectionInfo.FromProj4String("+proj=longlat +datum=WGS84 +no_defs ");
		double[] xy = new double[2];
        xy[0] = LatLong.x;
        xy[1] = LatLong.y ;
        //An array for the z coordinate
        double[] z = new double[1];
        z[0] = 1;
		Reproject.ReprojectPoints(xy, z, target, source, 0, 1);
        return new Pos(xy[0],xy[1]);

    }


}
