using UnityEngine;
using UnityEngine.UIElements;

public class WanderState : AnimalState
{
    private Vector3 targetPos;
    private float wanderTimer;

    public override void Enter(Animal a) {
        targetPos = a.position + new Vector3(
                Random.Range(-20f, 20f), 0,
                Random.Range(-20f, 20f));
        wanderTimer = Random.Range(2f, 10f);
        Debug.Log(a.species.name + " is now wandering.");
    }
    public override void Execute(Animal a, float timeStep) {
        wanderTimer -= timeStep;

        a.prevPosition = a.position;
        a.position = Vector3.MoveTowards(a.position, targetPos, a.species.moveSpeed * timeStep);


        if (a.hunger > 0.6f) //Todo introduce some randomness to treshold or the higher the hunger the higher the probability of looking for food with probability being 100 if hunger is over a ceratin threshold
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
    public override void Exit(Animal a) {
        a.prevPosition = a.position;
    }
}
