using UnityEngine;

public class FSMPassengerMissedTrain : StateMachineBehaviour
{
    [SerializeField] private Passenger passenger;

    //private static readonly int BOOL_HAS_TICKET = Animator.StringToHash("BoughtTicket");

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.SetBool(BOOL_HAS_TICKET, false);
        passenger = animator.gameObject.GetComponent<Passenger>();
        passenger.name += "_MISSEDTRAIN";
        passenger.QuitEverything();

        //Debug.Log(passenger.name + " ### FSMPassengerMissedTrain::OnStateEnter");
    }

}
