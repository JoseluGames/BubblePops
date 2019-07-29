using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.spacepuppy.Tween;

public class GameController : MonoBehaviour
{
	public static GameController _gameController;
	public const float _yDistanceBetweenCircles = 0.86602540378f;//0.5f*Mathf.Sqrt(3);
	public ObjectsPool pool;

	public bool _isMovingBubbles;
	public ArrowBehaviour arrow;
	public ScoreBehaviour score;

	private bool _isShooting;
	private bool _isMerging;
	private BubbleBehaviour[,] _bubblesGrid = new BubbleBehaviour[6, 10];
	private GameObject _currentBubble;
	private GameObject _nextBubble;
	private List<Vector2Int> _checkedPositions;
	private List<MovingBubble> _movingBubbles = new List<MovingBubble>();
	private bool _lastRowHasOffset;

	private uint _baseExponent = 1;

	private Vector2Int[] positionsToCheckWithoutOffset = new Vector2Int[]
	{
		new Vector2Int(-1, 1),
		new Vector2Int(0, 1),
		new Vector2Int(-1, 0),
		new Vector2Int(1, 0),
		new Vector2Int(-1, -1),
		new Vector2Int(0, -1)
	};

	private Vector2Int[] positionsToCheckWithOffset = new Vector2Int[]
	{
		new Vector2Int(0, 1),
		new Vector2Int(1, 1),
		new Vector2Int(-1, 0),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(1, -1)
	};

	void Awake()
	{
		_gameController = this;
		Colors.GenerateColors();
	}

	void Start()
	{
		SetupStartingBubbles();
		StartCoroutine(FloatingBubblesFinderRoutine());
	}

	void FixedUpdate()
	{
		UpdateMovingBubbles();
	}

	void SetupStartingBubbles()
	{
		for (int x = 0; x < _bubblesGrid.GetLength(0); x++)
		{
			for (int y = _bubblesGrid.GetLength(1) - 1; y >= 4; y--)
			{
				InstantiateBubbleInGrid(new Vector2Int(x, y), y % 2 == 0, GetRandomExponent());
			}
		}

		_currentBubble = pool.CreateBubble(new Vector3(2.75f, -3)).gameObject;
		_currentBubble.GetComponent<BubbleBehaviour>()._exponent = GetRandomExponent();
		_currentBubble.GetComponent<BubbleBehaviour>().Refresh();
		_currentBubble.GetComponent<CircleCollider2D>().enabled = false;

		_nextBubble = pool.CreateBubble(new Vector3(1.5f, -3)).gameObject;
		_nextBubble.transform.localScale = new Vector3(0.75f, 0.75f, 1);
		_nextBubble.GetComponent<BubbleBehaviour>()._exponent = GetRandomExponent();
		_nextBubble.GetComponent<BubbleBehaviour>().Refresh();
		_nextBubble.GetComponent<CircleCollider2D>().enabled = false;
	}

	public void ShootBubble(Vector2Int position, bool hasOffset)
	{
		if (IsInsideBoard(position))
		{
			StartCoroutine(ShootBubbleRoutine(position, hasOffset));
		}
	}

	IEnumerator FloatingBubblesFinderRoutine()
	{
		while (true)
		{
			if (!_isMerging)
				FindFloatingBubbles();
			yield return new WaitForFixedUpdate();
		}
	}

