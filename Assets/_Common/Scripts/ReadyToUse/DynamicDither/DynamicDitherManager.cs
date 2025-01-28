using AYellowpaper.SerializedCollections;
using UnityEngine;

public class DynamicDitherManager : Singleton<DynamicDitherManager>
{
    private const string ALPHA_PROPERTY = "_Alpha";

    public int AlphaPropertyId { get; private set; }

    [field: SerializeField] public SerializedDictionary<Shader, Shader> ShadersToDither { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        AlphaPropertyId = Shader.PropertyToID(ALPHA_PROPERTY);
    }

    public Material MakeSeeThroughVersion(Material material)
    {
        Material lSeeThroughMat = new Material(ShadersToDither[material.shader]);
        lSeeThroughMat.CopyPropertiesFromMaterial(material);
        lSeeThroughMat.SetFloat(ALPHA_PROPERTY, 1f);

        return lSeeThroughMat;
    }
}