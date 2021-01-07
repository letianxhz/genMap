using System.Collections.Generic;
using map;
using UnityEngine;

namespace d3_delaunay_cs
{
    public class NoisyEdges
    {
        public static float NOISY_LINE_TRADEOFF = 0.5F;
        public Dictionary<int, List<Vector2>> path0 = new Dictionary<int, List<Vector2>>();
        public Dictionary<int, List<Vector2>> path1 = new Dictionary<int, List<Vector2>>();
        public static List<Vector2> points;// = new List<Vector2> ();

        public NoisyEdges()
        {
        }

        public void buildNoisyEdges(DelaunayVoronoi d,  System.Random random)
        {
            foreach (var center in d.Points)
            {
                
            }
        }
        
        
        public static void subdivide(Vector2 A, Vector2 B, Vector2 C, Vector2 D, int minLength){

            if ((A - C).magnitude < minLength || (B - D).magnitude < minLength) {
                return;
            }
		
            var p = Random.Range(0.2f, 0.8f);
            var q = Random.Range (0.2f, 0.8f);
            //var p = Random.Range(0.2F, 0.8F);
            //var q = Random.Range (0.2F, 0.8F);
		
            var E = Vector2.Lerp (A, D, p);
            var F = Vector2.Lerp (B, C, p);
            var G = Vector2.Lerp (A, B, q);
            var I = Vector2.Lerp (D, C, q);
		
            var H = Vector2.Lerp (E, F, q);
		
            var s = 1.0F - Random.Range (-1.0f, +1.0f);
            var t = 1.0F - Random.Range(-1.0f, +1.0f);
            //var s = 1.0F - Random.Range (-0.9F, +0.9F);
            //var t = 1.0F - Random.Range (-0.9F, +0.9F);

            subdivide (A, Vector2.Lerp (G, B, s), H, Vector2.Lerp (E, D, t), minLength);
            points.Add (H);
            subdivide (H, Vector2.Lerp (F, C, s), H, Vector2.Lerp (I, D, t), minLength);
        }
    }
}