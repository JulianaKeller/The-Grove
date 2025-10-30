using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EatState : AnimalState
{
    private Plant targetFood;
    private float eatTimer;

    public EatState(Plant targetFood)
    {
        this.targetFood = targetFood;
    }

    public override void Enter(Animal a) {
        eatTimer = Random.Range(2f, 4f);
    }
    public override void Execute(Animal a, float timeStep) {
        if (targetFood == null)
        {
            a.ChangeState(new WanderState());
            return;
        }

        eatTimer -= timeStep;
        a.hunger -= 0.2f * timeStep;

        if (eatTimer <= 0f || a.hunger <= 0.2f)
        {
            //PlantManager.Instance.ConsumePlant(targetFood);
            a.ChangeState(new IdleState());
        }
        //ToDo introduce nutrition parameter in plants, size should influence eatTimer, plant should be smaller but not disappear if not eaten completely
    }
    public override void Exit(Animal a) { }
}
