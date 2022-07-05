using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using System;
using Net;

public class NetServer : ITicked
{
    public event Action<ENet.Event> OnClientConnected;
    public event Action<ENet.Event> OnClientDisconnected;
    public event Action<ENet.Event> OnClientTimeout;
    public event Action<Cmd> OnCmd;

    public Host ENetClient { get { return client; } }

    private Host client;
    public void Start()
    {
        client = new Host();
        Address address = new Address();
        address.Port = 7777;
        client.Create(address, 64);
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
                Debug.Log($"Peer {netEvent.Peer.ID}/{netEvent.Peer.IP} connected.");
                OnClientConnected.Invoke(netEvent);
                break;
            case ENet.EventType.Disconnect:
                Debug.Log($"Peer {netEvent.Peer.ID}/{netEvent.Peer.IP} disconnected.");
                OnClientDisconnected.Invoke(netEvent);
                break;
            case ENet.EventType.Timeout:
                Debug.Log($"Peer {netEvent.Peer.ID}/{netEvent.Peer.IP} timed out.");
                break;
            case ENet.EventType.Receive:
                Debug.Log($"Packet received from peer {netEvent.Peer.ID}/{netEvent.Peer.IP} on channel {netEvent.ChannelID} with length {netEvent.Packet.Length}.");
                netEvent.Packet.Dispose();
                break;
        }

        client.Flush();
    }

    ~NetServer()
    {
        client.Dispose();
    }
}
