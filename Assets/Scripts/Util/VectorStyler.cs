using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorStyler : MonoBehaviour
{
    private GameObject target;
    private bool hovered;
    private bool hoveredDone;
    [SerializeField] private Material vectorInteracted;
    [SerializeField] private Material defaultState;
    private void Update()
    {
        if (hovered)
        {
            target.GetComponent<MeshRenderer>().material = vectorInteracted;
            target.transform.localScale = Vector3.Lerp(target.transform.localScale, new Vector3(1.1f, 1.1f, 1.1f), 0.01f);
        }

        if (hoveredDone)
        {
            target.GetComponent<MeshRenderer>().material = defaultState;
            target.transform.localScale = Vector3.Lerp(target.transform.localScale, new Vector3(1f, 1f, 1f), 0.5f);
        }
    }

    public void onHover(GameObject target)
    {
        hoveredDone = false;
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
