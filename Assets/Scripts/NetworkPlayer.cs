using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Net
{
    public class NetworkPlayer : MonoBehaviour
    {
        private static List<NetworkPlayer> instances = new();
        public static NetworkPlayer FindById(ushort id)
        {
            return instances.Find((ply) => ply.Id == id);
        }
        public static List<NetworkPlayer> All()
        {
            return instances;
        }
        public static NetworkPlayer GetLocal()
        {
            return instances.Find((ply) => ply.IsLocal);
        }

        public ushort Id;
        public bool IsLocal;

        private void Update()
        {
            if (IsLocal)
            {
                NetworkGame.instance.OutCmdPosition(Id, transform.position, transform.rotation);
            }
        }

        private void Start()
        {
            instances.Add(this);
        }
    }
}