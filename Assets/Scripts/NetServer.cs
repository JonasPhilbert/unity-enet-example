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
    private List<Peer> peers = new();
    public void Start()
    {
        enet = new Host();
        Address address = new Address();
        address.Port = 7777;
        enet.Create(address, 64);
    }

    public void SendCmd(Cmd cmd, uint peerId)
    {
        Peer? peer = FindPeerById(peerId);
        if (!peer.HasValue) return;

        var packet = NetParser.PreparePacket(cmd);
        enet.Broadcast(0, ref packet, new Peer[] { peer.Value });
    }

    public void BroadcastCmd(Cmd cmd)
    {
        var packet = NetParser.PreparePacket(cmd);
        enet.Broadcast(0, ref packet);
    }

    public void BroadcastCmd(Cmd cmd, uint excludedPeerId)
    {
        Peer? peer = FindPeerById(excludedPeerId);
        if (!peer.HasValue) return;

        var packet = NetParser.PreparePacket(cmd);
        enet.Broadcast(0, ref packet, peer.Value);
    }

    public void BroadcastCmd(Cmd cmd, List<uint> peerIds)
    {
        // TODO
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
                peers.Add(netEvent.Peer);
                //OnClientConnected.Invoke();
                break;
            case ENet.EventType.Disconnect:
                Log($"Client disconnected from server.");
                peers.Remove(netEvent.Peer);
                //OnClientDisconnected.Invoke();
                break;
            case ENet.EventType.Timeout:
                Log($"Client timed out of server.");
                peers.Remove(netEvent.Peer);
                //OnClientTimeout.Invoke();
                break;
            case ENet.EventType.Receive:
                Log($"Packet received from client on channel {netEvent.ChannelID} with length {netEvent.Packet.Length}.");
                Cmd cmd = NetParser.ParsePacket(netEvent);
                OnCmd.Invoke(cmd);
                netEvent.Packet.Dispose();
                break;
        }

        enet.Flush();
    }

    private Peer? FindPeerById(uint peerId)
    {
        return peers.Find((peer) => peer.ID == peerId);
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
