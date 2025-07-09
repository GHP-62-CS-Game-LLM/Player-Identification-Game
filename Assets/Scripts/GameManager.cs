using System;
using System.Collections.Generic;
using Chat;
using Interactions;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(ChatManager))]

public class GameManager : MonoBehaviour
{
    public bool IsInteracting { get; set; }
    
    public List<PlayerController> players = new();

    public ChatManager chatManager;
    
    private NetworkManager _networkManager;

    [CanBeNull]
    private IInteraction _interaction;
    private string _message = string.Empty;
    

    private void Awake()
    {
        _networkManager = GetComponent<NetworkManager>();
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

        if (_interaction is not null)
        {
            _message = GUILayout.TextArea(_message);

            if (GUILayout.Button("Send"))
            {
                _interaction.AddMessage(new Message
                {
                    Sender = GameCharacterType.Seeker,
                    Reciever = GameCharacterType.Npc,
                    Content = _message
                });

                print(_interaction.ToString());
            }

            if (GUILayout.Button("Print Interaction")) print(_interaction.ToString());

            GUILayout.Label(_interaction.ToString());
        }
        
        GUILayout.EndArea();
        
    }

    public void StartInteraction(IGameCharacter seeker, IGameCharacter other)
    {
        _interaction = other.Type == GameCharacterType.Hider ? new PlayerPlayerInteraction() : new PlayerNpcInteraction(chatManager.MakeConversation(((NpcController)other).GetContext()));
        IsInteracting = true;

        // TODO: good UI
    }

    private void DisplayConnectionButtons()
    {
        if (GUILayout.Button("Host")) _networkManager.StartHost();
        if (GUILayout.Button("Client")) _networkManager.StartClient();
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
            if (players.Count >= 2) StartInteraction(players[0], players[1]);
        }
    } 
}
