using ENet;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

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

    public class NetParser
    {
        public static Cmd ParsePacket(ENet.Event netEvent)
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


    public class BitReader
    {
        private byte[] source;
        private ushort cursor = 0;
        public BitReader(byte[] source)
        {
            this.source = source;
        }

        public ushort ReadUInt16()
        {
            var result = BitConverter.ToUInt16(source, cursor);
            cursor += 2; // UInt16 is 2 bytes.
            return result;
        }

        public float ReadSingle()
        {
            var result = BitConverter.ToSingle(source, cursor);
            cursor += 2; // Single is 2 bytes.
            return result;
        }

        public Vector3 ReadVector()
        {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }
    }

    public class BitWriter
    {
        private byte[] destination;
        private ushort cursor = 0;
        public BitWriter(byte[] destination)
        {
            this.destination = destination;
        }

        public void WriteUInt16(ushort val)
        {
            byte[] bitVal = BitConverter.GetBytes(val);
            Array.Copy(bitVal, 0, destination, cursor, bitVal.Length);
            cursor += 2;
        }
        public void WriteSingle(float val)
        {
            byte[] bitVal = BitConverter.GetBytes(val);
            Array.Copy(bitVal, 0, destination, cursor, bitVal.Length);
            cursor += 2;
        }

        public void WriteVector(Vector3 val)
        {
            WriteSingle(val.x);
            WriteSingle(val.y);
            WriteSingle(val.z);
        }

        public void WriteQuaternion(Quaternion val)
        {
            WriteSingle(val.x);
            WriteSingle(val.y);
            WriteSingle(val.z);
            WriteSingle(val.w);
        }
    }
}