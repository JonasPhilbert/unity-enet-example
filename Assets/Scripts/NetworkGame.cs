using UnityEngine;
using UnityEngine.SceneManagement;

namespace Net
{
    /**
     * Responsible for translating network commands into game logic and vice-versa.
     * Always from the client perspective.
     */
    public class NetworkGame : MonoBehaviour
    {
        public static NetworkGame instance;
        public GameObject playerPrefab;

        public void InCmdPosition(Cmd cmd)
        {
            BitReader reader = Reader(cmd);
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

        /**
         * When the server has confirmed the local player's request to join the server.
         * Not when other players join the server.
         */
        public void InCmdConfirmJoin(Cmd cmd)
        {
            SceneManager.LoadScene(1);
            var reader = Reader(cmd);
            ushort plyId = reader.ReadUInt16();
            GameObject ply = Instantiate(playerPrefab);
            NetworkPlayer netPly = ply.GetComponent<NetworkPlayer>();
            netPly.Id = plyId;
            netPly.IsLocal = true;
        }

        /**
         * When another client joins the server the player is current on.
         * Is not invoked when the local player joins.
         */
        public void InCmdPlayerJoined(Cmd cmd)
        {
            var reader = Reader(cmd);
            ushort plyId = reader.ReadUInt16();
            GameObject ply = Instantiate(playerPrefab);
            NetworkPlayer netPly = ply.GetComponent<NetworkPlayer>();
            netPly.Id = plyId;
            netPly.IsLocal = false;
            // TODO: Disable movement script (and other local-only scripts) of instantiated player object.
        }

        private BitReader Reader(Cmd cmd)
        {
            return new BitReader(cmd.payload);
        }

        private void Start()
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }
}