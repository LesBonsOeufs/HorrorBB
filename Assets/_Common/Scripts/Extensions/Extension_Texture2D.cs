using UnityEngine;

public static class Extension_Texture2D
{
    public static Texture2D GetSubdivision(this Texture2D texture2D, Vector2Int pGridSize, Vector2Int pPos)
    {
        pGridSize = Vector2Int.Max(pGridSize, new Vector2Int(1, 1));

        uint lPosX = (uint)Mathf.Clamp(pPos.x, 0, pGridSize.x - 1);
        uint lPosY = (uint)Mathf.Clamp(pPos.y, 0, pGridSize.y - 1);

        // Invert Y axis
        lPosY = (uint)(pGridSize.y - lPosY - 1);

        if (!texture2D.isReadable)
        {
            Debug.LogError("Image is not readable");
            return null;
        }

        Texture2D lTexture = new Texture2D(Mathf.CeilToInt(texture2D.width / pGridSize.x), Mathf.CeilToInt(texture2D.height / pGridSize.y));
        lTexture.SetPixels(texture2D.GetPixels((int)(lPosX * lTexture.width), (int)(lPosY * lTexture.height), lTexture.width, lTexture.height));
        lTexture.wrapMode = TextureWrapMode.Clamp;
        lTexture.filterMode = texture2D.filterMode;
        lTexture.anisoLevel = texture2D.anisoLevel;
        lTexture.Apply();

        return lTexture;
    }
}