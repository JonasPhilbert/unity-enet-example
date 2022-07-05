using ENet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    public TMPro.TMP_InputField inputHostname;
    public string peerState
    {
        get
        {
            if (client != null && client is NetClient)
            {
                return ((NetClient)client).ENetPeer.State.ToString();
            }
            else
            {
                return "";
            }
        }
    }

    private ITicked client;

    public void StartServer()
    {
        print($"Starting server XXX ->(ignore)-> on hostname: {inputHostname.text}");
        client = new NetServer();
        ((NetServer)client).Start();
    }

    public void ConnectPeer()
    {
        print($"Connecting peer on hostname: {inputHostname.text}");
        client = new NetClient();
        ((NetClient)client).Connect(inputHostname.text);
    }

    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        print($"ENet init result: {ENet.Library.Initialize()}");
    }

    void Update()
    {
        if (client != null)
        {
            client.Tick();
        }
    }

    private void OnDestroy()
    {
        ENet.Library.Deinitialize();
    }
}
