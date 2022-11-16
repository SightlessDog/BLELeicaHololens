 using System;
 using UnityEngine;
 using System.Collections.Generic;
 using System.Linq;
 using Vector3 = UnityEngine.Vector3;
 using Vector2 = UnityEngine.Vector2;

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

     public void BuildShapeAndReturnJson()
     {
         NotificationManager.Instance.SetNewNotification("Going to save the json file");
         Room room = new Room();
         List<Vector3> points = getPolys().OrderBy(x => Math.Atan2(x.x, x.y)).ToList();
         Vector3 firstPoint = points[0];
         foreach (Vector3 point in points)
         {
             room.points.Add(new Vector2(point.x - firstPoint.x, point.z - firstPoint.z));
         }
         string json = JsonUtility.ToJson(room);
         NotificationManager.Instance.SetNewNotification("Json file saved");
         string jsonFilePath = Application.persistentDataPath + "/room.json";
         System.IO.File.WriteAllText(jsonFilePath, json);
     }
 }