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
    public void Set(Object _o)
    {
    	this.name = _o.name;
        this.position = _o.position;
        this.rotation = _o.rotation;
        this.velocity = _o.velocity;
    }
}

[Serializable] public class Player
{
	public string ip;
	public long lastTimeServer;
	public Object transform;


	private bool _isAdd = false;
	private bool _isExit = false;
	private GameObject player;
	private TextMesh name;
    
	public Player(){ transform = new Object(); }
	public Player(Object _o){ transform = _o; }
	public void Create(GameObject _o)
	{ 
		player = _o;
		name = _o.transform.GetChild(0).GetComponent<TextMesh>();
	}
	public void Update()
	{
        if (player == null) return;
		if (_isExit) GameObject.Destroy(player);

        player.transform.localPosition = Vector3.Lerp(player.transform.localPosition, transform.position, 1);
	 	player.transform.rotation = Quaternion.Lerp(player.transform.rotation, Quaternion.Euler(transform.rotation), 1);
	 	name.text = transform.name;
	}
	public void Exit(){ this._isExit = true; }
	public void Add(){ this._isAdd = true; }
	public bool isAdd(){ return _isAdd; }

}
public class Request
{
	public int msgid;
}
public class RequestObjects
{
	public int msgid;
	public string ip;
	public List<Player> clients;
    public RequestObjects() { clients = new List<Player>(); }
}
public class RequestObject
{
    public int msgid;
    public Player client;
    public RequestObject() { client = new Player(); }
}

public class Game : MonoBehaviour
{
    Thread readThread;
    public UdpClient client;

    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public bool isConnecting = false;

    public int Port = 22023;
    public string IP = "78.24.222.166";

    public float TimerUpdate = 1f; // 1 sec
    public float TimerReconnect = 3f; // 3 sec

    private float tempTimerUpdate = 1f; // 1 sec
    private float tempTimerReconnect = 3f; // 3 sec

    public GameObject TemplatePlayer;
    public InputField Name;

    private GameObject scene;
    private GameObject player;
    private Player playerSync;

    private List<Player> players;

    public void SaveName()
    {
    	playerSync.transform.name = Name.text;
    	player.transform.GetChild(1).GetComponent<TextMesh>().text = playerSync.transform.name;
    }
    private void AddPlayer(Player _player)
    {
        if(scene != null)
        {
            GameObject __player = Instantiate(TemplatePlayer, scene.transform, false);
            SyncServerDown __syncPlayer = __player.GetComponent<SyncServerDown>();
            __syncPlayer.SetPlayer(_player);
            _player.Add();
        }
    }
    void Send(string json, bool secure = false)
    {
    	if(secure)
    		if (!isConnected) return;
    	byte[] sendBytes = new UTF8Encoding(true).GetBytes(json);
	    client.Send(sendBytes, sendBytes.Length);
        print(json);
    }

    // Start is called before the first frame update
    void Start()
    {
    	players = new List<Player>();

        scene = GameObject.Find("Game").gameObject;
        player = GameObject.Find("Player").gameObject;

        playerSync = new Player();
        playerSync.transform.name = Name.text;

    	client = new UdpClient(IP, Port);

        // Create thread for reading UDP messages
        readThread = new Thread(new ThreadStart(ReceiveData));
        readThread.IsBackground = true;
        readThread.Start();
    }


    // Update is called once per frame
    void Update()
    {
    	string request;

        if(!isConnecting)
        {
            request = "{\"msgid\":10000}";
            Send(request);
            isConnecting = true;
        }

        if(isConnected)
        {
            if (tempTimerUpdate <= 0)
            {
                request = "{\"msgid\":20000}";
                Send(request);
                tempTimerUpdate = TimerUpdate;
            }
            else tempTimerUpdate -= Time.fixedDeltaTime;

            if(playerSync.transform.position != player.transform.localPosition || playerSync.transform.rotation != player.transform.eulerAngles)
            {
	            playerSync.transform.position = player.transform.localPosition;
	        	playerSync.transform.rotation = player.transform.eulerAngles;

	        	string json = JsonUtility.ToJson(playerSync);
	        	request = "{\"msgid\":10003, \"client\":"+json+"}";
	            Send(request);            	
            }
        }
        else
        {
            tempTimerReconnect -= Time.fixedDeltaTime;
        }

        if (tempTimerReconnect <= 0)
        {
            isConnecting = false;
            tempTimerReconnect = TimerReconnect;
        }

        for(int i = 0; i < players.Count; i++)
        {
        	if(!players[i].isAdd())
        		AddPlayer(players[i]);
        	else players[i].Update();
        }
    }

    // Receive thread function
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

                Request response = JsonUtility.FromJson<Request>(text);

                switch(response.msgid)
                {
                	case 10000: //connected
                	{
                		RequestObjects response_objects = JsonUtility.FromJson<RequestObjects>(text);
                        print("Connected");
                        playerSync.ip = response_objects.ip;
                		isConnected = true;
                	}
                	break;
                	case 10001: //add player
                	{
                		print("connect player");
                		RequestObject response_object = JsonUtility.FromJson<RequestObject>(text);
                		players.Add(response_object.client);
                	}
                	break;
                	case 10002: //remove player
                	{
                		print("exit player");
                		RequestObject response_object = JsonUtility.FromJson<RequestObject>(text);
                        int id = players.FindIndex(s => s.ip == response_object.client.ip);
                        if (id > -1)
                			players[id].Exit();
                	}
                	break;
                	case 10003: //update player
                	{
                		print(text);
                		RequestObject response_object = JsonUtility.FromJson<RequestObject>(text);
                        int id = players.FindIndex(s => s.ip == response_object.client.ip);
                        if (id > -1)
                			players[id].transform.Set(response_object.client.transform);
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
}
