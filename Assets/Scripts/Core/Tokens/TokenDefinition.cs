using UnityEngine;

public enum TokenType
{
    SpawnAnimal,
    SpawnPlant,
    ChangeWeather,
    HealEntity,
    CreateWaterSource
}

[CreateAssetMenu(fileName = "TokenDefinition", menuName = "TheGrove/PlayerResources/TokenDefinition")]
public class TokenDefinition : ScriptableObject
{
    public TokenType type;
    public int maxAmount = 10;
    public int startingAmount = 5;
    public float rechargeTime = 10f;
}
