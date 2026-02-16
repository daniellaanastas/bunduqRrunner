using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private List<GameObject> activeTiles;
    public float tileLength = 30;
    public int numberOfTiles = 5;
    public int totalNumOfTiles = 8;
    public float spawnAheadDistance = 240f;
    
    [SerializeField] private float initialZSpawn = -400f;
    private float zSpawn;
    
    private Transform playerTransform;
    private Transform cameraTransform;
    private int previousIndex;
    private List<int> recentIndices = new List<int>();
    private int cachedHighScore;
    
    [Header("Seamless Spawning")]
    public float tileOverlap = 0.5f;
    public bool preloadTiles = true;
    
    void Start()
    {
        ResetTileSystem();
        
        cachedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        
        int preloadCount = preloadTiles ? numberOfTiles + 2 : numberOfTiles;
        
        for (int i = 0; i < preloadCount; i++)
        {
            if (i == 0)
                SpawnTile(0);
            else
                SpawnTile(GetRandomTileIndex());
        }
        
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        cameraTransform = Camera.main.transform;
    }
    
    private void ResetTileSystem()
    {
        zSpawn = initialZSpawn;
        
        // Reset all pooled tiles
        if (TilePool.instance != null)
            TilePool.instance.ResetAllTiles();
        
        if (activeTiles != null)
            activeTiles.Clear();
        else
            activeTiles = new List<GameObject>();
            
        recentIndices.Clear();
        previousIndex = 0;
    }
    
    void Update()
    {
        if (cameraTransform == null) return;
        float cameraZ = cameraTransform.position.z;
        
        EnsureTilesAhead(cameraZ);
        CleanupTilesBehind(cameraZ);
    }
    
    private void EnsureTilesAhead(float cameraZ)
    {
        while (activeTiles.Count > 0)
        {
            GameObject lastTile = activeTiles[activeTiles.Count - 1];
            float lastTileEndZ = lastTile.transform.position.z + tileLength;
            
            if (cameraZ + spawnAheadDistance >= lastTileEndZ)
            {
                SpawnTile(GetRandomTileIndex());
            }
            else
            {
                break;
            }
        }
    }
    
    private void CleanupTilesBehind(float cameraZ)
    {
        while (activeTiles.Count > 0)
        {
            GameObject firstTile = activeTiles[0];
            float tileEndZ = firstTile.transform.position.z + tileLength;
            
            if (tileEndZ < cameraZ - 50f)
            {
                DeleteTile();
            }
            else
            {
                break;
            }
        }
    }
    
    private int GetRandomTileIndex()
    {
        int index;
        int attempts = 0;
        
        do
        {
            index = Random.Range(0, totalNumOfTiles);
            attempts++;
            if (attempts > 20) break;
        } while (recentIndices.Contains(index));
        
        recentIndices.Add(index);
        if (recentIndices.Count > 5)
            recentIndices.RemoveAt(0);
        
        return index;
    }
    
    public void SpawnTile(int index)
    {
        GameObject tile = TilePool.instance.GetTile(index);
        
        if (tile == null)
        {
            Debug.LogError("No tiles available!");
            return;
        }
        
        // FIRST: Position the tile
        float spawnZ = zSpawn - tileOverlap;
        tile.transform.position = new Vector3(0, 0, spawnZ);
        tile.transform.rotation = Quaternion.identity;
        
        // THEN: Setup the tile (gems, obstacles, bears)
        Tile tileScript = tile.GetComponent<Tile>();
        if (tileScript != null)
        {
            tileScript.SetupTile(PlayerManager.score, cachedHighScore);
        }
        
        // FINALLY: Activate and track
        tile.SetActive(true);
        activeTiles.Add(tile);
        
        zSpawn += tileLength;
        previousIndex = index;
    }
    
    private void DeleteTile()
    {
        if (activeTiles.Count == 0) return;
        
        TilePool.instance.ReturnTile(activeTiles[0]);
        activeTiles.RemoveAt(0);
        PlayerManager.score += 1;
    }
}