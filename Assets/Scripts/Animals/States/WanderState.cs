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

        Vector3 newPos = Vector3.MoveTowards(a.position, targetPos, a.species.moveSpeed * timeStep);
        a.prevPosition = a.position;
        a.position = newPos;


        if (a.hunger > 0.6f) //Todo introduce some randomness to treshold or the higher the hunger the higher the probability of looking for food with probability being 100 if hunger is over a ceratin threshold
        {
            a.ChangeState(new SeekFoodState());
            //ToDo here the position of the animal a should be set to the current position that the animal view managed to reach before the wander state was cancelled before animal view managed to reach the target position
            //This is done in Animal.ChangeState now
            return;
        }

        //ToDo check thirst, mate etc thresholds

        if (wanderTimer <= 0f)
        {
            a.ChangeState(new IdleState());
            //ToDo here the position of the animal a should be set to the current position that the animal view managed to reach before the wander state was cancelled before animal view managed to reach the target position
            //This is done in Animal.ChangeState now
            return;
        }

        if (a.position == targetPos)
        {
            a.ChangeState(new IdleState());
            return;
        }
    }

    public override void Exit(Animal a) {
        a.prevPosition = a.position;
    }
}
