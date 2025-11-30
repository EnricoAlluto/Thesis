using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace SojaExiles
{
    public class opencloseDoor1 : MonoBehaviour
    {
        public Animator openandclose1;
        public bool open;
        public Transform Player;
        public float interactionDistance = 15f;

        void Start()
        {
            open = false;
        }

        void Update()
        {
            // Controlla se c'è un tocco
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                // Controlla se si è toccata un'interfaccia utente
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return;

                // Controlla la distanza dal giocatore
                if (Player && Vector3.Distance(Player.position, transform.position) < interactionDistance)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, interactionDistance))
                    {
                        if (hit.collider.gameObject == gameObject) // Se si è toccata questa porta
                        {
                            if (open)
                                StartCoroutine(closing());
                            else
                                StartCoroutine(opening());
                        }
                    }
                }
            }
        }

        IEnumerator opening()
        {
            Debug.Log("Apertura porta");
            openandclose1.Play("Opening 1");
            open = true;
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator closing()
        {
            Debug.Log("Chiusura porta");
            openandclose1.Play("Closing 1");
            open = false;
            yield return new WaitForSeconds(0.5f);
        }
    }
}