using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.spacepuppy.Tween;
using TMPro;

public class BubbleBehaviour : MonoBehaviour
{
	protected SpriteRenderer _spriteRenderer;
	public Color _color;
	public TextMeshPro _exponentText;

	public bool _hasOffset;
	public Vector2Int _positionOnGrid;
	public uint _exponent;

	void Start()
	{
		Setup();
	}

	public void Refresh()
	{
		Setup();
	}

	protected void Setup()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();

		_color = Colors.colors[_exponent % 10];
		_spriteRenderer.color = _color;
		_exponentText.text = Mathf.Pow(2, _exponent).ToString();
	}

	public void MoveToNewPosition(Vector2Int newPosition)
	{
		if (gameObject.activeInHierarchy)
			StartCoroutine(MoveToNewPositionRoutine(newPosition));
	}

	IEnumerator MoveToNewPositionRoutine(Vector2Int newPosition)
	{
		GameController._gameController._isMovingBubbles = true;
		_positionOnGrid = newPosition;
		Vector2 startingPosition = transform.position;
		Vector2 finishingPosition = new Vector2(transform.position.x, newPosition.y * GameController._yDistanceBetweenCircles);
		for (float i = 0; i < 1; i += Time.fixedDeltaTime * 2)
		{
			transform.position = EaseMethods.EaseVector2(EaseMethods.GetEase(EaseStyle.BackEaseInOut), startingPosition, finishingPosition, i, 1);
			yield return new WaitForFixedUpdate();
		}
		transform.position = finishingPosition;
		GameController._gameController._isMovingBubbles = false;
	}

	public void CopyValuesFromBubble(BubbleBehaviour original)
	{
		_hasOffset = original._hasOffset;
		_positionOnGrid = original._positionOnGrid;
		_exponent = original._exponent;
		_color = original._color;

		Setup();
	}

	public void Push(Vector3 origin)
	{
		if (gameObject.activeInHierarchy)
			StartCoroutine(PushRoutine(origin));
	}

	IEnumerator PushRoutine(Vector3 origin)
	{
		Vector2 startingPosition = transform.position;
		Vector2 finishingPosition = transform.position + ((transform.position - origin).normalized) * 0.2f;
		for (float i = 0; i < 0.2f; i += Time.fixedDeltaTime * 2)
		{
			transform.position = EaseMethods.EaseVector2(EaseMethods.GetEase(EaseStyle.SineEaseOut), startingPosition, finishingPosition, i, 0.2f);
			yield return new WaitForFixedUpdate();
		}
		for (float i = 0; i < 0.2f; i += Time.fixedDeltaTime * 2)
		{
			transform.position = EaseMethods.EaseVector2(EaseMethods.GetEase(EaseStyle.SineEaseInOut), finishingPosition, startingPosition, i, 0.2f);
			yield return new WaitForFixedUpdate();
		}
			transform.position = startingPosition;
	}
}
