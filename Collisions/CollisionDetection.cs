using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collisions
{
    public class CollisionDetection
    {
        List<Tile> touchedTiles;
        public CollisionDetection() {
            touchedTiles = new List<Tile>();
        }

        public bool IsColliding(AABB box, TileMap tileMap) {

            // Broad Phase
            // -----------------

            // GetTiles that box is "on"
            touchedTiles.Clear();
            touchedTiles.Add(tileMap.PositionToTile(box.X, box.Y)); // Top Left
            touchedTiles.Add(tileMap.PositionToTile(box.X + box.Width, box.Y)); // Top Right
            touchedTiles.Add(tileMap.PositionToTile(box.X, box.Y + box.Height)); // Bottom Left
            touchedTiles.Add(tileMap.PositionToTile(box.X + box.Width, box.Y + box.Height)); // Bottom Right

            foreach (Tile tile in touchedTiles) {
                tile.Intersected = true;
                if (tile.IsSolid && tile.Shape == Shape.Box && /* && box.Shape == Shape.Box*/ Overlapping(box, tile.AABB)) {
                    tile.Collided = true;
                }
            }

            // Narrow Phase
            // -----------------

            return touchedTiles.Any(t => t.Collided);
        }

        /**
          * Just to check if we are overlapping
          */
        private bool Overlapping(AABB a, AABB b) {
            if (a.X + a.Width < b.X || a.X > b.X + b.Width)
                return false;
            if (a.Y + a.Height < b.Y || a.Y > b.Y + b.Height)
                return false;

            return true;
        }
    }
}
