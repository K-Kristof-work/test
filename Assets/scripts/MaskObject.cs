using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskObject : MonoBehaviour
{
    public GameObject objectToMask;
    // Start is called before the first frame update
    void Start()
    {
        objectToMask.GetComponent<MeshRenderer>().material.renderQueue = 3002;
    }
}
