using System.Collections.Generic;
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
        //ShapeBuilder.Instance.addToPoly(position);
        if (ShapeBuilder.Instance.getPolys().Count > 0)
        {
            List<Vector3> existingPoints = ShapeBuilder.Instance.getPolys();
            Vector3 finishPoint = position + rotation * new Vector3(1, 0, 0) * length;

            foreach (var p in existingPoints)
            {
                if ((position.x <= p.x + pointThreshold && position.x >= p.x - pointThreshold) &&
                    (position.y <= p.y + pointThreshold && position.y >= p.y - pointThreshold) &&
                    (position.z <= p.z + pointThreshold && position.z >= p.z - pointThreshold))
                {
                    NotificationManager.Instance.SetNewNotification("binding start point");
                    GameObject secPoi = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    secPoi.transform.localScale = new Vector3(this.length, 0.01f, 0.01f);
                    secPoi.transform.position = p;
                    secPoi.transform.rotation = rotation;
                    ShapeBuilder.Instance.addToPoly(finishPoint);
                }
                else if ((finishPoint.x <= p.x + pointThreshold && finishPoint.x >= p.x - pointThreshold) &&
                         (finishPoint.y <= p.y + pointThreshold && finishPoint.y >= p.y - pointThreshold) &&
                         (finishPoint.z <= p.z + pointThreshold && finishPoint.z >= p.z - pointThreshold))
                {
                    NotificationManager.Instance.SetNewNotification("binding end popint");
                    GameObject secPoi = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    Vector3 secondPoint = p + rotation * new Vector3(-1, 0, 0) * length;
                    secPoi.transform.localScale = new Vector3(-this.length, 0.01f, 0.01f);
                    secPoi.transform.position = p;
                    secPoi.transform.rotation = rotation;
                    ShapeBuilder.Instance.addToPoly(secondPoint);
                }
                else
                {
                    GameObject firstPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    firstPoint.transform.localScale = new Vector3(this.length, 0.01f, 0.01f);
                    firstPoint.transform.position = position;
                    firstPoint.transform.rotation = rotation;
                    ShapeBuilder.Instance.addToPoly(position);
                    ShapeBuilder.Instance.addToPoly(finishPoint);
                }
            }
        }
        else
        {
            Vector3 finishPoint = position + rotation * new Vector3(1, 0, 0) * length;
            GameObject firstPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            firstPoint.transform.localScale = new Vector3(this.length, 0.01f, 0.01f);
            firstPoint.transform.position = position;
            firstPoint.transform.rotation = rotation;
            ShapeBuilder.Instance.addToPoly(position);
            ShapeBuilder.Instance.addToPoly(finishPoint);
        }
        
    }
}