using UnityEngine;

namespace Net
{
    /**
     * Responsible for translating network commands into game logic and vice-versa.
     */
    public class NetworkGame : MonoBehaviour
    {
        public static NetworkGame instance;

        public void InCmdPosition(Cmd cmd)
        {
            BitReader reader = new BitReader(cmd.payload);
            ushort plyId = reader.ReadUInt16();
            Vector3 pos = reader.ReadVector();
            Quaternion rot = reader.ReadQuaternion();

            NetworkPlayer ply = NetworkPlayer.FindById(plyId);
            ply.transform.position = pos;
            ply.transform.rotation = rot;
        }

        public void OutCmdPosition(ushort plyId, Vector3 pos, Quaternion rot)
        {
            byte[] payload = new byte[16];
            BitWriter writer = new BitWriter(payload);
            writer.WriteUInt16(plyId);
            writer.WriteVector(pos);
            writer.WriteQuaternion(rot);

            Cmd cmd = new Cmd
            {
                cmdType = (byte)CmdType.POSITION,
                payload = payload,
            };

            NetworkManager.instance.SendCmdToServer(cmd);
        }

        private void Start()
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }
}