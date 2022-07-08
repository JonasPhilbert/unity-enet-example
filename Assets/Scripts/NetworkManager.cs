using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Net
{
    /**
     * Responsible for networking.
     * Shouldn't interact with game directly - see NetworkGame.
     */
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        public TMPro.TMP_InputField inputHostnameElement;

        public bool IsHost => server != null;
        private string inputHostname => inputHostnameElement.text;
        private NetServer server;
        private NetClient client;
        private ushort plyIdCursor = 0;

        public void StartServer()
        {
            print($"Starting server.");
            server = new NetServer();
            server.Start();
            server.OnCmd += Server_OnCmd;
            ConnectPeer("127.0.0.1");
        }

        private void Server_OnCmd(Cmd cmd)
        {
            switch ((CmdType)cmd.cmdType)
            {
                case CmdType.REQUEST_JOIN:
                    byte[] payload = new byte[2];
                    BitWriter writer = new(payload);
                    writer.WriteUInt16(++plyIdCursor);
                    Cmd confirmCmd = new Cmd
                    {
                        cmdType = (byte)CmdType.CONFIRM_JOIN,
                        payload = payload,
                    };
                    server.SendCmd(confirmCmd, cmd.peerId);

                    Cmd joinCmd = new Cmd
                    {
                        cmdType = (byte)CmdType.PLAYER_JOINED,
                        payload = payload,
                    };
                    server.BroadcastCmd(joinCmd, cmd.peerId);
                    break;
            }
        }

        public void ConnectPeer(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
            {
                hostname = inputHostname;
            }
            print($"Attempting to connecting peer on hostname: {hostname}");

            client = new NetClient();
            client.Connect(hostname);
            client.OnCmd += Client_OnCmd;
            client.OnConnected += Client_OnConnected;
        }

        private void Client_OnCmd(Cmd cmd)
        {
            switch ((CmdType)cmd.cmdType)
            {
                case CmdType.INVALID:
                    break;
                case CmdType.POSITION:
                    NetworkGame.instance.InCmdPosition(cmd);
                    break;
                case CmdType.CONFIRM_JOIN:
                    NetworkGame.instance.InCmdConfirmJoin(cmd);
                    break;
                case CmdType.PLAYER_JOINED:
                    NetworkGame.instance.InCmdPlayerJoined(cmd);
                    break;
            }
        }
        private void Client_OnConnected()
        {
            Cmd joinCmd = new Cmd
            {
                cmdType = (byte)CmdType.REQUEST_JOIN,
                payload = new byte[] { 42 }, // We have to send something. In the future, maybe some client information, like player name.
            };
            SendCmdToServer(joinCmd);
        }

        public void SendCmdToServer(Cmd cmd)
        {
            client.SendCmd(cmd);
        }

        private void Start()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            print($"ENet init result: {ENet.Library.Initialize()}");
        }

        void Update()
        {
            if (server != null) server.Tick();
            if (client != null) client.Tick();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                print("Sending test package:");
                client.SendCmd(new Cmd
                {
                    cmdType = (byte)CmdType.INVALID,
                    payload = new byte[] { 1, 2, 3},
                });
            }
        }

        private void OnDestroy()
        {
            ENet.Library.Deinitialize();
        }
    }
}