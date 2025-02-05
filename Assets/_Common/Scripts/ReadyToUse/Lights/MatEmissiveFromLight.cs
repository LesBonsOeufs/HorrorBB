using UnityEngine;

namespace Root
{
    public class MatEmissiveFromLight : MonoBehaviour
    {
        private const string EMISSION_PROPERTY = "_EmissionColor";

        [SerializeField] private Light targetLight;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private int materialIndex;
        [SerializeField] private float emissionMultiplier = 1f;

        private Color initColor;
        private Material targetMaterial;
        private int emissionPropertyId;

        private void Start()
        {
            emissionPropertyId = Shader.PropertyToID(EMISSION_PROPERTY);

            if (targetRenderer == null)
                targetRenderer = GetComponent<Renderer>();

            if (targetRenderer != null)
            {
                targetMaterial = targetRenderer.materials[materialIndex];
                initColor = targetMaterial.GetColor(emissionPropertyId);
            }
            else
                Debug.LogError("No renderer found. Please assign a renderer in the inspector or attach this script to an object with a renderer.");

            if (targetLight == null)
                Debug.LogError("No light assigned. Please assign a light in the inspector.");
        }

        private void Update()
        {
            if (targetLight != null && targetMaterial != null)
            {
                Color lEmissionColor = initColor * targetLight.intensity * emissionMultiplier;
                targetMaterial.SetColor(emissionPropertyId, lEmissionColor);
            }
        }
    }
}
