using UnityEngine;

/*
3] STATE wait ticket => has ticket
- je demande un ticket à la machine
- j'attends
- la machine me donne un ticket
*/

public class FSMPassengerBuyTicket : StateMachineBehaviour
{
    [SerializeField] private Passenger passenger;

    private static readonly int BOOL_HAS_TICKET = Animator.StringToHash("BoughtTicket");

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(BOOL_HAS_TICKET, false);
        passenger = animator.gameObject.GetComponent<Passenger>();
        passenger.name += "_BUYTTICKET";
        passenger.BuyTicket();
        passenger.AnimationAction();

        //Debug.Log(passenger.name + " ### FSMPassengerBuyTicket::OnStateEnter");
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log(passenger.name + " ###### FSMPassengerBuyTicket::OnStateUpdate");

        if (passenger.HasTicket)
            animator.SetBool(BOOL_HAS_TICKET, true);
    }

}
