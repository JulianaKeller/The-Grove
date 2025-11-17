using UnityEngine;

public class MateState : AnimalState
{
    private Animal mate;

    private const float mateDuration = 4f;
    private const float mateRandomVariance = 1.5f;

    private float mateTimer;

    public MateState(Animal mate)
    {
        this.mate = mate;
    }

    public override void Enter(Animal a) {
        Debug.Log(a.species.name + " is now mating.");
        a.isMating = true;

        mateTimer = mateDuration + Random.Range(-mateRandomVariance, mateRandomVariance);
        if (mateTimer < WorldManager.Instance.timeStep) mateTimer = WorldManager.Instance.timeStep;
    }
    public override void Execute(Animal a, float timeStep) {
        //Go towards mate
        //When close enough, mate for mateTimer + randomization
        //When close enough, also make the other Animal go into Mate State, but only if mating drive is over a threshold, otherwise this animal goes into follow state
        //After mate time has successfully reached zero, spawn a new animal of the same species with AnimalManager
        //For the new baby animal, this or the mate Animal should be set as mother (whichever is female)

        if (mate == null || !mate.isAlive)
        {
            a.ChangeState(new IdleState());
            return;
        }

        mateTimer -= timeStep;
        if (mateTimer > 0f)
            return;

        Vector3 birthPos = (a.position + mate.position) * 0.5f;

        Animal mother = (a.isFemale ? a : mate.isFemale ? mate : a);
        AnimalManager.Instance.SpawnAnimal(a.species, birthPos, mother);

        a.ChangeState(new IdleState());
    }

    public override void Exit(Animal a) {
        a.matingDrive = 0;
        a.isMating = false;
    }
}
