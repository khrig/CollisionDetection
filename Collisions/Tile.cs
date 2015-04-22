using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collisions
{
    public enum Shape {
        Box
    }

    public class Tile
    {
        public string Name { get; set; }
        public bool IsSolid { get; set; }
        public bool Intersected { get; set; }
        public AABB AABB { get; private set; }
        public bool Collided { get; set; }
        public Shape Shape { get; set; }

        public Tile(string name, int x, int y, int width, int height)
        {
            Name = name;
            AABB = new AABB(x * width, y * height, width, height);
        }
    }
}
