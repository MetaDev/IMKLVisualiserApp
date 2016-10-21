using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using Utility;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using MoreLinq;

namespace IMKL_Logic
{
    public class MapHelper : MonoBehaviour
    {
        void Start()
        {
            //override the tile download process
            // OnlineMaps.instance.OnStartDownloadTile += OnStartDownloadTile;
             MapPath = Path.Combine(Application.persistentDataPath, "OnlineMapsTiles");
        }
        static void OnStartDownloadTile(OnlineMapsTile tile)
        {
            // Get local path (different than the one defined in editor).
            string path = GetTilePath(tile);
            // If the tile is cached.
            if (File.Exists(path))
            {

                // Load tile texture from cache.
                Texture2D tileTexture = new Texture2D(256, 256);
                tileTexture.LoadImage(File.ReadAllBytes(path));
                tileTexture.wrapMode = TextureWrapMode.Clamp;

                // Send texture to map.
                if (OnlineMaps.instance.target == OnlineMapsTarget.texture)
                {
                    tile.ApplyTexture(tileTexture);
                    OnlineMaps.instance.buffer.ApplyTile(tile);
                }
                else
                {
                    tile.texture = tileTexture;
                    tile.status = OnlineMapsTileStatus.loaded;
                }
                // Redraw map.
                OnlineMaps.instance.Redraw();
            }
            else
            {
                // If the tile is not cached, download tile with a standard loader.
                OnlineMaps.instance.StartDownloadTile(tile);
            }
        }
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


        static string MapPath;

        /// <summary>
        /// Gets the local path for tile.
        /// </summary>
        /// <param name="tile">Reference to tile</param>
        /// <returns>Local path for tile</returns>
        static string GetTilePath(OnlineMapsTile tile)
        {
            string[] parts =
            {
           MapPath,
            tile.zoom.ToString(),
            tile.x.ToString(),
            tile.y + ".png"
        };
            return FileExtension.CombinePaths(parts);
        }



        public static void DeleteCachedMaps()
        {
            Directory.Delete(MapPath, true);
        }
        static Tuple<Vector2d, Vector2d> GetTopLeftBottomRightFromMap(Vector2 mapPosition, int zoom)
        {
            var map = OnlineMaps.instance;
            int width = map.target == OnlineMapsTarget.tileset ? map.tilesetWidth : map.width;
            int height = map.target == OnlineMapsTarget.tileset ? map.tilesetHeight : map.height;

            int countX = width / OnlineMapsUtils.tileSize;
            int countY = height / OnlineMapsUtils.tileSize;

            double px, py;
            map.GetPosition(out px, out py);
            map.projection.CoordinatesToTile(px, py, zoom, out px, out py);

            Vector2 ptl = new Vector2((float)px - countX / 2f, (float)py - countY / 2f);
            Vector2 pbr = new Vector2((float)px + countX / 2f, (float)py + countY / 2f);

            double tlx, tly, brx, bry;
            map.projection.TileToCoordinates(ptl.x, ptl.y, zoom, out tlx, out tly);
            map.projection.TileToCoordinates(pbr.x, pbr.y, zoom, out brx, out bry);

            return Tuple.Create(new Vector2d(tlx, tly), new Vector2d(brx, bry));

        }
      
        public static List<OnlineMapsTile> GetRequiredTilesForPackage(IMKLPackage package, int minZoom = 17, int maxZoom = 20)
        {
            OnlineMaps map = OnlineMaps.instance;
            var rect = GetEnclosingRectOfMapRequestZone(package.MapRequestZone);
            var min = rect.Item1;
            var max = rect.Item2;
            var absCenter = (max + min) / 2;

            List<OnlineMapsTile> tiles = new List<OnlineMapsTile>();
            int i=0;
            for (int zoom = minZoom; zoom <= maxZoom; zoom++)
            {
                //I only download those tile that are around a particular zoom level
                var rectDownload = GetTopLeftBottomRightFromMap(absCenter, zoom);
                var topLeftCoordinates = rectDownload.Item1;
                var bottomRightCoordinates = rectDownload.Item2;

                double tlx, tly, brx, bry;
                map.projection.CoordinatesToTile(topLeftCoordinates.x, topLeftCoordinates.y, zoom, out tlx, out tly);
                map.projection.CoordinatesToTile(bottomRightCoordinates.x, bottomRightCoordinates.y, zoom, out brx, out bry);

                int maxX = 1 << zoom;

                if (brx < tlx) brx += maxX;
                for (int x = (int)tlx; x < (int)brx; x++)
                {
                    int cx = x;
                    if (cx >= maxX) cx -= maxX;

                    for (int y = (int)tly; y < (int)bry; y++)
                    {
                        OnlineMapsTile tile = new OnlineMapsTile(cx, y, zoom, map);

                        string tilePath = GetTilePath(tile);
                        if (!File.Exists(tilePath)) tiles.Add(tile);
                    }
                }
            }
            return tiles;
        }
        public static IObservable<Unit> DownloadTilesForPackage(IMKLPackage package, IProgress<float> progressNotifier)
        {
            var tiles = GetRequiredTilesForPackage(package);
            List<IObservable<Unit>> list = new List<IObservable<Unit>>();

            foreach (OnlineMapsTile tile in tiles)
            {
                list.Add(UniRXExtensions.GetWWW(UnityWebRequest.Get(tile.url)).Do(webrequest =>
                {
                    string path = GetTilePath(tile);
                    FileInfo fileInfo = new FileInfo(path);
                    DirectoryInfo directory = fileInfo.Directory;
                    if (!directory.Exists) directory.Create();
                    File.WriteAllBytes(path, webrequest.downloadHandler.data);
                }).AsUnitObservable());
            }
            return list.Merge().ForEachAsync((_,idx)=>progressNotifier.Report((float)idx / tiles.Count()));

        }


    }
}
