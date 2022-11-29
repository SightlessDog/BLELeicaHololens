using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ImageProcessor : Singleton<ImageProcessor>
{
    [SerializeField] private GameObject pointPrefab;
    private float pointThreshold = 0.2f;
    private float length;

    public void TriggerShot(float value)
    {
        this.length = value;
        TreatPoints(Positioner.Instance.GetPosition(), Positioner.Instance.GetRotation(), this.length);
    }

    private void TreatPoints(Vector3 position, Quaternion rotation, float length)
    {
        if (ShapeBuilder.Instance.GetMeasurements().Count > 0)
        {
            List<Measurement> existingMeasurements = ShapeBuilder.Instance.GetMeasurements();
            Vector3 finishPoint = position + rotation * Vector3.right * length;
            bool bound = false;

            foreach (var m in existingMeasurements)
            {
                if (Vector3.Distance(m.GetPoints()[0], position) < pointThreshold)
                {
                    NotificationManager.Instance.SetNewNotification("binding start point");
                    GameObject secondPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    secondPoint.name = secondPoint.name + ShapeBuilder.Instance.GetMeasurements().Count;
                    var vector = secondPoint.transform.Find("Vec");
                    var text = secondPoint.transform.Find("Length");
                    text.GetComponentInChildren<TextMeshProUGUI>().SetText(length.ToString());
                    vector.localScale = new Vector3(this.length, 0.01f, 0.01f);
                    secondPoint.transform.position = m.GetPoints()[0];
                    secondPoint.transform.rotation = rotation;
                    List<Vector3> points = new List<Vector3>();
                    points.Add(m.GetPoints()[0]);
                    points.Add(finishPoint);
                    ShapeBuilder.Instance.AddToMeasurements(new Measurement(secondPoint.name, points));
                    bound = true;
                    break;
                } 
                else if (Vector3.Distance(m.GetPoints()[1], position) < pointThreshold)
                {
                    NotificationManager.Instance.SetNewNotification("binding end point");
                    GameObject secondPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    secondPoint.name = secondPoint.name + ShapeBuilder.Instance.GetMeasurements().Count;
                    var vector = secondPoint.transform.Find("Vec");
                    var text = secondPoint.transform.Find("Length");
                    text.GetComponentInChildren<TextMeshProUGUI>().SetText(length.ToString());
                    vector.localScale = new Vector3(this.length, 0.01f, 0.01f);
                    secondPoint.transform.position = m.GetPoints()[1];
                    secondPoint.transform.rotation = rotation;
                    List<Vector3> points = new List<Vector3>();
                    points.Add(m.GetPoints()[1]);
                    points.Add(finishPoint);
                    ShapeBuilder.Instance.AddToMeasurements(new Measurement(secondPoint.name, points));
                    bound = true;
                    break;
                }
                else if (Vector3.Distance(m.GetPoints()[1], finishPoint) < pointThreshold)
                {
                    NotificationManager.Instance.SetNewNotification("binding end point");
                    GameObject secondPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    secondPoint.name = secondPoint.name + ShapeBuilder.Instance.GetMeasurements().Count;
                    Vector3 point = m.GetPoints()[1] + rotation * Vector3.right * length;
                    var vector = secondPoint.transform.Find("Vec");
                    var text = secondPoint.transform.Find("Length");
                    text.GetComponentInChildren<TextMeshProUGUI>().SetText(length.ToString());
                    vector.localScale = new Vector3(-this.length, 0.01f, 0.01f);
                    secondPoint.transform.position = m.GetPoints()[1];
                    secondPoint.transform.rotation = rotation;
                    List<Vector3> points = new List<Vector3>();
                    points.Add(m.GetPoints()[1]);
                    points.Add(point);
                    ShapeBuilder.Instance.AddToMeasurements(new Measurement(secondPoint.name, points));
                    bound = true;
                    break;
                } 
                else if (Vector3.Distance(m.GetPoints()[0], finishPoint) < pointThreshold)
                {
                    NotificationManager.Instance.SetNewNotification("binding start point");
                    GameObject secondPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    secondPoint.name = secondPoint.name + ShapeBuilder.Instance.GetMeasurements().Count;
                    var vector = secondPoint.transform.Find("Vec");
                    var text = secondPoint.transform.Find("Length");
                    text.GetComponentInChildren<TextMeshProUGUI>().SetText(length.ToString());
                    vector.localScale = new Vector3(-this.length, 0.01f, 0.01f);
                    secondPoint.transform.position = m.GetPoints()[0];
                    secondPoint.transform.rotation = rotation;
                    List<Vector3> points = new List<Vector3>();
                    points.Add(m.GetPoints()[0]);
                    points.Add(position);
                    ShapeBuilder.Instance.AddToMeasurements(new Measurement(secondPoint.name, points));
                    bound = true;
                    break;
                }
            }
            if (!bound)
            {
                GameObject firstPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                firstPoint.name = firstPoint.name + ShapeBuilder.Instance.getPolys().Count;
                var vector = firstPoint.transform.Find("Vec");
                var text = firstPoint.transform.Find("Length");
                text.GetComponentInChildren<TextMeshProUGUI>().SetText(length + " m");
                vector.localScale = new Vector3(this.length, 0.01f, 0.01f);
                firstPoint.transform.position = new Vector3(position.x, position.y, position.z);
                firstPoint.transform.rotation = rotation;
                List<Vector3> points = new List<Vector3>();
                points.Add(position);
                points.Add(finishPoint);
                ShapeBuilder.Instance.AddToMeasurements(new Measurement(firstPoint.name, points));
            }
        }
        else
        {
            GameObject firstPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            firstPoint.name = firstPoint.name + ShapeBuilder.Instance.getPolys().Count;
            var vector = firstPoint.transform.Find("Vec");
            var text = firstPoint.transform.Find("Length");
            text.GetComponentInChildren<TextMeshProUGUI>().SetText(length + " m");
            vector.localScale = new Vector3(this.length, 0.01f, 0.01f);
            firstPoint.transform.position = position;
            firstPoint.transform.rotation = rotation;
            List<Vector3> points = new List<Vector3>();
            points.Add(position);
            Vector3 finishPoint = position + rotation * Vector3.right * length;
            points.Add(finishPoint);
            ShapeBuilder.Instance.AddToMeasurements(new Measurement(firstPoint.name, points));
        }
        
    }
}