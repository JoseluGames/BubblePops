using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleExplosionBehaviour : MonoBehaviour
{
	void OnParticleSystemStopped()
	{
		GameController._gameController.pool.DestroyExplosionParticles(this.gameObject);
	}
}
