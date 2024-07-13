using UnityEngine;

namespace Ekkam
{
    public class NetworkComponent : MonoBehaviour
    {
        public string ownerID;
        public string ownerName;

        public bool IsMine()
        {
            return ownerID == NetworkManager.instance.playerData.id;
        }
    }
}