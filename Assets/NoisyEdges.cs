using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace map
{
    public class NoisyEdges
    {
        public Dictionary<int, List<Vector2>> path = new Dictionary<int, List<Vector2>>();
        public static int level = 4;//递归层数
        public static float divisionSpan = 0.22f;
        public class Recursive
        {
            public List<Vector2> line = new List<Vector2>();
            public List<Quads> quads = new List<Quads>();
        }
        
        public class Quads
        {                
            public int level;
            public Vector2 a;
            public Vector2 b;
            public Vector2 p;
            public Vector2 q;

            public Quads(int level, Vector2 a, Vector2 b, Vector2 p, Vector2 q)
            {
                this.level = level;
                this.a = a;
                this.b = b;
                this.p = p;
                this.q = q;
            }
        }
        
        public void BuildNoisyEdges(Map map)
        {
            foreach (var edge in map.edges)
            {
                if (path.ContainsKey(edge.index))
                {
                    continue;
                }
                //1: 检查两个共边点边缘点分别到两个相邻到维洛中心点的夹角,如果超过150度, 该四边形就不做嘈杂处理
                //2: 检查共边边长太短也不做嘈杂处理
                if ((edge.d0 - edge.d1).sqrMagnitude < Math.Pow(GenMap.radius*(0.5), 2) || CheckAngle(edge))
                {   
                    path[edge.index] = new List<Vector2>();
                    path[edge.index].Add(new Vector2(edge.d0.x * GenMap._textureScale, edge.d0.y * GenMap._textureScale));
                    path[edge.index].Add(new Vector2(edge.d1.x * GenMap._textureScale, edge.d1.y * GenMap._textureScale));
                    continue;
                }
                edge.c0 = new Vector2(edge.c0.x * GenMap._textureScale, edge.c0.y * GenMap._textureScale);
                edge.d0 = new Vector2(edge.d0.x * GenMap._textureScale, edge.d0.y * GenMap._textureScale);
                edge.c1 = new Vector2(edge.c1.x * GenMap._textureScale, edge.c1.y * GenMap._textureScale);
                edge.d1 = new Vector2(edge.d1.x * GenMap._textureScale, edge.d1.y * GenMap._textureScale);
                path[edge.index] = genNoisyEdges(level, edge.d0, edge.d1, edge.c0, edge.c1).line;
            }
        }

        public Recursive genNoisyEdges(int lv, Vector2 d0, Vector2 d1, Vector2 v0, Vector2 v1)
        {
            if (lv == 0)
            {
                var ret = new Recursive();
                ret.line.Add(d0);
                ret.line.Add(d1);
                return ret;
            }

            var ap = Vector2.Lerp(d0, v0, 0.5f);
            var bp = Vector2.Lerp(d1, v0, 0.5f);
            var aq = Vector2.Lerp(d0, v1, 0.5f);
            var bq = Vector2.Lerp(d1, v1, 0.5f);
            
            
            var div = 0.5 * (1 - divisionSpan) + Random.Range(0.1f, 0.9f) * divisionSpan;

            var center = Vector2.Lerp(v0, v1, /*Random.Range(0.4f, 0.6f)*/(float)div);
            
            var ret1 = genNoisyEdges(lv - 1, d0, center, ap, aq);
            var ret2 = genNoisyEdges(lv - 1, center, d1, bp, bq);
		
            var result = new Recursive();
            result.line.AddRange(ret1.line);

            foreach (var point in ret2.line)
            {
                if (!result.line.Contains(point))
                {
                    result.line.Add(point);
                }
            }
            //result.line.AddRange(ret2.line);
            //result.quads.AddRange(ret1.quads);
            //result.quads.Add(new Quads(level, c0, c1, v0, v1));
            //result.quads.AddRange(ret2.quads);
            //ret1.line.AddRange(ret2.line);
            return result;
        }


        public bool CheckAngle(Graphs.Edge edge)
        {
            var angle = Vector2.Angle(edge.d0 - edge.c0 , edge.d0 - edge.c1);
            var angle1 = Vector2.Angle(edge.d1 - edge.c0, edge.d1 - edge.c1);

            return angle > 165 || angle1 > 165;
        }
    }
}