using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace SojaExiles
{
    public class opencloseDoor : MonoBehaviour
    {
        public Animator openandclose;
        public bool open;
        public Transform Player;
        public float interactionDistance = 15f;
        public LayerMask doorLayer; // Aggiungi qui il layer della porta

        void Start()
        {
            open = false;
            // Se il giocatore non è assegnato, prova a trovarlo automaticamente
            if (Player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    Player = playerObj.transform;
                }
                else
                {
                    Debug.LogError("Player not assigned and no GameObject with 'Player' tag found!");
                }
            }
        }

        void Update()
        {
            // Controlla se c'è almeno un tocco
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                // Controlla se il tocco è appena iniziato
                if (touch.phase == TouchPhase.Began)
                {
                    // Ignora se si è toccata l'UI
                    if (EventSystem.current != null && 
                        EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        return;

                    // Controlla la distanza dal giocatore
                    if (Player != null && 
                        Vector3.Distance(Player.position, transform.position) < interactionDistance)
                    {
                        // Controlla se il tocco ha colpito questa porta
                        Ray ray = Camera.main.ScreenPointToRay(touch.position);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, interactionDistance, doorLayer))
                        {
                            if (hit.collider.gameObject == gameObject)
                            {
                                if (!open)
                                    StartCoroutine(opening());
                                else
                                    StartCoroutine(closing());
                            }
                        }
                    }
                }
            }
        }

        IEnumerator opening()
        {
            Debug.Log("Apertura porta");
            if (openandclose != null)
            {
                openandclose.Play("Opening");
                open = true;
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.LogError("Animator non assegnato alla porta: " + gameObject.name);
            }
        }

        IEnumerator closing()
        {
            Debug.Log("Chiusura porta");
            if (openandclose != null)
            {
                openandclose.Play("Closing");
                open = false;
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                Debug.LogError("Animator non assegnato alla porta: " + gameObject.name);
            }
        }

        // Visualizza il raggio di interazione nell'editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}