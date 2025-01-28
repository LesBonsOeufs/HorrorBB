using DG.Tweening;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class DynamicDitherObject : MonoBehaviour
{
    private const float ACTIVE_ALPHA_VALUE = 0f;//.4f;
    private const float ALPHA_TWEEN_DURATION = .4f;

    [SerializeField, ReadOnly] private MeshRenderer[] meshRenderers;

    private readonly List<Material> dithers = new();
    private readonly List<Material> materials = new();

    public bool IsActive
    {
        get => _isActive;

        set
        {
            if (_isActive == value)
                return;

            _isActive = value;
            this.DOKill();
                
            foreach (MeshRenderer lMeshRenderer in meshRenderers)
            {
                Material[] lSharedMats = lMeshRenderer.sharedMaterials;
                Material lSharedMat;

                for (int i = 0; i < lSharedMats.Length; i++)
                {
                    lSharedMat = lSharedMats[i];

                    if (_isActive)
                    {
                        int lMatIndex = materials.IndexOf(lSharedMat);

                        //If not see through
                        if (lMatIndex != -1)
                        {
                            lSharedMat = dithers[lMatIndex];
                            lSharedMats[i] = lSharedMat;
                        }

                        Fade(lSharedMat, _isActive);
                    }
                    else
                    {
                        int lDitherIndex = dithers.IndexOf(lSharedMat);

                        if (lDitherIndex != -1)
                        {
                            Fade(lSharedMat, _isActive);
                            lSharedMat = materials[lDitherIndex];
                            lSharedMats[i] = lSharedMat;
                        }
                    }
                }

                if (_isActive)
                    lMeshRenderer.sharedMaterials = lSharedMats;
                else
                {
                    DOVirtual.DelayedCall(ALPHA_TWEEN_DURATION, () => lMeshRenderer.sharedMaterials = lSharedMats, false)
                        .SetTarget(this);
                }

                if (ACTIVE_ALPHA_VALUE == 0f)
                {
                    if (_isActive)
                    {
                        DOVirtual.DelayedCall(ALPHA_TWEEN_DURATION, () => SetVisible(false), false)
                            .SetTarget(this);
                    }
                    else
                        SetVisible(true);
                }
            }
        }
    }
    private bool _isActive = false;

    private void Awake()
    {
        Init();
    }

#if UNITY_EDITOR

    [Button]
    private void TestToggle() => IsActive = !IsActive;

#endif

    private void Init()
    {
        foreach (MeshRenderer lMeshRenderer in meshRenderers)
        {
            foreach (Material lMaterial in lMeshRenderer.sharedMaterials)
            {
                if (!DynamicDitherManager.Instance.ShadersToDither.ContainsKey(lMaterial.shader) || materials.Contains(lMaterial))
                    continue;

                dithers.Add(DynamicDitherManager.Instance.MakeSeeThroughVersion(lMaterial));
                materials.Add(lMaterial);
            }
        }
    }

    private void Fade(Material material, bool active)
    {
        material.DOKill();
        material.DOFloat(active ? ACTIVE_ALPHA_VALUE : 1f, DynamicDitherManager.Instance.AlphaPropertyId, ALPHA_TWEEN_DURATION);
    }

    private void SetVisible(bool value)
    {
        foreach (MeshRenderer lMeshRenderer in meshRenderers)
            lMeshRenderer.enabled = value;
    }

    [Button]
    private void RetrieveMeshRenderers()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    private void OnValidate()
    {
        RetrieveMeshRenderers();
    }
}