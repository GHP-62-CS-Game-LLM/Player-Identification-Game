using System;
using System.Collections.Generic;
using Chat;
using Interaction;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Properties;
using UnityEngine;

[RequireComponent(typeof(ChatManager))]

public class GameManager : NetworkBehaviour
{
    [ReadOnly]
    public bool isInteracting;
    
    public List<IGameCharacter> characters = new();
    public PlayerController localPlayer;

    [HideInInspector]
    public ChatManager chatManager;

    private NetworkManager _networkManager;

    [CanBeNull]
    private IInteraction _interaction;
    private NetworkVariable<List<InteractionHistory>> _interactionHistories = new(new List<InteractionHistory>());
    private string _message = string.Empty;
    private string _hostAddress = "127.0.0.1";
    

    private void Awake()
    {
        _networkManager = FindAnyObjectByType<NetworkManager>();
        chatManager = GetComponent<ChatManager>();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (!_networkManager.IsClient && !_networkManager.IsServer)
        {
            DisplayConnectionButtons();
        }
        else
        {
            DisplayPlayingButtons();
        }

        if (_interaction is not null && isInteracting && IsClient)
        {
            _message = GUILayout.TextArea(_message);

            if (GUILayout.Button("Send"))
            {
                _interaction.AddMessage(_message);

                print(_interaction.ToString());
            }

            if (GUILayout.Button("Print Interaction")) print(_interaction.ToString());

            GUILayout.Label(_interaction.ToString());
        }
        
        GUILayout.EndArea();
    }

    [Rpc(SendTo.Server)]
    public void StartInteractionRpc(int senderIndex, int receiverIndex)
    {
        print("Testing Interaction");
        PlayerController sender = (PlayerController)characters[senderIndex];
        IGameCharacter receiver = characters[receiverIndex];

        if (characters[receiverIndex].Type == GameCharacterType.Hider)
        {
            _interaction = new PlayerPlayerInteraction(sender, (PlayerController)receiver);
            _interactionHistories.Value.Add(_interaction.GetHistory());
            _interactionHistories.CheckDirtyState();
            StartPlayerPlayerClientInteractionRpc(receiverIndex, senderIndex);
        }
        else
        {
            _interaction = new PlayerNpcInteraction(chatManager.MakeConversation(string.Empty), sender, (NpcController)receiver);
            _interactionHistories.Value.Add(_interaction.GetHistory());
            _interactionHistories.CheckDirtyState();
        }
        
        isInteracting = true;

        // TODO: good UI
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartPlayerPlayerClientInteractionRpc(int localPlayerIndex, int otherPlayerIndex)
    {
        if (characters.IndexOf(localPlayer) == localPlayerIndex)
        {
            _interaction = new PlayerPlayerInteraction(characters[localPlayerIndex] as PlayerController, characters[otherPlayerIndex] as PlayerController);
            isInteracting = true;
        }
    }

    private void DisplayConnectionButtons()
    {
        _hostAddress = GUILayout.TextField(_hostAddress);
        
        if (GUILayout.Button("Host")) _networkManager.StartHost();

        if (GUILayout.Button("Client"))
        {
            print($"HostAddress: {_hostAddress}");
            _networkManager.GetComponent<UnityTransport>().SetConnectionData(_hostAddress, 7777);
            _networkManager.StartClient();
        }
        if (GUILayout.Button("Server")) _networkManager.StartServer();
    }

    private void DisplayPlayingButtons()
    {
        if (GUILayout.Button("Print UIDs"))
        {
            List<ulong> uids = _networkManager.SpawnManager.GetConnectedPlayers();
            print("Connected UIDs:");
            foreach (ulong uid in uids) print($"UID: {uid}");
        }

        if (GUILayout.Button("Test Interaction"))
        {
            StartInteractionRpc(1, 0);
        }
        
        GUILayout.Label($"IsInteracting (local): {isInteracting}");
        GUILayout.Label($"Interaction Histories Count: {_interactionHistories.Value.Count}");
    } 
}