	IEnumerator ShootBubbleRoutine(Vector2Int position, bool hasOffset)
	{
		_isShooting = true;

		//Shoot bubble to position
		Vector2 startingPosition = _currentBubble.transform.position;
		Vector2 finishingPosition = new Vector2(position.x + (hasOffset ? 0.5f : 0), position.y * GameController._yDistanceBetweenCircles);

		Vector3 arrowMidPoint = arrow.GetPosition(1);
		float lengthOfFirstSegment = Vector3.Distance(arrow.GetPosition(0), arrow.GetPosition(1));
		float lengthOfSecondSegment = Vector3.Distance(arrow.GetPosition(1), finishingPosition);
		float timeToCompleteFirstSegment = lengthOfFirstSegment / (lengthOfFirstSegment + lengthOfSecondSegment);
		float timeToCompleteSecondSegment = 1 - timeToCompleteFirstSegment;

		float stepSize = Time.fixedDeltaTime * 5;
		for (float i = 0; i < timeToCompleteFirstSegment; i += stepSize)
		{
			_currentBubble.transform.position = EaseMethods.EaseVector2(EaseMethods.GetEase(EaseStyle.LinearEaseInOut), startingPosition, arrowMidPoint, i, timeToCompleteFirstSegment);
			yield return new WaitForFixedUpdate();
		}
		for (float i = 0; i < timeToCompleteSecondSegment; i += stepSize)
		{
			_currentBubble.transform.position = EaseMethods.EaseVector2(EaseMethods.GetEase(EaseStyle.SineEaseOut), arrowMidPoint, finishingPosition, i, timeToCompleteSecondSegment);
			yield return new WaitForFixedUpdate();
		}

		//Place bubble on grid
		_currentBubble.transform.position = finishingPosition;
		_currentBubble.GetComponent<CircleCollider2D>().enabled = true;
		BubbleBehaviour currentBubbleComponent = _currentBubble.GetComponent<BubbleBehaviour>();
		_bubblesGrid[position.x, position.y] = currentBubbleComponent;
		currentBubbleComponent._hasOffset = hasOffset;
		currentBubbleComponent._positionOnGrid = position;

		Vibration.VibrateGlobal(75);
		foreach (var bubble in GetSurroundingBubbles(currentBubbleComponent._positionOnGrid, true))
		{
			bubble.Push(currentBubbleComponent.transform.position);
		}

		//Try mergin bubbles;
		Vector2Int lastMergedBubblePosition;
		Vector2Int newMergedBubblePosition = position;
		int comboSize = 0;
		do
		{
			lastMergedBubblePosition = newMergedBubblePosition;
			newMergedBubblePosition = MergeBubbles(newMergedBubblePosition);
			Vibration.VibrateGlobal(75);
			yield return new WaitWhile(() => _movingBubbles.Count > 0);
			comboSize++;
		} while (!newMergedBubblePosition.Equals(new Vector2Int(-1, -1)));

		//Explode if more than 2048
		if (GetBubbleAtPosition(lastMergedBubblePosition)._exponent > 10)
		{
			ExplodeAround(lastMergedBubblePosition, 2 + (int)(GetBubbleAtPosition(lastMergedBubblePosition)._exponent - 10));
		}

		yield return new WaitForSeconds(0.2f);

		bool isAllEmpty = true;

		for (int y = 0; y < _bubblesGrid.GetLength(1); y++)
		{
			if (!IsRowEmpty(y))
			{
				isAllEmpty = false;
				break;
			}
		}

		if (isAllEmpty)
		{
			pool.CreatePerfectText();
			Vibration.VibrateGlobal(300);
		}

		if (position.y == 0)
		{
			MoveAllBubblesUp();
		}

		if (IsRowEmpty(1))
		{
			MoveAllBubblesDown();
		}

		if (IsRowEmpty(2))
		{
			MoveAllBubblesDown();
		}

		if (IsRowEmpty(3))
		{
			MoveAllBubblesDown();
		}

		if (IsRowEmpty(4))
		{
			MoveAllBubblesDown();
		}

		Vector3 currentScale = _nextBubble.transform.localScale;
		startingPosition = _nextBubble.transform.position;
		for (float i = 0; i < 1; i += Time.fixedDeltaTime * 5)
		{
			_nextBubble.transform.localScale = EaseMethods.EaseVector3(EaseMethods.GetEase(EaseStyle.BackEaseOut), currentScale, new Vector3(1, 1, 1), i, 1);
			_nextBubble.transform.position = EaseMethods.EaseVector2(EaseMethods.GetEase(EaseStyle.SineEaseOut), startingPosition, new Vector2(2.75f, -3), i, 1);
			yield return new WaitForFixedUpdate();
		}
		_nextBubble.transform.localScale = new Vector3(1, 1, 1);
		_nextBubble.transform.position = new Vector2(2.75f, -3);
		_currentBubble = _nextBubble;
		_nextBubble = pool.CreateBubble(new Vector3(1.5f, -3)).gameObject;
		_nextBubble.transform.localScale = new Vector3(0.75f, 0.75f, 1);
		_nextBubble.GetComponent<BubbleBehaviour>()._exponent = GetRandomExponent();
		_nextBubble.GetComponent<BubbleBehaviour>().Refresh();
		_nextBubble.GetComponent<CircleCollider2D>().enabled = false;
		_isShooting = false;
	}

