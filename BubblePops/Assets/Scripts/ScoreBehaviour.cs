using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBehaviour : MonoBehaviour
{
	Slider _slider;
	public Text _totalScoreText;
	public Text _currentLevelText;
	public Text _nextLevelText;
	public Image _fillArea;
	public Image _currentLevelImage;
	public Image _nextLevelImage;
	public ParticleSystem _sliderParticles;

	public int _lastLevel;
	public int _currentLevel;
	public float _objectiveScore;
	public float _score;

	void Start()
	{
		_slider = GetComponent<Slider>();
		_objectiveScore = PlayerPrefs.GetFloat("Score", 0);
		_score = _objectiveScore;
		_totalScoreText.text = _objectiveScore.ToString();
		_currentLevel = Mathf.Max(Mathf.FloorToInt((float)_objectiveScore / 10000), 1);
		_currentLevelText.text = (_currentLevel).ToString();
		_nextLevelText.text = (_currentLevel + 1).ToString();
		SetupColors();
		StartCoroutine(AnimateScoreRoutine());
	}

	public void AddToScore(uint exponent)
	{
		_lastLevel = _currentLevel;
		_objectiveScore += Mathf.Pow(2, exponent);
		PlayerPrefs.SetFloat("Score", _objectiveScore);
		_currentLevel = Mathf.Max(Mathf.FloorToInt((float)_objectiveScore / 10000), 1);
		_currentLevelText.text = (_currentLevel).ToString();
		_nextLevelText.text = (_currentLevel + 1).ToString();
		if (_currentLevel != _lastLevel)
		{
			GameController._gameController.pool.CreateLevelUp();
			SetupColors();
		}
	}

	IEnumerator AnimateScoreRoutine()
	{
		while (true)
		{
			_totalScoreText.text = Mathf.RoundToInt(_score).ToString();
			_score = Mathf.Lerp(_score, _objectiveScore, Time.deltaTime * 10);
			_slider.value = ((float)_score / 10000) % 1;
			yield return null;
		}
	}

	void SetupColors()
	{
		Color baseColor = Colors.colors[_currentLevel % 10];
		_fillArea.color = baseColor;
		_currentLevelImage.color = baseColor;
		_nextLevelImage.color = Colors.colors[(_currentLevel + 1) % 10];
		Gradient particlesGradient = new Gradient();
		particlesGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(baseColor, 0), new GradientColorKey(baseColor, 1) },
		new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
		var colorOverLifetime = _sliderParticles.colorOverLifetime;
		colorOverLifetime.color = particlesGradient;
	}
}
