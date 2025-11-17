using UnityEngine;

public class FleeState : AnimalState
{
    private Animal enemy;
    private const float safeDistance = 30f;
    private const float directionRandomization = 0.4f;

    public FleeState(Animal enemy)
    {
        this.enemy = enemy;
    }

    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now fleeing.");
        a.isFleeing = true;
    }
    public override void Execute(Animal a, float timeStep) {
        //Run away in the direction that enemy is facing with some random direction variation
        //End fleeing if enemy is far enough away, then go back to idle state

        if (enemy == null)
        {
            a.ChangeState(new IdleState());
            return;
        }

        Vector3 toEnemy = a.position - enemy.position;
        float dist = toEnemy.magnitude;

        if (dist > safeDistance)
        {
            a.ChangeState(new IdleState());
            return;
        }

        Vector3 baseDir = toEnemy.normalized;
        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-directionRandomization, directionRandomization),
            0f,
            UnityEngine.Random.Range(-directionRandomization, directionRandomization)
        );

        Vector3 fleeDir = (baseDir + randomOffset).normalized;

        a.MoveTo(fleeDir * safeDistance, timeStep);
    }
    public override void Exit(Animal a) {
        a.isFleeing = false;
    }
}
