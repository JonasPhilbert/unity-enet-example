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

        public void StartServer()
        {
            print($"Starting server.");
            server = new NetServer();
            server.Start();
            ConnectPeer("127.0.0.1");
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
        }

        private void Client_OnCmd(Cmd cmd)
        {
            switch (cmd.cmdType)
            {
                case (byte)CmdType.INVALID:
                    break;
                case (byte)CmdType.POSITION:
                    NetworkGame.instance.InCmdPosition(cmd);
                    break;
            }
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