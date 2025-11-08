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

        //ToDO facilitate migrating behavior to find food by taking a direction close to the previous one

        a.isWalking = true;
        a.isRunning = false;

        Debug.Log(a.species.name + " is now wandering.");
    }

    public override void Execute(Animal a, float timeStep) {

        currentWanderTimer -= timeStep;

        Vector3 newPos = Vector3.MoveTowards(a.position, targetPos, a.species.walkingSpeed * timeStep);
        a.prevPosition = a.position;
        a.position = newPos;

        Debug.Log($"{a.species.name} wandering. Timer: {currentWanderTimer:F2}, Distance: {Vector3.Distance(a.position, targetPos):F2}");

        // Face target direction
        if (a.view != null)
            a.view.FaceTowards(newPos);

        //-----Decision making-----

        a.EvaluateNeeds();

        if (currentWanderTimer <= 0f || Vector3.Distance(a.position, targetPos) < 0.1f)
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
