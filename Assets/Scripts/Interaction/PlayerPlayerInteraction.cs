using System;
using System.Collections.Generic;
using System.Text;
using Chat;
using UnityEngine;
using Unity.Netcode;

namespace Interaction 
{
    public class PlayerPlayerInteraction : IInteraction, IEquatable<PlayerPlayerInteraction>
    {
        public InteractionType Type => InteractionType.PlayerPlayer;

        public PlayerController Sender;
        public PlayerController Receiver;

        private List<Message> Messages { get; set; } = new();


        public PlayerPlayerInteraction(PlayerController sender, PlayerController receiver)
        {
            Sender = sender;
            Receiver = receiver;
        }
    
        public void AddMessage(Message message) => Messages.Add(message);

        public void AddMessage(string message) =>
            AddMessage(new Message { Sender = Sender.Type, Receiver = Receiver.Type, Content = message });

        public List<Message> GetAllMessages() => Messages;

        public bool Equals(IInteraction other) =>
            other is not null &&
            Type == other.Type &&
            Equals(other as PlayerPlayerInteraction);

        public bool Equals(PlayerPlayerInteraction other) => 
            other is not null &&
            Type == other.Type &&
            Sender.Equals(other.Sender) &&
            Receiver.Equals(other.Receiver) &&
            Messages == other.Messages;
        
        public override string ToString()
        {
            StringBuilder sb = new();

            foreach (Message message in GetAllMessages()) sb.AppendLine(message.ToString());

            return sb.ToString();
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsWriter)
            {
                serializer.GetFastBufferWriter().WriteValueSafe(Type);
                serializer.GetFastBufferWriter().WriteValue(Sender.Index);
                serializer.GetFastBufferWriter().WriteValue(Receiver.Index);
                serializer.GetFastBufferWriter().WriteValue(Messages.Count);
                foreach (Message message in Messages) 
                    serializer.GetFastBufferWriter().WriteValueSafe(message);
            }
            else
            {
                serializer.GetFastBufferReader().ReadValueSafe(out InteractionType type);
                serializer.GetFastBufferReader().ReadValueSafe(out int senderIndex);
                serializer.GetFastBufferReader().ReadValueSafe(out int receiverIndex);
                serializer.GetFastBufferReader().ReadValueSafe(out int messagesCount);
                
                Sender = GameManager.Instance.characters[senderIndex] as PlayerController;
                Receiver = GameManager.Instance.characters[receiverIndex] as PlayerController;
                
                Messages = new List<Message>(messagesCount);

                for (int i = 0; i < messagesCount; i++)
                {
                    serializer.GetFastBufferReader().ReadValueSafe(out Message message);
                    Messages.Add(message);
                }
            }
        }

        public SerializableInteraction GetSerializable() => new()
        {
            Type = Type,
            SenderIndex = Sender.Index,
            ReceiverIndex = Receiver.Index,
            Messages = Messages.ToArray()
        };
    }
}
