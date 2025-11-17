using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Herd
{
    private List<Animal> animals = new List<Animal>();
    public Animal Leader { get; private set; }

    public void AddAnimal(Animal a)
    {
        animals.Add(a);
        UpdateLeader();
    }

    public void RemoveAnimal(Animal a)
    {
        animals.Remove(a);
        if (Leader == a) UpdateLeader();
    }

    private void UpdateLeader()
    {
        Leader = animals.OrderByDescending(x => x.dominance).FirstOrDefault();
    }

    public IEnumerable<Animal> GetAnimals() => animals;
    public int Count => animals.Count;
}
