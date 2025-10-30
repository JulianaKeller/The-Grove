using UnityEngine;

public abstract class Entity
{
    public int id;
    public Vector3 position; //current position
    public bool isAlive = true;

    public virtual void UpdateEntity(float dt) { }

    public virtual void Die()
    {
        isAlive = false;
    }

    
}
