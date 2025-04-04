// This is a separate C# script for Unity to create and manage NPC
// Name the script "NPCCreator.cs"

using UnityEngine;

public class NPCCreator : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private Material npcMaterial;
    [SerializeField] private float npcHeight = 1.8f;
    
    public void SetMaterial(Material material)
    {
        npcMaterial = material;
    }
    
    public GameObject CreateNPC(Vector3 position)
    {
        // Create main NPC object
        GameObject npc = new GameObject("NPC");
        npc.transform.position = position;
        
        // Add a body for the NPC (capsule)
        GameObject npcBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npcBody.name = "Body";
        npcBody.transform.parent = npc.transform;
        npcBody.transform.localPosition = new Vector3(0, npcHeight / 2, 0);
        npcBody.transform.localScale = new Vector3(1, npcHeight / 2, 1);
        
        // Add a head for the NPC (sphere)
        GameObject npcHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        npcHead.name = "Head";
        npcHead.transform.parent = npc.transform;
        npcHead.transform.localPosition = new Vector3(0, npcHeight + 0.15f, 0);
        npcHead.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Apply material if available
        if (npcMaterial != null)
        {
            npcBody.GetComponent<Renderer>().material = npcMaterial;
            npcHead.GetComponent<Renderer>().material = npcMaterial;
        }
        
        // Add collider for the NPC
        CapsuleCollider collider = npc.AddComponent<CapsuleCollider>();
        collider.height = npcHeight;
        collider.center = new Vector3(0, npcHeight / 2, 0);
        collider.radius = 0.5f;
        
        // Add NPC behavior script
        npc.AddComponent<NPCBehavior>();
        
        return npc;
    }
}

// Script for NPC behavior
public class NPCBehavior : MonoBehaviour
{
    private Transform player;
    public float rotationSpeed = 2.0f;
    
    void Start()
    {
        // Find the main camera (player)
        if (Camera.main != null)
        {
            player = Camera.main.transform;
        }
    }
    
    void Update()
    {
        // NPC turns to face player when nearby
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            
            if (distance < 5.0f)
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0; // Keep rotation only on Y axis
                
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if player entered the NPC's area
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {
            Debug.Log("Player entered NPC area");
        }
    }
}

// Camera controller for first person view and NPC interaction
public class NPCCameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float moveSpeed = 5f;
    private float xRotation = 0f;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        // Add a collider to the camera for collision detection
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = 0.2f;
        collider.isTrigger = true;
        
        // Add a Rigidbody to enable collision detection, but without physics
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // Set the camera tag to identify it
        gameObject.tag = "Player";
    }
    
    void Update()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(xRotation, transform.eulerAngles.y + mouseX, 0f);
        
        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * x + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}