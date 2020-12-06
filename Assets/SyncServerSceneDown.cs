using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// для обновления сцены
public class SyncServerSceneDown : MonoBehaviour
{
    public float speed = 0.1f;
    private Game game;
    private Rigidbody body;
    private Object objectSync;

    // Start is called before the first frame update
    void Start()
    {
        game = GameObject.Find("Game").GetComponent<Game>();
        body = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(objectSync != null)
        {
            //body.detectCollisions = false;
            //body.isKinematic = true;
            //body.useGravity = false;

            body.interpolation = RigidbodyInterpolation.None;
            body.position = Vector3.Lerp(body.position, objectSync.position, speed);
            body.rotation = Quaternion.Lerp(body.rotation, Quaternion.Euler(objectSync.rotation), speed);
        }
    }
    
    public void Link(Object _object) { objectSync = _object; }
}
