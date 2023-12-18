using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

// POUR TESTER EN CONTROLANT UN PASSENGER
//=================================================================================================
public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;
    public NavMeshAgent agent;
    private float elapsed;

    void Start()
    {
        name = ">>> PLAYER <<<";
        elapsed = 0;
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();

        TryFindNearestMachine();
    }

    MachineWithQueue nearestMachine;
    private void TryFindNearestMachine()
    {
        nearestMachine = MachinesManager.Instance.NearestAvailableMachine(transform.position);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 200))
            {
                // move
                agent.SetDestination(hit.point);

                //Debug.Log("HIT " + hit.transform.name);
            }
            else
                Debug.Log("HIT NOTHING !!!");
        }

        // Update the way to the goal every second.
        elapsed += Time.deltaTime;
        if (elapsed > 1.0f) // && nearestMachine == null)
        {
            elapsed -= 1.0f;
            TryFindNearestMachine();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PLAYER trigger ENTER >>> " + other.name);

        if (other.CompareTag("Door"))
        {
            ChangeNavMeshMask();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("PLAYER trigger EXIT >>> " + other.name);

        if (other.CompareTag("Door"))
        {
            ChangeNavMeshMask("Walkable");
        }
    }

    void ChangeNavMeshMask()
    {
        //agent.areaMask = 0;
        //agent.areaMask &= NavMesh.GetAreaFromName("Door");

        //int areaMask = agent.areaMask;
        //areaMask += 1 << NavMesh.GetAreaFromName("Everything");//turn off all
        //areaMask += 1 << NavMesh.GetAreaFromName("Door");


        int areaMask = 0;
        areaMask += 1 << NavMesh.GetAreaFromName("Walkable");//turn off all
        areaMask += 1 << NavMesh.GetAreaFromName("Door");

        agent.areaMask = areaMask;

    }

    void ChangeNavMeshMask(string name)
    {
        //agent.areaMask = 0;
        //agent.areaMask &= 0 << NavMesh.GetAreaFromName("Door");
        //agent.areaMask &= 1 << NavMesh.GetAreaFromName(name);

        int areaMask = agent.areaMask;
        areaMask = 0;
        //areaMask += 1 << NavMesh.GetAreaFromName("Everything");//turn on all
        //areaMask += 1 << NavMesh.GetAreaFromName("Door");
        areaMask += 1 << NavMesh.GetAreaFromName("Walkable");//turn off all

        agent.areaMask = areaMask;
    }

}
