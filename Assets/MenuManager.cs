using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    private readonly HashSet<IMenu> openMenus = new();
    private IMenu activeMenu;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RequestOpen(IMenu menu)
    {
        if (activeMenu == menu)
            return;

        CloseAll();
        activeMenu = menu;
        openMenus.Add(menu);
        menu.OpenInternal();
    }

    public void RequestClose(IMenu menu)
    {
        if (activeMenu == menu)
            activeMenu = null;

        openMenus.Remove(menu);
        menu.CloseInternal();
    }

    public void CloseAll()
    {
        foreach (var menu in openMenus)
            menu.CloseInternal();

        openMenus.Clear();
        activeMenu = null;
    }

    public bool IsAnyMenuOpen() => activeMenu != null;
}
