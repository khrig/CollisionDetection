using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collisions
{
    public class CollisionHandler
    {
        List<Tile> touchedTiles;
        public CollisionHandler() {
            touchedTiles = new List<Tile>();
        }

        public CollisionResult ReallySimpleBeforeMoveAABB(AABB box, TileMap tileMap, Vector velocity)
        {
            int velocityX = (int)velocity.X;
            int velocityY = (int)velocity.Y;

            touchedTiles.Clear();
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX, box.Y + velocityY)); // Top Left
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX + box.Width, box.Y + velocityY)); // Top Right
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX, box.Y + box.Height + velocityY)); // Bottom Left
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX + box.Width, box.Y + velocityY + box.Height)); // Bottom Right

            CollisionResult result = new CollisionResult { Intersect = false, WillIntersect = false, MinimumTranslationVector = velocity };
            foreach (Tile tile in touchedTiles) {
                tile.Intersected = true;
                if (tile.IsSolid && tile.Shape == Shape.Box) {
                    tile.Collided = true;
                    result.Intersect = true;
                    result.WillIntersect = true;
                    if(velocityX > 0)
                        result.MinimumTranslationVector = new Vector(tile.AABB.X -1, 0);
                    if (velocityX < 0)
                        result.MinimumTranslationVector = new Vector(Math.Abs(tile.AABB.X + tile.AABB.Width - box.X), 0);
                }
            }
            return result;
        }


        /* 
         * http://www.opentk.com/node/869
         */
        public Vector MinimumTranslation(AABB box, AABB other)
        {
            // Min refers to the top left corner, Max refers to the bottom right corner.

            Vector amin = new Vector(box.X, box.Y);
            Vector amax = new Vector(box.X + box.Width, box.Y + box.Height);
            Vector bmin = new Vector(other.X, other.Y);
            Vector bmax = new Vector(other.X + other.Width, other.Y + other.Height);

            Vector mtd = new Vector();

            float left = (bmin.X - amax.X);
            float right = (bmax.X - amin.X);
            float top = (bmin.Y - amax.Y);
            float bottom = (bmax.Y - amin.Y);

            // box dont intersect   
            if (left > 0 || right < 0) return new Vector();
            if (top > 0 || bottom < 0) return new Vector();

            // box intersect. work out the mtd on both x and y axes.
            if (Math.Abs(left) < right)
                mtd.X = left;
            else
                mtd.X = right;

            if (Math.Abs(top) < bottom)
                mtd.Y = top;
            else
                mtd.Y = bottom;

            // 0 the axis with the largest mtd value.
            if (Math.Abs(mtd.X) < Math.Abs(mtd.Y))
                mtd.Y = 0;
            else
                mtd.X = 0;
            return mtd;
        }

        // Remove velocity to check after a move is done
        public CollisionResult IsCollidingSimple(AABB box, TileMap tileMap, Vector velocity)
        {
            // Broad Phase
            // -----------------

            int velocityX = (int)velocity.X;
            int velocityY = (int)velocity.Y;

            AABB boxAfterVelocity = new AABB(box.X + velocityX, box.Y + velocityY, box.Width, box.Height);

            // Get tiles that box is "on"
            touchedTiles.Clear();
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX, box.Y + velocityY)); // Top Left
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX + box.Width, box.Y + velocityY)); // Top Right
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX, box.Y + box.Height + velocityY)); // Bottom Left
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX + box.Width, box.Y + velocityY + box.Height)); // Bottom Right

            CollisionResult result = new CollisionResult { Intersect = false, WillIntersect = false };
            foreach (Tile tile in touchedTiles) {
                tile.Intersected = true;
                if (tile.IsSolid && tile.Shape == Shape.Box) {
                    tile.Collided = true;

                    var trans = MinimumTranslation(boxAfterVelocity, tile.AABB);
                    result.Intersect = true;
                    result.WillIntersect = true;
                    result.MinimumTranslationVector = trans;
                    return result;
                }
            }
            return result;
        }

        public CollisionResult IsColliding(AABB box, TileMap tileMap, Vector velocity) {

            // Broad Phase
            // -----------------

            // Get tiles that box is "on"
            int velocityX = (int)velocity.X;
            int velocityY = (int)velocity.Y;
            touchedTiles.Clear();
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX, box.Y + velocityY)); // Top Left
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX + box.Width, box.Y + velocityY)); // Top Right
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX, box.Y + box.Height + velocityY)); // Bottom Left
            touchedTiles.Add(tileMap.PositionToTile(box.X + velocityX + box.Width, box.Y + velocityY + box.Height)); // Bottom Right


            // Narrow phase
            // -----------------

            float highestMagnitude = 0;
            CollisionResult result = new CollisionResult { Intersect = false, WillIntersect = false };
            foreach (Tile tile in touchedTiles) {
                tile.Intersected = true;
                if (tile.IsSolid && tile.Shape == Shape.Box) {
                    tile.Collided = true;

                    var collision = AABBtoAABB(box, tile.AABB, velocity);
                    if (collision.Intersect || collision.WillIntersect)
                    {
                        if (collision.MinimumTranslationVector.Magnitude > highestMagnitude)
                            result = collision;
                    }
                }
            }
            return result;
        }

        // Check if polygon A is going to collide with polygon B for the given velocity
        public CollisionResult AABBtoAABB(AABB a, AABB b, Vector velocity)
        {
            CollisionResult result = new CollisionResult { Intersect = true, WillIntersect = true };

            int edgeCountA = 4;
            int edgeCountB = 4;

            /*
            int edgeCountA = polygonA.Edges.Count;
            int edgeCountB = polygonB.Edges.Count;
             * */
            float minIntervalDistance = float.PositiveInfinity;
            Vector translationAxis = new Vector();
            Vector edge;

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = a.Edges[edgeIndex];
                }
                else
                {
                    edge = b.Edges[edgeIndex - edgeCountA];
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector axis = new Vector(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, a, ref minA, ref maxA);
                ProjectPolygon(axis, b, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (IntervalDistance(minA, maxA, minB, maxB) > 0) result.Intersect = false;

                // ===== 2. Now find if the polygons *will* intersect =====

                // Project the velocity on the current axis
                float velocityProjection = axis.DotProduct(velocity);

                // Get the projection of polygon A during the movement
                if (velocityProjection < 0)
                {
                    minA += velocityProjection;
                }
                else
                {
                    maxA += velocityProjection;
                }

                // Do the same test as above for the new projection
                float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                if (intervalDistance > 0) result.WillIntersect = false;

                // If the polygons are not intersecting and won't intersect, exit the loop
                if (!result.Intersect && !result.WillIntersect) break;

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector
                intervalDistance = Math.Abs(intervalDistance);
                if (intervalDistance < minIntervalDistance)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    Vector d = a.Center - b.Center;
                    if (d.DotProduct(translationAxis) < 0) translationAxis = -translationAxis;
                }
            }

            // The minimum translation vector can be used to push the polygons appart.
            // First moves the polygons by their velocity
            // then move polygonA by MinimumTranslationVector.
            if (result.WillIntersect) result.MinimumTranslationVector = translationAxis * minIntervalDistance;

            return result;
        }

        // Calculate the distance between [minA, maxA] and [minB, maxB]
        // The distance will be negative if the intervals overlap
        public float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }
            return minA - maxB;
        }

        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        public void ProjectPolygon(Vector axis, AABB aabb, ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            float d = axis.DotProduct(aabb.Points[0]);
            min = d;
            max = d;
            for (int i = 0; i < aabb.Points.Count; i++)
            {
                d = aabb.Points[i].DotProduct(axis);
                if (d < min) {
                    min = d;
                } else {
                    if (d > max) {
                        max = d;
                    }
                }
            }
        }
    }
}
