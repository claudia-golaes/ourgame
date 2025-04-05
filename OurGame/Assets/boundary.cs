using UnityEngine;

public class PlayerBoundaryController : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTransform;
    
    [Header("Boundary Settings")]
    public float boundarySize = 100f;
    public Vector3 centerPoint = new Vector3(571.8793f, 5.916159f, 516.7263f);
    
    [Header("Options")]
    public bool restrictX = true;
    public bool restrictY = false;
    public bool restrictZ = true;
    
    private Vector3 minBoundary;
    private Vector3 maxBoundary;
    
    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            if (playerTransform == null)
            {
                Debug.LogError("Nu a fost găsit niciun obiect cu tag-ul 'Player'. Vă rugăm să atribuiți manual Transform-ul jucătorului.");
                enabled = false;
                return;
            }
        }
        
        // Calculăm limitele pentru fiecare axă
        float halfSize = boundarySize / 2f;
        minBoundary = centerPoint - new Vector3(halfSize, halfSize, halfSize);
        maxBoundary = centerPoint + new Vector3(halfSize, halfSize, halfSize);
    }
    
    private void LateUpdate()
    {
        if (playerTransform == null)
            return;
        
        Vector3 newPosition = playerTransform.position;
        
        // Restricționăm poziția pe fiecare axă dacă este activată restricția
        if (restrictX)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minBoundary.x, maxBoundary.x);
        }
        
        if (restrictY)
        {
            newPosition.y = Mathf.Clamp(newPosition.y, minBoundary.y, maxBoundary.y);
        }
        
        if (restrictZ)
        {
            newPosition.z = Mathf.Clamp(newPosition.z, minBoundary.z, maxBoundary.z);
        }
        
        // Aplicăm noua poziție dacă s-a schimbat
        if (newPosition != playerTransform.position)
        {
            playerTransform.position = newPosition;
        }
    }
    
    // Metodă opțională pentru a vizualiza limitele în editor
    private void OnDrawGizmos()
    {
        float halfSize = boundarySize / 2f;
        Vector3 boundarySize3D = new Vector3(
            restrictX ? boundarySize : 0,
            restrictY ? boundarySize : 0,
            restrictZ ? boundarySize : 0
        );
        
        Gizmos.color = new Color(1, 0, 0, 0.3f); // Roșu transparent
        Gizmos.DrawCube(centerPoint, boundarySize3D);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(centerPoint, boundarySize3D);
    }
}
