using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Passenger : MonoBehaviour
{
    private static int NUM = 0;
    [SerializeField] private float defaultSpeed = 1; // real Animation Mecanim
    [SerializeField] private Animator animator; // real Animation Mecanim
    private string id;
    private bool hasTicket;
    private bool boardedTrain;

    public string Id => id;
    public bool BoardedTrain => boardedTrain;
    public bool HasTicket => hasTicket;

    public bool FirstInLine => NearestAvailableMachine != null && NearestAvailableMachine.IsFirstInLine(this);

    internal bool ReachedTrain => (Vector3.Distance(transform.position, Train.Instance.OnBoardTrainPos) <= 1f) && ReachedDestination;

    public bool ReachedMachine => (Vector3.Distance(transform.position, currentDestination) <= .4f) && ReachedDestination;

    public bool ReachedDestination => (NMAgent.hasPath && (NMAgent.path.status == NavMeshPathStatus.PathComplete || NMAgent.remainingDistance <= NMAgent.stoppingDistance));

    public static bool NAVMESH_FREEZE = false;

    private Animator FSM; // for logic Finite State Machine

    public MachineWithQueue NearestAvailableMachine;
    public NavMeshAgent NMAgent;

    [SerializeField] public Vector3 currentDestination;
    private static float SpeedFactor = 1;

    public static void SetPassengersSpeedFactor(float sf) => SpeedFactor = sf;

    private void Awake()
    {
        AnimationIdle();
        Train.Instance.OnTrainDeparted += OnMissedTrain;
        NMAgent = GetComponent<NavMeshAgent>();
        FSM = GetComponent<Animator>();

        if (GameManager.Instance.globalForceRealData)
        {
            NMAgent.speed = defaultSpeed;
            NMAgent.acceleration = 2.6f;
        }

        ChangeSpeedAccordingToFactor();
    }

    private void Start()
    {
        name = id = "P-" + (NUM++);
        hasTicket = false;
        boardedTrain = false;
        NMAgent.isStopped = NAVMESH_FREEZE;
    }

    // Recherche de la machine la plus proche pour y aller et faire la queue
    public void SearchNearestAvailableMachineAndAddToLine()
    {
        MachineWithQueue machine = MachinesManager.Instance.NearestAvailableMachine(transform.position);

        if (machine != null)
        {
            Debug.Log(name + " ____________________ NEAREST:" + machine.name);
            Vector3 machinePosInLine;

            if (machine.AddToLine(this, out machinePosInLine))
            {
                currentDestination = machinePosInLine;
                NearestAvailableMachine = machine;
            }
        }
        else
        {
            Debug.LogWarning(name + " ____________________ NO MACHINE AVAILABLE NEARBY !!!");
        }
    }

    internal void GetTrainTicket()
    {
        //Debug.LogWarning(name + " ■■■ GOT ATICKET!");
        hasTicket = true;
    }

    public void GotoInLineNearestMachine()
    {
        transform.LookAt(NearestAvailableMachine.transform);
        NMAgent.path.ClearCorners();
        NMAgent.SetDestination(currentDestination);
    }

    public void TrySearchMachineAgain()
    {
        StartCoroutine(WaitAndSearchMachine());
    }

    private static WaitForSeconds waitBeforeSearchAgain = new WaitForSeconds(1);

    // Boucle tant qu'un machine disponibles n'a pas été trouvée.
    private IEnumerator WaitAndSearchMachine()
    {
        while (NearestAvailableMachine == null)
        {
            yield return waitBeforeSearchAgain;
            SearchNearestAvailableMachineAndAddToLine();
        }
    }

    public void NewPositionInLine(Vector3 pos)
    {
        //Debug.Log(name + "NewPositionInLine /// " + transform.position + " >>> " + pos);

        currentDestination = pos;

        NMAgent.path.ClearCorners();
        NMAgent.SetDestination(pos);
        AnimationWalk();
    }

    public void GiveAccessGate()
    {
        int areaMask = 0;
        areaMask += 1 << NavMesh.GetAreaFromName("Walkable");//turn off all
        areaMask += 1 << NavMesh.GetAreaFromName("Door");

        NMAgent.areaMask = areaMask;
    }
    public void GoToTrain()
    {
        NMAgent.path.ClearCorners();
        NMAgent.SetDestination(Train.Instance.OnBoardTrainPos);

        currentDestination = Train.Instance.OnBoardTrainPos;
        transform.LookAt(currentDestination);
    }

    void Update()
    {
        if (NMAgent.hasPath)
        {
            for (int i = NMAgent.path.corners.Length - 1; i >= 1; i--)
                Debug.DrawLine(NMAgent.path.corners[i], NMAgent.path.corners[i - 1], Color.black);

            Debug.DrawLine(transform.position, currentDestination, hasTicket ? Color.magenta : Color.yellow);
        }
    }

    public void LookAtMachine()
    {
        transform.LookAt(NearestAvailableMachine.transform);
    }

    public void BuyTicket()
    {
        NearestAvailableMachine.AskToBuyTicket(this);
    }

    public void SitAndCry()
    {
        NMAgent.path.ClearCorners();
        NMAgent.isStopped = true;
        AnimationSit();
    }

    // Animations =================================================================================

    private static readonly int ANIM_ACTION = Animator.StringToHash("Action");
    private static readonly int ANIM_SIT = Animator.StringToHash("Sit");
    private static readonly int ANIM_WALK = Animator.StringToHash("Walk");

    public void AnimationAction()
    {
        animator.SetBool(ANIM_ACTION, true);
        animator.SetBool(ANIM_SIT, false);
        animator.SetBool(ANIM_WALK, false);
    }

    public void AnimationIdle()
    {
        animator.SetBool(ANIM_ACTION, false);
        animator.SetBool(ANIM_SIT, false);
        animator.SetBool(ANIM_WALK, false);
    }

    public void AnimationWalk()
    {
        animator.SetBool(ANIM_ACTION, false);
        animator.SetBool(ANIM_SIT, false);
        animator.SetBool(ANIM_WALK, true);
    }

    public void AnimationSit()
    {
        animator.SetBool(ANIM_ACTION, false);
        animator.SetBool(ANIM_SIT, true);
        animator.SetBool(ANIM_WALK, false);
    }

    internal void InTrain()
    {
        boardedTrain = true;
        transform.SetParent(transform);
    }

    private void OnMissedTrain()
    {
        Train.Instance.OnTrainDeparted -= OnMissedTrain;
        FSM.SetBool("TrainDeparted", true);
    }

    internal void QuitEverything()
    {
        SitAndCry();
        //NearestAvailableMachine.RemoveFromLine(this);
        NMAgent.isStopped = true;
        StopAllCoroutines();
    }

    internal void ChangeSpeedAccordingToFactor()
    {
        NMAgent.speed = defaultSpeed * SpeedFactor;
    }

    internal static void SetPassengersFreeze(bool freeze)
    {
        NAVMESH_FREEZE = freeze;
    }

    internal void Freeze(bool freeze)
    {
        if (gameObject.activeSelf)
        {
            NMAgent.isStopped = freeze;
            if (NMAgent.hasPath)
            {
                if (freeze)
                    AnimationSit();
                else
                    AnimationWalk();
            }
        }
    }
}
