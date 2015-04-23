using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Collisions
{
    public partial class Form1 : Form
    {
        private TileMap tileMap;
        private int gridWidth = 10;
        private int gridHeight = 10;
        private int tileSize = 32;

        private readonly CollisionDetection collisionDetection;
        private readonly CollisionHandler collisionHandler;
        private readonly AABB player;
        private readonly AABB obstacle;

        Pen borderPen = new Pen(Color.Blue, 2);
        Pen intersectedPen = new Pen(Color.CadetBlue, 2);
        Pen collidedPen = new Pen(Color.DarkRed, 2);
        SolidBrush brush = new SolidBrush(Color.LightSkyBlue);
        SolidBrush blueBrush = new SolidBrush(Color.Gray);
        Pen playerBorder = new Pen(Color.Black, 2);

        List<Polygon> polygons = new List<Polygon>();
        Polygon playerPoly;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;

            this.KeyDown += Form1_KeyDown;
            this.Paint += Form1_Paint;

            //Polygon p = new Polygon();
            //p.Points.Add(new Vector(0, 0));
            //p.Points.Add(new Vector(32, 0));
            //p.Points.Add(new Vector(32, 32));
            //p.Points.Add(new Vector(0, 32));

            //polygons.Add(p);

            //p = new Polygon();
            //p.Points.Add(new Vector(0, 0));
            //p.Points.Add(new Vector(32, 0));
            //p.Points.Add(new Vector(32, 32));
            //p.Points.Add(new Vector(0, 32));
            //p.Offset(100, 100);

            //polygons.Add(p);

            //foreach (Polygon polygon in polygons) polygon.BuildEdges();

            //playerPoly = polygons[0];

            collisionDetection = new CollisionDetection();
            collisionHandler = new CollisionHandler();
            tileMap = new TileMap(tileSize, gridWidth, gridHeight);
            player = new AABB(50, 50, tileSize, tileSize);
            obstacle = new AABB(100, 100, tileSize, tileSize);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            /*
            Vector p1;
            Vector p2;
            foreach (Polygon polygon in polygons)
            {
                for (int i = 0; i < polygon.Points.Count; i++)
                {
                    p1 = polygon.Points[i];
                    if (i + 1 >= polygon.Points.Count)
                    {
                        p2 = polygon.Points[0];
                    }
                    else
                    {
                        p2 = polygon.Points[i + 1];
                    }
                    e.Graphics.DrawLine(new Pen(Color.Black), p1, p2);
                }
            }
            */
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1)
                        tileMap.Tiles[x, y].IsSolid = true;

                    if (tileMap.Tiles[x, y].Collided)
                    {
                        e.Graphics.DrawRectangle(collidedPen, x * tileSize, y * tileSize, tileSize, tileSize);
                    }
                    else if (tileMap.Tiles[x, y].Intersected)
                    {
                        e.Graphics.DrawRectangle(intersectedPen, x * tileSize, y * tileSize, tileSize, tileSize);
                    }
                    else if (tileMap.Tiles[x, y].IsSolid)
                    {
                        e.Graphics.FillRectangle(brush, x * tileSize, y * tileSize, tileSize, tileSize);
                        e.Graphics.DrawRectangle(borderPen, x * tileSize, y * tileSize, tileSize, tileSize);
                    }
                }
            }
            
            e.Graphics.FillRectangle(blueBrush, player.X, player.Y, player.Width, player.Height);
            e.Graphics.DrawRectangle(playerBorder, player.X, player.Y, player.Width, player.Height);
            
            Invalidate();
        }

        //private void Form1_Paint(object sender, PaintEventArgs e) {
        //    Vector polygonATranslation = new Vector();

        //    Vector p1;
        //    Vector p2;
        //    foreach (Polygon polygon in polygons)
        //    {
        //        for (int i = 0; i < polygon.Points.Count; i++)
        //        {
        //            p1 = polygon.Points[i];
        //            if (i + 1 >= polygon.Points.Count)
        //            {
        //                p2 = polygon.Points[0];
        //            }
        //            else
        //            {
        //                p2 = polygon.Points[i + 1];
        //            }
        //            e.Graphics.DrawLine(new Pen(Color.Black), p1, p2);
        //        }
        //    }

            
        //    Invalidate();
        //}

        //private void Form1_Paint(object sender, PaintEventArgs e) {
        //    using (Graphics graphics = CreateGraphics())
        //    {
        //        Pen playerBorder = new Pen(Color.Black, 2);
        //        graphics.DrawLine(playerBorder, 0, 322, 320, 322);

        //        ClearTileState();

        //        Vector response = new Vector(0, 0);
        //        if (collisionDetection.IsColliding(player, tileMap, out response))
        //        {
        //            playerBorder = new Pen(Color.Red, 2);
        //        }
        //        
        //        for (int x = 0; x < gridWidth; x++)
        //        {
        //            for (int y = 0; y < gridHeight; y++)
        //            {
        //                if (x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1)
        //                    tileMap.Tiles[x, y].IsSolid = true;

        //                if (tileMap.Tiles[x, y].Collided) {
        //                    graphics.DrawRectangle(collidedPen, x * tileSize, y * tileSize, tileSize, tileSize);
        //                }
        //                else if (tileMap.Tiles[x, y].Intersected) {
        //                    graphics.DrawRectangle(intersectedPen, x * tileSize, y * tileSize, tileSize, tileSize);
        //                }
        //                else if (tileMap.Tiles[x, y].IsSolid) {
        //                    graphics.FillRectangle(brush, x * tileSize, y * tileSize, tileSize, tileSize);
        //                    graphics.DrawRectangle(borderPen, x * tileSize, y * tileSize, tileSize, tileSize);
        //                }
        //            }
        //        }

        //        
        //        graphics.FillRectangle(blueBrush, player.X, player.Y, player.Width, player.Height);
        //        graphics.DrawRectangle(playerBorder, player.X, player.Y, player.Width, player.Height);


        //        Pen responsePen = new Pen(Color.Yellow, 2);
        //        if (response.X != 0 || response.Y != 0)
        //            graphics.DrawRectangle(responsePen, player.X + response.X, player.Y + response.Y, player.Width, player.Height);

        //    }
        //}

        private void ClearTileState()
        {
            for (int x = 0; x < gridWidth; x++) {
                for (int y = 0; y < gridHeight; y++) {
                    tileMap.Tiles[x, y].Intersected = false;
                    tileMap.Tiles[x, y].Collided = false;
                }
            }
        }
        
        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            ClearTileState();
            Vector velocity = new Vector();

            int speed = 5;

            if (e.KeyCode == Keys.D)
            {
                velocity.X += speed;
            }
            if (e.KeyCode == Keys.Right)
            {
                velocity.X += speed;
            }
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) {
                velocity.X -= speed;
            }
            if (e.KeyCode == Keys.W)
            {
                velocity.Y -= speed;
            }
            if (e.KeyCode == Keys.Up) {
                velocity.Y -= speed;
            }
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                velocity.Y += speed;
            }

            Vector playerTranslation = velocity;

            var r = collisionHandler.IsCollidingSimple(player, tileMap, velocity);
            //CollisionResult r = collisionHandler.IsColliding(player, tileMap, velocity);
            if (r.WillIntersect || r.Intersect)
            {
                playerTranslation = velocity + r.MinimumTranslationVector;
            }

            /*
            foreach (Polygon polygon in polygons)
            {
                if (polygon == playerPoly) continue;

                CollisionResult r = collisionHandler.AABBtoAABB(player, obstacle, velocity);

                if (r.WillIntersect)
                {
                    playerTranslation = velocity + r.MinimumTranslationVector;
                    break;
                }
            }
            */

            player.Offset(playerTranslation);
        }
    }
}
