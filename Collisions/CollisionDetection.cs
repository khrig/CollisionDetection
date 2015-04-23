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
        // Structure that stores the results of the PolygonCollision function
        public struct PolygonCollisionResult
        {
            public bool WillIntersect; // Are the polygons going to intersect forward in time?
            public bool Intersect; // Are the polygons currently intersecting
            public Vector MinimumTranslationVector; // The translation to apply to polygon A to push the polygons appart.
        }

        // Check if polygon A is going to collide with polygon B for the given velocity
        public PolygonCollisionResult PolygonCollision(Polygon polygonA, Polygon polygonB, Vector velocity)
        {
            PolygonCollisionResult result = new PolygonCollisionResult();
            result.Intersect = true;
            result.WillIntersect = true;

            int edgeCountA = polygonA.Edges.Count;
            int edgeCountB = polygonB.Edges.Count;
            float minIntervalDistance = float.PositiveInfinity;
            Vector translationAxis = new Vector();
            Vector edge;

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = polygonA.Edges[edgeIndex];
                }
                else
                {
                    edge = polygonB.Edges[edgeIndex - edgeCountA];
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector axis = new Vector(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, polygonB, ref minB, ref maxB);

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

                    Vector d = polygonA.Center - polygonB.Center;
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
            else
            {
                return minA - maxB;
            }
        }

        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        public void ProjectPolygon(Vector axis, Polygon polygon, ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            float d = axis.DotProduct(polygon.Points[0]);
            min = d;
            max = d;
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                d = polygon.Points[i].DotProduct(axis);
                if (d < min)
                {
                    min = d;
                }
                else
                {
                    if (d > max)
                    {
                        max = d;
                    }
                }
            }
        }


        List<Tile> touchedTiles;
        public CollisionDetection() {
            touchedTiles = new List<Tile>();
        }

        public bool IsColliding(AABB box, TileMap tileMap, out Vector response) {

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

            Vector currentPos = new Vector(box.X, box.Y);
            response = new Vector(0, 0);
            foreach (Tile tile in touchedTiles.Where(t => t.Collided)) {
                if (Overlapping(new AABB((int)currentPos.X, (int)currentPos.Y, 32, 32), tile.AABB)) {
                    var overlapX = (box.Width/2
                                    + tile.AABB.Width/2)
                                   - (Math.Abs((currentPos.X + box.Width / 2) - (tile.AABB.X + tile.AABB.Width / 2)));

                    var overlapY = (box.Height / 2
                                + tile.AABB.Height / 2)
                               - ((tile.AABB.Y + tile.AABB.Height / 2)
                                  - (currentPos.Y + box.Height / 2));

                    if (overlapY < overlapX)
                    {
                        if (overlapY >= 0 && currentPos.Y < tile.AABB.Y)
                        {
                            // over 
                            response.Y -= overlapY + 1;
                            currentPos.Y += response.Y;
                        }
                        else if (overlapY >= 0 && currentPos.Y > tile.AABB.Y)
                        {
                            response.Y += overlapY + 1;
                            currentPos.Y += response.Y;
                        }
                        
                    } else {
                        if (overlapX >= 0 && currentPos.X < tile.AABB.X)
                        {
                            // left side
                            response.X -= overlapX + 1;
                            currentPos.X += response.X;
                        }
                        else if (overlapX >= 0 && currentPos.X > tile.AABB.X)
                        {
                            response.X += overlapX + 1;
                            currentPos.X += response.X;
                        }
                    }
                }


                /*
                if (overlapX < overlapY) 
                    currentPos.X -= overlapX;
                */
                /*
                overlapY = (Game1.p[0].boundingBox.halfHeight
                         + Game1.p[1].boundingBox.halfHeight)
                         - (Game1.p[1].boundingBox.center.Y
                         - Game1.p[0].boundingBox.center.Y);
                */


               // var diff = new Vector2(currentPos.X + box.Width - tile.AABB.X, currentPos.Y + box.Width - tile.AABB.Y);

                // If no overlap - keep going
                //if((diff.X < -box.Width || diff.X > tile.AABB.Width) && (diff.Y < -box.Height || diff.Y > tile.AABB.Height))
                  //  continue;

                // Check X first

                /* 
                 * 
                 * If diff.x is smaller than -O1.width, then O1 is on the far left. It can move freely on the horizontal. So S.x remains as is.

Else if diff.x is bigger than O2.width, then O1 is on the far right, and can move freely on the horizontal. So S.x remains as is.

Otherwise, we have a situation where O1 overlaps O2 on the horizontal. So just check if(diff.x<0) S.x -= O1.width+diff.x; and, respectively if(diff.x>=0) S.x += O2.width-diff.x;
                 * */

                //if (!(diff.X < -box.Width || diff.X > tile.AABB.Width)) {
                //        if (diff.X < 0)
                //            response.X += diff.X;
                //        if (diff.X >= 0)
                //            response.X -= diff.X;

                //        currentPos.X += response.X;
                //        diff = new Vector2(currentPos.X + box.Width - tile.AABB.X, currentPos.Y + box.Width - tile.AABB.Y);
                //    }
                
                // Then check Y (after X move)

                /*
                 * If diff.y is smaller than -O1.height, then O1 is far below. It can move freely on the vertical. So S.y remains as is.

Else if diff.y is bigger than O2.height, then O1 is far up, and can move freely on the vertical. So S.y remains as is.

Otherwise, we have a situation where O1 overlaps O2 on the vertical. So just check if(diff.y<0) S.y -= O1.height+diff.y; and, respectively if(diff.y>=0) S.y += O2.height-diff.y;
                 * */

                //if (!(diff.Y < -box.Height || diff.Y > tile.AABB.Height)) {
                //    if (diff.Y < 0) 
                //        response.Y += diff.Y;
                //    if (diff.Y >= 0) 
                //        response.Y -= diff.Y;

                //    currentPos.Y += response.Y;
                //    diff = new Vector2(currentPos.X + box.Width - tile.AABB.X, currentPos.Y + box.Width - tile.AABB.Y);
                //}
            }
            
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
