using UnityEngine;
using UnityEngine.UIElements;

public class WanderState : AnimalState
{
    private Vector3 targetPos;
    private float wanderTimer;

    public override void Enter(Animal a) {
        targetPos = a.position + new Vector3(
                Random.Range(-5f, 5f), 0,
                Random.Range(-5f, 5f));
        wanderTimer = Random.Range(2f, 5f);
    }
    public override void Execute(Animal a, float timeStep) {
        wanderTimer -= timeStep;

        a.position = Vector3.MoveTowards(a.position, targetPos, a.species.moveSpeed * timeStep);


        if (a.hunger > 0.6f) //Todo introduce some randomness to treshold
        {
            a.ChangeState(new SeekFoodState());
            return;
        }

        //ToDo check thirst, mate etc thresholds

        if (wanderTimer <= 0f)
        {
            a.ChangeState(new IdleState());
        }
    }
    public override void Exit(Animal a) { }
}
