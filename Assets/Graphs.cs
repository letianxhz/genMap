using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace map
{
    public class Graphs
    {
        //维洛图信息
        public class Center {
            public int index;
            public Vector2 point; //中心点坐标
            public bool border;
            public float elevation;
            public float moisture;
            public List<Center> neighbors;//相邻的维洛图信息
            public List<Edge> borders;//划分的mesh点
            public List<Corner> corners;//凸边形的点
            
            
            
        }
        
        //维洛图的多边形角上的点
        public class Corner {
            public int index;
            public Vector2 point; //坐标
            public bool border;
            public float elevation;
            public float moisture;
            public int watershed_size;
            public Corner downslope;
            public Corner watershed;
            public List<Center> touches; //归属的维洛图中心点
            public List<Edge> protrudes;
            public List<Corner> adjacent;
        }
        
        //mesh 四边形 用于构建维洛图边界线随机
        public class Edge {
            public int index;
            public Vector2 c0, c1; //两个维洛图中心点
            public Vector2 d0, d1; //维洛图边上的点
            public Vector2 midpoint;//中央点
        }
    }
    
}