using System;

public static class Extension_Array
{
    /// <summary>
    /// Shuffles array using the Fisher-Yates algorithm
    /// </summary>
    public static void Shuffle<T>(this T[] array)
    {
        Random lRandom = new();

        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = lRandom.Next(i, array.Length);
            // Swap elements
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }
}