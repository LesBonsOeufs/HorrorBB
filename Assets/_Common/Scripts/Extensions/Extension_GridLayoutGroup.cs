using System;
using UnityEngine;
using UnityEngine.UI;

public static class Extension_GridLayoutGroup
{
    /// <summary>
    /// Forces the update of all content positions
    /// </summary>
    public static void Refresh(this GridLayoutGroup gridLayout)
    {
        gridLayout.CalculateLayoutInputHorizontal();
        gridLayout.CalculateLayoutInputVertical();
        gridLayout.SetLayoutHorizontal();
        gridLayout.SetLayoutVertical();
    }

    /// <summary>
    /// WARNING! Destroys child transforms!
    /// </summary>
    /// <param name="cellGenerationCallback">Needs to be used for instantiating. Parameters are x & y grid positions.</param>
    public static void FillFromXYCount(this GridLayoutGroup gridLayout, int xCount, int yCount, 
        Action<int, int> cellGenerationCallback)
    {
        for (int i = gridLayout.transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(gridLayout.transform.GetChild(i).gameObject);

        gridLayout.SetCellSizeFromNCells(xCount, yCount);

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < yCount; j++)
                cellGenerationCallback?.Invoke(i, j);
        }
    }

    public static void SetCellSizeFromNCells(this GridLayoutGroup gridLayout, int xCount, int yCount)
    {
        Vector2 lSize = gridLayout.GetComponent<RectTransform>().rect.size;
        Vector2 lCellSize;
        lCellSize.x = CalculateCellSize(lSize.x, gridLayout.spacing.x,
            gridLayout.padding.left + gridLayout.padding.right, xCount);
        lCellSize.y = CalculateCellSize(lSize.y, gridLayout.spacing.y,
            gridLayout.padding.top + gridLayout.padding.bottom, yCount);

        gridLayout.cellSize = lCellSize;
    }

    private static float CalculateCellSize(float gridSize, float spacing, float addedPadding, int count)
    {
        return (gridSize - (spacing * (count - 1) + addedPadding)) / count;
    }
}