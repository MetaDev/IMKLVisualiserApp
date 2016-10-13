using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using Utility;
using System.IO;
using UniRx;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using MoreLinq;
namespace IMKL_Logic
{
    public class Tile
    {
        public int x;
        public int y;
        public int zoom;

        public Tile(int x, int y, int zoom)
        {
            this.x = x;
            this.y = y;
            this.zoom = zoom;
        }
        public string CustomProviderReplaceToken(Match match)
        {
            string v = match.Value.ToLower().Trim('{', '}');
            if (v == "zoom") return zoom.ToString();
            if (v == "x") return x.ToString();
            if (v == "y") return y.ToString();
            if (v == "quad") return OnlineMapsUtils.TileToQuadKey(x, y, zoom);
            return v;
        }


    }
    public class MapHelper
    {
        static Tuple<Vector2d, Vector2d> GetEnclosingRectOfMapRequestZone(IEnumerable<Vector2d> MapRequestZone)
        {
            Vector2d min = new Vector2d(MapRequestZone.Min(v => v.x), MapRequestZone.Min(v => v.y));
            //set camera of scene to center of geometry
            Vector2d max = new Vector2d(MapRequestZone.Max(v => v.x), MapRequestZone.Max(v => v.y));
            return Tuple.Create(min, max);
        }

        public static void ZoomAndCenterOnElements(IEnumerable<Vector2d> MapRequestZone, IEnumerable<DrawElement> elements)
        {
            var points = MapRequestZone;
            var rect = GetEnclosingRectOfMapRequestZone(points);
            var min = rect.Item1;
            var max = rect.Item2;
            var absCenter = (max + min) / 2;
            Debug.Log(max + " " + min);

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




        /// <summary>
        /// Gets the local path for tile.
        /// </summary>
        /// <param name="tile">Reference to tile</param>
        /// <returns>Local path for tile</returns>
        private static string GetTilePath(Tile tile)
        {
            string[] parts =
            {
            "Assets",
            "Resources",
            "OnlineMapsTiles",
            tile.zoom.ToString(),
            tile.x.ToString(),
            tile.y + ".png"
        };
            return string.Join("/", parts);
        }



        public static void DeleteCachedMaps()
        {
            Directory.Delete(Path.Combine(Application.persistentDataPath,
               "OnlineMapsTileCache"), true);
        }
        static string GetMapProviderPattern()
        {
            string pattern;
            var map = OnlineMaps.instance;
            OnlineMapsProvider.MapType type = map.activeType;
            bool useLabels = type.hasLabels ? map.labels : type.labelsEnabled;
            bool labels = true;
            if (useLabels)
            {
                if (!string.IsNullOrEmpty(type.urlWithLabels)) pattern = type.urlWithLabels;
                else if (!string.IsNullOrEmpty(type.provider.url)) pattern = type.provider.url;
                else
                {
                    pattern = type.urlWithoutLabels;
                    labels = false;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(type.urlWithoutLabels))
                {
                    pattern = type.urlWithoutLabels;
                    labels = false;
                }
                else if (!string.IsNullOrEmpty(type.provider.url))
                {
                    pattern = type.provider.url;
                    labels = false;
                }
                else pattern = type.urlWithLabels;
            }
            pattern = Regex.Replace(pattern, @"{\w+}", delegate (Match match)
                {
                    string v = match.Value.ToLower().Trim('{', '}');
                    if (v == "lng") return map.language;
                    if (v == "ext") return type.ext;
                    if (v == "prop") return labels ? type.propWithLabels : type.propWithoutLabels;
                    if (v == "variant") return labels ? type.variantWithLabels : type.variantWithoutLabels;
                    return "{" + v + "}";
                });
            return Regex.Replace(pattern, @"{rnd(\d+)-(\d+)}", match => match.Groups[1].Value);
        }
        public static IObservable<Unit> DownloadTilesForPackage(IMKLPackage package, IProgress<float> progressNotifier,
                                                                                    int minZoom = 15, int maxZoom = 20)
        {
            OnlineMaps map = OnlineMaps.instance;
            var rect = GetEnclosingRectOfMapRequestZone(package.MapRequestZone);
            var topLeftCoordinates = rect.Item1;
            var bottomRightCoordinates = rect.Item2;

            int iMin = Mathf.RoundToInt(minZoom);
            int iMax = Mathf.RoundToInt(maxZoom);
            var tiles = new List<Tile>();
            Debug.Log("brak");
            for (int zoom = iMin; zoom <= iMax; zoom++)
            {
                Debug.Log(topLeftCoordinates);
                Debug.Log(bottomRightCoordinates);
                double tlx, tly, brx, bry;
                map.projection.CoordinatesToTile(topLeftCoordinates.x, topLeftCoordinates.y, zoom, out tlx, out tly);
                map.projection.CoordinatesToTile(bottomRightCoordinates.x, bottomRightCoordinates.y, zoom, out brx, out bry);

                int maxX = 1 << zoom;

                if (brx < tlx) brx += maxX;
                Debug.Log(tlx+ " "+brx);
                for (int x = (int)tlx; x < (int)brx; x++)
                {
                    int cx = x;
                    if (cx >= maxX) cx -= maxX;

                    for (int y = (int)tly; y < (int)bry; y++)
                    {
                        Tile tile = new Tile(cx, y, zoom);

                        string tilePath = GetTilePath(tile);
                        Debug.Log(File.Exists(tilePath));
                        if (!File.Exists(tilePath)) tiles.Add(tile);
                    }
                }
            }
            var pattern = GetMapProviderPattern();
            List<IObservable<Unit>> list = new List<IObservable<Unit>>();
            foreach (Tile tile in tiles)
            {
                string url = Regex.Replace(pattern, @"{\w+}", tile.CustomProviderReplaceToken);
                list.Add(UniRXExtensions.GetWWW(UnityWebRequest.Get(url)).Select((webrequest, idx) =>
                {
                    string path = GetTilePath(tile);
                    FileInfo fileInfo = new FileInfo(path);
                    DirectoryInfo directory = fileInfo.Directory;
                    if (!directory.Exists) directory.Create();
                    File.WriteAllBytes(path, webrequest.downloadHandler.data);
                    progressNotifier.Report((float)idx / tiles.Count());
                    return Unit.Default;
                }));
            }
            Debug.Log(tiles.Count());
            return list.Merge();

        }


    }
}
