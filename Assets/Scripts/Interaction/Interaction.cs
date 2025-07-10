using System;
using System.Collections.Generic;
using Unity.Netcode;

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

    public interface IInteraction : INetworkSerializable
    {
        public InteractionType Type { get; }
        
        public Action<Message> OnMessageAdd { get; set; }
        
        public void AddMessage(Message message);
        public void AddMessage(string message);

        public List<Message> GetAllMessages();

        public InteractionHistory GetHistory();
    }

    public struct InteractionHistory : IEquatable<InteractionHistory>, INetworkSerializable
    {
        public List<Message> Messages { get; set; }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsWriter)
            {
                serializer.GetFastBufferWriter().WriteValueSafe(Messages.Count);

                foreach (Message message in Messages) 
                    serializer.GetFastBufferWriter().WriteValueSafe(message);
            }
            else
            {
                serializer.GetFastBufferReader().ReadValueSafe(out int size);
                Messages = new List<Message>(size);

                for (int i = 0; i < size; i++)
                {
                    serializer.GetFastBufferReader().ReadValueSafe(out Message message);
                    Messages[i] = message;
                }
            }
        }
        
        public bool Equals(InteractionHistory other) => Equals(Messages, other.Messages);
        public override bool Equals(object obj) => obj is InteractionHistory other && Equals(other);
        public override int GetHashCode() => (Messages != null ? Messages.GetHashCode() : 0);
    }
}
