using System;
using System.Collections.Generic;
using System.Text;
using Chat;
using UnityEngine;
using Unity.Netcode;

namespace Interaction 
{
    public class PlayerNpcInteraction : IInteraction, IEquatable<PlayerNpcInteraction>
    {
        public InteractionType Type => InteractionType.PlayerNpc;

        public PlayerController Player;
        public NpcController Npc;

        private List<Message> Messages { get; set; } = new();
        
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
            Messages.Add(message);

            // Generate and add response
            if (message.Receiver == GameCharacterType.Npc)
            {
                string response = await _conversation.Message(message.Content);
                Debug.Log("Waiting");
                Debug.Log($"Result:\n{response}");

                Messages.Add(new Message
                {
                    Sender = GameCharacterType.Npc,
                    Receiver = GameCharacterType.Seeker,
                    Content = response
                });
            }
        }

        public void AddMessage(string message) =>
            AddMessage(new Message { Sender = Player.Type, Receiver = Npc.Type, Content = message });

        public List<Message> GetAllMessages() => Messages;

        public bool Equals(IInteraction other) =>
            other is not null &&
            Type == other.Type &&
            Equals(other as PlayerNpcInteraction);

        public bool Equals(PlayerNpcInteraction other) =>
            other is not null &&
            Type == other.Type &&
            Player.Equals(other.Player) &&
            Npc.Equals(other.Npc) &&
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
                serializer.GetFastBufferWriter().WriteValue(Player.Index);
                serializer.GetFastBufferWriter().WriteValue(Npc.Index);
                serializer.GetFastBufferWriter().WriteValue(Messages.Count);
                foreach (Message message in Messages) 
                    serializer.GetFastBufferWriter().WriteValueSafe(message);
            }
            else
            {
                serializer.GetFastBufferReader().ReadValueSafe(out InteractionType type);
                serializer.GetFastBufferReader().ReadValueSafe(out int playerIndex);
                serializer.GetFastBufferReader().ReadValueSafe(out int npcIndex);
                serializer.GetFastBufferReader().ReadValueSafe(out int messagesCount);
                
                Player = GameManager.Instance.characters[playerIndex] as PlayerController;
                Npc = GameManager.Instance.characters[npcIndex] as NpcController;
                
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
            SenderIndex = Player.Index,
            ReceiverIndex = Npc.Index,
            Messages = Messages.ToArray()
        };
    }
}
