using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NpcSpawner : NetworkBehaviour
{
    public List<GameObject> npcs = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            for (int i = 0; i < npcs.Count; i++) SpawnNpc();
        }
    }

    public void SpawnNpc()
    {
        GameObject toSpawn = PickRandomToSpawn();
        
        Instantiate(toSpawn);

        toSpawn.transform.position = new Vector3(
            Random.Range(-47.0f, 48.0f),
            Random.Range(-28.0f, 28.0f),
            0.0f
        );
    }

    public GameObject PickRandomToSpawn()
    {
        GameObject toSpawn = npcs[Random.Range(0, npcs.Count)];
        npcs.Remove(toSpawn);

        return toSpawn;
    }
}
