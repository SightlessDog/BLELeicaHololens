 using UnityEngine;
 using System.Collections.Generic;

 public class ShapeBuilder : Singleton<ShapeBuilder> 
 {
     public Material mat;
     private List<Vector3> poly = new List<Vector3>();
     private int polyIndex = 0;

     public List<Vector3> getPolys()
     {
         return poly;
     }
     
     public void addToPoly(Vector3 point)
     {
         poly.Add(point);
     }
     
     public void BuildShape () {
         if (poly == null || poly.Count < 3) {
             //Debug.Log ("Define 2D polygon in 'poly' in the the Inspector");
             return;
         }
         
         MeshFilter mf = gameObject.AddComponent<MeshFilter>();
         
         Mesh mesh = new Mesh();
         mf.mesh = mesh;
         
         Renderer rend = gameObject.AddComponent<MeshRenderer>();
         rend.material = mat;
 
         Vector3 center = FindCenter();
                 
         Vector3[] vertices = new Vector3[poly.Count+1];
         //vertices[0] = Vector3.zero;
         
         for (int i = 0; i < poly.Count; i++)
         {
             var point = poly[i];
             point.y = 0.0f;
             poly[i] = point;
             vertices[i] = poly[i] - center;
         }
         
         mesh.vertices = vertices;
         
         int[] triangles = new int[poly.Count*3];
         
         for (int i = 0; i < poly.Count-1; i++) {
             triangles[i*3] = i+2;
             triangles[i*3+1] = 0;
             triangles[i*3+2] = i + 1;
         }
         
         triangles[(poly.Count-1)*3] = 1;
         triangles[(poly.Count-1)*3+1] = 0;
         triangles[(poly.Count-1)*3+2] = poly.Count;
         
         mesh.triangles = triangles;
         mesh.uv = BuildUVs(vertices);
 
         mesh.RecalculateBounds();
         mesh.RecalculateNormals();
 
     }
     
     private Vector3 FindCenter() {
         Vector3 center = Vector3.zero;
         foreach (Vector3 v3 in poly) {
             center += v3;    
         }
         return center / poly.Count;
     }
     
     private Vector2[] BuildUVs(Vector3[] vertices) {
         
         float xMin = Mathf.Infinity;
         float yMin = Mathf.Infinity;
         float xMax = -Mathf.Infinity;
         float yMax = -Mathf.Infinity;
         
         foreach (Vector3 v3 in vertices) {
             if (v3.x < xMin)
                 xMin = v3.x;
             if (v3.z < yMin)
                 yMin = v3.z;
             if (v3.x > xMax)
                 xMax = v3.x;
             if (v3.z > yMax)
                 yMax = v3.z;
         }
         
         float xRange = xMax - xMin;
         float yRange = yMax - yMin;
             
         Vector2[] uvs = new Vector2[vertices.Length];
         for (int i = 0; i < vertices.Length; i++) {
             uvs[i].x = (vertices[i].x - xMin) / xRange;
             uvs[i].y = (vertices[i].y - yMin) / yRange;
             
         }
         return uvs;
     }
 }