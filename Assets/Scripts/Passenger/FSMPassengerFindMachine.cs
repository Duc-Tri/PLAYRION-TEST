using UnityEngine;

/*
0] STATE init, cherche machine => trouvé machine
- je cherche la machine dispo la + proche
- si aucune de dispo, je patiente 1 seconde avant de recommencer
- si j'en trouve une dispo, ETAPE SUIVANTE (je fais la queue)
*/
public class FSMPassengerFindMachine : StateMachineBehaviour
{
    [SerializeField] private Passenger passenger;
    private static readonly int BOOL_FOUND_MACHINE = Animator.StringToHash("FoundMachine");

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(BOOL_FOUND_MACHINE, false);
        animator.SetBool("TrainDeparted", false);

        passenger = animator.gameObject.GetComponent<Passenger>();
        //passenger.transform.localScale = Vector3.one * .25f;
        passenger.name += "_SEARCH";
        passenger.AnimationIdle();

        passenger.SearchNearestAvailableMachineAndAddToLine();
        if (passenger.NearestAvailableMachine == null)
            passenger.TrySearchMachineAgain(); // boucle de coroutine

        //Debug.Log(passenger.name + " +++ FSMPassengerFindMachine::OnStateEnter");
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log(passenger.name + " ++++++ FSMPassengerFindMachine::OnStateUpdate");

        if (passenger.NearestAvailableMachine != null)
            animator.SetBool(BOOL_FOUND_MACHINE, true);
    }

}
