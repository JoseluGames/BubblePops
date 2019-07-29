using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JLGMHelper
{
	public static bool IsInsideBounds<T>(Vector2Int position, T[,] array)
	{
		return position.x >= 0 && position.x < array.GetLength(0) && position.y >= 0 && position.y < array.GetLength(1);
	}

	public static bool IsInsideBounds<T>(int position, T[] array)
	{
		return position >= 0 && position < array.GetLength(0);
	}

	public static double Clamp01(double value)
	{
		if (value < 0)
			return 0;
		else if (value > 1)
			return 1;
		else
			return value;
	}
}
