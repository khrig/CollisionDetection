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

        private readonly CollisionDetection collisionDetection;
        private readonly AABB player;

        public Form1()
        {
            InitializeComponent();
            this.KeyDown += Form1_KeyDown;
            this.Paint += Form1_Paint;

            collisionDetection = new CollisionDetection();
            tileMap = new TileMap(32, gridWidth, gridHeight);
            player = new AABB(100, 100, 32, 32);
        }

        private void Form1_Paint(object sender, PaintEventArgs e) {
            using (Graphics graphics = CreateGraphics())
            {
                ClearTileState();

                Pen playerBorder = new Pen(Color.Black, 2);
                if (collisionDetection.IsColliding(player, tileMap))
                {
                    playerBorder = new Pen(Color.Red, 2);
                }
                Pen borderPen = new Pen(Color.Blue, 2);
                Pen intersectedPen = new Pen(Color.CadetBlue, 2);
                Pen collidedPen = new Pen(Color.DarkRed, 2);
                SolidBrush brush = new SolidBrush(Color.LightSkyBlue);
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1)
                            tileMap.Tiles[x, y].IsSolid = true;

                        if (tileMap.Tiles[x, y].Collided) {
                            graphics.DrawRectangle(collidedPen, x * 32, y * 32, 32, 32);
                        }
                        else if (tileMap.Tiles[x, y].Intersected) {
                            graphics.DrawRectangle(intersectedPen, x * 32, y * 32, 32, 32);
                        }
                        else if (tileMap.Tiles[x, y].IsSolid) {
                            graphics.FillRectangle(brush, x * 32, y * 32, 32, 32);
                            graphics.DrawRectangle(borderPen, x * 32, y * 32, 32, 32);
                        }
                    }
                }

                SolidBrush blueBrush = new SolidBrush(Color.Gray);
                graphics.FillRectangle(blueBrush, player.X, player.Y, player.Width, player.Height);
                graphics.DrawRectangle(playerBorder, player.X, player.Y, player.Width, player.Height);

                brush.Dispose();
                intersectedPen.Dispose();
                borderPen.Dispose();
                collidedPen.Dispose();
            }
        }

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
            if (e.KeyCode == Keys.D)
            {
                player.X += 5;
            }
            if (e.KeyCode == Keys.Right)
            {
                player.X += 1;
            }
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left) {
                player.X -= 5;
            }
            if (e.KeyCode == Keys.W)
            {
                player.Y -= 5;
            }
            if (e.KeyCode == Keys.Up) {
                player.Y -= 1;
            }
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                player.Y += 5;
            }
            this.Refresh();
        }
    }
}
