using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colors
{
	public static Color[] colors = new Color[10];
	
	public static void GenerateColors()
	{
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = Color.HSVToRGB((((float)360/colors.Length)*i)/360, 0.7f, 1);
		}
	}
}