	public void InstantiateBubbleInGrid(Vector2Int position, bool hasOffset, uint exponent)
	{
		if (IsInsideBoard(position))
		{
			BubbleBehaviour bubbleComponent = pool.CreateBubble(position * new Vector2(1, _yDistanceBetweenCircles) + new Vector2(hasOffset ? 0.5f : 0, 0));
			bubbleComponent._hasOffset = hasOffset;
			bubbleComponent._positionOnGrid = position;
			bubbleComponent._exponent = exponent;
			bubbleComponent.Refresh();
			_bubblesGrid[position.x, position.y] = bubbleComponent;
		}
	}

	public void MoveAllBubblesUp()
	{
		for (int y = _bubblesGrid.GetLength(1) - 2; y >= 0; y--)
		{
			for (int x = 0; x < _bubblesGrid.GetLength(0); x++)
			{
				if (_bubblesGrid[x, y + 1] != null)
				{
					DestroyBubble(new Vector2Int(x, y + 1), false);
				}

				if (_bubblesGrid[x, y] != null)
				{
					_bubblesGrid[x, y + 1] = _bubblesGrid[x, y];
					_bubblesGrid[x, y + 1].GetComponent<BubbleBehaviour>().MoveToNewPosition(new Vector2Int(x, y + 1));
					_bubblesGrid[x, y] = null;
				}
			}
		}
	}

	public bool CanMoveDown()
	{
		bool isSecondLastRowEmpty = true;
		for (int x = 0; x < _bubblesGrid.GetLength(0); x++)
		{
			BubbleBehaviour bubble = GetBubbleAtPosition(new Vector2Int(x, 1));
			if (bubble != null)
			{
				isSecondLastRowEmpty = false;
			}
		}

		return isSecondLastRowEmpty;
	}

	public bool IsRowEmpty(int y)
	{
		bool isRowEmpty = true;
		for (int x = 0; x < _bubblesGrid.GetLength(0); x++)
		{
			BubbleBehaviour bubble = GetBubbleAtPosition(new Vector2Int(x, y));
			if (bubble != null)
			{
				isRowEmpty = false;
			}
		}

		return isRowEmpty;
	}

	public void MoveAllBubblesDown()
	{
		for (int y = 1; y < _bubblesGrid.GetLength(1); y++)
		{
			for (int x = 0; x < _bubblesGrid.GetLength(0); x++)
			{
				if (_bubblesGrid[x, y - 1] != null)
				{
					DestroyBubble(new Vector2Int(x, y - 1), false);
				}

				if (_bubblesGrid[x, y] != null)
				{
					_bubblesGrid[x, y - 1] = _bubblesGrid[x, y];
					_bubblesGrid[x, y - 1].GetComponent<BubbleBehaviour>().MoveToNewPosition(new Vector2Int(x, y - 1));
					_bubblesGrid[x, y] = null;
				}
			}
		}
		for (int x = 0; x < _bubblesGrid.GetLength(0); x++)
		{
			BubbleBehaviour bubble = GetBubbleAtPosition(new Vector2Int(x, _bubblesGrid.GetLength(1) - 2));
			if (bubble != null)
			{
				_lastRowHasOffset = bubble._hasOffset;
			}
		}

		for (int x = 0; x < _bubblesGrid.GetLength(0); x++)
		{
			InstantiateBubbleInGrid(new Vector2Int(x, _bubblesGrid.GetLength(1) - 1), !_lastRowHasOffset, GetRandomExponent());
		}
	}

