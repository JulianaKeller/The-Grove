using UnityEngine;

public class IdleState : AnimalState
{
    private float idleTimer;

    public override void Enter(Animal a)
    {
        idleTimer = Random.Range(1f, 4f);
    }

    public override void Execute(Animal a, float dt)
    {
        idleTimer -= dt;

        if (a.hunger > 0.6f) //Todo introduce some randomness to treshold
        {
            a.ChangeState(new SeekFoodState());
            return;
        }

        //ToDo check thirst, mate etc thresholds

        if (idleTimer <= 0f)
        {
            a.ChangeState(new WanderState());
        }
    }

    public override void Exit(Animal a) { }
}
