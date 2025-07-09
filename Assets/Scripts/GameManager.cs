using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]

public class GameManager : MonoBehaviour
{
    private NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = GetComponent<NetworkManager>();
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
            DisplayMessageButtons();
        }
        
        GUILayout.EndArea();
    }

    private void DisplayConnectionButtons()
    {
        if (GUILayout.Button("Host")) _networkManager.StartHost();
        if (GUILayout.Button("Client")) _networkManager.StartClient();
        if (GUILayout.Button("Server")) _networkManager.StartServer();
    }

    private void DisplayMessageButtons()
    {
        if (GUILayout.Button("Print UIDs"))
        {
            List<ulong> uids = _networkManager.SpawnManager.GetConnectedPlayers();
            print("Connected UIDs:");
            foreach (ulong uid in uids) print($"UID: {uid}");
        }
    } 
}
