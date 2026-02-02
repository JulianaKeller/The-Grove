using UnityEngine;
using UnityEngine.UIElements;

public class WanderState : AnimalState
{
    public float wanderRange = 20f;

    private Vector3 targetPos;

    public override void Enter(Animal a) {


        targetPos = a.position + new Vector3(
                Random.Range(-wanderRange, wanderRange), 0,
                Random.Range(-wanderRange, wanderRange));

        a.SetMoveTarget(targetPos);
    }

    public override void Execute(Animal a, float timeStep) 
    {
        if (!a.hasPath)
        {
            if (!a.EvaluateLongTermNeeds())
            {
                a.ChangeState(new IdleState());
                return;
            }
            else
            {
                return;
            }
        }
    }

    public override void Exit(Animal a) {
        a.prevPosition = a.position;
    }
}
