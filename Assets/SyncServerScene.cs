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

public class SyncServerScene : MonoBehaviour
{
    private Object syncObject;
    private bool isUpdate = false;
    private Game game;
    private Rigidbody body;
    // Start is called before the first frame update
    void Start()
    {
        syncObject = new Object();
        syncObject.name = gameObject.name;

        game = GameObject.Find("Game").GetComponent<Game>();
        body = gameObject.GetComponent<Rigidbody>();

        syncObject.position = body.position;
        syncObject.rotation = body.rotation.eulerAngles;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (syncObject.rotation == body.rotation.eulerAngles && syncObject.position == body.position)
            isUpdate = false;

        if (isUpdate)
        {
            body.position = Vector3.Lerp(body.position, syncObject.position, 1); ;
            body.rotation = Quaternion.Lerp(body.rotation, Quaternion.Euler(syncObject.rotation), 1);
        }
        else
        {
            if(game.isConnected && !body.IsSleeping())
            {
                syncObject.position = body.position;
                syncObject.rotation = body.rotation.eulerAngles;
                string json = JsonUtility.ToJson(syncObject);
                byte[] sendBytes = new UTF8Encoding(true).GetBytes("{\"msgid\":10004, \"id_player\":" + game.id_player.ToString() + ", \"name\":\"" + gameObject.name + "\",\"objectSync\":" + json + "}");
                game.client.Send(sendBytes, sendBytes.Length);
            }
        }
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.tag == "Player")
            isUpdate = false;
    }

    public void UpdateServer(Object Obj, bool isNow = false)
    {
        syncObject.position = Obj.position;
        syncObject.rotation = Obj.rotation;
        if(isNow)
        {
            body.position = syncObject.position;
            body.rotation = Quaternion.Euler(syncObject.rotation);
        }
        else
            isUpdate = true;
    }
}
