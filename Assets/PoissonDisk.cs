using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace map
{
    public static class PoissonDisk
    {
        public static int MapWidth = 1400;
        public static int MapHeight = 1400;
        
        public static double r = 10;
        public static int k = 200;
        public static double w = r / Math.Sqrt(2);
        public static int cols = (int)Math.Floor(MapWidth / w) + 1;
        public static int rows = (int)Math.Floor(MapHeight / w) + 1;
        
        public static System.Numerics.Vector2[,] grids; //泊松辅助格子
        public static List<System.Numerics.Vector2> actives = new List<System.Numerics.Vector2>();
        public static List<System.Numerics.Vector2> rets = new List<System.Numerics.Vector2>();

        public static List<System.Numerics.Vector2> genPoints(int seed)
        {
            
            grids = new System.Numerics.Vector2[cols, rows];
            for (var i = 0; i < cols; i++)
            {
               // Collections.grids[i] = ;
                for (var j = 0; j < rows; j++)
                {
                    grids[i, j] =new System.Numerics.Vector2(-1, -1);
                }
            }

            addFirstPoint();

            while (actives.Count > 0)
            {
                var random = new Random(seed);
                var index = random.Next(0, actives.Count);
                var p = actives[index];
                var find = false;
                for (var i = 0; i < k; i++)
                {
                    find |= getNewPoint(p, seed);
                }

                if (!find)
                {
                    actives.RemoveAt(index);
                }
            }
            return rets;
        }

        public static bool getNewPoint(System.Numerics.Vector2 point, int seed)
        {
            var find = false;
            var p = genRandomPoint(point, seed);
            if (p.X >= 0 && (p.X < MapHeight) && ((p.X >= 0) && (p.X < MapWidth)))
            {
                var gp = toGridPos(p);
                var ok = true;

                for (var i = Math.Max(0, gp.X - 2); i < Math.Min(rows, gp.X + 3) && ok; i++)
                {
                    for (var j = Math.Max(0, gp.Y - 2); j < Math.Min(rows, gp.Y + 3) && ok; j++)
                    {

                        var check = grids[(int)i, (int)j];
                        if (check.X < 0 && check.Y < 0)
                        {
                            continue;
                        }
                        var x = check.X - p.X;
                        var y = check.Y - p.Y;
                        var dis = Math.Sqrt(x * x + y * y);
                        if (dis < 1)
                        {
                            ok = false;
                        }
                    }
                }

                if (ok)
                {
                    find = true;
                    actives.Add(p);
                    rets.Add(p);
                    grids[(int) gp.X, (int) gp.Y] = p;
                }

            }
            return find;
        }

        public static System.Numerics.Vector2 toGridPos(System.Numerics.Vector2 p)
        {
            var gridX = p.X / w;
            var gridY = p.Y / w;
            return new System.Numerics.Vector2((int)Math.Floor(gridX), (int)Math.Floor(gridY));
        }

        public static System.Numerics.Vector2 genRandomPoint(System.Numerics.Vector2 point, int seed)
        {
            var random =  new Random(seed);
            var r1 = random.NextDouble();
            var r2 = random.NextDouble();
            var radius = r * (r1 + 1) - + 0.1;
            var angle = 2 * Math.PI * r2;
            var newX =  point.X + (int)(radius * Math.Cos(angle));
            var newY =  point.Y + (int)(radius * Math.Sin(angle));
            
            return new System.Numerics.Vector2(newX, newY);
        }

        public static void addFirstPoint()
        {
            var startX = MapHeight / 2;
            var startY = MapWidth / 2;

            var gx = (int)Math.Floor(startX / w);
            var gy = (int)Math.Floor(startY / w);
            
            System.Numerics.Vector2 startPoint = new System.Numerics.Vector2(startX, startY);
            //temp.Add(gy, startPoint);
            
            grids[(int) gx, (int) gy] = startPoint;
            
            actives.Add(startPoint);
            rets.Add(startPoint);
        }
    }
}