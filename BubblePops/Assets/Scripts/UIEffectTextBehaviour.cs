using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEffectTextBehaviour : MonoBehaviour
{
	Animation _animation;
	Text _text;

	void Start()
	{
		_animation = GetComponent<Animation>();
		_text = GetComponent<Text>();
		Play();
	}

	public void Play()
	{
		_animation.Stop();
		_animation.Play();
	}

	public void AnimationEnded()
	{
		GameController._gameController.pool.DestroyPerfectText(gameObject);
	}
}
