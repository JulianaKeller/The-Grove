using UnityEngine;

public class SeekFoodState : AnimalState
{
    private Entity targetFood;

    public override void Enter(Animal a) {
        targetFood = a.FindNearestFood();
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

        float distance = Vector3.Distance(a.position, targetFood.position);

        if (distance < 0.5f)
        {
            //ToDo attack or eat
            a.ChangeState(new EatState(targetFood));
            return;
        }

        a.MoveTo(targetFood.position, timeStep);

    }

    public override void Exit(Animal a) {
        a.isRunning = false;
        a.isWalking = false;
    }
}
