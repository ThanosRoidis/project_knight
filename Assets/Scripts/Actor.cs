using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Actor : MonoBehaviour
{
    [SerializeField] private int maxHealth;

    [SerializeField] private AudioClip hitSound;
    [SerializeField] private GameObject hitEffect;

    private AudioSource audioSource;
    private int currentHealth;


    void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0) { Die(); }
    }

    public void GetHit(Vector3 pos, int damage)
    {
        TakeDamage(damage);
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 20);
    }

    void Die()
    {
        // Death function
        // TEMPORARY: Destroy Object
        Destroy(gameObject);
    }
}
