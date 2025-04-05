using System.Collections.Generic;
using UnityEngine;

public class GrassGenerator : MonoBehaviour
{
    [System.Serializable]
    public class PrefabWithWeight
    {
        public GameObject prefab;
        [Range(0f, 100f)]
        public float spawnWeight = 1f;
    }

    [Header("Grass Prefabs")]
    public List<PrefabWithWeight> grassPrefabs = new List<PrefabWithWeight>();
    public int totalGrassCount = 1000;
    
    [Header("Spawn Area")]
    public Vector3 minCoordinates = new Vector3(0f, 0f, 0f);
    public Vector3 maxCoordinates = new Vector3(100f, 0f, 100f);
    
    [Header("Density Settings")]
    [Range(0f, 1f)]
    public float density = 0.7f;
    
    [Header("Random Properties")]
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
    public Vector2 rotationRange = new Vector2(0f, 360f);
    
    [Header("References")]
    public Terrain terrain;
    
    [Header("Advanced Options")]
    public bool alignToTerrainNormal = true;
    public float surfaceOffset = 0.01f;
    
    private float totalWeight = 0f;
    
    private void Start()
    {
        if (grassPrefabs.Count == 0)
        {
            Debug.LogError("No grass prefabs assigned!");
            return;
        }
        
        if (terrain == null)
        {
            terrain = FindObjectOfType<Terrain>();
            if (terrain == null)
            {
                Debug.LogError("No terrain found! Please assign a terrain in the inspector.");
                return;
            }
        }
        
        // Calculate total weight for prefab selection
        CalculateTotalWeight();
        
        GenerateGrass();
    }
    
    private void CalculateTotalWeight()
    {
        totalWeight = 0f;
        foreach (PrefabWithWeight prefabWeight in grassPrefabs)
        {
            totalWeight += prefabWeight.spawnWeight;
        }
    }
    
    private GameObject SelectRandomPrefab()
    {
        float random = Random.Range(0f, totalWeight);
        float weightSum = 0f;
        
        foreach (PrefabWithWeight prefabWeight in grassPrefabs)
        {
            weightSum += prefabWeight.spawnWeight;
            if (random <= weightSum)
            {
                return prefabWeight.prefab;
            }
        }
        
        // Fallback to the first prefab (should rarely happen)
        return grassPrefabs[0].prefab;
    }
    
    private void GenerateGrass()
    {
        // Create a parent object to organize the grass instances
        GameObject grassParent = new GameObject("Generated Grass");
        
        // Get terrain information
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;
        
        for (int i = 0; i < totalGrassCount; i++)
        {
            // Skip this position based on density
            if (Random.value > density)
                continue;
            
            // Generate random position within specified bounds
            float x = Random.Range(minCoordinates.x, maxCoordinates.x);
            float z = Random.Range(minCoordinates.z, maxCoordinates.z);
            
            // Convert world position to terrain-relative position
            float terrainX = x - terrainPosition.x;
            float terrainZ = z - terrainPosition.z;
            
            // Check if position is within terrain bounds
            if (terrainX < 0 || terrainX > terrainData.size.x || 
                terrainZ < 0 || terrainZ > terrainData.size.z)
            {
                // Skip if outside terrain bounds
                continue;
            }
            
            // Get the exact height at this position on the terrain
            float y = terrain.SampleHeight(new Vector3(x, 0, z));
            
            // Add a small offset to prevent z-fighting/clipping with the terrain
            y += surfaceOffset;
            
            // Create a position vector
            Vector3 position = new Vector3(x, y, z);
            
            // Select a random prefab based on weights
            GameObject selectedPrefab = SelectRandomPrefab();
            
            // Create the grass instance
            GameObject grassInstance = Instantiate(selectedPrefab, position, Quaternion.identity, grassParent.transform);
            
            // Apply random scale
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            grassInstance.transform.localScale = new Vector3(scale, scale, scale);
            
            // Apply random rotation around Y axis
            float rotation = Random.Range(rotationRange.x, rotationRange.y);
            grassInstance.transform.rotation = Quaternion.Euler(0, rotation, 0);
            
            // Optional: Align grass to terrain normal
            if (alignToTerrainNormal)
            {
                AlignToTerrainNormal(grassInstance.transform, x, z);
            }
        }
    }
    
    private void AlignToTerrainNormal(Transform objectTransform, float x, float z)
    {
        // Get the terrain normal at this position
        Vector3 normal = terrain.terrainData.GetInterpolatedNormal(
            (x - terrain.transform.position.x) / terrain.terrainData.size.x,
            (z - terrain.transform.position.z) / terrain.terrainData.size.z
        );
        
        // Calculate rotation to align with normal
        Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, normal);
        
        // Combine with the Y-axis rotation
        float yRotation = objectTransform.rotation.eulerAngles.y;
        objectTransform.rotation = normalRotation * Quaternion.Euler(0, yRotation, 0);
    }
    
    // Optional: Editor button to generate grass
    [ContextMenu("Generate Grass")]
    public void GenerateGrassFromEditor()
    {
        CalculateTotalWeight();
        GenerateGrass();
    }
    
    // Optional: Editor button to clear all generated grass
    [ContextMenu("Clear Generated Grass")]
    public void ClearGeneratedGrass()
    {
        GameObject existingParent = GameObject.Find("Generated Grass");
        if (existingParent != null)
        {
            if (Application.isPlaying)
            {
                Destroy(existingParent);
            }
            else
            {
                DestroyImmediate(existingParent);
            }
        }
    }
}