using System;
using System.Collections.Generic;
using System.Text;
using Chat;
using UnityEngine;
using Unity.Netcode;

namespace Interaction 
{
    public class PlayerPlayerInteraction : IInteraction
    {
        public InteractionType Type => InteractionType.PlayerPlayer;
        public Action<Message> OnMessageAdd { get; set; } = _ => { };

        public readonly PlayerController Sender;
        public readonly PlayerController Receiver;

        private readonly InteractionHistory _history = new() { Messages = new List<Message>() };

        public PlayerPlayerInteraction(PlayerController sender, PlayerController receiver)
        {
            Sender = sender;
            Receiver = receiver;
        }
    
        public void AddMessage(Message message) => _history.Messages.Add(message);

        public void AddMessage(string message) =>
            AddMessage(new Message { Sender = Sender.Type, Receiver = Receiver.Type, Content = message });

        public List<Message> GetAllMessages() => _history.Messages;
        public InteractionHistory GetHistory() => _history;

        public override string ToString()
        {
            StringBuilder sb = new();

            foreach (Message message in GetAllMessages()) sb.AppendLine(message.ToString());

            return sb.ToString();
        }
    }
}
