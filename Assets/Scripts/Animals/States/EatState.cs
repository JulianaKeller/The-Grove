using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EatState : AnimalState
{
    private Entity targetFood;
    private float eatTimer;

    public EatState(Entity targetFood)
    {
        this.targetFood = targetFood;
    }

    public override void Enter(Animal a) {
        a.isEating = true;
        Debug.Log(a.species.name + " is now eating.");
    }
    public override void Execute(Animal a, float timeStep) {
        if (targetFood == null || targetFood.GetNutritionValue() <= 0 * timeStep) //ToDo bite size
        {
            a.ChangeState(new WanderState());
            return;
        }

        targetFood.isBeingEaten = true; //ToDo bite size
        targetFood.BeEaten(timeStep);

        a.hunger = Mathf.Max(0, a.hunger - 0.2f * timeStep);

        if (a.hunger <= 0f)
        {
            a.ChangeState(new IdleState());
        }
    }
    public override void Exit(Animal a) {
        a.isEating = false;
        targetFood.isBeingEaten = false;
    }
}
