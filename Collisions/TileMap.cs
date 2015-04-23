using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collisions
{
    public class TileMap
    {
        private int _tileSize;
        public Tile[,] Tiles { get; set; }
        public TileMap(int tileSize, int width, int height)
        {
            _tileSize = tileSize;
            Tiles = new Tile[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tiles[x, y] = new Tile(string.Format("{0}:{1}", x * _tileSize, y * _tileSize), x, y, _tileSize, _tileSize);
                    Tiles[x, y].Shape = Shape.Box;
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                        Tiles[x, y].IsSolid = true;
                }
            }
        }

        public Tile PositionToTile(int x, int y) {
            var tileX = (int)Math.Floor((float)x / 32);
            var tileY = (int)Math.Floor((float)y / 32);
            return Tiles[tileX, tileY];
        }
    }
}
