using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample 
{
    public class GhostTraseAutomatScript : MonoBehaviour
    {
        [Header("Traseu")]
        public List<Transform> puncteTraseu = new List<Transform>();
        public float vitezaDeplasare = 4f;
        public float timpAsteptareInitial = 10f; // Timp de așteptare la început (10 secunde)
        public float timpAsteptareDespawn = 1f; // Timp de așteptare înainte de despawn (1 secundă)
        
        [Header("Setări Navigare")]
        public float razaActivarePunct = 5f; // Distanța la care considerăm că am ajuns în apropierea unui punct
        public float distantaMinimaTinta = 0.5f; // Distanța minimă la care ne apropiem de punctul țintă
        public LayerMask obstacoleLayerMask; // Layer mask pentru obstacolele de evitat

        // Referințe la componentele existente
        private Animator anim;
        private CharacterController ctrl;
        private Vector3 moveDirection = Vector3.zero;

        // Stările animator cache
        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");

        // Variabile pentru logica de urmărire a traseului
        private int indexPunctCurent = 0;
        private bool potIncepeUrmarire = false;
        private float timpTrecut = 0f;
        private bool inAsteptareDespawn = false;
        private Vector3 punctulTintaCurent;
        private bool amCalculatPunctTinta = false;

        void Start()
        {
            // Obține componentele necesare
            anim = GetComponent<Animator>();
            ctrl = GetComponent<CharacterController>();

            // Verifică dacă există animator și animații
            if (anim != null)
            {
                // La început, personajul este în idle
                anim.CrossFade(IdleState, 0.1f, 0, 0);
            }

            // Verifică dacă există puncte de traseu
            if (puncteTraseu.Count == 0)
            {
                Debug.LogWarning("Nu s-au definit puncte de traseu. Personajul va rămâne pe loc.");
            }

            // Dezactivează scriptul original GhostScript dacă există
            GhostScript originalScript = GetComponent<GhostScript>();
            if (originalScript != null)
            {
                originalScript.enabled = false;
            }
        }

        void Update()
        {
            // Aplicăm gravitația în orice caz
            AplicaGravitatie();

            // Verificăm dacă a trecut timpul de așteptare inițial
            if (!potIncepeUrmarire)
            {
                timpTrecut += Time.deltaTime;
                if (timpTrecut >= timpAsteptareInitial)
                {
                    potIncepeUrmarire = true;
                    // Începem să ne mișcăm - trecem în starea de mișcare
                    if (anim != null)
                    {
                        anim.CrossFade(MoveState, 0.1f, 0, 0);
                    }
                }
            }
            else
            {
                // Dacă putem începe urmărirea traseului și avem puncte definite și nu suntem în starea de așteptare pentru despawn
                if (puncteTraseu.Count > 0 && !inAsteptareDespawn)
                {
                    UrmeazaTraseu();
                }
            }
        }

        private void UrmeazaTraseu()
        {
            // Verificăm dacă indexul curent este valid
            if (indexPunctCurent < 0 || indexPunctCurent >= puncteTraseu.Count) return;
            
            // Obținem punctul curent din traseu
            Transform punctCurent = puncteTraseu[indexPunctCurent];
            if (punctCurent == null) return;
            
            // Calculăm punctul țintă o singură dată pentru punctul de traseu curent
            if (!amCalculatPunctTinta)
            {
                punctulTintaCurent = GasestePunctTintaInJur(punctCurent.position);
                amCalculatPunctTinta = true;
            }
            
            // Verifică dacă am ajuns suficient de aproape de punctul țintă
            float distantaPanaLaTinta = Vector3.Distance(transform.position, punctulTintaCurent);
            
            if (distantaPanaLaTinta < razaActivarePunct)
            {
                // Resetăm pentru următorul punct
                amCalculatPunctTinta = false;
                
                // Trecem la următorul punct
                indexPunctCurent++;
                
                // Verificăm dacă am ajuns la finalul traseului
                if (indexPunctCurent >= puncteTraseu.Count)
                {
                    StartCoroutine(DespawnDupaAsteptare());
                    return;
                }
            }
            
            // Mișcăm personajul către punctul țintă curent
            MiscaCatrePunct(punctulTintaCurent);
        }

        private Vector3 GasestePunctTintaInJur(Vector3 centruPunct)
        {
            // Generăm mai multe puncte posibile în jurul obiectului
            List<Vector3> punctePosibile = new List<Vector3>();
            
            // Adăugăm 8 direcții în jurul obiectului
            float razaEvitare = razaActivarePunct * 0.5f;
            for (int i = 0; i < 8; i++)
            {
                float unghi = i * 45f * Mathf.Deg2Rad;
                Vector3 directie = new Vector3(Mathf.Sin(unghi), 0, Mathf.Cos(unghi));
                Vector3 punctPosibil = centruPunct + directie * razaEvitare;
                
                // Verificăm să nu fie în coliziune cu alte obiecte
                if (!Physics.CheckSphere(punctPosibil, 1f, obstacoleLayerMask))
                {
                    punctePosibile.Add(punctPosibil);
                }
            }
            
            // Dacă am găsit puncte valide, alegem unul dintre ele (cel mai apropiat de noi)
            if (punctePosibile.Count > 0)
            {
                Vector3 celMaiApropiaPunct = centruPunct;
                float distantaMinima = float.MaxValue;
                
                foreach (Vector3 punct in punctePosibile)
                {
                    float dist = Vector3.Distance(transform.position, punct);
                    if (dist < distantaMinima)
                    {
                        distantaMinima = dist;
                        celMaiApropiaPunct = punct;
                    }
                }
                
                return celMaiApropiaPunct;
            }
            
            // Dacă nu găsim niciun punct valid, ne întoarcem la un punct la o distanță fixă
            Vector3 directieDeEvitare = (transform.position - centruPunct).normalized;
            if (directieDeEvitare == Vector3.zero)
            {
                directieDeEvitare = new Vector3(1, 0, 0); // Direcție implicită dacă suntem exact în centru
            }
            
            return centruPunct + directieDeEvitare * razaEvitare;
        }

        private void MiscaCatrePunct(Vector3 punctTinta)
        {
            // Ajustăm înălțimea țintei la înălțimea personajului
            punctTinta.y = transform.position.y;
            
            // Calculăm direcția către țintă
            Vector3 directieCatreTinta = (punctTinta - transform.position).normalized;
            
            // Verificăm dacă există obstacole în calea noastră
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directieCatreTinta, out hit, 1.5f, obstacoleLayerMask))
            {
                // Încercăm să găsim o cale alternativă
                Vector3 directieDreapta = Quaternion.Euler(0, 45, 0) * directieCatreTinta;
                Vector3 directieStanga = Quaternion.Euler(0, -45, 0) * directieCatreTinta;
                
                // Verificăm dacă avem o cale liberă în una din direcțiile alternative
                if (!Physics.Raycast(transform.position, directieDreapta, 1.5f, obstacoleLayerMask))
                {
                    directieCatreTinta = directieDreapta;
                }
                else if (!Physics.Raycast(transform.position, directieStanga, 1.5f, obstacoleLayerMask))
                {
                    directieCatreTinta = directieStanga;
                }
                else
                {
                    // Dacă ambele direcții sunt blocate, încercăm o direcție mai extremă
                    directieDreapta = Quaternion.Euler(0, 90, 0) * directieCatreTinta;
                    directieStanga = Quaternion.Euler(0, -90, 0) * directieCatreTinta;
                    
                    if (!Physics.Raycast(transform.position, directieDreapta, 1.5f, obstacoleLayerMask))
                    {
                        directieCatreTinta = directieDreapta;
                    }
                    else if (!Physics.Raycast(transform.position, directieStanga, 1.5f, obstacoleLayerMask))
                    {
                        directieCatreTinta = directieStanga;
                    }
                }
            }
            
            // Rotăm personajul către direcția de deplasare
            if (directieCatreTinta != Vector3.zero)
            {
                Quaternion rotatieTinta = Quaternion.LookRotation(directieCatreTinta);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotatieTinta, Time.deltaTime * 8f);
            }
            
            // Calculăm viteza de mișcare
            moveDirection.x = directieCatreTinta.x * vitezaDeplasare;
            moveDirection.z = directieCatreTinta.z * vitezaDeplasare;
            
            // Aplicăm mișcarea
            if (ctrl != null && ctrl.enabled)
            {
                ctrl.Move(new Vector3(moveDirection.x, moveDirection.y, moveDirection.z) * Time.deltaTime);
            }
        }

        private void AplicaGravitatie()
        {
            if (ctrl != null && ctrl.enabled)
            {
                if (EstePersonajulPePamant())
                {
                    if (moveDirection.y < -0.1f)
                    {
                        moveDirection.y = -0.1f;
                    }
                }
                moveDirection.y -= 0.1f;
                ctrl.Move(new Vector3(0, moveDirection.y, 0) * Time.deltaTime);
            }
        }

        private bool EstePersonajulPePamant()
        {
            if (ctrl != null && ctrl.isGrounded && ctrl.enabled)
            {
                return true;
            }
            Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
            float range = 0.2f;
            return Physics.Raycast(ray, range);
        }

        private IEnumerator DespawnDupaAsteptare()
        {
            if (inAsteptareDespawn) yield break;
            
            inAsteptareDespawn = true;
            
            // Trecem în idle în timpul așteptării
            if (anim != null)
            {
                anim.CrossFade(IdleState, 0.1f, 0, 0);
            }
            
            // Așteptăm timpul specificat
            yield return new WaitForSeconds(timpAsteptareDespawn);
            
            // Despawn - dezactivăm GameObject-ul
            gameObject.SetActive(false);
            
            // Alternativ, dacă vrei să distrugi obiectul:
            // Destroy(gameObject);
        }

        // Pentru a vizualiza traseul și punctele de navigație în editor
        private void OnDrawGizmos()
        {
            if (puncteTraseu.Count == 0) return;

            // Desenează punctele și liniile de traseu
            Gizmos.color = Color.yellow;

            for (int i = 0; i < puncteTraseu.Count; i++)
            {
                if (puncteTraseu[i] == null) continue;

                // Desenează sferă la fiecare punct
                Gizmos.DrawSphere(puncteTraseu[i].position, 0.3f);

                // Desenează linie între puncte
                if (i < puncteTraseu.Count - 1 && puncteTraseu[i + 1] != null)
                {
                    Gizmos.DrawLine(puncteTraseu[i].position, puncteTraseu[i + 1].position);
                }
            }
            
            // Desenează raza de activare pentru punctul curent
            if (Application.isPlaying && indexPunctCurent < puncteTraseu.Count && indexPunctCurent >= 0)
            {
                Transform punctCurent = puncteTraseu[indexPunctCurent];
                if (punctCurent != null)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.3f);
                    Gizmos.DrawWireSphere(punctCurent.position, razaActivarePunct);
                    
                    if (amCalculatPunctTinta)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(punctulTintaCurent, 0.5f);
                    }
                }
            }
        }
    }
}
