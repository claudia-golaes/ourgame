// This is a C# script for Unity to create a house with walls, floor, ceiling, camera and NPC
// Name the script "HouseBuilder.cs"

using UnityEngine;

public class HouseBuilder : MonoBehaviour
{
    // Serialized fields will be editable in the Inspector
    [Header("Room Dimensions")]
    [SerializeField] private float roomWidth = 100f;
    [SerializeField] private float roomLength = 100f;
    [SerializeField] private float roomHeight = 80f;
    [SerializeField] private float wallThickness = 0.2f;

    [Header("Materials")]
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material ceilingMaterial;

    [Header("NPC")]
    [SerializeField] private bool addNPC = true;
    [SerializeField] private Material npcMaterial;

    void Start()
    {
        BuildHouse();
        SetupCamera();
        
        if (addNPC)
        {
            AddNPC();
        }
    }

    void BuildHouse()
    {
        // Create a parent object for all house parts
        GameObject house = new GameObject("House");
        
        // Create floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.parent = house.transform;
        floor.transform.localPosition = new Vector3(0, 0, 0);
        floor.transform.localScale = new Vector3(roomWidth, wallThickness, roomLength);
        
        // Apply floor material if assigned
        if (floorMaterial != null)
        {
            floor.GetComponent<Renderer>().material = floorMaterial;
        }
        
        // Create ceiling
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.parent = house.transform;
        ceiling.transform.localPosition = new Vector3(0, roomHeight, 0);
        ceiling.transform.localScale = new Vector3(roomWidth, wallThickness, roomLength);
        
        // Apply ceiling material if assigned
        if (ceilingMaterial != null)
        {
            ceiling.GetComponent<Renderer>().material = ceilingMaterial;
        }
        
        // Create walls
        
        // Wall 1 (front)
        GameObject wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall1.name = "Wall_Front";
        wall1.transform.parent = house.transform;
        wall1.transform.localPosition = new Vector3(0, roomHeight / 2, roomLength / 2);
        wall1.transform.localScale = new Vector3(roomWidth, roomHeight, wallThickness);
        
        // Wall 2 (back)
        GameObject wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall2.name = "Wall_Back";
        wall2.transform.parent = house.transform;
        wall2.transform.localPosition = new Vector3(0, roomHeight / 2, -roomLength / 2);
        wall2.transform.localScale = new Vector3(roomWidth, roomHeight, wallThickness);
        
        // Wall 3 (left)
        GameObject wall3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall3.name = "Wall_Left";
        wall3.transform.parent = house.transform;
        wall3.transform.localPosition = new Vector3(-roomWidth / 2, roomHeight / 2, 0);
        wall3.transform.localScale = new Vector3(wallThickness, roomHeight, roomLength);
        
        // Wall 4 (right)
        GameObject wall4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall4.name = "Wall_Right";
        wall4.transform.parent = house.transform;
        wall4.transform.localPosition = new Vector3(roomWidth / 2, roomHeight / 2, 0);
        wall4.transform.localScale = new Vector3(wallThickness, roomHeight, roomLength);
        
        // Apply wall material to all walls if assigned
        if (wallMaterial != null)
        {
            wall1.GetComponent<Renderer>().material = wallMaterial;
            wall2.GetComponent<Renderer>().material = wallMaterial;
            wall3.GetComponent<Renderer>().material = wallMaterial;
            wall4.GetComponent<Renderer>().material = wallMaterial;
        }
    }

    void SetupCamera()
    {
        // Get or create main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
        }
        
        // Position the camera higher in the house (at 80% of the room height)
        mainCamera.transform.position = new Vector3(0, roomHeight * 0.6f, 0);
        mainCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        // Set a wider field of view (default is 60)
        mainCamera.fieldOfView = 90f;
        
        // Add an improved camera controller for better interaction with NPC
        mainCamera.gameObject.AddComponent<NPCCameraController>();
    }
    
    void AddNPC()
    {
        // Add NPCCreator component
        NPCCreator npcCreator = gameObject.AddComponent<NPCCreator>();
        
        // Set the NPC material if assigned
        if (npcMaterial != null)
        {
            npcCreator.SetMaterial(npcMaterial);
        }
        
        // Create NPC in the center of the room
        Vector3 npcPosition = new Vector3(0, 0, 0);
        npcCreator.CreateNPC(npcPosition);
    }
}