using DistantLands.Cozy;
using System.Linq;
using UnityEngine;

public class AnimalOutline : MonoBehaviour
{
    private SkinnedMeshRenderer outlineSmr;

    void Awake()
    {
        GameObject child = FindDeepChild(transform, "Outline_Shell");
        if (child != null)
            outlineSmr = child.transform.GetComponent<SkinnedMeshRenderer>();

        SetOutlineVisible(false);
    }

    GameObject FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.name == name) return child.gameObject;
            var result = FindDeepChild(child.transform, name);
            if (result != null) return result;
        }
        return null;
    }

    public void SetOutlineVisible(bool visible)
    {
        if (outlineSmr != null)
            outlineSmr.enabled = visible;
    }
}
