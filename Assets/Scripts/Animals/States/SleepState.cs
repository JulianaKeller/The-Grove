using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SleepState : AnimalState
{
    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now sleeping.");
        a.isSleeping = true;
    }
    public override void Execute(Animal a, float timeStep) {
        //regenerate health
        a.health = Mathf.Min(a.species.maxHP, a.health + a.species.hpRecoveryRate * 2 * timeStep);
        //regenerate energy
        a.energy = Mathf.Min(100f, a.energy + a.species.energyDepletionRate * 2 * timeStep);
        //regenerate stamina
        a.stamina = Mathf.Min(a.species.stamina, a.stamina + timeStep * 2);
    }
    public override void Exit(Animal a) {
        a.isSleeping = false;
    }
}
