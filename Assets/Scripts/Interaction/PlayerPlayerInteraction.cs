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
        private InteractionData _data;
        public InteractionData Data 
        {
            get => _data;
            set => _data = value;
        }

        public InteractionType Type => _data.Type;

        public PlayerController Sender
        {
            get => GameManager.Instance.characters[_data.SenderIndex] as PlayerController;
            set => _data.SenderIndex = GameManager.Instance.characters.IndexOf(value);
        }
        public PlayerController Receiver
        {
            get => GameManager.Instance.characters[_data.ReceiverIndex] as PlayerController;
            set => _data.ReceiverIndex = GameManager.Instance.characters.IndexOf(value);
        }

        public Message[] Messages => _data.Messages;

        public PlayerPlayerInteraction(PlayerController sender, PlayerController receiver) : this(new InteractionData
        {
            Type = InteractionType.PlayerPlayer,
            SenderIndex = GameManager.Instance.characters.IndexOf(sender),
            ReceiverIndex = GameManager.Instance.characters.IndexOf(receiver),
            Messages = Array.Empty<Message>()
        }) { }

        public PlayerPlayerInteraction(InteractionData data)
        {
            _data = data;
        }
    
        public void AddMessage(Message message, bool autoGenerateResponse = false) => _data.AddMessage(message);

        public void AddMessage(string message, bool autoGenerateResponse = false) =>
            AddMessage(new Message { Sender = Sender.Type, Receiver = Receiver.Type, Content = message });

        public Message[] GetAllMessages() => _data.Messages;

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
    }
}
