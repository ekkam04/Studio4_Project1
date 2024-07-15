using UnityEngine;

namespace Ekkam
{
    public class AgentData
    {
        public string id;
        public string name;

        public AgentData()
        {
            this.id = "";
            this.name = "";
        }

        public AgentData(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}