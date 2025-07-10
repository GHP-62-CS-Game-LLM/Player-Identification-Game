using System;
using System.Collections.Generic;
using Chat;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Interaction
{
    public class InteractionManager : NetworkBehaviour
    {
        public bool IsInteracting { get; set; }

        private GameManager _gameManager;
        private ChatManager _chatManager;

        public List<IGameCharacter> Characters => _gameManager.characters;
        public PlayerController LocalPlayer => _gameManager.localPlayer;
        
        private int _currentInteractionIndex;
        [CanBeNull] public IInteraction CurrentInteraction => _currentInteractionIndex < _interactions.Value.Count
            ? _interactions.Value[_currentInteractionIndex].ToPlayerPlayerInteraction()
            : null;

        public int Len => _interactions.Value.Count;
        private NetworkVariable<List<SerializableInteraction>> _interactions = new(new List<SerializableInteraction>());

        private bool _shouldCreateInteraction;
        private int _createInteractionInFrames = -1;
        private int _pendingInteractionIndex;

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();

            _chatManager = _gameManager.chatManager;
        }

        public void AddMessageToCurrentInteraction(string message)
        {
            if (!IsInteracting) return;

            PlayerPlayerInteraction interaction = _interactions.Value[_currentInteractionIndex].ToPlayerPlayerInteraction();
            RemoveInteractionAtRpc(_currentInteractionIndex);
            if (LocalPlayer.Type == GameCharacterType.Seeker)
            {
                interaction.AddMessage(new Message
                {
                    Sender = GameCharacterType.Seeker,
                    Receiver = GameCharacterType.Hider,
                    Content = message
                });
            }
            else if (LocalPlayer.Type == GameCharacterType.Hider)
            {
                interaction.AddMessage(new Message
                {
                    Sender = GameCharacterType.Hider,
                    Receiver = GameCharacterType.Seeker,
                    Content = message
                });
            }
            
            // _interactions.Value.Add(interaction.GetSerializable());
            // _interactions.CheckDirtyState();
            AddInteractionRpc(interaction.GetSerializable());
        }


        [Rpc(SendTo.Server)]
        public void StartInteractionRpc(int senderIndex, int receiverIndex)
        {
            IGameCharacter sender = Characters[senderIndex];
            IGameCharacter receiver = Characters[receiverIndex];

            if (sender.Type == GameCharacterType.Seeker && receiver.Type == GameCharacterType.Npc)
            {
                Assert.IsTrue(sender.GetType() == typeof(PlayerController));
                Assert.IsTrue(receiver.GetType() == typeof(NpcController));
                
                 _interactions.Value.Add(new PlayerNpcInteraction(
                    _chatManager.MakeConversation(string.Empty),
                    sender as PlayerController, 
                    receiver as NpcController
                ).GetSerializable());
            }
            else if (sender.Type == GameCharacterType.Seeker && receiver.Type == GameCharacterType.Hider)
            {
                Assert.IsTrue(sender.GetType() == typeof(PlayerController));
                Assert.IsTrue(receiver.GetType() == typeof(PlayerController));

                _interactions.Value.Add(new PlayerPlayerInteraction(
                    sender as PlayerController, 
                    receiver as PlayerController
                ).GetSerializable());
            }
            
            _currentInteractionIndex = _interactions.Value.Count - 1;
            IsInteracting = true;
            _interactions.CheckDirtyState();
            
            StartInteractionClientsRpc(senderIndex, receiverIndex, _interactions.Value.Count - 1);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void StartInteractionClientsRpc(int senderIndex, int receiverIndex, int interactionIndex)
        {
            if (LocalPlayer.Type == GameCharacterType.Seeker && senderIndex == Characters.IndexOf(LocalPlayer) ||
                LocalPlayer.Type == GameCharacterType.Hider && receiverIndex == Characters.IndexOf(LocalPlayer))
            {
                if (IsHost) return;

                _shouldCreateInteraction = true;
                _createInteractionInFrames = 2;
                _pendingInteractionIndex = interactionIndex;
            }
            else Debug.LogWarning("Invalid Interaction Configuration!");
        }

        [Rpc(SendTo.Server)]
        private void AddInteractionRpc(SerializableInteraction interaction)
        {
            _interactions.Value.Add(interaction);
            _interactions.CheckDirtyState();
        }

        [Rpc(SendTo.Server)]
        private void RemoveInteractionAtRpc(int index)
        {
            _interactions.Value.RemoveAt(index);
            _interactions.CheckDirtyState();
        }

        public void Update()
        {
            if (_shouldCreateInteraction && _createInteractionInFrames > 0) _createInteractionInFrames--;
            
            if (_shouldCreateInteraction && _createInteractionInFrames <= 0)
            {
                _currentInteractionIndex = _pendingInteractionIndex;
                IsInteracting = true;
                _createInteractionInFrames = -1;
                _shouldCreateInteraction = false;
            }
        }
    }
}
