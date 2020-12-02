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

[Serializable] public class Client{
    public string ip;
    public long lastTimeServer;
    private EndPoint point;
    public Object transform;
    public Client(EndPoint _p){ 
        point = _p; 
        ip = ((IPEndPoint)_p).Address.ToString(); 
        transform = new Object();
        transform.name = ip;
    }
    public void Send(Socket server, string json)
    {
        byte[] sendBytes = new UTF8Encoding(true).GetBytes(json);
        server.SendTo(sendBytes, point);
    }
}

public class GameServer : MonoBehaviour
{
    Thread serverThread;

    private Socket server;
    private int port = 22023;
    private List<Client> clients;
    private GameObject scene;

    private struct Request{
        public int msgid;
    }

    void Start()
    {
        scene = GameObject.Find("Game").gameObject;

        clients = new List<Client>();

        server = new Socket(IPAddress.Any.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint pointIP = new IPEndPoint(IPAddress.Any, port);
        
        try {
            server.Bind(pointIP);
        }
        catch (Exception e) {
            print("server error: " + e.ToString());
        }

        // create thread for reading UDP messages
        serverThread = new Thread(new ThreadStart(ReceiveData));
        serverThread.IsBackground = true;
        serverThread.Start();
    }

	// Unity Application Quit Function
    void OnApplicationQuit()
    {
        stopThread();
    }

    // Stop reading UDP messages
    private void stopThread()
    {
        if (serverThread.IsAlive)
        {
            serverThread.Abort();
        }
        server.Close();
    }

    // receive thread function
    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                byte[] data = new byte[1024];

                // Creates an IPEndPoint to capture the identity of the sending host.
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderRemote = (EndPoint)sender;

                server.ReceiveFrom(data, ref senderRemote);
                // encode UTF8-coded bytes to text format
                string text = new UTF8Encoding(true).GetString(data);

                Request request = JsonUtility.FromJson<Request>(text);
                string response = "";

                switch(request.msgid)
                {
                    case 10000:
                    {
                        Client client = new Client(senderRemote);
                        if(clients.IndexOf(client) > -1) 
                        {
                            print("Client already added -> " + ((IPEndPoint)senderRemote).Address.ToString());
                            break;
                        }
                        
                        string json_clients = JsonUtility.ToJson(clients);
                        response = "{\"msgid\":10000, \"clients\":\""+json_clients+"\"}";
                        client.Send(server, response);

                        for(int id = 0; id < clients.Count; id++)
                        {
                            string json_client = JsonUtility.ToJson(client);
                            response = "{\"msgid\":10001, \"client\":\""+json_client+"\"}";
                            clients[id].Send(server, response);
                        }

                        clients.Add(client);
                        print("Add client -> " + ((IPEndPoint)senderRemote).Address.ToString());
                    }
                    break;
                    case 20000:
                    {
                        Client client = new Client(senderRemote);
                        int id = clients.IndexOf(client);
                        if(id > -1)
                            clients[id].lastTimeServer = Time.time;
                    }
                    break;
                }

                for(int id = 0; id < clients.Count; id++)
                {
                    if(clients[id].lastTimeServer < Time.time + 3000)
                        clients.Remove(client);
                        for(int id = 0; id < clients.Count; id++)
                        {
                            string json_client = JsonUtility.ToJson(client);
                            response = "{\"msgid\":10002, \"client\":\""+json_client+"\"}";
                            clients[id].Send(server, response);
                        }
                        print("Exit client -> " + ((IPEndPoint)senderRemote).Address.ToString());
                }


            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }
}
