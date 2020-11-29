using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamePlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;

        /*Vector3 objectNormal = transform.rotation * Vector3.forward;
        Vector3 cameraToText = transform.position - cam.transform.position;
        float f = Vector3.Dot(objectNormal, cameraToText);
        if (f < 0f)
        {
            transform.Rotate(0f, 180f, 0f);
        }*/
    }
}
