using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
	void Awake()
	{
		Destroy(this.gameObject, 5f);
	}

	void OnCollisionEnter(Collision col)
	{
		Zombie zombie = col.gameObject.GetComponent<Zombie>();
		if (zombie)
		{
			zombie.Kill();
		}
		Destroy(this.gameObject);
	}
}
