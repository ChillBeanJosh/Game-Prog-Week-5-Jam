using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    private NPCSpawner spawner;
    public AudioSource womanTalk;


    private void Start()
    {
        spawner = FindObjectOfType<NPCSpawner>();
        womanTalk.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        { 
            Destroy(gameObject);

            spawner.SpawnPrefabAtRandomPosition();
        }
    }
}
