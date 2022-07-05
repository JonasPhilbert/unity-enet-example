using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using System;
using Net;

public class NetClient : ITicked {
    public event Action<ENet.Event> OnConnected;
    public event Action<ENet.Event> OnDisconnected;
    public event Action<ENet.Event> OnTimeout;
    public event Action<Cmd> OnCmd;

    public Host ENetClient { get { return client; } }
    public Peer ENetPeer { get { return peer; } }

    private Host client;
    private Peer peer;

    public void Connect(string hostname)
    {
        client = new Host();
        Address address = new Address();
        address.Port = 7777;
        address.SetHost(hostname);
        client.Create();

        peer = client.Connect(address, 0);
    }

    public void Tick()
    {
        ENet.Event netEvent;
        if (client.CheckEvents(out netEvent) <= 0)
        {
            client.Service(0, out netEvent); // 0 timeout to not block Unity thread.
        }
        switch (netEvent.Type)
        {
            case ENet.EventType.None:
                break;
            case ENet.EventType.Connect:
                Debug.Log($"Connected to server.");
                OnConnected.Invoke(netEvent);
                break;
            case ENet.EventType.Disconnect:
                Debug.Log($"Disconnected from server.");
                OnDisconnected.Invoke(netEvent);
                break;
            case ENet.EventType.Timeout:
                Debug.Log($"Timed out from server.");
                OnTimeout.Invoke(netEvent);
                break;
            case ENet.EventType.Receive:
                Debug.Log($"Packet received from server on channel {netEvent.ChannelID} with length {netEvent.Packet.Length}.");
                OnCmd.Invoke(NetHelper.ParsePackage(netEvent));
                netEvent.Packet.Dispose();
                break;
        }

        client.Flush();
    }

    ~NetClient() {
        client.Dispose();
    }
}
