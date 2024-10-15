using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject prefab;

    [Header("Spawn Range Settings")]
    public Vector3 minSpawnRange;  
    public Vector3 maxSpawnRange; 

    private GameObject spawnedObject;

    public AudioSource womanDeath;
    public AudioSource monsterSlash;

    private void Start()
    {
        SpawnPrefabAtRandomPosition();
    }

    public void SpawnPrefabAtRandomPosition()
    {
        womanDeath.Play();
        monsterSlash.Play();

        float randomX = Random.Range(minSpawnRange.x, maxSpawnRange.x);
        float randomY = Random.Range(minSpawnRange.y, maxSpawnRange.y);
        float randomZ = Random.Range(minSpawnRange.z, maxSpawnRange.z);

        Vector3 randomPosition = new Vector3(randomX, randomY, randomZ);

        spawnedObject = Instantiate(prefab, randomPosition, Quaternion.identity);
    }
}
