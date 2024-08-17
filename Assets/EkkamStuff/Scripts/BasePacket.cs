using UnityEngine;
using System.IO;

namespace Ekkam
{
    public class BasePacket
    {
        public enum Type
        {
            None,
            GameStart,
            MoveAction, // is a GridPositionPacket
            TeleportAction, // is a GridPositionPacket
            DestroyAgentAction, // is a GridPositionPacket
            AttackAction,
            AbilityAction,
            StartTurn,
            EndTurn,
            ItemPickup
        }

        public Type type;
        public AgentData AgentData;

        protected MemoryStream wms;
        protected BinaryWriter bw;

        public MemoryStream rms;
        public BinaryReader br;

        public BasePacket()
        {
            this.type = Type.None;
            this.AgentData = new AgentData();
        }

        public BasePacket(Type type, AgentData agentData)
        {
            this.type = type;
            this.AgentData = agentData;
        }

        public void BeginSerialize()
        {
            wms = new MemoryStream();
            bw = new BinaryWriter(wms);

            bw.Write((int)type);
            bw.Write(AgentData.id);
            bw.Write(AgentData.name);
        }

        public byte[] EndSerialize()
        {
            return wms.ToArray();
        }

        public BasePacket BaseDeserialize(byte[] data)
        {
            rms = new MemoryStream(data);
            br = new BinaryReader(rms);

            type = (Type)br.ReadInt32();
            AgentData = new AgentData(br.ReadString(), br.ReadString());

            return this;
        }

        public virtual byte[] Serialize()
        {
            BeginSerialize();
            return EndSerialize();
        }
    }
    
    public class GameStartPacket : BasePacket
    {
        public int clientIndex;
        public int clientCount;
        
        public GameStartPacket() : base(Type.GameStart, new AgentData("", ""))
        {
            this.clientIndex = 0;
            this.clientCount = 0;
        }
        
        public GameStartPacket(Type type, AgentData agentData, int clientIndex, int clientCount) : base(type, agentData)
        {
            this.clientIndex = clientIndex;
            this.clientCount = clientCount;
        }
        
        public override byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(clientIndex);
            bw.Write(clientCount);
            return EndSerialize();
        }
        
        public GameStartPacket Deserialize(byte[] data)
        {
            GameStartPacket packet = new GameStartPacket();
            BasePacket basePacket = packet.BaseDeserialize(data);
            packet.type = basePacket.type;
            packet.AgentData = basePacket.AgentData;
            packet.clientIndex = basePacket.br.ReadInt32();
            packet.clientCount = basePacket.br.ReadInt32();
            return packet;
        }
    }
    
    // This packet is sent with different types of actions that only require a target position (pro gamer move)
    public class GridPositionPacket : BasePacket
    {
        public Vector2Int targetPosition;

        public GridPositionPacket() : base(Type.None, new AgentData("", ""))
        {
            this.targetPosition = Vector2Int.zero;
        }

        public GridPositionPacket(Type type, AgentData agentData, Vector2Int targetPosition) : base(type, agentData)
        {
            this.targetPosition = targetPosition;
        }

        public override byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(targetPosition.x);
            bw.Write(targetPosition.y);
            return EndSerialize();
        }

        public GridPositionPacket Deserialize(byte[] data)
        {
            GridPositionPacket packet = new GridPositionPacket();
            BasePacket basePacket = packet.BaseDeserialize(data);
            packet.type = basePacket.type;
            packet.AgentData = basePacket.AgentData;
            packet.targetPosition = new Vector2Int(basePacket.br.ReadInt32(), basePacket.br.ReadInt32());
            return packet;
        }
    }
    
    // This packet is sent when an agent damages an agent at target position
    public class AttackActionPacket : GridPositionPacket
    {
        public float damage;

        public AttackActionPacket() : base(Type.AttackAction, new AgentData("", ""), Vector2Int.zero)
        {
            this.damage = 0f;
        }

        public AttackActionPacket(Type type, AgentData agentData, Vector2Int targetPosition, float damage) : base(type, agentData, targetPosition)
        {
            this.damage = damage;
        }

        public override byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(targetPosition.x);
            bw.Write(targetPosition.y);
            bw.Write(damage);
            return EndSerialize();
        }

        public AttackActionPacket Deserialize(byte[] data)
        {
            AttackActionPacket packet = new AttackActionPacket();
            BasePacket basePacket = packet.BaseDeserialize(data);
            packet.type = basePacket.type;
            packet.AgentData = basePacket.AgentData;
            packet.targetPosition = new Vector2Int(basePacket.br.ReadInt32(), basePacket.br.ReadInt32());
            packet.damage = basePacket.br.ReadSingle();
            return packet;
        }
    }
    
    // This packet is sent when an agent uses an ability
    public class AbilityActionPacket : BasePacket
    {
        public string attackName;
        public Agent.AttackDirection direction;

        public AbilityActionPacket() : base(Type.AbilityAction, new AgentData("", ""))
        {
            this.attackName = "";
            this.direction = Agent.AttackDirection.North;
        }

        public AbilityActionPacket(Type type, AgentData agentData, string abilityName, Agent.AttackDirection direction) : base(type, agentData)
        {
            this.attackName = abilityName;
            this.direction = direction;
        }

        public override byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(attackName);
            bw.Write((int)direction);
            return EndSerialize();
        }

        public AbilityActionPacket Deserialize(byte[] data)
        {
            AbilityActionPacket packet = new AbilityActionPacket();
            BasePacket basePacket = packet.BaseDeserialize(data);
            packet.type = basePacket.type;
            packet.AgentData = basePacket.AgentData;
            packet.attackName = basePacket.br.ReadString();
            packet.direction = (Agent.AttackDirection)basePacket.br.ReadInt32();
            return packet;
        }
    }
    
    public class StartTurnPacket : BasePacket
    {
        public string agentID;
        
        public StartTurnPacket() : base(Type.StartTurn, new AgentData("", ""))
        {
            this.agentID = "";
        }
        
        public StartTurnPacket(Type type, AgentData agentData, string agentID) : base(type, agentData)
        {
            this.agentID = agentID;
        }
        
        public override byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(agentID);
            return EndSerialize();
        }
        
        public StartTurnPacket Deserialize(byte[] data)
        {
            StartTurnPacket packet = new StartTurnPacket();
            BasePacket basePacket = packet.BaseDeserialize(data);
            packet.type = basePacket.type;
            packet.AgentData = basePacket.AgentData;
            packet.agentID = basePacket.br.ReadString();
            return packet;
        }
    }
    
    public class EndTurnPacket : BasePacket
    {
        public EndTurnPacket() : base(Type.EndTurn, new AgentData("", ""))
        {
        }

        public EndTurnPacket(Type type, AgentData agentData) : base(type, agentData)
        {
        }

        public override byte[] Serialize()
        {
            BeginSerialize();
            return EndSerialize();
        }

        public EndTurnPacket Deserialize(byte[] data)
        {
            EndTurnPacket packet = new EndTurnPacket();
            BasePacket basePacket = packet.BaseDeserialize(data);
            packet.type = basePacket.type;
            packet.AgentData = basePacket.AgentData;
            return packet;
        }
    }
}