using UnityEngine;

public class MigrateState : AnimalState
{
    public enum MigrationReason
    {
        Food,
        Water,
        Mate
    }

    private MigrationReason reason;

    private Vector3 targetPos;
    private float migrationDuration;
    private float timeLeft;

    private const float MIN_MIGRATION_DURATION = 10f;
    private const float MAX_MIGRATION_DURATION = 30f;
    private const float MIN_DISTANCE = 50f;
    private const float MAX_DISTANCE = 200f;

    public MigrateState(MigrationReason reason)
    {
        this.reason = reason;
    }

    public override void Enter(Animal a)
    {
        migrationDuration = Random.Range(MIN_MIGRATION_DURATION, MAX_MIGRATION_DURATION);
        timeLeft = migrationDuration;

        Vector3 direction = Vector3.zero;
        switch (reason)
        {
            case MigrationReason.Food:
                direction = (a.lastFoundFoodPos - a.position).normalized;
                break;
            case MigrationReason.Water:
                direction = (a.lastFoundWaterPos - a.position).normalized;
                break;
            case MigrationReason.Mate:
                direction = (a.lastFoundMatePos - a.position).normalized;
                break;
        }

        direction.y = 0;

        targetPos = a.position + direction * Random.Range(MIN_DISTANCE, MAX_DISTANCE);

        a.SetMoveTarget(targetPos);
    }

    public override void Execute(Animal a, float timeStep)
    {
        timeLeft -= timeStep;

        if (ReasonDetected(a))
        {
            CancelMigration(a);
            return;
        }

        if(timeLeft <= 0f || Vector3.Distance(a.position, targetPos) > 1.5f)
        {
            a.ChangeState(new IdleState());
        }
    }

    public override void Exit(Animal a) {
        
    }

    private bool ReasonDetected(Animal a)
    {
        switch (reason)
        {
            case MigrationReason.Food:
                return a.FindNearestFood() != null;
            case MigrationReason.Water:
                return a.FindNearestWaterSource() != null;
            case MigrationReason.Mate:
                return a.GetNearbyMate() != null;
        }
        return false;
    }

    private void CancelMigration(Animal a)
    {
        switch (reason)
        {
            case MigrationReason.Food:
                a.failedFoodSearches = 0;
                a.ChangeState(new SeekFoodState());
                break;
            case MigrationReason.Water:
                a.failedWaterSearches = 0;
                a.ChangeState(new SeekWaterState());
                break;
            case MigrationReason.Mate:
                a.failedMateSearches = 0;
                a.ChangeState(new SeekMateState());
                break;
        }
    }
}
