using System.Collections.Generic;
using UnityEngine;

public class Measurement
{
    private List<Vector3> points = new List<Vector3>();
    private string name;

    public Measurement(string name, List<Vector3> points)
    {
        this.name = name;
        this.points = points;
    }

    public List<Vector3> GetPoints()
    {
        return this.points;
    }

    public string GetName()
    {
        return name;
    }
}