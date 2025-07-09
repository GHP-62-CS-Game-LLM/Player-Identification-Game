using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Chat;
using UnityEngine;

namespace Interactions
{
    public struct Message
    {
        public GameCharacterType Sender { get; set; }
        public GameCharacterType Reciever { get; set; }
    
        public string Content { get; set; }

        public override string ToString() => $"{Sender} -> {Reciever}: {Content}";
    }

    public enum InteractionType
    {
        PlayerNpc,
        PlayerPlayer
    }

    public interface IInteraction
    {
        public InteractionType Type { get; }
        public void AddMessage(Message message);

        public List<Message> GetAllMessages();
    }

    public class PlayerNpcInteraction : IInteraction
    {
        public InteractionType Type => InteractionType.PlayerNpc;

        private readonly List<Message> _messages = new();
        private readonly Conversation _conversation;

        public PlayerNpcInteraction(Conversation conversation)
        {
            _conversation = conversation;
        }

        public async void AddMessage(Message message)
        {
            Debug.Log("PlayerNpc Interaction");
            _messages.Add(message);
            
            // Generate and add response
            if (message.Reciever == GameCharacterType.Npc)
            {
                string response = await _conversation.Message(message.Content);
                Debug.Log("Waiting");
                Debug.Log($"Result:\n{response}");
                
                _messages.Add(new Message 
                {
                    Sender = GameCharacterType.Npc,
                    Reciever = GameCharacterType.Seeker,
                    Content = response
                });
            }
        }

        public List<Message> GetAllMessages() => _messages;

        public override string ToString()
        {
            StringBuilder sb = new();

            foreach (Message message in GetAllMessages()) sb.AppendLine(message.ToString());

            return sb.ToString();
        }
    }

    public class PlayerPlayerInteraction : IInteraction
    {
        public InteractionType Type => InteractionType.PlayerPlayer;

        private readonly List<Message> _messages = new();
    
        public void AddMessage(Message message) => _messages.Add(message);
    
        public List<Message> GetAllMessages() => _messages;

        public override string ToString()
        {
            StringBuilder sb = new();

            foreach (Message message in GetAllMessages()) sb.AppendLine(message.ToString());

            return sb.ToString();
        }
    }
}
