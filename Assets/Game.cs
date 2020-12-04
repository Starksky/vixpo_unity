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
    public Vector3 force;

    private bool _isAdd = false;

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
    public void Add() { this._isAdd = true; }
    public bool isAdd() { return _isAdd; }
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
		//name = _o.transform.GetChild(0).GetComponent<TextMesh>();
	}
	public void Update()
	{
        if (player == null) return;
		if (_isExit) GameObject.Destroy(player);

        //player.transform.localPosition = Vector3.Lerp(player.transform.localPosition, transform.position, 1);
	 	//player.transform.rotation = Quaternion.Lerp(player.transform.rotation, Quaternion.Euler(transform.rotation), 1);
	 	//name.text = transform.name;
	}
	public void Exit(){ this._isExit = true; }
	public void Add(){ this._isAdd = true; }
	public bool isAdd(){ return _isAdd; }

}
public class Request
{
	public int msgid;
}
public class RequestClients
{
	public int msgid;
	public string ip;
	public List<Player> clients;
    public RequestClients() { clients = new List<Player>(); }
}
public class RequestClient
{
    public int msgid;
    public Player client;
    public RequestClient() { client = new Player(); }
}
public class RequestObjects
{
    public int msgid;
    public List<Object> objects;
    public RequestObjects() { objects = new List<Object>(); }
}
public class RequestObject
{
    public int msgid;
    public Object obj;
    public RequestObject() { obj = new Object(); }
}
public class Game : MonoBehaviour
{
    Thread readThread;
    public UdpClient client;

    [HideInInspector] public bool isInit = false;
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
    private Rigidbody body;
    public Player playerSync;
    public SyncServerDown syncServerDownPlayer;

    private List<Player> players;
    private List<Object> objects;

    public long GetTimestamp()
    {
        return (long)(new TimeSpan(DateTime.Now.Ticks)).TotalSeconds;
    }

    public void SaveName()
    {
    	playerSync.transform.name = Name.text;
    	playerSync.transform.force = Vector3.zero;
    	player.transform.GetChild(1).GetComponent<TextMesh>().text = playerSync.transform.name;
    	string json = JsonUtility.ToJson(playerSync);
	    string request = "{\"msgid\":10003, \"client\":"+json+"}";
	    Send(request); 
    }

    private void AddPlayer(Player _player)
    {
        if(scene != null)
        {
            GameObject __player = Instantiate(TemplatePlayer, scene.transform, false);
            SyncServerDown __syncPlayer = __player.GetComponent<SyncServerDown>();
            __syncPlayer.SetPlayer(_player);
            _player.Create(__player);
            _player.Add();
        }
    }

    private void LinkObject(Object _object)
    {
        try
        {
            SyncServerSceneDown linkObject = GameObject.Find(_object.name).gameObject.GetComponent<SyncServerSceneDown>();
            linkObject.Link(_object);
            _object.Add();
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    public void Send(string json, bool secure = false)
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
        objects = new List<Object>();

        scene = GameObject.Find("Game").gameObject;
        player = GameObject.Find("Player").gameObject;
        body = player.GetComponent<Rigidbody>();
        syncServerDownPlayer = player.GetComponent<SyncServerDown>();

        playerSync = new Player();
        playerSync.transform.name = Name.text;
        syncServerDownPlayer.SetPlayer(playerSync);

    	client = new UdpClient(IP, Port);

        // Create thread for reading UDP messages
        readThread = new Thread(new ThreadStart(ReceiveData));
        readThread.IsBackground = true;
        readThread.Start();

        isInit = true;
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

        for (int i = 0; i < objects.Count; i++)
        {
            if (!objects[i].isAdd())
                LinkObject(objects[i]);
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

                Request request = JsonUtility.FromJson<Request>(text);

                switch(request.msgid)
                {
                	case 10000: //connected
                	{
                		RequestClients request_objects = JsonUtility.FromJson<RequestClients>(text);
                        print("Connected");
                        playerSync.ip = request_objects.ip;
                        players.AddRange(request_objects.clients);
                        isConnected = true;
                	}
                	break;
                	case 10001: //add player
                	{
                		print("connect player");
                		RequestClient request_object = JsonUtility.FromJson<RequestClient>(text);
                		players.Add(request_object.client);
                	}
                	break;
                	case 10002: //remove player
                	{
                		print("exit player");
                		RequestClient request_object = JsonUtility.FromJson<RequestClient>(text);
                        int id = players.FindIndex(s => s.ip == request_object.client.ip);
                        if (id > -1)
                			players[id].Exit();
                	}
                	break;
                	case 10003: //update player
                	{
                		RequestClient request_object = JsonUtility.FromJson<RequestClient>(text);
                        int id = players.FindIndex(s => s.ip == request_object.client.ip);
                        if (id > -1)
                			players[id].transform.Set(request_object.client.transform);
                		else
                			if(playerSync.ip == request_object.client.ip)
                			{
                				print(JsonUtility.ToJson(request_object.client.transform));
                				playerSync.transform.Set(request_object.client.transform);
                			}
                	}
                	break;
                    case 10004: //objects
                    {
                        RequestObjects request_objects = JsonUtility.FromJson<RequestObjects>(text);
                        objects.AddRange(request_objects.objects);
                    }
                    break;
                    case 10005: // update object
                    {
                        print(text);
                        RequestObject request_object = JsonUtility.FromJson<RequestObject>(text);
                        int id = objects.FindIndex(s => s.name == request_object.obj.name);
                        if (id > -1)
                           objects[id].Set(request_object.obj);
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
