using UnityEngine;

namespace Ekkam
{
    public class PlayerData
    {
        public string id;
        public string name;

        public PlayerData()
        {
            this.id = "";
            this.name = "";
        }

        public PlayerData(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}