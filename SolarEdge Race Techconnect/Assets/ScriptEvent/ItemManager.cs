using System.Collections;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemEntry
    {
        public GameObject itemObject;
        [HideInInspector] public Vector3 originalPosition;
        [HideInInspector] public bool isRespawning;
    }

    public float respawnDelay = 5f;
    public ItemEntry[] items;

    private void Start()
    {
        // Lưu lại vị trí ban đầu
        foreach (var item in items)
        {
            if (item.itemObject != null)
                item.originalPosition = item.itemObject.transform.position;
        }
    }

    public void HideAndRespawn(GameObject obj)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemObject == obj && !items[i].isRespawning)
            {
                StartCoroutine(RespawnRoutine(i));
                break;
            }
        }
    }

    private IEnumerator RespawnRoutine(int index)
    {
        var item = items[index];
        item.isRespawning = true;
        item.itemObject.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);

        item.itemObject.transform.position = item.originalPosition;
        item.itemObject.SetActive(true);
        item.isRespawning = false;
    }
}
