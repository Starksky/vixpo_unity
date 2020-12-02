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
/*    private Object syncObject;
    private Object syncObjectBuffer;
    private Object syncObjectBuffer1;
    private bool isUpdate = false;
    private Game game;
    private Rigidbody body;
    // Start is called before the first frame update
    void Start()
    {
        syncObject = new Object();
        syncObjectBuffer = new Object();
        syncObjectBuffer1 = new Object();
        syncObject.name = gameObject.name;

        game = GameObject.Find("Game").GetComponent<Game>();
        body = gameObject.GetComponent<Rigidbody>();

        syncObject.position = body.position;
        syncObject.rotation = body.rotation.eulerAngles;
        syncObjectBuffer.position = body.position;
        syncObjectBuffer.rotation = body.rotation.eulerAngles;
        syncObjectBuffer1.position = body.position;
        syncObjectBuffer1.rotation = body.rotation.eulerAngles;
    }
    void Send()
    {
        if (!game.isConnected) return;

        string json = JsonUtility.ToJson(syncObject);
        byte[] sendBytes = new UTF8Encoding(true).GetBytes("{\"msgid\":10004, \"id_player\":" + game.id_player.ToString() + ", \"name\":\"" + gameObject.name + "\",\"objectSync\":" + json + "}");
        game.client.Send(sendBytes, sendBytes.Length);
    }
    void Update()
    {
        //if (syncObject.rotation == body.rotation.eulerAngles && syncObject.position == body.position)
        //    isUpdate = false;





    }
    // Update is called once per frame
    void FixedUpdate()
    {

        if (syncObject.rotation == syncObjectBuffer.rotation && syncObject.position == syncObjectBuffer.position)
           isUpdate = false;
        else isUpdate = true;

        if (game.id_player > 0)
        {
           //syncObjectBuffer.position = syncObject.position;
            //syncObjectBuffer.rotation = syncObject.rotation;
            //syncObjectBuffer.velocity = syncObject.velocity;
            body.position = syncObject.position;
            body.rotation = Quaternion.Euler(syncObject.rotation);
            body.velocity = syncObject.velocity;
        }

        if(game.id_player == 0 && isUpdate)
        {
            syncObject.position = body.position;
            syncObject.rotation = body.rotation.eulerAngles;
            syncObject.velocity = body.velocity;
            Send();
        }
        /*if (syncObject.rotation == body.rotation.eulerAngles && syncObject.position == body.position)
             isUpdate = false;

        if (isUpdate)
        {
            body.position = Vector3.Lerp(body.position, syncObject.position, 1f); ;
            body.rotation = Quaternion.Lerp(body.rotation, Quaternion.Euler(syncObject.rotation), 1f);
        }
        else
        {
            if (game.isConnected)
            {
                syncObject.position = body.position;
                syncObject.rotation = body.rotation.eulerAngles;
                Send();
            }
        }*/



   /* }

    void OnCollisionEnter(Collision collisionInfo)
    {
        //if (collisionInfo.gameObject.tag == "Player")
        //isUpdate = false;
    }

    public void UpdateServer(Object Obj, bool isNow = false)
    {
        syncObject.position = Vector3.Lerp(syncObject.position, Obj.position, 1f);
        syncObject.rotation = Vector3.Lerp(syncObject.rotation, Obj.rotation, 1f);

        if(isNow)
        {
            body.position = syncObject.position;
            body.rotation = Quaternion.Euler(syncObject.rotation);
        }
        //else
        //    isUpdate = true;
    }*/
}
