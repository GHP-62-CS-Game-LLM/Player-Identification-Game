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
[RequireComponent(typeof(InteractionManager))]

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    
    public List<IGameCharacter> characters = new();
    public PlayerController localPlayer;

    [HideInInspector]
    public ChatManager chatManager;
    
    public InteractionManager interactionManager;

    private NetworkManager _networkManager;

    private string _message = string.Empty;

    private string _hostAddress = "127.0.0.1";

    private void Awake()
    {
        Instance = this;
        
        _networkManager = FindAnyObjectByType<NetworkManager>();
        chatManager = GetComponent<ChatManager>();
        
        interactionManager = GetComponent<InteractionManager>();
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
            
            if (interactionManager.IsInteracting && IsClient)
            {
                _message = GUILayout.TextArea(_message);

                if (GUILayout.Button("Send"))
                {
                    interactionManager.AddMessageToCurrentInteraction(_message);
                }

                if (interactionManager.CurrentInteraction != null)
                {
                    if (GUILayout.Button("Print Interaction"))
                    {
                        print($"Interaction:\n{interactionManager.CurrentInteraction.ToString()}");
                    }
                    
                    GUILayout.Label(interactionManager.CurrentInteraction.ToString());
                }
                
            }
        }

        GUILayout.EndArea();
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

        if (characters.Count >= 3 && GUILayout.Button("Test Interaction"))
        {
            interactionManager.StartInteractionRpc(0, 2);
        }
        
        GUILayout.Label($"IsInteracting (local): {interactionManager.IsInteracting}");
        GUILayout.Label($"Interactions Len: {interactionManager.Len}");
    } 
}
