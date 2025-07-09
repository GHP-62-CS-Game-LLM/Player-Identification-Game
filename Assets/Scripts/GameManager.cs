using System;
using System.Collections.Generic;
using Chat;
using Interactions;
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
    public NetworkVariable<bool> isInteracting;
    
    public List<PlayerController> players = new();

    [HideInInspector]
    public ChatManager chatManager;

    private NetworkManager _networkManager;

    [CanBeNull]
    private IInteraction _interaction;
    private string _message = string.Empty;
    private string _hostAddress = "127.0.0.1:7777";
    

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

    [Rpc(SendTo.Server)]
    public void StartInteractionRpc(GameCharacterType other)
    {
        print("Testing Interaction");
        _interaction = other == GameCharacterType.Hider ? 
            new PlayerPlayerInteraction() : 
            new PlayerNpcInteraction(chatManager.MakeConversation(string.Empty));
        isInteracting.Value = true;
        
        _interaction.AddMessage(new Message
        {
            Sender = GameCharacterType.Seeker,
            Reciever = GameCharacterType.Npc,
            Content = "Hello, how are you?"
        });

        // TODO: good UI
    }

    private void DisplayConnectionButtons()
    {
        _hostAddress = GUILayout.TextField(_hostAddress);
        
        if (GUILayout.Button("Host")) _networkManager.StartHost();

        if (GUILayout.Button("Client"))
        {
            Uri host = new(_hostAddress);
            print($"Host: {host.Host}, Port: {host.Port}");
            GetComponent<UnityTransport>().SetConnectionData(host.Host, (ushort)host.Port);
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
            StartInteractionRpc(GameCharacterType.Npc);
        }
    } 
}