	public BubbleBehaviour GetBubbleAtPosition(Vector2Int position)
	{
		if (position.x >= 0 && position.y >= 0 && position.x < _bubblesGrid.GetLength(0) && position.y < _bubblesGrid.GetLength(1))
		{
			return _bubblesGrid[position.x, position.y];
		}
		else
		{
			return null;
		}
	}

	public bool IsInsideBoard(Vector2Int position)
	{
		return JLGMHelper.IsInsideBounds(position, _bubblesGrid);
	}

	public bool CanShoot()
	{
		return !_isShooting && !_isMovingBubbles;
	}

	public void FindFloatingBubbles()
	{
		_checkedPositions = new List<Vector2Int>();
		for (int x = 0; x < _bubblesGrid.GetLength(0); x++)
		{
			for (int y = 0; y < _bubblesGrid.GetLength(1); y++)
			{
				if (GetBubbleAtPosition(new Vector2Int(x, y)) == null)
				{
					continue;
				}
				List<BubbleBehaviour> bubblesCluster = GetCluster(new Vector2Int(x, y), false);
				if (bubblesCluster.Count <= 0)
				{
					continue;
				}

				bool floating = true;
				foreach (var bubble in bubblesCluster)
				{
					if (bubble._positionOnGrid.y == _bubblesGrid.GetLength(1) - 1)
					{
						floating = false;
						break;
					}
				}

				if (floating)
				{
					foreach (var bubble in bubblesCluster)
					{
						FallBubble(bubble._positionOnGrid);
					}
				}
			}
		}
	}

	public Vector2Int MergeBubbles(Vector2Int startingPosition)
	{
		_isMerging = true;
		_checkedPositions = new List<Vector2Int>();
		List<BubbleBehaviour> bestPossibleMerges = new List<BubbleBehaviour>();
		List<BubbleBehaviour> bubblesCluster = GetCluster(startingPosition, true);
		bool newMergedBubbleBubbleHasOffset;
		uint newMergedBubbleExponent;
		Vector2Int newMergedBubblePosition;

		_checkedPositions = new List<Vector2Int>();
		foreach (var bubble in bubblesCluster)
		{
			if (bubble != null)
			{
				bestPossibleMerges.AddRange(GetSurroundingBubbles(bubble._positionOnGrid, false));
			}
		}

		int bestPossibleBubbleIndex = -1;

		if (bestPossibleMerges.Count > 1)
		{
			bestPossibleMerges.Sort((bubble1, bubble2) => bubble2._exponent.CompareTo(bubble1._exponent));
			bestPossibleBubbleIndex = bestPossibleMerges.FindIndex((bubble) => bubble._exponent == (uint)(bestPossibleMerges[0]._exponent + bubblesCluster.Count - 1));
		}

		if (bestPossibleBubbleIndex != -1)
		{
			newMergedBubbleBubbleHasOffset = bestPossibleMerges[0]._hasOffset;
			newMergedBubbleExponent = (uint)(bestPossibleMerges[0]._exponent + bubblesCluster.Count - 1);
			newMergedBubblePosition = bestPossibleMerges[0]._positionOnGrid;
		}
		else
		{
			bubblesCluster.Sort((bubble1, bubble2) => bubble2._positionOnGrid.y.CompareTo(bubble1._positionOnGrid.y));
			newMergedBubbleBubbleHasOffset = bubblesCluster[0]._hasOffset;
			newMergedBubbleExponent = (uint)(bubblesCluster[0]._exponent + bubblesCluster.Count - 1);
			newMergedBubblePosition = bubblesCluster[0]._positionOnGrid;
		}

		if (bubblesCluster.Count > 1)
		{
			foreach (var bubble in bubblesCluster)
			{
				VisualMergeBubble(bubble._positionOnGrid, newMergedBubblePosition);
			}
			foreach (var bubble in bubblesCluster)
			{
				DestroyBubble(bubble._positionOnGrid, true);
			}

			score.AddToScore(newMergedBubbleExponent);
			InstantiateBubbleInGrid(newMergedBubblePosition, newMergedBubbleBubbleHasOffset, newMergedBubbleExponent);
			return newMergedBubblePosition;
		}
		_isMerging = false;
		return new Vector2Int(-1, -1);
	}

