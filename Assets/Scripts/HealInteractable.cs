using Unity.VisualScripting;
using UnityEngine;

public class HealInteractable : Interactable
{
    [SerializeField] private int healAmount = 2;
    public override void OnInteract()
    {
        //TODO: Heal the player

        Destroy(gameObject);
    }
}
