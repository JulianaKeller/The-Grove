using UnityEngine;

public class SeekFoodState : AnimalState
{
    private Entity targetFood;

    public override void Enter(Animal a) {
        targetFood = FindNearestFood(a);
        Debug.Log(a.species.name + " is now seeking food.");
    }

    public override void Execute(Animal a, float timeStep) {

        if (targetFood == null)
        {
            // If no food found, wander
            a.ChangeState(new WanderState());
            Debug.Log("No food found. ");
            return;
        }
        else
        {
            Debug.Log("Food found. " + a.species.name + " is now targeting food.");
        }

        //ToDo Check for other needs and weigh against hunger

        float distance = Vector3.Distance(a.position, targetFood.position);

        if (distance < 0.5f)
        {
            //ToDo attack or eat
            a.ChangeState(new EatState(targetFood));
            return;
        }

        // Decide whether to run or walk
        bool shouldRun = a.ShouldRun(timeStep) && (a.hunger > 50f) && (a.energy > 20f);

        if (shouldRun)
        {
            a.isRunning = true;
            a.isWalking = false;
        }
        else
        {
            a.isRunning = false;
            a.isWalking = true;
        }

        float speed = a.isRunning ? a.species.runningSpeed : a.species.walkingSpeed;

        // Move towards the food target
        a.prevPosition = a.position;
        a.position = Vector3.MoveTowards(a.position, targetFood.position, speed * timeStep);

        // Face target direction
        if (a.view != null)
            a.view.FaceTowards(targetFood.position);

    }

    public override void Exit(Animal a) {
        a.isRunning = false;
        a.isWalking = false;
    }

    private Entity FindNearestFood(Animal a)
    {
        return a.FindNearestFood();
    }
}
