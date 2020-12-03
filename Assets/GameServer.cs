﻿using System;
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
    private bool _isAdd = false;
    private EndPoint point;

    public string ip;
    public long lastTimeServer;
    public Object transform;

    private GameObject player;

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

    public void Add(GameObject _player){ _isAdd = true; player = _player; }
    public void Remove(){ GameObject.Destroy(player); Debug.Log("remove player"); }
    public bool isAdd(){ return _isAdd; }
}

public class ReceiveRequest{
    public EndPoint point;
    public string request;
    public bool isSet = false;
    public ReceiveRequest(EndPoint _p, string _r){ point = _p; request = _r; }
}

public class GameServer : MonoBehaviour
{
    public Socket server;
    Thread serverThread;

    public int Port = 22023;
    public GameObject TemplatePlayer; 
    
    private List<Client> clients;
    private GameObject scene;

    private struct Request{
        public int msgid;
    }

    private long GetTimestamp()
    {
        return (long)(new TimeSpan(DateTime.Now.Ticks)).TotalSeconds;
    }
    private void AddPlayer(Client client)
    {
        if(scene != null)
        {
            GameObject player = Instantiate(TemplatePlayer, scene.transform, false);
            SyncServerUp playerSync = player.GetComponent<SyncServerUp>();
            playerSync.SetClient(client);
            client.Add(player);
            print("add player");
        }
    }

    void Start()
    {
        scene = GameObject.Find("Game").gameObject;

        clients = new List<Client>();

        server = new Socket(IPAddress.Any.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint pointIP = new IPEndPoint(IPAddress.Any, Port);
        
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

    void Update()
    {
        string response = "";
        List<Client> removeClients = new List<Client>();

        for(int i = 0; i < clients.Count; i++)
        {
            if(clients[i] != null)
            {
                if (GetTimestamp() - clients[i].lastTimeServer > 3)
                {
                    for(int id = 0; id < clients.Count; id++)
                    {
                        if (i == id) continue;

                        string json_client = JsonUtility.ToJson(clients[i]);
                        response = "{\"msgid\":10002, \"client\":" + json_client + "}";
                        clients[id].Send(server, response);
                    }                 
                    removeClients.Add(clients[i]); 
                }

                if(!clients[i].isAdd())
                    AddPlayer(clients[i]);             
            }
        }

        for(int i = 0; i < removeClients.Count; i++)
        {
            string ip = clients[i].ip;
            clients[i].Remove();
            clients.Remove(clients[i]);
            print("Exit client -> " + ip);
        }

        removeClients.Clear();
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

    // Receive thread function
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

                Thread readThread = new Thread(new ThreadStart(delegate { ReadData( new ReceiveRequest(senderRemote, text) ); } ));
                readThread.IsBackground = true;
                readThread.Start();
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    private void ReadData(ReceiveRequest receive)
    {
        try
        {
            string response = "";

            Request request = JsonUtility.FromJson<Request>(receive.request);
            
            switch(request.msgid)
            {
                case 10000:
                {
                    Client client = new Client(receive.point);
                    client.lastTimeServer = GetTimestamp();

                    int id = clients.FindIndex(s => s.ip == client.ip);

                    if (id > -1) 
                    {
                        print("Client already added -> " + ((IPEndPoint)receive.point).Address.ToString());
                        break;
                    }
                    
                    string json_clients = JsonUtility.ToJson(clients);
                    response = "{\"msgid\":10000, \"ip\":\""+ ((IPEndPoint)receive.point).Address.ToString() +"\", \"clients\":"+json_clients+"}";
                    client.Send(server, response);

                    for(int i = 0; i < clients.Count; i++)
                    {
                        string json_client = JsonUtility.ToJson(client);
                        response = "{\"msgid\":10001, \"client\":"+json_client+"}";
                        clients[i].Send(server, response);
                    }

                    
                    clients.Add(client);
                    print("Add client -> " + ((IPEndPoint)receive.point).Address.ToString());
                }
                break;
                case 10003:
                {
                    RequestObject request_object = JsonUtility.FromJson<RequestObject>(receive.request);
                    Client client = new Client(receive.point);
                    int id = clients.FindIndex(s => s.ip == client.ip);
                    string json_client = JsonUtility.ToJson(request_object.client);

                    if(id > -1)
                        clients[id].transform.Set(request_object.client.transform);

                    response = "{\"msgid\":10003, \"client\":"+json_client+"}";
                    for(int i = 0; i < clients.Count; i++)
                    {
                        if(id == i) continue;
                        clients[i].Send(server, response);
                    }
                }
                break;
                case 20000:
                {
                    Client client = new Client(receive.point);
                    int id = clients.FindIndex(s => s.ip == client.ip);
                    if(id > -1)
                        clients[id].lastTimeServer = GetTimestamp();
                }
                break;
            }
        }
        catch(Exception err) { print(err.ToString()); }
    }
}
