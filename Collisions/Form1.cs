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
        private readonly AABB player;

        List<Polygon> polygons = new List<Polygon>();
        Polygon playerPoly;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;

            this.KeyDown += Form1_KeyDown;
            this.Paint += Form1_Paint;

            Polygon p = new Polygon();
            p.Points.Add(new Vector(100, 0));
            p.Points.Add(new Vector(150, 50));
            p.Points.Add(new Vector(100, 150));
            p.Points.Add(new Vector(0, 100));

            polygons.Add(p);

            p = new Polygon();
            p.Points.Add(new Vector(50, 50));
            p.Points.Add(new Vector(100, 0));
            p.Points.Add(new Vector(150, 150));
            p.Offset(80, 80);

            polygons.Add(p);

            foreach (Polygon polygon in polygons) polygon.BuildEdges();

            playerPoly = polygons[0];

            collisionDetection = new CollisionDetection();
            tileMap = new TileMap(tileSize, gridWidth, gridHeight);
            player = new AABB(100, 100, tileSize, tileSize);
        }

        private void Form1_Paint(object sender, PaintEventArgs e) {
            Vector polygonATranslation = new Vector();

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

            
            Invalidate();
        }

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
        //        Pen borderPen = new Pen(Color.Blue, 2);
        //        Pen intersectedPen = new Pen(Color.CadetBlue, 2);
        //        Pen collidedPen = new Pen(Color.DarkRed, 2);
        //        SolidBrush brush = new SolidBrush(Color.LightSkyBlue);
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

        //        SolidBrush blueBrush = new SolidBrush(Color.Gray);
        //        graphics.FillRectangle(blueBrush, player.X, player.Y, player.Width, player.Height);
        //        graphics.DrawRectangle(playerBorder, player.X, player.Y, player.Width, player.Height);


        //        Pen responsePen = new Pen(Color.Yellow, 2);
        //        if (response.X != 0 || response.Y != 0)
        //            graphics.DrawRectangle(responsePen, player.X + response.X, player.Y + response.Y, player.Width, player.Height);

        //        brush.Dispose();
        //        intersectedPen.Dispose();
        //        borderPen.Dispose();
        //        collidedPen.Dispose();
        //        responsePen.Dispose();
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
            Vector velocity = new Vector();
            
            if (e.KeyCode == Keys.D)
            {
                velocity.X += 5;
            }
            if (e.KeyCode == Keys.Right)
            {
                velocity.X += 1;
            }
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) {
                velocity.X -= 5;
            }
            if (e.KeyCode == Keys.W)
            {
                velocity.Y -= 5;
            }
            if (e.KeyCode == Keys.Up) {
                velocity.Y -= 1;
            }
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                velocity.Y += 5;
            }

            Vector playerTranslation = velocity;

            foreach (Polygon polygon in polygons)
            {
                if (polygon == playerPoly) continue;

                CollisionDetection.PolygonCollisionResult r = collisionDetection.PolygonCollision(playerPoly, polygon, velocity);

                if (r.WillIntersect)
                {
                    playerTranslation = velocity + r.MinimumTranslationVector;
                    break;
                }
            }

            playerPoly.Offset(playerTranslation);
        }
    }
}
