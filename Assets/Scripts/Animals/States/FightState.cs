using UnityEngine;
using UnityEngine.UI;

public class FightState : AnimalState
{
    private Animal enemy;

    private const float fightRange = 1.5f; // required distance to apply damage
    private const float approachStopBuffer = 0.2f;

    public FightState(Animal enemy)
    {
        this.enemy = enemy;
    }

    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now fighting.");
        a.isFighting = true;
    }
    public override void Execute(Animal a, float timeStep) {
        //Fighting either in defense or to get food
        //Fight or flight should be evaulated in either case
        //Fighting should otherwise be done until the target is dead

        Vector3 offset = (enemy.position - a.position);
        float dist = offset.magnitude;

        if (dist > fightRange + approachStopBuffer)
        {
            a.MoveTo(enemy.position, timeStep);

            return; //ToDo attack when close enough?
        }

        if (enemy == null)
        {
            a.ChangeState(new IdleState());
            return;
        }

        if (!enemy.isAlive)
        {
            a.ChangeState(new IdleState());
            return;
        }

        a.FaceTowards(enemy.position);

        float randomFactor = UnityEngine.Random.Range(0.85f, 1.15f);
        float damage = a.species.power - enemy.species.defense * randomFactor;

        enemy.health -= damage;

        Debug.Log($"{a.species.name} hit {enemy.species.name} for {damage:F1} damage. Enemy HP: {enemy.health:F1}");

        if(enemy.health <= 0)
        {
            a.ChangeState(new IdleState());
        }
    }
    public override void Exit(Animal a) {
        a.isFighting = false;
    }
}
