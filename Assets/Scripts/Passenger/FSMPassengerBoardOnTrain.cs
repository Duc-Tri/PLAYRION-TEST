using UnityEngine;

/*
5] STATE end :  boarding => sit , désactiver
- j'atteins le train, je m'asseois dedans
*/

public class FSMPassengerBoardOnTrain : StateMachineBehaviour
{
    [SerializeField] private Passenger passenger;

    private static readonly int BOOL_ON_TRAIN = Animator.StringToHash("OnTrain");

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(BOOL_ON_TRAIN, false);
        passenger = animator.gameObject.GetComponent<Passenger>();

        //Debug.Log(passenger.name + " %%% FSMPassengerBoardOnTrain::OnStateEnter");

        if (Train.Instance.BoardAPassenger(passenger))
        {
            passenger.name += "_ONTRAIN";
            passenger.InTrain();
            passenger.AnimationSit();
            passenger.gameObject.SetActive(false);

            animator.SetBool(BOOL_ON_TRAIN, true);
        }
    }

}
