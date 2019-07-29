using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugExtensions
{
	public static void DrawCircle(Vector2 position, float radius, Color color, float duration = 0)
	{
		var segments = 360;

		var pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
		var points = new Vector3[pointCount];
		
		var rad1 = Mathf.Deg2Rad * (0 * 360f / segments);
		points[0] = new Vector3(Mathf.Sin(rad1) * radius + position.x, Mathf.Cos(rad1) * radius + position.y, 0);

		for (int i = 1; i < pointCount; i++)
		{
			var rad = Mathf.Deg2Rad * (i * 360f / segments);
			points[i] = new Vector3(Mathf.Sin(rad) * radius + position.x, Mathf.Cos(rad) * radius + position.y, 0);
			Debug.DrawLine(points[i - 1], points[i], color, duration);
		}
	}
}
