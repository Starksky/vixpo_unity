using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// обновление на стороне сервера
public class SyncServerUp : MonoBehaviour 
{
	private GameServer game_server;
	private Client client;
	private Rigidbody body;
    private long currentPack = -1;

    // Start is called before the first frame update
    void Start()
    {
        game_server = GameObject.Find("Game").gameObject.GetComponent<GameServer>();
        body = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(client != null)
        {
            if (client.isExit()) return;

            //if(client.transform.force != Vector3.zero)
            //{
                string response;
                string json_client;

                client.transform.position = body.position;
                client.transform.rotation = body.rotation.eulerAngles;

                json_client = JsonUtility.ToJson(client);
                response = "{\"msgid\":10003, \"client\":" + json_client + "}";
                client.Send(game_server.server, response);
                             
            //}
 
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(client != null)
        {
            if (client.isExit()) return;

            //if(client.transform.force != Vector3.zero)
            //{
                //body.velocity = Vector3.zero;
                body.AddForce(client.transform.force, ForceMode.Impulse);
                client.transform.force = Vector3.zero; 
            //}

            //body.position = client.transform.position;
            //body.rotation = Quaternion.Euler(client.transform.rotation);
        }
    }

    public void SetClient(Client _c){ client = _c; }
}
