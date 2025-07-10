using System;
using System.Collections.Generic;
using System.Text;
using Chat;
using UnityEngine;
using Unity.Netcode;

namespace Interaction 
{
    public class PlayerNpcInteraction : IInteraction
    {
        public InteractionType Type => InteractionType.PlayerNpc;
        public Action<Message> OnMessageAdd { get; set; } = _ => { };

        public readonly PlayerController Player;
        public readonly NpcController Npc;

        private readonly InteractionHistory _history = new() { Messages = new List<Message>() };
        private readonly Conversation _conversation;

        public PlayerNpcInteraction(Conversation conversation, PlayerController player, NpcController npc)
        {
            _conversation = conversation;

            Player = player;
            Npc = npc;
        }

        public async void AddMessage(Message message)
        {
            Debug.Log("PlayerNpc Interaction");
            _history.Messages.Add(message);
            OnMessageAdd(message);

            // Generate and add response
            if (message.Receiver == GameCharacterType.Npc)
            {
                string response = await _conversation.Message(message.Content);
                Debug.Log("Waiting");
                Debug.Log($"Result:\n{response}");

                _history.Messages.Add(new Message
                {
                    Sender = GameCharacterType.Npc,
                    Receiver = GameCharacterType.Seeker,
                    Content = response
                });
            }
        }

        public void AddMessage(string message) =>
            AddMessage(new Message { Sender = Player.Type, Receiver = Npc.Type, Content = message });

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
