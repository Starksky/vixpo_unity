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

[Serializable] public class Player
{
	public string address;
	public int port;
	public long last_time;
	public bool leave;
	public Vector3 position;
	private GameObject _object;
	public Player(){}
	public Player(Vector3 _p){ position = _p; }
	public void Create(GameObject _o){ _object = _o; }
	public void Update(){ if(_object != null) _object.transform.localPosition = position; }
}
public class Answer
{
	public int msgid;
	public int id_player;
	public Player[] players;
	public Answer(){ players = new Player[]{}; }
}
public class Game : MonoBehaviour
{
    Thread readThread;
    UdpClient client;

    public int port = 22023;
    public string ip = "78.24.222.166";
    public GameObject templatePlayer;
    private GameObject scene;
    private int id_player = 0;
    private GameObject _player;
    private Player player;
    private List<Player> players;
    private bool isConnected = false;

    // Start is called before the first frame update
    void Start()
    {
    	players = new List<Player>();

    	scene = GameObject.Find("Game").gameObject;
    	_player = GameObject.Find("Player").gameObject;
    	player = new Player(_player.transform.localPosition);

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
    void Update()
    {
    	for(int id = 0; id < players.Count; id++)
    		players[id].Update();

        //player.position = _player.transform.localPosition;
        if(isConnected)
        {

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

                // show received message
                print(">> " + text);

                Answer answer = JsonUtility.FromJson<Answer>(text);
                switch(answer.msgid)
                {
                	case 10001:
                	{
                		id_player = answer.id_player;
                		for(int id = 0; id < answer.players.Length; id++)
                		{
                			players.Add(new Player(answer.players[id].position));
                			players[players.Count - 1].Create(Instantiate(templatePlayer, scene.transform, false).gameObject);
                		}

                		isConnected = true;
                	}
                	break;

                	case 10002:
                	{
                		players.Add(new Player(answer.players[0].position));
                		players[players.Count - 1].Create(Instantiate(templatePlayer, scene.transform, false).gameObject);
                	}
                	break;

                	case 10003:
                	{
                		players[answer.id_player].position = answer.players[0].position;
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
}
