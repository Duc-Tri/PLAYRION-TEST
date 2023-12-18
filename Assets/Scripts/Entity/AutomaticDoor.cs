using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//La porte entre la gare et le quai ne doit pas s'ouvrir que si le passager a un ticket.
//=================================================================================================
public class AutomaticDoor : MonoBehaviour
{
    private SphereCollider hitbox;
    private static int nPlayersIn;

    [SerializeField] private GameObject doorL;
    [SerializeField] private GameObject doorR;

    private void Awake()
    {
        nPlayersIn = 0;
        CloseDoor();
        hitbox = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Passenger"))
        {
            Passenger p = other.GetComponent<Passenger>();
            if (p.HasTicket)
            {
                Debug.Log("Door ... player with ticket enter");
                nPlayersIn++;
                OpenDoor();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Passenger"))
        {
            Debug.Log("Door ... player exit");
            if (--nPlayersIn == 0)
            {
                CloseDoor();
            }
        }
    }

    private void CloseDoor()
    {
        doorL.SetActive(true);
        doorR.SetActive(true);
    }

    private void OpenDoor()
    {
        doorL.SetActive(false);
        doorR.SetActive(false);
    }

}
