using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SeekWaterState : AnimalState
{
    private WaterSource targetWaterSource;

    public override void Enter(Animal a) {
        targetWaterSource = a.FindNearestWaterSource();
        a.SetMoveTarget(targetWaterSource != null ? targetWaterSource.center : a.position);
        //ToDo offset of water source radius
    }

    public override void Execute(Animal a, float timeStep) {
        if (targetWaterSource == null)
        {
            a.ChangeState(new WanderState());
            return;
        }

        List<WaterSource> nearbyWaterSources = WorldManager.Instance.GetNearbyWaterSources(a.position, 1);
        //Check if adjacent to water source
        if (nearbyWaterSources.Count > 0)
        {
            a.ChangeState(new DrinkState(nearbyWaterSources[0]));
            return;
        }
    }

    public override void Exit(Animal a) { }
}
