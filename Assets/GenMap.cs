using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using map;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class GenMap : MonoBehaviour
{
    public const int radius = 20;
    public int k = 30;
    
    public const int WIDTH = 2048;
    public const int HEIGHT = 2048;
    public const int _textureScale = 7; //用于画图像素点，使地图更加精细
    
    public Map m = new Map();
    
    public Dictionary<string, bool> linePoints = new Dictionary<string, bool>(); //用来记录已经画过点线
    
    // Update is called once per frame
    void Update()
    {
        
    }

    void Start()
    {
        DelaunayVoronoi d1 = new DelaunayVoronoi(WIDTH, HEIGHT, (int)1, radius);
        var w = WIDTH * _textureScale;
        var h = HEIGHT * _textureScale;
        Texture2D texture2D = new Texture2D(w, h);
        texture2D.SetPixels(Enumerable.Repeat(Color.gray, h * w).ToArray());
        
        m.buildGraph(d1);
        //DrawDelaunay(d1, texture2D);
        //DrawVoronoi(d1, texture2D);
        DrawVoronoiEdge(d1, texture2D);
        //BuildAssistGrids(d1, texture2D);
        
        texture2D.Apply();
        byte[] bytes1 = texture2D.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/test.png", bytes1);
    }

    /**
     * 画维洛图凸边形轮廓
     */
    void DrawVoronoi(DelaunayVoronoi d, Texture2D texture2D)
    {
        foreach (var polygon in d.CellPolygons)
        {
            //texture2D.FillPolygon(polygon.Select(point => new Vector2((int)point[0] * _textureScale, (int)point[1] * _textureScale )).ToArray(), GetRandomColor());
            //int x0, int y0, int x1, int y1, Color col
            var points = polygon
                .Select(point => new Vector2((int) point[0] * _textureScale, (int) point[1] * _textureScale)).ToArray();
            Vector2 startpos = points[0];
            texture2D.SetPixel((int)startpos.x, (int)startpos.y, Color.blue);
            Color color = GetRandomColor();
            for (int i = 1; i < points.Length; i++)
            {
                Vector2 nextPos = points[i];
                texture2D.SetPixel((int)nextPos.x, (int)nextPos.y, Color.blue);
                string key1 = startpos.ToString() + nextPos.ToString();
                string key2 = nextPos.ToString() + startpos.ToString();
                if (!linePoints.ContainsKey(key1) && !linePoints.ContainsKey(key2))
                {
                    texture2D.DrawLine((int)startpos.x, (int)startpos.y, (int)nextPos.x, (int)nextPos.y, Color.white);
                    linePoints.Add(key1, true);
                }
                startpos = nextPos;
            }
        }
    }

    /**
     * 画德诺内三角
     */
    void DrawDelaunay(DelaunayVoronoi d, Texture2D texture2D)
    {
        foreach (var des in d.Delaunay.trianglePolygons())
        {
               List<Double> startpos = des[0];
               for (int i = 1; i < des.Count; i++)
               {
                   List<Double> nextPos = des[i];
                   //texture2D.SetPixel((int)nextPos.x, (int)nextPos.y, Color.blue);
                   texture2D.DrawLine((int)startpos[0]*_textureScale, (int)startpos[1]*_textureScale, (int)nextPos[0]*_textureScale, (int)nextPos[1]*_textureScale, Color.green);
                   startpos = nextPos;
               }
        }
    }

    /**
     * 画生成嘈杂多边形的辅助四边形
     */
    public void BuildAssistGrids(DelaunayVoronoi d, Texture2D texture2D)
    {
        foreach (var center in d.Points)
        {
            var hoverIndex = d.Delaunay.find(center.X, center.Y);
            var polygon = d.CellPolygons[hoverIndex];
            if (polygon.Any(point => double.IsNaN(point[0]) || double.IsNaN(point[1]))) break;
            var points = polygon
                .Select(point => new Vector2((int) point[0] * _textureScale, (int) point[1] * _textureScale)).ToArray();
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 nextPos = points[i];
                texture2D.SetPixel((int)nextPos.x, (int)nextPos.y, Color.blue);
                texture2D.DrawLine((int)center.X * _textureScale, (int)center.Y * _textureScale, (int)nextPos.x, (int)nextPos.y, Color.green);
            }
        }
    }

    /**
     * 画带嘈杂算法的维洛图边界
     */
    public void DrawVoronoiEdge(DelaunayVoronoi d, Texture2D texture2D)
    {
        /*foreach (var point in m.centers)
        {    
            texture2D.DrawPoint((int)point.point.x * _textureScale, (int)point.point.y * _textureScale, Color.red, 3);
        }
        var t0 = new Vector2(150, 150);
        
        texture2D.DrawPoint((int)t0.x * _textureScale, (int)t0.y * _textureScale, Color.red, 3);
        
        var t1 = new Vector2(175, 130);
        
        texture2D.DrawPoint((int)t1.x * _textureScale, (int)t1.y * _textureScale, Color.magenta, 3);
        
        var r0 = new Vector2(160, 110);
        
        texture2D.DrawPoint((int)r0.x * _textureScale, (int)r0.y * _textureScale, Color.green, 3);
        
        var r1 = new Vector2(165, 170);
        
        texture2D.DrawPoint((int)r1.x * _textureScale, (int)r1.y * _textureScale, Color.yellow, 3);
        
        
        //t0 ~ r1 
        texture2D.DrawLine((int)t0.x * _textureScale, (int)t0.y * _textureScale, (int)r1.x * _textureScale, (int)r1.y * _textureScale, Color.black);
        
        //r1 ~ t1
        texture2D.DrawLine((int)r1.x * _textureScale, (int)r1.y * _textureScale, (int)t1.x * _textureScale, (int)t1.y * _textureScale, Color.black);
        
        //t1 ~ r0
        texture2D.DrawLine((int)t1.x * _textureScale, (int)t1.y * _textureScale, (int)r0.x * _textureScale, (int)r0.y * _textureScale, Color.black);
        
        
        //r0 ~ t0
        texture2D.DrawLine((int)r0.x * _textureScale, (int)r0.y * _textureScale, (int)t0.x * _textureScale, (int)t0.y * _textureScale, Color.black);
        
        
        var path = m.noisyEdges.genNoisyEdges(4, t0, t1, r0, r1).line;
        for (var i = 0; i < path.Count - 1; i++)
        {
            texture2D.DrawLine((int)path[i].x * _textureScale, (int)path[i].y * _textureScale, (int)path[i + 1].x * _textureScale, (int)path[i + 1].y * _textureScale, Color.green);
        }*/

        //drawVoronoi(d, texture2D);

        foreach (var p in m.noisyEdges.path)
        {
            var ps = p.Value;
            
            List<Vector2> rm = new List<Vector2>();
            
            for (var k = 0; k < ps.Count - 2; ++k)
            {
                Vector2 pre = ps[k];
                Vector2 curr = ps[k + 1];
                Vector2 next = ps[k + 2];

                var v1 = curr - pre;
                var v2 = curr - next;

                var angle = Vector2.Angle(pre - curr, next - curr);
                if (angle <= 150)
                {
                    //texture2D.DrawLine((int)pre.x * _textureScale, (int)pre.y * _textureScale, (int)next.x * _textureScale, (int)next.y * _textureScale, Color.yellow);
                    rm.Add(curr);
                }
            }

            for (int i = 0; i < rm.Count; i++)
            {
                ps.Remove(rm[i]);
            }
            
            for (var j = 0; j < ps.Count - 1; ++j)
            {
                Vector2 pre = ps[j];
                Vector2 next = ps[j + 1];
                //texture2D.DrawLine((int)pre.x * _textureScale, (int)pre.y * _textureScale, (int)next.x * _textureScale, (int)next.y * _textureScale, Color.yellow);
                texture2D.DrawLine((int)pre.x, (int)pre.y, (int)next.x, (int)next.y, Color.yellow);
            }
        }
        
    }

    public static Color GetRandomColor()
    {
        float r = Random.Range(0f,1f);
        float g = Random.Range(0f,1f);
        float b = Random.Range(0f,1f);
        Color color = new Color(r,g,b);
        return color;
    } 
    
}
