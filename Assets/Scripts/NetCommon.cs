using ENet;
using System;
using System.Runtime.InteropServices;

namespace Net
{
    public enum CmdType
    {
        INVALID = 0,
        POSITION = 1,
    }

    public struct Cmd
    {
        public uint peerId;
        public byte cmdType;
        public byte[] payload;

        public CmdType Type
        {
            get
            {
                return (CmdType)cmdType;
            }
        }
    }

    public class NetHelper
    {
        public static Cmd ParsePackage(ENet.Event netEvent)
        {
            uint peerId = netEvent.Peer.ID;
            byte cmdType = Marshal.ReadByte(netEvent.Packet.Data);
            byte[] buffer = new byte[1024];
            Marshal.Copy(netEvent.Packet.Data, buffer, 1, netEvent.Packet.Length - 1);

            return new Cmd
            {
                peerId = peerId,
                cmdType = cmdType,
                payload = buffer,
            };
        }
    }
}