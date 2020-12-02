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

	private bool exit;
	private GameObject player;
	private TextMesh name;

    public Object transform;

    public bool isAdd(){ return player != null ? true : false; }
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
		if(exit) GameObject.Destroy(player);

        player.transform.localPosition = Vector3.Lerp(player.transform.localPosition, transform.position, 1);
	 	player.transform.rotation = Quaternion.Lerp(player.transform.rotation, Quaternion.Euler(transform.rotation), 1);
	 	name.text = transform.name;
	}
	public void Exit(){ this.exit = true; }

}
public class Response
{
	public int msgid;
}
public class ResponseObjects
{
	public int msgid;
	public List<Player> players;
    public ResponseObjects() { players = new List<Player>(); }
}
public class ResponseObject
{
    public int msgid;
    public Player player;
    public ResponseObject() { player = new Player(); }
}

public class Game : MonoBehaviour
{
    Thread readThread;
    public UdpClient client;

    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public bool isConnecting = false;

    public int Port = 22023;
    public string IP = "78.24.222.166";
    public GameObject TemplatePlayer;
    public InputField Name;

    private float TimerUpdate = 1f; // 1 sec
    private float TimerReconnect = 3f; // 1 sec
    private GameObject scene;
    private GameObject player;
    private Object playerSync;

    private List<Player> players;

    public void SaveName()
    {
    	playerSync.name = Name.text;
    	player.transform.GetChild(1).GetComponent<TextMesh>().text = playerSync.name;
    }

    void Send(string json, bool secure)
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

        playerSync = new Object();
        playerSync.position = player.transform.localPosition;
        playerSync.rotation = player.transform.eulerAngles;
        playerSync.name = Name.text;

    	client = new UdpClient(IP, Port);

        // create thread for reading UDP messages
        readThread = new Thread(new ThreadStart(ReceiveData));
        readThread.IsBackground = true;
        readThread.Start();
    }


    // Update is called once per frame
    void Update()
    {
        if(!isConnecting)
        {
            string request = "{\"msgid\":10000}";
            Send(request, false);
            isConnecting = true;
        }
        if(isConnected)
        {
            if (TimerUpdate <= 0)
            {
                string request = "{\"msgid\":20000}";
                Send(request, false);
                TimerUpdate = 1f;
            }
            else TimerUpdate -= Time.fixedDeltaTime;
        }
        else
        {
            TimerReconnect -= Time.fixedDeltaTime;
        }

        if (TimerReconnect <= 0)
        {
            isConnecting = false;
            TimerReconnect = 3f;
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

                Response response = JsonUtility.FromJson<Response>(text);

                switch(response.msgid)
                {
                	//Connected user
                	case 10000:
                	{
                        print(text);
                		isConnected = true;
                	}
                	break;
                	case 10001: //add player
                	{
                		ResponseObject response_object = JsonUtility.FromJson<ResponseObject>(text);
                		players.Add(response_object.player);
                	}
                	break;
                	case 10002: //remove player
                	{
                		ResponseObject response_object = JsonUtility.FromJson<ResponseObject>(text);
                        int id = players.FindIndex(s => s.ip == response_object.player.ip);
                        if (id > -1)
                			players[id].Exit();
                	}
                	break;
                	case 10003: //update player
                	{
                		ResponseObject response_object = JsonUtility.FromJson<ResponseObject>(text);
                        int id = players.FindIndex(s => s.ip == response_object.player.ip);
                        if (id > -1)
                			players[id].transform.Set(response_object.player.transform);
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
