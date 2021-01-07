using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace map
{
    public class Map
    {

        public class Margine
        {
            public Vector2 center; //维洛中心点
            public List<Vector2> edges = new List<Vector2>(); //多边形边界点
            public Dictionary<int, Vector2[]> edgeIndex = new Dictionary<int, Vector2[]>();
        }
        
        public static float SIZE;
        public List<Vector2> points; // Only useful during map construction
        public Dictionary<int, Vector2> index2Center = new Dictionary<int, Vector2>();
        public List<Graphs.Center> centers; //维洛图中心点信息
        public List<Graphs.Corner> corners;
        public List<Graphs.Edge>  edges = new List<Graphs.Edge>(); //边缘四边形
        public int numPoints;
        public Dictionary<int, Margine> margins = new Dictionary<int, Margine>(); //key centter pos index 
        
        public Dictionary<string, int> path = new Dictionary<string, int>();//相邻边的索引信息
        
        public NoisyEdges noisyEdges = new NoisyEdges(); //维洛图边界边嘈杂算法
        
        public Map(float size){
            SIZE = size;
            numPoints = 1;
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
                    p.corners.Clear();
                    p.borders.Clear();
                }
                centers.Clear();
            }

            if (corners != null) {
                foreach (var q in corners){
                    q.adjacent.Clear();
                    q.touches.Clear();
                    q.protrudes.Clear();
                    q.downslope = null;
                    q.watershed = null;
                }
                corners.Clear();
            }

            if (points == null) points = new List<Vector2> ();
            if (edges == null) edges = new List<Graphs.Edge> ();
            if (centers == null) centers = new List<Graphs.Center> ();
            if (corners == null) corners = new List<Graphs.Corner> ();
        }

        private int pathIndex = 0;
        public void buildGraph(DelaunayVoronoi d)
        {
            pathIndex = 0;
            Graphs.Center p;
            var centerLoopup = new Dictionary<Vector2, Graphs.Center>();
            
            foreach (var center in d.Points)
            {
                p = new Graphs.Center();
                p.index = centers.Count;
                p.point = new Vector2(center.X, center.Y);
                p.neighbors = new List<Graphs.Center>();
                p.borders = new List<Graphs.Edge>();
                p.corners = new List<Graphs.Corner>();
                centers.Add(p);
                centerLoopup[p.point] = p;
                
                var hoverIndex = d.Delaunay.find(center.X, center.Y);
                
                index2Center.Add(hoverIndex, p.point);
                
                var polygon = d.CellPolygons[hoverIndex];
                    
                var points = polygon
                    .Select(point => new Vector2((int) point[0], (int) point[1])).ToArray();
                
                if (polygon.Any(point => double.IsNaN(point[0]) || double.IsNaN(point[1]))) break;
                
                var marginPos = new  Margine();
                marginPos.center = new Vector2(center.X, center.Y);

                var par = points[0];
                for (int i = 1; i < points.Length; i++)
                {
                    if (par.Equals(points[i]))
                    {
                        continue;
                    }

                    par = points[i];
                    
                    marginPos.edges.Add(points[i]);
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
                    
                    marginPos.edgeIndex.Add(index, new Vector2[] {points[i - 1], points[i]});
                }
                

                /*
                foreach (var pos in polygon)
                {
                    marginPos.edges.Add(new Vector2((float)pos[0], (float)pos[1]));
                }*/
                
                margins.Add(hoverIndex, marginPos);
                
                if (polygon.Any(point => double.IsNaN(point[0]) || double.IsNaN(point[1]))) break;
                foreach (var polygonIndex in d.Delaunay.neighbors(hoverIndex))
                {
                    var ps = d.CellPolygons[(int)polygonIndex];
                }
            }

            edges.Clear();
            edges.AddRange(getEdges(d));
            noisyEdges.buildNoisyEdges(this);
            
            
            
            
            var  _cornerMap = new Dictionary<int, List<Graphs.Corner>> ();
            Func<Vector2, Graphs.Corner> makeCorner = delegate(Vector2 point) {
                int bucket;
			
                if (point == Vector2.zero) { return null; }
			
                for (bucket = (int)(point.x)-1; bucket <= (int)(point.x)+1; bucket++) {
                    if(_cornerMap.ContainsKey(bucket)){
                        foreach(var c in _cornerMap[bucket]){
                            var dx = point.x - c.point.x;
                            var dy = point.y - c.point.y;
                            if(dx*dx+dy*dy<1e-6F){
                                return c;
                            }
                        }
                    }
                }
			
                bucket = (int)point.x;
                if (!_cornerMap.ContainsKey (bucket)) {
                    _cornerMap.Add(bucket, null);
                }
                if(_cornerMap[bucket] == null) {_cornerMap[bucket] = new List<Graphs.Corner>();}
			
			
                var q = new Graphs.Corner ();
                q.index = corners.Count;
                corners.Add (q);
                q.point = point;
                q.border = (point.x == 0F || point.x == SIZE || point.y == 0F || point.y == SIZE);
                q.touches = new List<Graphs.Center> ();
                q.protrudes = new List<Graphs.Edge> ();
                q.adjacent = new List<Graphs.Corner> ();
                _cornerMap[bucket].Add(q);
                return q;
            };
            
            Action<List<Graphs.Corner>, Graphs.Corner> addToCornerList = delegate(List<Graphs.Corner> v, Graphs.Corner x) 
            {
                if (x != null && v.IndexOf (x) < 0) { v.Add (x); }
            };
            
            Action<List<Graphs.Center>, Graphs.Center> addToCenterList = delegate(List<Graphs.Center> v, Graphs.Center x) 
            {
                if (x != null && v.IndexOf (x) < 0) { v.Add (x); }
            };

            foreach (var center in d.Points)
            {
                var hoverIndex = d.Delaunay.find(center.X, center.Y);

                var polygon = d.CellPolygons[hoverIndex];
                if (polygon.Any(point => double.IsNaN(point[0]) || double.IsNaN(point[1]))) break;
                
                
                foreach (var polygonIndex in d.Delaunay.neighbors(hoverIndex))
                {
                }
            }
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
                Margine margine = null;    
                var succ= margins.TryGetValue(hoverIndex, out margine); 
                
                
                foreach (var polygonIndex in d.Delaunay.neighbors(hoverIndex))
                {
                    Margine checkMargine = null;    
                    var ok= margins.TryGetValue((int)polygonIndex, out checkMargine);
                    
                    Graphs.Edge edge = new Graphs.Edge();
                    

                    if (center.X < checkMargine.center.x)
                    {
                        edge.c0 = new Vector2(center.X, center.Y);
                        edge.c1 = checkMargine.center;
                    }
                    else
                    {
                        edge.c1 = new Vector2(center.X, center.Y);
                        edge.c0 = checkMargine.center;
                    }

                    bool ch = false;
                    foreach (KeyValuePair<int, Vector2[]> kv in checkMargine.edgeIndex)
                    {
                        if (margine.edgeIndex.ContainsKey(kv.Key))
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

    }
}