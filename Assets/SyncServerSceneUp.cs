using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncServerSceneUp : MonoBehaviour
{
    private GameServer game_server;
    private Object objectSync;
    private Rigidbody body;
    private bool isStartThread = false;
    // Start is called before the first frame update
    void Start()
    {
        game_server = GameObject.Find("Game").gameObject.GetComponent<GameServer>();
        body = gameObject.GetComponent<Rigidbody>();

        objectSync = new Object();
        objectSync.name = gameObject.name;
        objectSync.position = body.position;
        objectSync.rotation = body.rotation.eulerAngles;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (game_server.isConnected)
        {

            if(objectSync.position != body.position || objectSync.rotation != body.rotation.eulerAngles)
            {
                objectSync.position = body.position;
                objectSync.rotation = body.rotation.eulerAngles;

                try
                {
                    string json_object = JsonUtility.ToJson(objectSync);
                    string response = "{\"msgid\":10005, \"obj\":" + json_object + "}";
                    for (int i = 0; i < game_server.clients.Count; i++)
                        if (!game_server.clients[i].isExit())
                            game_server.clients[i].Send(game_server.server, response);
                }
                catch (Exception e)
                {
                    print("server error: " + e.ToString());
                }
            }
        }
    }
}
