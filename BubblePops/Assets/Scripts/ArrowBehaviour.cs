using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour
{
	public GhostBubbleBehaviour _ghostBubble;

	LineRenderer _line;
	Vector2Int _ghostBubbleGridPosition;
	Vector2Int _ghostBubbleAltGridPosition;
	bool _ghostBubbleHasOffset;
	bool _ghostBubbleAltHasOffset;

	int lastTouchCount;

	void Start()
	{
		_line = GetComponent<LineRenderer>();
	}

	void Update()
	{
		if (((lastTouchCount > 0 && Input.touchCount == 0) || Input.GetMouseButtonUp(0)) && GameController._gameController.CanShoot() && _ghostBubble.isShown)
		{
			if (GameController._gameController.GetBubbleAtPosition(_ghostBubbleGridPosition) == null)
			{
				GameController._gameController.ShootBubble(_ghostBubbleGridPosition, _ghostBubbleHasOffset);
			}
			else
			{
				GameController._gameController.ShootBubble(_ghostBubbleAltGridPosition, _ghostBubbleAltHasOffset);
			}
		}
		lastTouchCount = Input.touchCount;
	}

	void FixedUpdate()
	{
		if (Input.touchCount > 0 || Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
		{
			Vector2 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition);
			RaycastHit2D hit1 = Physics2D.Raycast(_line.GetPosition(0), worldMousePosition - (Vector2)_line.GetPosition(0));
			if (hit1.collider != null)
			{
				_line.SetPosition(1, hit1.point);
				if (hit1.collider.tag == "Wall")
				{
					RaycastHit2D hit2 = Physics2D.Raycast(_line.GetPosition(1), Vector2.Reflect(worldMousePosition - (Vector2)_line.GetPosition(0), Vector2.right), 10, ~LayerMask.GetMask("Wall"));
					if (hit2.collider != null)
					{
						_line.enabled = true;
						_line.SetPosition(2, hit2.point);
						MoveGhostBubble(hit2);
					}
					else
					{
						_line.enabled = false;
						_ghostBubble.Hide();
						_line.SetPosition(2, hit1.point);
					}
				}
				else
				{
					_line.enabled = true;
					_line.SetPosition(2, hit1.point);
					_line.SetPosition(1, (_line.GetPosition(2) + _line.GetPosition(0)) / 2);
					MoveGhostBubble(hit1);
				}
			}
			else
			{
				_line.enabled = false;
				_ghostBubble.Hide();
				_line.SetPosition(2, worldMousePosition);
				_line.SetPosition(1, (_line.GetPosition(2) + _line.GetPosition(0)) / 2);
			}
			if (GameController._gameController._isMovingBubbles)
			{
				_ghostBubble.Hide();
			}
		}
		else
		{
			_ghostBubble.Hide();
			_line.enabled = false;
		}
	}

	void MoveGhostBubble(RaycastHit2D hit)
	{
		Vector2 hitBubblePosition = hit.collider.transform.position;
		BubbleBehaviour hitBubble = hit.collider.GetComponent<BubbleBehaviour>();
		Vector2 direction = (hit.point - (hitBubblePosition)).normalized;

		int ghostBubbleXPosition;
		if (direction.y < 0)
		{
			ghostBubbleXPosition = (direction.x < 0 ? 0 : 1) + (hitBubble._hasOffset ? 0 : -1);
		}
		else
		{
			ghostBubbleXPosition = direction.x < 0 ? -1 : 1;
		}
		_ghostBubbleGridPosition = hitBubble._positionOnGrid + new Vector2Int(ghostBubbleXPosition, direction.y < 0 ? -1 : 0);
		_ghostBubbleHasOffset = direction.y < 0 ? !hitBubble._hasOffset : hitBubble._hasOffset;

		int ghostBubbleAltXPosition;
		if (direction.y < -(Mathf.Sqrt(2) / 2))
		{
			ghostBubbleAltXPosition = (direction.x < 0 ? 1 : 0) + (hitBubble._hasOffset ? 0 : -1);
		}
		else
		{
			ghostBubbleAltXPosition = direction.x < 0 ? -1 : 1;
		}
		_ghostBubbleAltGridPosition = hitBubble._positionOnGrid + new Vector2Int(ghostBubbleAltXPosition, direction.y < -(Mathf.Sqrt(2) / 2) ? -1 : 0);
		_ghostBubbleAltHasOffset = direction.y < -(Mathf.Sqrt(2) / 2) ? !hitBubble._hasOffset : hitBubble._hasOffset;

		//Debug.DrawLine(hitBubblePosition, hit.point, Color.green);
		//DebugExtensions.DrawCircle(_ghostBubbleGridPosition * new Vector2(1, GameController._yDistanceBetweenCircles) + new Vector2(_ghostBubbleHasOffset ? 0.5f : 0, 0), .5f, Color.blue, 0);
		//DebugExtensions.DrawCircle(_ghostBubbleAltGridPosition * new Vector2(1, GameController._yDistanceBetweenCircles) + new Vector2(_ghostBubbleAltHasOffset ? 0.5f : 0, 0), .4f, Color.red, 0);

		if (GameController._gameController.GetBubbleAtPosition(_ghostBubbleGridPosition) == null)
		{
			if (GameController._gameController.IsInsideBoard(_ghostBubbleGridPosition))
			{
				_ghostBubble.Show(_ghostBubbleGridPosition * new Vector2(1, GameController._yDistanceBetweenCircles) + new Vector2(_ghostBubbleHasOffset ? 0.5f : 0, 0));
			}
			else
			{
				_ghostBubble.Hide();
			}
		}
		else
		{
			if (GameController._gameController.IsInsideBoard(_ghostBubbleAltGridPosition))
			{
				_ghostBubble.Show(_ghostBubbleAltGridPosition * new Vector2(1, GameController._yDistanceBetweenCircles) + new Vector2(_ghostBubbleAltHasOffset ? 0.5f : 0, 0));
			}
			else
			{
				_ghostBubble.Hide();
			}
		}
	}

	public Vector3 GetPosition(int index)
	{
		return _line.GetPosition(index);
	}
}
