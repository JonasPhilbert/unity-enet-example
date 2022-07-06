using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using System;
using Net;

public class NetClient : ITicked {
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnTimeout;
    public event Action<Cmd> OnCmd;

    private Host enet;
    private Peer? server;

    public void Connect(string hostname)
    {
        enet = new Host();
        Address address = new Address();
        address.Port = 7777;
        address.SetHost(hostname);
        enet.Create();

        server = enet.Connect(address, 0);
    }

    public void SendCmd(Cmd cmd)
    {
        if (server is null) throw new Exception("Attempting to send cmd when not connected to server.");

        Packet packet = default(Packet);
        packet.Create(cmd.payload);
        server.Value.Send(0, ref packet);
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
                Log($"Connected to server.");
                //OnConnected.Invoke();
                break;
            case ENet.EventType.Disconnect:
                Log($"Disconnected from server.");
                //OnDisconnected.Invoke();
                break;
            case ENet.EventType.Timeout:
                Log($"Timed out from server.");
                //OnTimeout.Invoke();
                break;
            case ENet.EventType.Receive:
                Log($"Packet received from server on channel {netEvent.ChannelID} with length {netEvent.Packet.Length}.");
                OnCmd.Invoke(NetParser.ParsePacket(netEvent));
                netEvent.Packet.Dispose();
                break;
        }

        enet.Flush();
    }

    private void Log(string msg)
    {
        Debug.Log($"[CLIENT] {msg}");
    }

    ~NetClient() {
        enet.Dispose();
    }
}
