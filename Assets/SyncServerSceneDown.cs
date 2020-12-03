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

public class SyncServerSceneDown : MonoBehaviour
{
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
            body.detectCollisions = false;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.None;
            body.position = objectSync.position;
            body.rotation = Quaternion.Euler(objectSync.rotation);
        }
    }
    
    public void Link(Object _object) { objectSync = _object; }
}
