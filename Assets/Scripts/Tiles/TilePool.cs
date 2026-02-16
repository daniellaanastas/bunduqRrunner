using System.Collections.Generic;
using UnityEngine;

public class TilePool : MonoBehaviour
{
    public static TilePool instance;
    
    public GameObject[] tilePrefabs;
    public int instancesPerTile = 3;
    
    private Dictionary<int, List<GameObject>> pools;
    private int sessionSeed;
    
    void Awake()
    {
        instance = this;
        sessionSeed = System.DateTime.Now.Millisecond + Random.Range(0, 10000);
        InitializePools();
    }
    
    private void InitializePools()
    {
        pools = new Dictionary<int, List<GameObject>>();
        
        for (int tileType = 0; tileType < tilePrefabs.Length; tileType++)
        {
            pools[tileType] = new List<GameObject>();
            
            for (int i = 0; i < instancesPerTile; i++)
            {
                // Instantiate WITHOUT parent first to preserve local positions
                GameObject tile = Instantiate(tilePrefabs[tileType]);
                tile.transform.position = Vector3.zero;
                tile.transform.rotation = Quaternion.identity;
                tile.SetActive(false);
                tile.name = $"Tile_{tileType}_Instance_{i}";
                pools[tileType].Add(tile);
            }
        }
    }
    
    public GameObject GetTile(int tileType)
    {
        if (!pools.ContainsKey(tileType)) return null;
        
        foreach (GameObject tile in pools[tileType])
        {
            if (!tile.activeInHierarchy)
                return tile;
        }
        
        // Fallback to any available tile
        for (int alt = 0; alt < tilePrefabs.Length; alt++)
        {
            if (alt == tileType) continue;
            foreach (GameObject tile in pools[alt])
            {
                if (!tile.activeInHierarchy)
                    return tile;
            }
        }
        
        return null;
    }
    
    public void ReturnTile(GameObject tile)
    {
        tile.SetActive(false);
    }
    
    public void ResetAllTiles()
    {
        sessionSeed = System.DateTime.Now.Millisecond + Random.Range(0, 10000);
        
        foreach (var pool in pools.Values)
        {
            foreach (GameObject tile in pool)
            {
                tile.SetActive(false);
            }
        }
    }
    
    public int GetSessionSeed() => sessionSeed;
}
