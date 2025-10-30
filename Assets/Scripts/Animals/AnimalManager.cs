using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    //Updates all animals (state transitions, needs, movement)
    public static AnimalManager Instance { get; private set; }

    private List<Animal> animals = new List<Animal>();
    private List<AnimalView> views = new List<AnimalView>();

    public AnimalSpeciesData startingSpecies;

    private static int nextId = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //ToDo Testing Setup, initial spawn should be initiated by player later
        Vector3 SpawnPosition = new Vector3(Random.Range(-50.0f, 50.0f), 0.5f, Random.Range(-50.0f, 50.0f));
        SpawnAnimal(startingSpecies, SpawnPosition);
    }

    public void SpawnAnimal(AnimalSpeciesData species, Vector3 pos)
    {
        if (species.prefab != null)
        {
            Animal data = new Animal(species, pos, nextId++);
            GameObject obj = Instantiate(species.prefab, pos, Quaternion.identity);
            AnimalView view = obj.GetComponent<AnimalView>();

            view.data = data;
            data.view = view;

            views.Add(view);
            animals.Add(data);

            Debug.Log("Spwaned a " + data.species.name);
        }
    }

    public void UpdateAnimals(float timeStep, int tick)
    {
        //Reset interpolation factors for all animal views
        /*foreach (var view in AnimalManager.Instance.views)
        {
            Debug.Log("Resetting Interpolation...");
            view.ResetInterpolation();
        }*/

        for (int i = animals.Count - 1; i >= 0; i--)
        {
            var animal = animals[i];

            // update 1/3 of animals per tick
            if ((tick + animal.id) % 3 == 0)
            {
                animal.view.ResetInterpolation();
                animal.UpdateAI(timeStep);
            }
        }
    }

    public void RemoveAnimal(Animal a)
    {
        animals.Remove(a);
        if (a.view != null)
        {
            views.Remove(a.view);
            Destroy(a.view.gameObject);
        }
    }
}
