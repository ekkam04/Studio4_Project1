using System.Collections;
using System.Collections.Generic;
using Ekkam;
using System.IO;
using UnityEngine;

public class ItemPacket : BasePacket
{
    // its taking information from the base packet
    public string itemKey;
    
    public ItemPacket() : base(Type.None, new AgentData("", ""))
    {
        this.itemKey = "";
    }

    public ItemPacket(Type type, AgentData agentData, string itemKey) : base(type, agentData)
    {
        this.itemKey = itemKey;
    }
    public override byte[] Serialize()
    {
        BeginSerialize();
        bw.Write(itemKey);
        return EndSerialize();
    }

    public ItemPacket Deserialize(byte[] data)
    {
        ItemPacket itemPacket = new ItemPacket();
        BasePacket basePacket = itemPacket.BaseDeserialize(data);
        itemPacket.type = basePacket.type;
        itemPacket.AgentData = basePacket.AgentData;
        itemPacket.itemKey = basePacket.br.ReadString();
        return itemPacket;
    }
}
