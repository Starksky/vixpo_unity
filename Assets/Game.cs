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


[Serializable] public class Object{
	public string name;
	public Vector3 position;
	public Vector3 rotation;
    public Vector3 velocity;
    public Object() { }
    public Object(Object _o)
    {
        this.name = _o.name;
        this.position = _o.position;
        this.rotation = _o.rotation;
        this.velocity = _o.velocity;
    }
}

[Serializable] public class Player
{
	public string address;
	public int port;
	public long last_time;
	public bool leave;

	private GameObject _object;
    public Object transform;
    public float syncTime;
    public float syncDelay;

    public bool isAdd(){ return _object != null ? true : false; }
	public Player(){ transform = new Object(); }
	public Player(Object _o){ transform = _o; }
	public void Create(GameObject _o){ _object = _o; }
	public void Update()
	{
        if (_object == null) return;
		if(leave) GameObject.Destroy(_object);

        _object.transform.localPosition = Vector3.Lerp(_object.transform.localPosition, transform.position, 1);
	 	_object.transform.rotation = Quaternion.Lerp(_object.transform.rotation, Quaternion.Euler(transform.rotation), 1);
	 	_object.transform.GetChild(0).GetComponent<TextMesh>().text = transform.name;
	}
}
public class Answer
{
	public int msgid;
	public int id_player;
	public Player[] players;
    public Object[] objects;
    public Answer(){ players = new Player[]{}; }
}

public class AnswerObject
{
    public int msgid;
    public Object objectSync;
    public AnswerObject() { objectSync = new Object(); }
    public AnswerObject(AnswerObject _o)
    {
        this.msgid = _o.msgid;
        this.objectSync = new Object(_o.objectSync);
    }
}

public class Game : MonoBehaviour
{
    Thread readThread;
    public UdpClient client;

    public int port = 22023;
    public string ip = "78.24.222.166";
    public GameObject templatePlayer;
    public InputField name;
    private GameObject scene;
    public int id_player = 0;
    private GameObject _player;
    private Object player;
    private List<Player> players;
    private List<AnswerObject> answers;
    private List<Object> objects;
    public bool isConnected = false;

    // Start is called before the first frame update
    void Start()
    {
    	players = new List<Player>();
        answers = new List<AnswerObject>();
        objects = new List<Object>();

        scene = GameObject.Find("Game").gameObject;
    	_player = GameObject.Find("Player").gameObject;

        player = new Object();
    	player.position = _player.transform.localPosition;
    	player.rotation = _player.transform.eulerAngles;
    	player.name = name.text;

    	client = new UdpClient(ip, port);

        // create thread for reading UDP messages
        readThread = new Thread(new ThreadStart(ReceiveData));
        readThread.IsBackground = true;
        readThread.Start();

        string json = JsonUtility.ToJson(player);
        byte[] sendBytes = new UTF8Encoding(true).GetBytes("{\"msgid\":10001, \"player\":"+json+"}");
	    client.Send(sendBytes, sendBytes.Length);
    }
	// Unity Application Quit Function
    void OnApplicationQuit()
    {
        stopThread();
    }
    // Stop reading UDP messages
    private void stopThread()
    {
        if (readThread.IsAlive)
        {
            readThread.Abort();
        }
        client.Close();
        isConnected = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    	for(int id = 0; id < players.Count; id++)
    	    if(players[id].isAdd())
    		    players[id].Update();
    	    else if(!players[id].leave) players[id].Create(Instantiate(templatePlayer, scene.transform, false).gameObject);

        for (int id = 0; id < objects.Count; id++)
        {
            try
            {
                print(objects[id].name);
                GameObject.Find(objects[id].name).gameObject.GetComponent<SyncServerScene>().UpdateServer(objects[id], true);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
        for (int id = 0; id < answers.Count; id++)
        {
            try
            {
                print(answers[id].objectSync.name);
                GameObject.Find(answers[id].objectSync.name).gameObject.GetComponent<SyncServerScene>().UpdateServer(answers[id].objectSync);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }

        objects.Clear();
        answers.Clear();

        if (isConnected)
        {
            player.position = _player.transform.localPosition;
            player.rotation = _player.transform.eulerAngles;

         	string json = JsonUtility.ToJson(player);
        	byte[] sendBytes = new UTF8Encoding(true).GetBytes("{\"msgid\":10002, \"id_player\":"+id_player.ToString()+",\"player\":"+json+"}");
        	client.Send(sendBytes, sendBytes.Length);
        	sendBytes = new UTF8Encoding(true).GetBytes("{\"msgid\":10003}");
	    	client.Send(sendBytes, sendBytes.Length);     	
        }
    }

        // receive thread function
    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                // receive bytes
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                // encode UTF8-coded bytes to text format
                string text = new UTF8Encoding(true).GetString(data);

                Answer answer = JsonUtility.FromJson<Answer>(text);
                switch(answer.msgid)
                {
                	//Connected user
                	case 10001:
                	{
                		id_player = answer.id_player;
                		for(int id = 0; id < answer.players.Length; id++)
                		{
                			Object _o = new Object();
					    	_o.position = answer.players[id].transform.position;
					    	_o.rotation = answer.players[id].transform.rotation;
					    	_o.name = answer.players[id].transform.name;
                			players.Add(new Player(_o));
                		}
                        for (int id = 0; id < answer.objects.Length; id++)
                            objects.Add(new Object(answer.objects[id]));
                		isConnected = true;

                		// show received message
                		//print(">> " + text);
                	}
                	break;

                	//Connected guest
                	case 10002:
                	{
                		// show received message
                		//print(">> " + text);
            			Object _o = new Object();
				    	_o.position = answer.players[0].transform.position;
				    	_o.rotation = answer.players[0].transform.rotation;
				    	_o.name = answer.players[0].transform.name;
                		players.Add(new Player(_o));
                	}
                	break;

                	//Update guest
                	case 10003:
                	{
                		// show received message
                		//print(">> " + text);

                		if(id_player < answer.id_player)
                		{
                            players[answer.id_player - 1].transform.name = answer.players[0].transform.name;
               			    players[answer.id_player - 1].transform.position = answer.players[0].transform.position;
                			players[answer.id_player - 1].transform.rotation = answer.players[0].transform.rotation;
                			players[answer.id_player - 1].leave = answer.players[0].leave;
                		}
                		else{
                			players[answer.id_player].transform.name = answer.players[0].transform.name;
                			players[answer.id_player].transform.position = answer.players[0].transform.position;
                			players[answer.id_player].transform.rotation = answer.players[0].transform.rotation; 
                			players[answer.id_player].leave = answer.players[0].leave;
                		}
                	}
                	break;
                    //Update
                    case 10004:
                        {
                            AnswerObject answerObj = JsonUtility.FromJson<AnswerObject>(text);
                            // show received message
                            //print(">> " + text);
                            //print(">> " + answerObj.objectSync.name);
                            answers.Add(new AnswerObject(answerObj));
                        }
                    break;
                }

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    public void SaveName()
    {
    	player.name = name.text;
    	_player.transform.GetChild(1).GetComponent<TextMesh>().text = player.name;
    }
}
