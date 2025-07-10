using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UIElements;

namespace Interaction
{
    public struct Message : INetworkSerializable
    {
        public GameCharacterType Sender { get; set; }
        public GameCharacterType Receiver { get; set; }
    
        public string Content { get; set; }

        public override string ToString() => $"{Sender} -> {Receiver}: {Content}";
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsWriter)
            {
                serializer.GetFastBufferWriter().WriteValueSafe(Sender);
                serializer.GetFastBufferWriter().WriteValueSafe(Receiver);
                serializer.GetFastBufferWriter().WriteValueSafe(Content);
            }
            else
            {
                serializer.GetFastBufferReader().ReadValueSafe(out GameCharacterType sender);
                serializer.GetFastBufferReader().ReadValueSafe(out GameCharacterType reciever);
                serializer.GetFastBufferReader().ReadValueSafe(out string content);

                Sender = sender;
                Receiver = reciever;
                Content = content;
            }
        }
    }

    public enum InteractionType
    {
        PlayerNpc,
        PlayerPlayer
    }

    public interface IInteraction : INetworkSerializable, IEquatable<IInteraction>
    {
        public InteractionType Type { get; }
        
        public void AddMessage(Message message);
        public void AddMessage(string message);

        public List<Message> GetAllMessages();

        public SerializableInteraction GetSerializable();
    }

    public struct SerializableInteraction : INetworkSerializable, IEquatable<SerializableInteraction>
    {
        public static PlayerPlayerInteraction ToPlayerPlayerInteraction(SerializableInteraction serializable)
        {
            PlayerPlayerInteraction interaction = new(
                (PlayerController)GameManager.Instance.characters[serializable.SenderIndex],
                (PlayerController)GameManager.Instance.characters[serializable.ReceiverIndex]);

            foreach (Message message in serializable.Messages) interaction.AddMessage(message);

            return interaction;
        }

        public PlayerPlayerInteraction ToPlayerPlayerInteraction() => ToPlayerPlayerInteraction(this);

        //public static PlayerNpcInteraction ToPlayerNpcInteraction(SerializableInteraction interaction) => new();
        
        public InteractionType Type { get; set; }
        public int SenderIndex { get; set; }
        public int ReceiverIndex { get; set; }
        public Message[] Messages { get; set; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsWriter)
            {
                serializer.GetFastBufferWriter().WriteValueSafe(Type);
                serializer.GetFastBufferWriter().WriteValueSafe(SenderIndex);
                serializer.GetFastBufferWriter().WriteValueSafe(ReceiverIndex);
                serializer.GetFastBufferWriter().WriteValueSafe(Messages.Length);
                foreach (Message message in Messages) 
                    serializer.GetFastBufferWriter().WriteValueSafe(message);
            }
            else
            {
                serializer.GetFastBufferReader().ReadValueSafe(out InteractionType type);
                serializer.GetFastBufferReader().ReadValueSafe(out int senderIndex);
                serializer.GetFastBufferReader().ReadValueSafe(out int receiverIndex);
                serializer.GetFastBufferReader().ReadValueSafe(out int messagesCount);
                Type = type;
                SenderIndex = senderIndex;
                ReceiverIndex = receiverIndex;

                Messages = new Message[messagesCount];

                for (int i = 0; i < messagesCount; i++)
                {
                    serializer.GetFastBufferReader().ReadValueSafe(out Message message);
                    Messages[i] = message;
                }
            }
        }
        public bool Equals(SerializableInteraction other) =>
            Type == other.Type &&
            SenderIndex == other.SenderIndex && 
            ReceiverIndex == other.ReceiverIndex &&
            Messages.Equals(other.Messages);
    }

    // public struct InteractionHistory : IEquatable<InteractionHistory>, INetworkSerializable
    // {
    //     public List<Message> Messages { get; set; }
    //     
    //     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    //     {
    //         if (serializer.IsWriter)
    //         {
    //             serializer.GetFastBufferWriter().WriteValueSafe(Messages.Count);
    //
    //             foreach (Message message in Messages) 
    //                 serializer.GetFastBufferWriter().WriteValueSafe(message);
    //         }
    //         else
    //         {
    //             serializer.GetFastBufferReader().ReadValueSafe(out int size);
    //             Messages = new List<Message>(size);
    //
    //             for (int i = 0; i < size; i++)
    //             {
    //                 serializer.GetFastBufferReader().ReadValueSafe(out Message message);
    //                 Messages[i] = message;
    //             }
    //         }
    //     }
    //     
    //     public bool Equals(InteractionHistory other) => Equals(Messages, other.Messages);
    //     public override bool Equals(object obj) => obj is InteractionHistory other && Equals(other);
    //     public override int GetHashCode() => (Messages != null ? Messages.GetHashCode() : 0);
    // }
}