	public List<BubbleBehaviour> GetCluster(Vector2Int startingPosition, bool matchExponent)
	{
		List<BubbleBehaviour> bubblesCluster = new List<BubbleBehaviour>();

		if (_checkedPositions.Contains(startingPosition))
		{
			return bubblesCluster;
		}
		_checkedPositions.Add(startingPosition);

		bubblesCluster.Add(GetBubbleAtPosition(startingPosition));

		for (int currentBubbleID = 0; currentBubbleID < bubblesCluster.Count; currentBubbleID++)
		{
			BubbleBehaviour currentBubble = bubblesCluster[currentBubbleID];
			if (currentBubble == null)
			{
				continue;
			}
			foreach (var surroundingPos in currentBubble._hasOffset ? positionsToCheckWithOffset : positionsToCheckWithoutOffset)
			{
				Vector2Int searchingPosition = currentBubble._positionOnGrid + surroundingPos;

				if (_checkedPositions.Contains(searchingPosition))
				{
					continue;
				}
				_checkedPositions.Add(searchingPosition);

				//DebugExtensions.DrawCircle(searchingPosition * new Vector2(1, _yDistanceBetweenCircles) + new Vector2((surroundingPos.y == 0 ? currentBubble._hasOffset : !currentBubble._hasOffset) ? 0.5f : 0, 0), .4f, Color.blue, .1f);

				if (!IsInsideBoard(searchingPosition))
				{
					continue;
				}

				BubbleBehaviour surroundingBubble = GetBubbleAtPosition(searchingPosition);

				if (surroundingBubble == null)
				{
					continue;
				}

				if (matchExponent)
				{
					if (surroundingBubble._exponent == currentBubble._exponent)
					{
						bubblesCluster.Add(surroundingBubble);
					}
				}
				else
				{
					bubblesCluster.Add(surroundingBubble);
				}
			}
		}

		return bubblesCluster;
	}

	public List<BubbleBehaviour> GetSurroundingBubbles(Vector2Int position, bool cleanCheckedPositions)
	{
		if (cleanCheckedPositions)
		{
			_checkedPositions = new List<Vector2Int>();
		}
		List<BubbleBehaviour> surroundingBubbles = new List<BubbleBehaviour>();
		BubbleBehaviour currentBubble = GetBubbleAtPosition(position);
		if (currentBubble == null)
		{
			return surroundingBubbles;
		}
		foreach (var surroundingPos in currentBubble._hasOffset ? positionsToCheckWithOffset : positionsToCheckWithoutOffset)
		{
			Vector2Int searchingPosition = currentBubble._positionOnGrid + surroundingPos;

			if (_checkedPositions.Contains(searchingPosition))
			{
				continue;
			}
			_checkedPositions.Add(searchingPosition);

			if (!IsInsideBoard(searchingPosition))
			{
				continue;
			}

			BubbleBehaviour surroundingBubble = GetBubbleAtPosition(searchingPosition);

			if (surroundingBubble == null)
			{
				continue;
			}
			surroundingBubbles.Add(surroundingBubble);
		}
		return surroundingBubbles;
	}

