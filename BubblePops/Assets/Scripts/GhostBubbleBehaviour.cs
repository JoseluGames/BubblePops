using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostBubbleBehaviour : MonoBehaviour
{
	Animation _animation;
	SpriteRenderer _spriteRenderer;
	public bool isShown;

	void Start()
	{
		_animation = GetComponent<Animation>();
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Show(Vector2 position)
	{
		if (!transform.position.Equals(position) || !_spriteRenderer.enabled == true)
		{
			transform.position = position;
			_spriteRenderer.enabled = true;
			_animation.Stop();
			_animation.Play();
		}
		isShown = true;
	}

	public void Hide()
	{
		_spriteRenderer.enabled = false;
		isShown = false;
	}
}
