using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncServerUp : MonoBehaviour
{
	private GameServer game_server;
	private Client client;
	private Rigidbody body;

    // Start is called before the first frame update
    void Start()
    {
        game_server = GameObject.Find("Game").gameObject.GetComponent<GameServer>();
        body = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(client != null)
        {
            body.position = client.transform.position;
            body.rotation = Quaternion.Euler(client.transform.rotation);

        	client.transform.position = body.position;
    		client.transform.rotation = body.rotation.eulerAngles;

        	string json_client = JsonUtility.ToJson(client);
        	string response = "{\"msgid\":10003, \"client\":" + json_client + "}";
        	client.Send(game_server.server, response);
        }
    }

    public void SetClient(Client _c){ client = _c; }
}
