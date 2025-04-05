// Adăugați acest script la obiectul dvs. pentru a-l poziționa pe teren

using UnityEngine;

public class PositionOnGround : MonoBehaviour
{
    public LayerMask groundLayer; // Setați la layer-ul terenului
    
    void Start()
    {
        AlignWithGround();
    }
    
    public void AlignWithGround()
    {
        // Obținem poziția curentă
        Vector3 position = transform.position;
        
        // Lansăm un ray în jos pentru a detecta terenul
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 100f, groundLayer))
        {
            // Poziționăm obiectul direct pe teren
            transform.position = hit.point;
            
            // Opțional: Rotați obiectul pentru a se alinia cu normala terenului
            // transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }
}