	public void DestroyBubble(Vector2Int position, bool showEffects)
	{
		if (IsInsideBoard(position) && _bubblesGrid[position.x, position.y] != null)
		{
			if (showEffects)
			{
				ParticleSystem explosionParticles = pool.CreateExplosionParticles(_bubblesGrid[position.x, position.y].transform.position);
				Color baseColor = GetBubbleAtPosition(position)._color;
				Gradient particlesGradient = new Gradient();
				particlesGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(baseColor, 0), new GradientColorKey(baseColor, 1) },
				new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
				var colorOverLifetime = explosionParticles.colorOverLifetime;
				colorOverLifetime.color = particlesGradient;
				explosionParticles.Play();
			}
			pool.DestroyBubble(_bubblesGrid[position.x, position.y]);
			_bubblesGrid[position.x, position.y] = null;
		}
	}

	void FallBubble(Vector2Int position)
	{
		if (IsInsideBoard(position) && _bubblesGrid[position.x, position.y] != null)
		{
			score.AddToScore(GetBubbleAtPosition(position)._exponent);
			BubbleBehaviour fallingBubble = pool.CreateFallingBubble(GetBubbleAtPosition(position).transform.position);
			fallingBubble.CopyValuesFromBubble(GetBubbleAtPosition(position));
			fallingBubble.GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle, ForceMode2D.Impulse);
			DestroyBubble(position, true);
		}
	}

	void VisualMergeBubble(Vector2Int original, Vector2Int objective)
	{
		if (IsInsideBoard(original) && IsInsideBoard(objective) && _bubblesGrid[original.x, original.y] != null && _bubblesGrid[objective.x, objective.y] != null)
		{
			BubbleBehaviour movingBubble = pool.CreateMovingBubble(GetBubbleAtPosition(original).transform.position);
			movingBubble.CopyValuesFromBubble(GetBubbleAtPosition(original));
			_movingBubbles.Add(new MovingBubble(
				movingBubble,
				GetBubbleAtPosition(original).transform.position,
				GetBubbleAtPosition(objective).transform.position,
				0
			));
		}
	}

	void UpdateMovingBubbles()
	{
		float stepSize = Time.fixedDeltaTime * 5;
		for (int i = 0; i < _movingBubbles.Count; i++)
		{
			_movingBubbles[i].SetTime(_movingBubbles[i].time + stepSize);
			_movingBubbles[i].bubble.transform.position = EaseMethods.EaseVector3(EaseMethods.GetEase(EaseStyle.LinearEaseInOut), _movingBubbles[i].origin, _movingBubbles[i].objective, _movingBubbles[i].time, 1);
			if (_movingBubbles[i].time + stepSize > 1 || Mathf.Abs(Vector3.Distance(_movingBubbles[i].bubble.transform.position, _movingBubbles[i].objective)) < 0.05f)
			{
				_movingBubbles[i].bubble.transform.position = _movingBubbles[i].objective;
				Destroy(_movingBubbles[i].bubble.gameObject);
				_movingBubbles.Remove(_movingBubbles[i]);
			}
		}
	}

	void ExplodeAround(Vector2Int center, int radius)
	{
		for (int x = -radius; x < radius; x++)
		{
			for (int y = -radius; y < radius; y++)
			{
				Vector2Int currentPos = new Vector2Int(x, y) + center;
				DebugExtensions.DrawCircle(currentPos * new Vector2(1, _yDistanceBetweenCircles), .4f, Color.blue, .1f);
				if (!IsInsideBoard(currentPos))
				{
					continue;
				}
				BubbleBehaviour bubble = GetBubbleAtPosition(currentPos);

				if (bubble != null)
				{
					score.AddToScore(bubble._exponent);
					DestroyBubble(currentPos, true);
				}
			}
		}
	}

	uint GetRandomExponent()
	{
		return (uint)Random.Range(_baseExponent, _baseExponent + 8);
	}

	[System.Serializable]
	class MovingBubble
	{
		public BubbleBehaviour bubble;
		public Vector3 origin;
		public Vector3 objective;
		public float time;

		public MovingBubble(BubbleBehaviour bubble, Vector3 origin, Vector3 objective, float time)
		{
			this.bubble = bubble;
			this.origin = origin;
			this.objective = objective;
			this.time = time;
		}

		public void SetTime(float newTime)
		{
			time = newTime;
		}
	}
}
