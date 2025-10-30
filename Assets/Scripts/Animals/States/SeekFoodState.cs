using UnityEngine;

public class SeekFoodState : AnimalState
{
    private Plant targetFood;

    public override void Enter(Animal a) {
        targetFood = FindNearestFood(a);
        Debug.Log(a.species.name + " is now seeking food.");
    }

    public override void Execute(Animal a, float dt) {
        if (targetFood == null)
        {
            // If no food found, wander
            a.ChangeState(new WanderState());
            return;
        }

        //ToDo Check for other needs and weigh against hunger

        float distance = Vector3.Distance(a.position, targetFood.position);
        if (distance < 0.5f)
        {
            a.ChangeState(new EatState(targetFood));
            return;
        }

        a.position = Vector3.MoveTowards(a.position, targetFood.position, a.species.moveSpeed * dt);
    }

    public override void Exit(Animal a) { }

    private Plant FindNearestFood(Animal a)
    {
        return null; //ToDo query environmentgrid
    }
}
