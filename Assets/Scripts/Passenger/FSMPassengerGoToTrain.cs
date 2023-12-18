using UnityEngine;

/*
4/ STATE go to train => destination ok
- je vais vers le train
- je peux passer le portique grâce au ticket
- je travers le portique et j'atteins le train
*/

public class FSMPassengerGoToTrain : StateMachineBehaviour
{
    [SerializeField] private Passenger passenger;

    private static readonly int BOOL_REACHED_TRAIN_OR_DEPARTED = Animator.StringToHash("ReachedTrain");

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(BOOL_REACHED_TRAIN_OR_DEPARTED, false);
        passenger = animator.gameObject.GetComponent<Passenger>();
        passenger.name += "_GOTRAIN";
        passenger.AnimationWalk();
        passenger.GiveAccessGate();
        passenger.GoToTrain();
        //passenger.transform.localScale = Vector3.one;

        //Debug.Log(passenger.name + " ■■■ FSMPassengerGoToTrain::OnStateEnter");
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log(passenger.name + " ■■■■■■ FSMPassengerGoToTrain::OnStateUpdate");

        if (passenger.ReachedTrain || Train.Instance.Departed)
            animator.SetBool(BOOL_REACHED_TRAIN_OR_DEPARTED, true);
    }

}
