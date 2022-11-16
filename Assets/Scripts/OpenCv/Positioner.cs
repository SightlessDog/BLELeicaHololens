using UnityEngine;

public class Positioner : Singleton<Positioner>
{
    private Vector3 position;
    private Quaternion rotation;

    public Vector3 GetPosition()
    {
        return position;
    }

    public Quaternion GetRotation()
    {
        return rotation;
    }

    public void SetPosition(Vector3 position)
    {
        this.position = position;
    }

    public void SetRotation(Quaternion rotation)
    {
        this.rotation = rotation;
    }
} 