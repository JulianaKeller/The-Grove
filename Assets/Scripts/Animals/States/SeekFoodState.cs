using UnityEngine;

public class SeekFoodState : AnimalState
{
    private Entity targetFood;

    public override void Enter(Animal a) {
        targetFood = a.FindNearestFood();

        a.SetMoveTarget(targetFood != null ? targetFood.position : a.position);
    }

    public override void Execute(Animal a, float timeStep) {

        if (targetFood == null)
        {
            a.ChangeState(new WanderState());
            return;
        }
        else
        {
            Debug.Log("Food found. " + a.species.name + " is now targeting food.");
        }

        if (!a.hasPath)
        {
            if (targetFood.isAnimal && targetFood.isAlive)
            {
                a.ChangeState(new FightState(targetFood as Animal));
            }
            else
            {
                a.ChangeState(new EatState(targetFood));
            }
            return;
        }
    }

    public override void Exit(Animal a) {
        a.isRunning = false;
        a.isWalking = false;
    }
}
