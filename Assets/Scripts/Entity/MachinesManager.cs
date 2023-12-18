using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using System.IO;
using System;
using System.Linq;

// Gère toutes les machines (placées dans les enfants de l objet)
//=================================================================================================
public class MachinesManager : MonoBehaviour
{
    [SerializeField] private bool forceAllRealTime = false; // pour forcer les vraies valeurs du VendingMachine
    private MachineWithQueue[] allMachines;
    private List<float> speedsFromServer; // null if no connection
    private NavMeshPath tempPath; // pour calculs temp

    private static MachinesManager instance;
    public static MachinesManager Instance => instance;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else if (instance != this)
            Destroy(this);
    }

    private void Init()
    {
        if (allMachines == null)
        {
            allMachines = GetComponentsInChildren<MachineWithQueue>();
            Debug.Log(allMachines.Length + " MACHINES");
        }

        if (speedsFromServer != null)
        {
            ComputeAllMachinesSpeed();
        }

        tempPath = new NavMeshPath();
    }

    private void Start()
    {
        if (forceAllRealTime || GameManager.Instance.globalForceRealData)
        {
            foreach (var machine in allMachines)
            {
                machine.ComputeRealMakingTicketTime(); // valeur qui déclenche le calcul du vrai temps
            }
        }
    }

    // Recherche de la machine disponible (file non pleine) la plus proche par rapport à une pos
    //=============================================================================================
    public MachineWithQueue NearestAvailableMachine(Vector3 fromPos)
    {
        float minDist = float.MaxValue;
        MachineWithQueue nearest = null;

        foreach (MachineWithQueue t in allMachines)
        {
            if (!t.CanGetIntoLine)
                continue;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(t.NextPosInLine, out hit, 1, NavMesh.GetAreaFromName("Walkabke"))) // NavMesh.AllAreas
            {
                // hit is OK !

                //Debug.Log("NearestMachine HIT IS OK !");

                tempPath.ClearCorners();
                if (NavMesh.CalculatePath(fromPos, hit.position, NavMesh.AllAreas, tempPath)) // NavMesh.GetAreaFromName("Walkable") 
                {
                    // path exists !

                    float d = GetPathLength(tempPath);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = t;

                        // Debug.Log("NearestMachine ***** " + t.name + " d=" + d + " from:" + fromPos);
                    }
                }
            }
        }

        return nearest; // peut être null
    }

    public static float GetPathLength(NavMeshPath path)
    {
        float len = 0f;

        if ((path.status != NavMeshPathStatus.PathInvalid) && (path.corners.Length > 1))
        {
            for (int i = 1; i < path.corners.Length; ++i)
            {
                len += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
        }

        return len;
    }

    internal void SetSpeedsList(List<float> speeds)
    {
        speedsFromServer = speeds;
        if (allMachines != null)
            ComputeAllMachinesSpeed();
    }

    private void ComputeAllMachinesSpeed()
    {
        int maxIndex = Math.Min(speedsFromServer.Count(), allMachines.Count());

        for (int i = 0; i < maxIndex; ++i)
        {
            allMachines[i].SetSpeed(speedsFromServer[i]);
        }
    }
}