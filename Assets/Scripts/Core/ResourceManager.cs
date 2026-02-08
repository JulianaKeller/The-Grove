using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [System.Serializable]
    public class TokenChangedEvent : UnityEvent<TokenType, int, int> { }

    [HideInInspector]
    public TokenChangedEvent OnTokenChanged = new();

    [Header("Token Definitions")]

    private Dictionary<TokenType, Token> tokens;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        tokens = new Dictionary<TokenType, Token>();

        TokenDefinition[] defs =
        Resources.LoadAll<TokenDefinition>("ScriptableObjects/Tokens");

        foreach (var def in defs)
        {
            if (tokens.ContainsKey(def.type))
            {
                Debug.LogWarning($"Duplicate token definition for {def.type}");
                continue;
            }

            tokens[def.type] = new Token(def);
        }
    }

    public void UpdateTokens(float timeStep)
    {
        foreach (var token in tokens.Values)
        {
            int before = token.current;
            token.Tick(timeStep);

            if (before != token.current)
                Notify(token);
        }
    }

    public bool TryConsume(TokenType type)
    {
        if (!tokens.TryGetValue(type, out var token))
            return false;

        if (!token.Consume())
            return false;

        Notify(token);
        return true;
    }

    public int GetCurrent(TokenType type) {
        if(tokens == null || tokens.Count == 0)
        {
            return 0;
        }
        if(tokens.TryGetValue(type,out var token))
        {
            return token.current;
        }
        else
        {
            return 0;
        }
    } 

    public bool Has(TokenType type)
    {
        return GetCurrent(type) > 0;
    }

    public int GetMax(TokenType type) => tokens[type].definition.maxAmount;

    private void Notify(Token token)
    {
        OnTokenChanged.Invoke(
            token.definition.type,
            token.current,
            token.definition.maxAmount
        );
    }
}
