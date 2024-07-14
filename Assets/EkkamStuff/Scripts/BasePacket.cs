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
            MoveAction,
            TeleportAction,
            AttackAction,
            EndTurn
        }

        public Type type;
        public PlayerData playerData;

        protected MemoryStream wms;
        protected BinaryWriter bw;

        public MemoryStream rms;
        public BinaryReader br;

        public BasePacket()
        {
            this.type = Type.None;
            this.playerData = new PlayerData();
        }

        public BasePacket(Type type, PlayerData playerData)
        {
            this.type = type;
            this.playerData = playerData;
        }

        public void BeginSerialize()
        {
            wms = new MemoryStream();
            bw = new BinaryWriter(wms);

            bw.Write((int)type);
            bw.Write(playerData.id);
            bw.Write(playerData.name);
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
            playerData = new PlayerData(br.ReadString(), br.ReadString());

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
        
        public GameStartPacket() : base(Type.GameStart, new PlayerData("", ""))
        {
            this.clientIndex = 0;
            this.clientCount = 0;
        }
        
        public GameStartPacket(Type type, PlayerData playerData, int clientIndex, int clientCount) : base(type, playerData)
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
            packet.playerData = basePacket.playerData;
            packet.clientIndex = basePacket.br.ReadInt32();
            packet.clientCount = basePacket.br.ReadInt32();
            return packet;
        }
    }
    
    // This packet is sent with different types of actions that only require a target position (pro gamer move)
    public class GridPositionPacket : BasePacket
    {
        public Vector2Int targetPosition;

        public GridPositionPacket() : base(Type.None, new PlayerData("", ""))
        {
            this.targetPosition = Vector2Int.zero;
        }

        public GridPositionPacket(Type type, PlayerData playerData, Vector2Int targetPosition) : base(type, playerData)
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
            packet.playerData = basePacket.playerData;
            packet.targetPosition = new Vector2Int(basePacket.br.ReadInt32(), basePacket.br.ReadInt32());
            return packet;
        }
    }
    
    public class EndTurnPacket : BasePacket
    {
        public EndTurnPacket() : base(Type.EndTurn, new PlayerData("", ""))
        {
        }

        public EndTurnPacket(Type type, PlayerData playerData) : base(type, playerData)
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
            packet.playerData = basePacket.playerData;
            return packet;
        }
    }
}