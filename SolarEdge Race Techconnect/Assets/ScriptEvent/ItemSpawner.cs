//using UnityEngine;

//public class ItemSpawner : MonoBehaviour
//{
//    public GameObject[] itemPrefabs; // Danh sách các prefab item khác nhau
//    public float spawnRadius = 2f;
//    public float spawnInterval = 15f;

//    private void Start()
//    {
//        InvokeRepeating(nameof(SpawnItem), spawnInterval, spawnInterval);
//    }

//    void SpawnItem()
//    {
//        if (itemPrefabs.Length == 0) return;

//        // Chọn 1 item ngẫu nhiên
//        GameObject itemToSpawn = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

//        // Tạo vị trí ngẫu nhiên trong bán kính spawnRadius
//        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
//        randomOffset.y = 0f; // Không đổi chiều cao

//        Vector3 spawnPosition = transform.position + randomOffset;
//        Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);
//    }
//}
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs;    // Object đã có sẵn trong scene (dùng làm mẫu để spawn quanh)
    public float spawnRadius = 2f;
    public float spawnInterval = 15f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnItem), spawnInterval, spawnInterval);
    }

    private void SpawnItem()
    {
        if (itemPrefabs.Length == 0) return;

        // Chọn 1 object gốc ngẫu nhiên trong scene
        GameObject original = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

        if (original == null)
        {
            Debug.LogWarning("Một itemPrefab trong scene đã bị null.");
            return;
        }

        // Lấy vị trí hiện tại của object trong scene làm gốc
        Vector3 basePosition = original.transform.position;

        // Tính vị trí spawn ngẫu nhiên quanh vị trí gốc
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        randomOffset.y = 0f;

        Vector3 spawnPosition = basePosition + randomOffset;

        // Tạo bản sao của object gốc tại vị trí spawn mới
        Instantiate(original, spawnPosition, Quaternion.identity);
    }
}
