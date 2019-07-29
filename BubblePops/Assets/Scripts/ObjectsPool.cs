using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsPool : MonoBehaviour
{
	public Transform _poolTransform;
	public Transform _uiPoolTransform;
	public Transform _canvas;
	
	public GameObject _bubblePrefab;
	public GameObject _fallingBubblePrefab;
	public GameObject _movingBubblePrefab;
	public GameObject _explosionParticlesPrefab;
	public GameObject _perfectTextPrefab;
	public GameObject _levelUpPrefab;

	Queue<GameObject> _bubblesPool = new Queue<GameObject>();
	Queue<GameObject> _fallingBubblesPool = new Queue<GameObject>();
	Queue<GameObject> _movingBubblesPool = new Queue<GameObject>();
	Queue<GameObject> _explosionParticlesPool = new Queue<GameObject>();
	Queue<GameObject> _perfectTextsPool = new Queue<GameObject>();
	Queue<GameObject> _levelUpsPool = new Queue<GameObject>();
	
	public BubbleBehaviour CreateBubble(Vector3 position)
	{
		return CustomCreateObject (_bubblesPool, _bubblePrefab, position, Quaternion.identity).GetComponent<BubbleBehaviour>();
	}

	public void DestroyBubble(BubbleBehaviour bubble)
	{
		CustomDestroyObject(_bubblesPool, bubble.gameObject);
	}
	
	public BubbleBehaviour CreateFallingBubble(Vector3 position)
	{
		return CustomCreateObject (_fallingBubblesPool, _fallingBubblePrefab, position, Quaternion.identity).GetComponent<BubbleBehaviour>();
	}

	public void DestroyFallingBubble(BubbleBehaviour bubble)
	{
		CustomDestroyObject(_fallingBubblesPool, bubble.gameObject);
	}
	
	public BubbleBehaviour CreateMovingBubble(Vector3 position)
	{
		return CustomCreateObject (_movingBubblesPool, _movingBubblePrefab, position, Quaternion.identity).GetComponent<BubbleBehaviour>();
	}

	public void DestroyMovingBubble(BubbleBehaviour bubble)
	{
		CustomDestroyObject(_movingBubblesPool, bubble.gameObject);
	}
	
	public ParticleSystem CreateExplosionParticles(Vector3 position)
	{
		return CustomCreateObject (_explosionParticlesPool, _explosionParticlesPrefab, position, Quaternion.identity).GetComponent<ParticleSystem>();
	}

	public void DestroyExplosionParticles(GameObject particles)
	{
		CustomDestroyObject(_explosionParticlesPool, particles);
	}
	
	public UIEffectTextBehaviour CreatePerfectText()
	{
		GameObject text = CustomCreateObject (_perfectTextsPool, _perfectTextPrefab, Vector3.zero, Quaternion.identity, _canvas);
		text.GetComponent<RectTransform>().localPosition = Vector3.zero;
		text.GetComponent<RectTransform>().SetParent(_canvas);
		return text.GetComponent<UIEffectTextBehaviour>();
	}

	public void DestroyPerfectText(GameObject text)
	{
		CustomDestroyObject(_perfectTextsPool, text, _uiPoolTransform);
	}
	
	public UIEffectTextBehaviour CreateLevelUp()
	{
		GameObject text = CustomCreateObject (_levelUpsPool, _levelUpPrefab, Vector3.zero, Quaternion.identity, _canvas);
		text.GetComponent<RectTransform>().localPosition = Vector3.zero;
		text.GetComponent<RectTransform>().SetParent(_canvas);
		return text.GetComponent<UIEffectTextBehaviour>();
	}

	public void DestroyLevelUp(GameObject text)
	{
		CustomDestroyObject(_levelUpsPool, text, _uiPoolTransform);
	}
	
	public GameObject CustomCreateObject (Queue<GameObject> pool, GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return CustomCreateObject(pool, prefab, position, rotation, null);
	}
	
	public GameObject CustomCreateObject (Queue<GameObject> pool, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
	{
		if (pool.Count > 0)
		{
			GameObject dequeuedBubble = pool.Dequeue();
			dequeuedBubble.transform.SetParent(parent);
			dequeuedBubble.transform.position = position;
			dequeuedBubble.SetActive(true);
			return dequeuedBubble;
		}
		else
		{
			return Instantiate(prefab, position, rotation, parent);
		}
	}
	
	public void CustomDestroyObject (Queue<GameObject> pool, GameObject gameObject)
	{
		CustomDestroyObject(pool, gameObject, _poolTransform);
	}
	
	public void CustomDestroyObject (Queue<GameObject> pool, GameObject gameObject, Transform parent)
	{
		pool.Enqueue(gameObject);
		gameObject.transform.transform.SetParent(parent);
		gameObject.SetActive(false);
	}
}
