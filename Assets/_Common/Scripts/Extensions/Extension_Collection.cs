using System.Collections.Generic;
using System.Linq;

public static class Extension_Collection
{
	public static bool Contain<T>(this T[] array, T toFind)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Equals(toFind)) { return true; }
		}
		return false;
	}

	public static T Last<T>(this T[] array) { return array[array.Length - 1]; }
	public static T Last<T>(this List<T> list) { return list[list.Count - 1]; }

	public static T FromEnd<T>(this T[] array, int nbr) { return array[array.Length - (1 + nbr)]; }
	public static T FromEnd<T>(this List<T> list, int nbr) { return list[list.Count - (1 + nbr)]; }

	public static T Random<T>(this T[] array) { return array[UnityEngine.Random.Range(0, array.Length)]; }
	public static T Random<T>(this List<T> list) { return list[UnityEngine.Random.Range(0, list.Count)]; }

	public static T[] ShufleRandom<T>(this T[] array)
	{
		var result = new T[array.Length];
		var available = array.ToList();

		for (int i = 0; i < result.Length; i++)
		{
			result[i] = available.Random();
			available.Remove(result[i]);
		}

		return result;
	}

}
