using UnityEngine;

public class IdleState : AnimalState
{
    private float idleTimer;

    public override void Enter(Animal a)
    {
        idleTimer = Random.Range(1f, 10f);
        //Debug.Log(a.species.name + " is now idle.");
    }

    public override void Execute(Animal a, float timeStep)
    {
        idleTimer -= timeStep;

        if (idleTimer <= 0f)
        {
            if (!a.EvaluateLongTermNeeds())
            {
                a.ChangeState(new WanderState());
                return;
            }
            else
            {
                return;
            }
        }
    }

    public override void Exit(Animal a) {
        
    }
}
