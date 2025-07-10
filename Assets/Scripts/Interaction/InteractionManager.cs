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
        // Sorry..
        [CanBeNull] public IInteraction CurrentInteraction => _currentInteractionIndex < _interactions.Count
            ? _interactions[_currentInteractionIndex]
            : null;

        public int InteractionsLength => _interactions.Count;
        private List<IInteraction> _interactions = new();
        private NetworkVariable<List<InteractionData>> _interactionsData = new(new List<InteractionData>());

        public List<Conversation> Conversations = new();

        private bool _shouldCreateInteraction;
        private int _createInteractionInFrames = -1;
        private int _pendingInteractionIndex;

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();

            _chatManager = _gameManager.chatManager;

            _interactionsData.OnValueChanged = (value, newValue) =>
            {
                for (int i = 0; i < newValue.Count; i++)
                {
                    if (i < _interactions.Count)
                    {
                        if (!_interactions[i].Data.Equals(newValue[i]))
                            _interactions[i].Data = newValue[i];
                    }
                    else
                    {
                        _interactions.Add(IInteraction.FromData(newValue[i]));
                    }
                }
            };
        }

        public void AddMessageToCurrentInteraction(string message)
        {
            if (!IsInteracting) return;

            IInteraction interaction = _interactions[_currentInteractionIndex];
            //RemoveInteractionAtRpc(_currentInteractionIndex);
            if (LocalPlayer.Type == GameCharacterType.Seeker)
            {
                interaction.AddMessage(new Message
                {
                    Sender = GameCharacterType.Seeker,
                    Receiver = interaction.Type == InteractionType.PlayerPlayer ? GameCharacterType.Hider : GameCharacterType.Npc,
                    Content = message
                }, true);
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

            SetInteractionDataAtRpc(_currentInteractionIndex, interaction.Data);
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
                
                Conversations.Add(_chatManager.MakeConversation(string.Empty));
                
                 _interactions.Add(new PlayerNpcInteraction(
                    Conversations[^1],
                    sender as PlayerController, 
                    receiver as NpcController
                ));
                 
                _interactionsData.Value.Add(_interactions[^1].Data);
            }
            else if (sender.Type == GameCharacterType.Seeker && receiver.Type == GameCharacterType.Hider)
            {
                Assert.IsTrue(sender.GetType() == typeof(PlayerController));
                Assert.IsTrue(receiver.GetType() == typeof(PlayerController));

                _interactions.Add(new PlayerPlayerInteraction(
                    sender as PlayerController, 
                    receiver as PlayerController
                ));
                
                _interactionsData.Value.Add(_interactions[^1].Data);
            }
            
            _currentInteractionIndex = _interactions.Count - 1;
            IsInteracting = true;
            _interactionsData.CheckDirtyState();
            
            StartInteractionClientsRpc(senderIndex, receiverIndex, _interactions.Count - 1);
        }

        public void StopCurrentInteraction()
        {
            if (!IsClient) return;
            
            StopInteractionRpc(_currentInteractionIndex);
        }

        [Rpc(SendTo.Server)]
        public void StopInteractionRpc(int interactionIndex)
        {
            _interactions.RemoveAt(interactionIndex);
            _interactionsData.Value.RemoveAt(interactionIndex);
            _interactionsData.CheckDirtyState();
            _currentInteractionIndex = -1;
            IsInteracting = false;
            
            StopInteractionClientRpc(interactionIndex);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void StopInteractionClientRpc(int interactionIndex)
        {
            _interactions.RemoveAt(interactionIndex);
            _currentInteractionIndex = -1;
            IsInteracting = false;
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
        private void AddInteractionDataRpc(InteractionData interaction)
        {
            _interactions.Add(IInteraction.FromData(interaction));
            _interactionsData.Value.Add(_interactions[^1].Data);
            _interactionsData.CheckDirtyState();
        }

        [Rpc(SendTo.Server)]
        private void RemoveInteractionDataAtRpc(int index)
        {
            _interactions.RemoveAt(index);
            _interactionsData.Value.RemoveAt(index);
            _interactionsData.CheckDirtyState();
        }

        [Rpc(SendTo.Server)]
        private void SetInteractionDataAtRpc(int index, InteractionData data)
        {
            _interactions[index].Data = data;
            _interactionsData.Value[index] = data;
            _interactionsData.CheckDirtyState();
        }

        public void Update()
        {
            if (_shouldCreateInteraction && _createInteractionInFrames > 0) _createInteractionInFrames--;
            
            if (_shouldCreateInteraction && _pendingInteractionIndex < _interactionsData.Value.Count)
            {
                _currentInteractionIndex = _pendingInteractionIndex;
                _interactions.Insert(_currentInteractionIndex, IInteraction.FromData(_interactionsData.Value[_currentInteractionIndex]));
                IsInteracting = true;
                _createInteractionInFrames = -1;
                _shouldCreateInteraction = false;
            }
        }
    }
}
