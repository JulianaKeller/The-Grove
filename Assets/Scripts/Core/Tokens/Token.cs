using UnityEngine;

[System.Serializable]
public class Token
{
    public TokenDefinition definition;

    public int current { get; private set; }

    private float rechargeTimer;

    public Token(TokenDefinition def)
    {
        definition = def;
        current = def.startingAmount;
    }

    public bool CanConsume()
    {
        return current > 0;
    }

    public bool Consume()
    {
        if (current <= 0)
            return false;

        current--;
        return true;
    }

    public void Tick(float deltaTime)
    {
        if (current >= definition.maxAmount)
            return;

        rechargeTimer += deltaTime;

        if (rechargeTimer >= definition.rechargeTime)
        {
            rechargeTimer = 0f;
            current++;
        }
    }
}
