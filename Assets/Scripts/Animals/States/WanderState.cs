using UnityEngine;
using UnityEngine.UIElements;

public class WanderState : AnimalState
{
    public float wanderRange = 20f;
    public float wanderTimeMin = 5f;
    public float wanderTimeMax = 15f;

    private Vector3 targetPos;
    private float currentWanderTimer;
    private float wanderTimer;

    public override void Enter(Animal a) {

        targetPos = a.position + new Vector3(
                Random.Range(-wanderRange, wanderRange), 0,
                Random.Range(-wanderRange, wanderRange));
        wanderTimer = Random.Range(wanderTimeMin, wanderTimeMax);
        currentWanderTimer = wanderTimer;

        a.isWalking = true;
        a.isRunning = false;

        Debug.Log(a.species.name + " is now wandering.");
    }

    public override void Execute(Animal a, float timeStep) {

        currentWanderTimer -= timeStep;

        Vector3 newPos = Vector3.MoveTowards(a.position, targetPos, a.species.walkingSpeed * timeStep);
        a.prevPosition = a.position;
        a.position = newPos;

        // Face target direction
        if (a.view != null)
            a.view.FaceTowards(newPos);

        //-----Decision making-----

        if(wanderTimer < wanderTimer - wanderTimeMin) //wander for the minimum amount of time
        {
            a.EvaluateNeeds();
        }

        if (currentWanderTimer <= 0f || a.position == targetPos)
        {
            a.ChangeState(new IdleState());
            return;
        }
    }

    public override void Exit(Animal a) {
        a.isWalking = false;
        a.isRunning = false;
        a.prevPosition = a.position;
    }
}
