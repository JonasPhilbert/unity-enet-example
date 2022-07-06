using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using System;
using Net;

public class NetServer : ITicked
{
    public event Action OnClientConnected;
    public event Action OnClientDisconnected;
    public event Action OnClientTimeout;
    public event Action<Cmd> OnCmd;

    private Host enet;
    public void Start()
    {
        enet = new Host();
        Address address = new Address();
        address.Port = 7777;
        enet.Create(address, 64);
    }
    public void BroadcastCmd(Cmd cmd)
    {
        Packet packet = default(Packet);

        packet.Create(cmd.payload);
        enet.Broadcast(0, ref packet);
    }

    public void Tick()
    {
        ENet.Event netEvent;
        if (enet.CheckEvents(out netEvent) <= 0)
        {
            enet.Service(0, out netEvent); // 0 timeout to not block Unity thread.
        }
        switch (netEvent.Type)
        {
            case ENet.EventType.None:
                break;
            case ENet.EventType.Connect:
                Log($"Client connected to server.");
                //OnClientConnected.Invoke();
                break;
            case ENet.EventType.Disconnect:
                Log($"Client disconnected from server.");
                //OnClientDisconnected.Invoke();
                break;
            case ENet.EventType.Timeout:
                Log($"Client timed out of server.");
                //OnClientTimeout.Invoke();
                break;
            case ENet.EventType.Receive:
                Log($"Packet received from client on channel {netEvent.ChannelID} with length {netEvent.Packet.Length}.");
                Cmd cmd = NetParser.ParsePacket(netEvent);
                //OnCmd.Invoke(cmd);
                BroadcastCmd(cmd);
                netEvent.Packet.Dispose();
                break;
        }

        enet.Flush();
    }

    private void Log(string msg)
    {
        Debug.Log($"[SERVER] {msg}");
    }

    ~NetServer()
    {
        enet.Dispose();
    }
}
