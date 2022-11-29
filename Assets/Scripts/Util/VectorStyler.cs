using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorStyler : MonoBehaviour
{
    private GameObject target;
    private bool hovered;
    private bool hoveredDone;
    private void Update()
    {
        if (hovered)
        {
            this.target.GetComponent<Renderer>().material.color = new Color(196, 104, 255, 197);
            hovered = false;
        }

        if (hoveredDone)
        {
            this.target.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 255);
            hoveredDone = false;
        }
    }

    public void onHover(GameObject target)
    {
        this.target = target;
        hovered = true;
    }
    
    public void onHoverDone(GameObject target)
    {
        hovered = false;
        this.target = target;
        hoveredDone = true;
    }
}
