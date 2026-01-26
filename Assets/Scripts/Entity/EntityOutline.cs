using DistantLands.Cozy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class EntityOutline : MonoBehaviour
{
    [SerializeField]
    private GameObject outlinePrefab;

    private readonly List<Renderer> outlineRenderers = new List<Renderer>();

    void Awake()
    {
        if (outlinePrefab == null)
            return;

        SetupOutline();
    }

    private void SetupOutline()
    {
        //Handle MeshFilters -> Plants
        MeshFilter[] sourceMeshFilters = GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter sourceMF in sourceMeshFilters)
        {
            GameObject outlineInstance = Instantiate(outlinePrefab, transform);

            MeshFilter outlineMF = outlineInstance.GetComponent<MeshFilter>();
            if (outlineMF == null)
            {
                Destroy(outlineInstance);
                continue;
            }

            outlineMF.sharedMesh = sourceMF.sharedMesh;

            CopyLocalTransform(sourceMF.transform, outlineInstance.transform);

            MeshRenderer mr = outlineInstance.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = false;
                outlineRenderers.Add(mr);
            }
        }

        //Handle SkinnedMeshRenderers -> Animals
        SkinnedMeshRenderer[] sourceSkinnedRenderers =
        GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer sourceSMR in sourceSkinnedRenderers)
        {
            GameObject outlineInstance = Instantiate(outlinePrefab, transform);

            SkinnedMeshRenderer outlineSMR =
                outlineInstance.GetComponent<SkinnedMeshRenderer>();

            if (outlineSMR == null)
            {
                Destroy(outlineInstance);
                continue;
            }

            outlineSMR.sharedMesh = sourceSMR.sharedMesh;
            outlineSMR.bones = sourceSMR.bones;
            outlineSMR.rootBone = sourceSMR.rootBone;

            CopyLocalTransform(sourceSMR.transform, outlineInstance.transform);

            outlineSMR.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            outlineSMR.receiveShadows = false;

            outlineSMR.enabled = false;

            outlineRenderers.Add(outlineSMR);
        }
    }

    private void CopyLocalTransform(Transform srcTransform, Transform dstTransform)
    {
        dstTransform.localPosition = srcTransform.localPosition;
        dstTransform.localRotation = srcTransform.localRotation;
        dstTransform.localScale = srcTransform.localScale;
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
        for (int i = 0; i < outlineRenderers.Count; i++)
        {
            if (outlineRenderers[i] != null)
                outlineRenderers[i].enabled = visible;
        }
    }
}
