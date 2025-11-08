using UnityEngine;

public class IdleState : AnimalState
{
    private float idleTimer;

    public override void Enter(Animal a)
    {
        idleTimer = Random.Range(1f, 5f);
        Debug.Log(a.species.name + " is now idle.");
    }

    public override void Execute(Animal a, float timeStep)
    {
        idleTimer -= timeStep;

        a.EvaluateNeeds();

        if (idleTimer <= 0f)
        {
            a.ChangeState(new WanderState());
        }
    }

    public override void Exit(Animal a) { }
}
