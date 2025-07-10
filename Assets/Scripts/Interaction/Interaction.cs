using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Assertions;
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

    public interface IInteraction : IEquatable<IInteraction>
    {
        public static IInteraction FromData(InteractionData data)
        {
            return data.Type switch
            {
                InteractionType.PlayerPlayer => new PlayerPlayerInteraction(data),
                InteractionType.PlayerNpc => new PlayerNpcInteraction(data),
                _ => throw new ArgumentException("Invalid InteractionData Type!")
            };
        }
        
        public InteractionData Data { get; set; }
        
        public InteractionType Type => Data.Type;
        
        public void AddMessage(Message message, bool autoGenerateResponse = false);
        public void AddMessage(string message, bool autoGenerateResponse = false);

        public Message[] GetAllMessages();
    }

    public struct InteractionData : INetworkSerializable, IEquatable<InteractionData>
    {
        public InteractionType Type { get; set; }
        public int SenderIndex { get; set; }
        public int ReceiverIndex { get; set; }
        public Message[] Messages { get; set; }
        
        public int ConversationIndex { get; set; }

        public void AddMessage(Message message)
        {
            Message[] messages = new Message[Messages.Length + 1];
            Array.Copy(Messages, messages, Messages.Length);

            messages[^1] = message;

            Messages = messages;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsWriter)
            {
                serializer.GetFastBufferWriter().WriteValueSafe(Type);
                serializer.GetFastBufferWriter().WriteValueSafe(SenderIndex);
                serializer.GetFastBufferWriter().WriteValueSafe(ReceiverIndex);
                serializer.GetFastBufferWriter().WriteValueSafe(Messages.Length);
                if (Type == InteractionType.PlayerNpc) serializer.GetFastBufferWriter().WriteValueSafe(ConversationIndex);
                foreach (Message message in Messages) 
                    serializer.GetFastBufferWriter().WriteValueSafe(message);
            }
            else
            {
                serializer.GetFastBufferReader().ReadValueSafe(out InteractionType type);
                serializer.GetFastBufferReader().ReadValueSafe(out int senderIndex);
                serializer.GetFastBufferReader().ReadValueSafe(out int receiverIndex);
                serializer.GetFastBufferReader().ReadValueSafe(out int messagesCount);
                if (type == InteractionType.PlayerNpc) serializer.GetFastBufferReader().ReadValueSafe(out int conversationIndex);
                Type = type;
                SenderIndex = senderIndex;
                ReceiverIndex = receiverIndex;
                ConversationIndex = ConversationIndex;

                Messages = new Message[messagesCount];

                for (int i = 0; i < messagesCount; i++)
                {
                    serializer.GetFastBufferReader().ReadValueSafe(out Message message);
                    Messages[i] = message;
                }
            }
        }
        public bool Equals(InteractionData other) =>
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
