using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// обновление на стороне сервера
public class SyncServerUp : MonoBehaviour 
{
	private GameServer game_server;
	private Client client;
	private Rigidbody body;
    private long currentPack = 0;
    private bool isSend = false;
    private bool isMove = false;
    private string pointName;
    private Vector3 pointPosition;

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

            if(isSend)
            {
                string response;
                string json_client;

                client.transform.position = body.position;
                client.transform.rotation = body.rotation.eulerAngles;

                json_client = JsonUtility.ToJson(client);
                response = "{\"msgid\":10003, \"client\":" + json_client + "}";
                client.Send(game_server.server, response);

                isSend = false;
            }
 
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(client != null)
        {
            if (client.isExit()) return;

            if(isMove)
            {
                body.position = Vector3.Lerp(body.position, pointPosition, 0.1f);
                isSend = true;
            }

            if(client.transform.force != Vector3.zero)
            {
                body.velocity = Vector3.zero;
                body.AddForce(client.transform.force, ForceMode.Impulse);
                client.transform.force = Vector3.zero;
                isSend = true;
            }
            else body.velocity = Vector3.zero;
        }
    }

    public void SetClient(Client _c){ client = _c; }
    public void MoveTo(string name)
    {
        GameObject point = GameObject.Find(name);
        if(point != null)
        {
            pointPosition = GameObject.Find(name).transform.position;
            pointName = name;
            isMove = true;            
        }
    }
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.name == pointName)
            isMove = false;
    }
}