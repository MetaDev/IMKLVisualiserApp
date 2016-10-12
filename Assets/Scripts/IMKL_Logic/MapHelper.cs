using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using Utility;
using System.IO;
using UniRx;
namespace IMKL_Logic
{
    public class MapHelper 
    {


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




        /// <summary>
        /// Gets the local path for tile.
        /// </summary>
        /// <param name="tile">Reference to tile</param>
        /// <returns>Local path for tile</returns>
        private static string GetTilePath(OnlineMapsTile tile)
        {
            string[] parts =
            {
                Application.persistentDataPath,
                "OnlineMapsTileCache",
                tile.mapType.provider.id,
                tile.mapType.id,
                tile.zoom.ToString(),
                tile.x.ToString(),
                tile.y + ".png"
            };
            return string.Join("/", parts);
        }

        /// <summary>
        /// This method is called when loading the tile.
        /// </summary>
        /// <param name="tile">Reference to tile</param>
        private static void OnStartDownloadTile(OnlineMapsTile tile)
        {
            // Get local path.
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

        /// <summary>
        /// This method is called when tile is success downloaded.
        /// </summary>
        /// <param name="tile">Reference to tile.</param>
        private static void OnTileDownloaded(OnlineMapsTile tile)
        {
            Debug.Log("test");
            // Get local path.
            string path = GetTilePath(tile);

            // Cache tile.
            FileInfo fileInfo = new FileInfo(path);
            DirectoryInfo directory = fileInfo.Directory;
            if (!directory.Exists) directory.Create();

            File.WriteAllBytes(path, tile.www.bytes);
        }
        public static void DeleteCachedMaps(){
             File.Delete(Path.Combine(Application.persistentDataPath,
                "OnlineMapsTileCache"));
        }
        public static void SetCacheMap(bool cache)
        {
            if (cache)
            {
                // Subscribe to the event of success download tile.
                OnlineMapsTile.OnTileDownloaded += OnTileDownloaded;

                // Intercepts requests to the download of the tile.
                OnlineMaps.instance.OnStartDownloadTile += OnStartDownloadTile;
                OnlineMaps.instance.Redraw();
            }else{
                 // Subscribe to the event of success download tile.
                OnlineMapsTile.OnTileDownloaded -= OnTileDownloaded;

                // Intercepts requests to the download of the tile.
                OnlineMaps.instance.OnStartDownloadTile += OnStartDownloadTile;
            }
        }
        
    }
}
