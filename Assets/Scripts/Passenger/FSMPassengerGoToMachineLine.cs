using UnityEngine;

/*
1] STATE je vais vers la machine pour faire la queue => destination ok
- j'avance vers la position qu'on m a donné dans la queue
-  si j'atteins l 'endroit ETAPE SUIVANT, je fais la queue
*/
public class FSMPassengerGoToMachineLine : StateMachineBehaviour
{
    [SerializeField] private Passenger passenger;

    private static readonly int BOOL_REACH_MACHINE = Animator.StringToHash("ReachedMachine");

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(BOOL_REACH_MACHINE, false);
        passenger = animator.gameObject.GetComponent<Passenger>();
        passenger.name += "_GOLINE";
        passenger.GotoInLineNearestMachine();
        passenger.AnimationWalk();
        passenger.transform.localScale = Vector3.one;

        //Debug.Log(passenger.name + " @@@ FSMPassengerGoToMachineLine::OnStateEnter");
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log(passenger.name + " @@@@@@ FSMPassengerGoToMachineLine::OnStateUpdate");

        if (passenger.ReachedMachine)
            animator.SetBool(BOOL_REACH_MACHINE, true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log(passenger.name + " @@@@@@@@@ FSMPassengerGoToMachineLine::OnStateExit");

        passenger.LookAtMachine();
        //passenger.transform.localScale = Vector3.one * .5f;
    }
}
