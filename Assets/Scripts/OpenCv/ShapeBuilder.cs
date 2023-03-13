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
     private List<Measurement> measurements = new List<Measurement>();
     private int polyIndex = 0;

     public void AddToMeasurements(Measurement measurement)
     {
         measurements.Add(measurement);
     }

     public void RemoveFromMeasurements(string name)
     {
         var item = measurements.Find(x => x.GetName() == name);
         measurements.Remove(item);
     }

     public List<Measurement> GetMeasurements()
     {
         return measurements;
     }

     public List<Vector3> getPolys()
     {
         return poly;
     }
     
     public void addToPoly(Vector3 point)
     {
         poly.Add(point);
     }

     public void removeFromPoly(Vector3 point)
     {
         bool removed = poly.Remove(point);
         if (removed)
         {
             Debug.Log("point " + point + "deleted");
             NotificationManager.Instance.SetNewNotification("point " + point + "deleted");
         }
     }

     public void BuildShapeAndReturnJson()
     {
         NotificationManager.Instance.SetNewNotification("Going to save the json file");
         Room room = new Room();
         List<Vector3> points = new List<Vector3>();
         foreach (var m in measurements) 
         {
             points.Add(m.GetPoints()[0]);
             points.Add(m.GetPoints()[1]);
         }
         Debug.Log("EE after for each");
         points.OrderBy(x => Math.Atan2(x.x, x.z)).ToList();
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