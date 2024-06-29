using UnityEngine;
using System.IO;

namespace Ekkam
{
    public class BasePacket
    {
        public enum Type
        {
            None,
            Position,
            Rotation,
            AnimationState
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
    
    public class PositionPacket : BasePacket
    {
        public Vector3 position;

        public PositionPacket() : base(Type.Position, new PlayerData("", ""))
        {
            this.position = Vector3.zero;
        }

        public PositionPacket(Type type, PlayerData playerData, Vector3 position) : base(type, playerData)
        {
            this.position = position;
        }

        public override byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(position.x);
            bw.Write(position.y);
            bw.Write(position.z);
            return EndSerialize();
        }

        public PositionPacket Deserialize(byte[] data)
        {
            PositionPacket packet = new PositionPacket();
            BasePacket basePacket = packet.BaseDeserialize(data);
            packet.type = basePacket.type;
            packet.playerData = basePacket.playerData;
            packet.position = new Vector3(basePacket.br.ReadSingle(), basePacket.br.ReadSingle(), basePacket.br.ReadSingle());
            return packet;
        }
    }
    
    public class RotationYPacket : BasePacket
    {
        public float rotationY;

        public RotationYPacket() : base(Type.Rotation, new PlayerData("", ""))
        {
            this.rotationY = 0;
        }

        public RotationYPacket(Type type, PlayerData playerData, float rotationY) : base(type, playerData)
        {
            this.rotationY = rotationY;
        }

        public override byte[] Serialize()
        {
            BeginSerialize();
            bw.Write(rotationY);
            return EndSerialize();
        }

        public RotationYPacket Deserialize(byte[] data)
        {
            RotationYPacket packet = new RotationYPacket();
            BasePacket basePacket = packet.BaseDeserialize(data);
            packet.type = basePacket.type;
            packet.playerData = basePacket.playerData;
            packet.rotationY = basePacket.br.ReadSingle();
            return packet;
        }
    }
    
    public class AnimationStatePacket : BasePacket
    {
        public enum AnimationCommandType
        {
            Bool,
            Trigger,
            Float
        }

        public AnimationCommandType commandType;
        public string parameterName;
        public bool boolValue;
        public float floatValue;

        public AnimationStatePacket() : base(Type.AnimationState, new PlayerData("", ""))
        {
            this.commandType = AnimationCommandType.Bool;
            this.parameterName = "";
            this.boolValue = false;
            this.floatValue = 0f;
        }

        public AnimationStatePacket(Type type, PlayerData playerData, AnimationCommandType commandType, string parameterName, bool boolValue, float floatValue) 
            : base(type, playerData)
        {
            this.commandType = commandType;
            this.parameterName = parameterName;
            this.boolValue = boolValue;
            this.floatValue = floatValue;
        }

        public override byte[] Serialize()
        {
            BeginSerialize();
            bw.Write((int)commandType);
            bw.Write(parameterName);
            bw.Write(boolValue);
            bw.Write(floatValue);
            return EndSerialize();
        }

        public AnimationStatePacket Deserialize(byte[] data)
        {
            AnimationStatePacket packet = new AnimationStatePacket();
            BasePacket basePacket = packet.BaseDeserialize(data);
            packet.type = basePacket.type;
            packet.playerData = basePacket.playerData;
            packet.commandType = (AnimationCommandType)basePacket.br.ReadInt32();
            packet.parameterName = basePacket.br.ReadString();
            packet.boolValue = basePacket.br.ReadBoolean();
            packet.floatValue = basePacket.br.ReadSingle();
            return packet;
        }
    }
}