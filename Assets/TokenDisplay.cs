using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TokenDisplay : MonoBehaviour
{
    public TokenType tokenType;

    [Header("UI")]
    public TMP_Text tokenText;
    public Image[] buttonBG;
    public Color buttonColor = Color.white;
    public Color disabledColor = new Color(0.776f, 0.776f, 0.776f);

    void Awake()
    {
        if (buttonBG != null)
        {
            foreach (var img in buttonBG)
            {
                img.color = buttonColor;
            }
        }
    }

    void OnEnable()
    {
        
    }

    private void Start()
    {
        if (tokenText == null)
        {
            return;
        }

        ResourceManager.Instance?.OnTokenChanged.AddListener(OnTokenChanged);
        Refresh();
    }

    void OnDisable()
    {
        ResourceManager.Instance?.OnTokenChanged.RemoveListener(OnTokenChanged);
    }

    void OnTokenChanged(TokenType type, int current, int max)
    {
        if (type != tokenType)
            return;

        tokenText.text = $"{current}";
        UpdateButtonColor(current);
    }

    void Refresh()
    {
        int current = ResourceManager.Instance.GetCurrent(tokenType);
        tokenText.text = $"{current}";
        UpdateButtonColor(current);
    }

    void UpdateButtonColor(int currentAmount)
    {
        if (buttonBG == null)
            return;

        Color bgColor = currentAmount > 0 ? buttonColor : disabledColor;

        foreach (var img in buttonBG)
        {
            img.color = bgColor;
        }
    }
}
