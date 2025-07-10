using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NpcSpawner : NetworkBehaviour
{
    public List<GameObject> npcs = new(4);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            for (int i = 0; i < 4; i++) SpawnNpc();
        }
    }

    public void SpawnNpc()
    {
        GameObject toSpawn = PickRandomToSpawn();
        
        Instantiate(toSpawn);

        toSpawn.transform.position = new Vector3(
            Random.Range(-49.5f, 49.5f),
            Random.Range(-33.0f, 33.0f),
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


