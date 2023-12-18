using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
2 / STATE WAIT in queue => 1er dans la file
- j'attends mon tour (1er dans la queue)
- j'avance dès que quelqu'un devant moi obtienne un ticket
- si je suis 1er dans la queue, je lance l'achat
*/
public class FSMPassengerWaitInLine : StateMachineBehaviour
{
    [SerializeField] private Passenger passenger;

    private static readonly int BOOL_FIRST_IN_LINE = Animator.StringToHash("FirstInLine");

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(BOOL_FIRST_IN_LINE, false);
        passenger = animator.gameObject.GetComponent<Passenger>();
        passenger.name += "_WAIT";
        passenger.AnimationIdle();

        //Debug.Log(passenger.name + " §§§ FSMPassengerWaitInLine::OnStateEnter");
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log(passenger.name + " §§§§§§ FSMPassengerWaitInLine::OnStateUpdate");

        if (passenger.ReachedDestination)
            passenger.AnimationIdle();

        if (passenger.FirstInLine)
            animator.SetBool(BOOL_FIRST_IN_LINE, true);
    }

}
