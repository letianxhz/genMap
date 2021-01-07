using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace map
{
    public class Map
    {
        public List<Vector2> points; // Only useful during map construction
        public Dictionary<int, Vector2> index2Center = new Dictionary<int, Vector2>();
        public List<Graphs.Center> centers; //维洛图中心点信息
        public List<Graphs.Edge>  edges = new List<Graphs.Edge>(); //边缘四边形
        public int numPoints; // 记录生成的维洛点点个数
        public Dictionary<int, VoronoiBoundary> margins = new Dictionary<int, VoronoiBoundary>(); //key centter pos index 
        
        public Dictionary<string, int> path = new Dictionary<string, int>();//相邻边的索引信息
        
        public NoisyEdges noisyEdges = new NoisyEdges(); //维洛图边界边嘈杂算法
        
        public Map(){
            numPoints = 0;
            reset();
        }
        
        public void reset(){
            if (points != null)
                points.Clear();

            if (edges != null) {
                foreach (var edge in edges) {
                    edge.d0 = edge.d1 = Vector2.zero;
                    edge.c0 = edge.c1 = Vector2.zero;;
                }
                edges.Clear();
            }

            if (centers != null) {
                foreach(var p in centers){
                    p.neighbors.Clear();
                    p.borders.Clear();
                }
                centers.Clear();
            }

            if (points == null) points = new List<Vector2> ();
            if (edges == null) edges = new List<Graphs.Edge> ();
            if (centers == null) centers = new List<Graphs.Center> ();
        }

        private int pathIndex = 0;
        public void buildGraph(DelaunayVoronoi d)
        {
            pathIndex = 0;
            Graphs.Center p;
            
            foreach (var center in d.Points)
            {
                p = new Graphs.Center();
                p.index = centers.Count;
                p.point = new Vector2(center.X, center.Y);
                p.neighbors = new List<Graphs.Center>();
                p.borders = new List<Graphs.Edge>();
                centers.Add(p);
                ++numPoints;
                
                var hoverIndex = d.Delaunay.find(center.X, center.Y);
                
                index2Center.Add(hoverIndex, p.point);
                
                var polygon = d.CellPolygons[hoverIndex];
                    
                var points = polygon
                    .Select(point => new Vector2((int) point[0], (int) point[1])).ToArray();
                
                if (polygon.Any(point => double.IsNaN(point[0]) || double.IsNaN(point[1]))) break;
                
                var v = new  VoronoiBoundary();
                v.center = new Vector2(center.X, center.Y);

                var par = points[0];
                for (int i = 1; i < points.Length; i++)
                {
                    if (par.Equals(points[i]))
                    {
                        continue;
                    }

                    par = points[i];
                    
                    v.boundarys.Add(points[i]);
                    string key1 = points[i].ToString() + points[i - 1].ToString();
                    string key2 = points[i - 1].ToString() + points[i].ToString();

                    var index = pathIndex;
                    //两个点。两个方向都没有索引 需要添加新的
                    if (!path.ContainsKey(key1) && !path.ContainsKey(key2))
                    {
                        ++pathIndex;
                        index = pathIndex;
                    }

                    if (!path.ContainsKey(key2))
                    {
                        path.Add(key2, index);
                    }

                    if (!path.ContainsKey(key1))
                    {
                        path.Add(key1, index);   
                    }

                    path.TryGetValue(key1, out index);
                    
                    v.index2Boundary.Add(index, new Vector2[] {points[i - 1], points[i]});
                }
                margins.Add(hoverIndex, v);
            }

            edges.Clear();
            edges.AddRange(getEdges(d));
            noisyEdges.BuildNoisyEdges(this);
        }
        
        /**
         * 获取构建德罗内和维洛图对偶点形成点四边形信息
         */
        public List<Graphs.Edge> getEdges(DelaunayVoronoi d)
        {
            List<Graphs.Edge> ret = new List<Graphs.Edge>();
            foreach (var center in d.Points)
            {
                var hoverIndex = d.Delaunay.find(center.X, center.Y);

                
                var polygon = d.CellPolygons[hoverIndex];
                if (polygon.Any(point => double.IsNaN(point[0]) || double.IsNaN(point[1]))) break;
                VoronoiBoundary vb = null;    
                var succ= margins.TryGetValue(hoverIndex, out vb); 
                
                
                foreach (var polygonIndex in d.Delaunay.neighbors(hoverIndex))
                {
                    VoronoiBoundary checkVb = null;    
                    var ok= margins.TryGetValue((int)polygonIndex, out checkVb);
                    
                    Graphs.Edge edge = new Graphs.Edge();
                    

                    if (center.X < checkVb.center.x)
                    {
                        edge.c0 = new Vector2(center.X, center.Y);
                        edge.c1 = checkVb.center;
                    }
                    else
                    {
                        edge.c1 = new Vector2(center.X, center.Y);
                        edge.c0 = checkVb.center;
                    }

                    bool ch = false;
                    foreach (KeyValuePair<int, Vector2[]> kv in checkVb.index2Boundary)
                    {
                        if (vb.index2Boundary.ContainsKey(kv.Key))
                        {

                            if (kv.Value[0].x < kv.Value[1].x)
                            {
                                edge.d0 = kv.Value[0];
                                edge.d1 = kv.Value[1];
                            }
                            else
                            {
                                edge.d1 = kv.Value[0];
                                edge.d0 = kv.Value[1];
                            }
                            edge.index = kv.Key;
                            ch = true;
                        }
                    }

                    if (ch)
                    {
                        ret.Add(edge);
                    }
                }
            }

            return ret;
        }

        
        
        /**
         * 维洛多边形共边信息数据
         */
        public class VoronoiBoundary
        {
            public Vector2 center; //维洛中心点
            public List<Vector2> boundarys = new List<Vector2>(); //多边形边界点
            public Dictionary<int, Vector2[]> index2Boundary = new Dictionary<int, Vector2[]>(); //边界边索引数据 key：边界边的索引 value: 组成该边的两个点
        }
        
    }
}