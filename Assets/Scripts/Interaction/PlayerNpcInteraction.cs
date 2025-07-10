using System;
using System.Text;
using Chat;
using UnityEngine;

namespace Interaction 
{
    public class PlayerNpcInteraction : IInteraction, IEquatable<PlayerNpcInteraction>
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
        public NpcController Receiver
        {
            get => GameManager.Instance.characters[_data.ReceiverIndex] as NpcController;
            set => _data.ReceiverIndex = GameManager.Instance.characters.IndexOf(value);
        }

        public Message[] Messages => _data.Messages;

        public Conversation Conversation
        {
            get => GameManager.Instance.interactionManager.Conversations[Data.ConversationIndex];
            set => _data.ConversationIndex = GameManager.Instance.interactionManager.Conversations.IndexOf(value);
        }

        public PlayerNpcInteraction(Conversation conversation, PlayerController sender, NpcController receiver) : this(new InteractionData
        {
            Type = InteractionType.PlayerNpc,
            SenderIndex = GameManager.Instance.characters.IndexOf(sender),
            ReceiverIndex = GameManager.Instance.characters.IndexOf(receiver),
            Messages = Array.Empty<Message>(),
            ConversationIndex = GameManager.Instance.interactionManager.Conversations.IndexOf(conversation)
        }) { }

        public PlayerNpcInteraction(InteractionData data)
        {
            _data = data;
        }

        public async void AddMessage(Message message, bool autoGenerateResponse = false)
        {
            _data.AddMessage(message);

            // Generate and add response
            if (autoGenerateResponse && message.Receiver == GameCharacterType.Npc)
            {
                string response = await Conversation.Message(message.Content);
                Debug.Log("Waiting");
                Debug.Log($"Result:\n{response}");

                _data.AddMessage(new Message
                {
                    Sender = GameCharacterType.Npc,
                    Receiver = GameCharacterType.Seeker,
                    Content = response
                });
            }
        }

        public void AddMessage(string message, bool autoGenerateResponse = false) =>
           AddMessage(new Message { Sender = Sender.Type, Receiver = Receiver.Type, Content = message }, autoGenerateResponse);

        public Message[] GetAllMessages() => _data.Messages;

        public bool Equals(IInteraction other) =>
            other is not null &&
            Type == other.Type &&
            Equals(other as PlayerNpcInteraction);

        public bool Equals(PlayerNpcInteraction other) =>
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
