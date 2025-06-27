using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
    private ItemManager itemManager;

    private void Start()
    {
        itemManager = FindObjectOfType<ItemManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") && itemManager != null)
        {
            itemManager.HideAndRespawn(gameObject);
        }
    }
}
