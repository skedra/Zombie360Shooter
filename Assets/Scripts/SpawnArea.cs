using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnArea : MonoBehaviour
{
	[SerializeField]
	Transform m_ClosestCrossing;

	[SerializeField]
	Zombie m_OrdinaryZombiePrefab;

	[SerializeField]
	Zombie m_RedZombiePrefab;

	[SerializeField]
	float m_SpawnTime;

	[SerializeField]
	int m_RedZombieEvery;

	float m_SpawnTimer;

	Renderer m_SpawnBounds;

	List<Zombie> m_Zombies = new List<Zombie>();

	const float SPAWN_HEIGHT = 0f;

	int m_ZombiesSpawned = 0;

	public void Set(float spawnTime, int redZombieAfter)
	{
		// setup for a new level
		m_SpawnTime = spawnTime;
		m_RedZombieEvery = redZombieAfter;
	}

	void Start()
	{
		// Get the bounds of the spawn area
		m_SpawnBounds = GetComponent<Renderer>();
	}

	void Update()
	{
		// if spawn time passed
		if (m_SpawnTimer >= m_SpawnTime)
		{
			m_ZombiesSpawned++;;

			// spawn new zombie and set it up
			Zombie newZombie = Instantiate(m_ZombiesSpawned % m_RedZombieEvery == 0 ? m_RedZombiePrefab : m_OrdinaryZombiePrefab, GetRandomSpawnPosition(), Quaternion.identity) as Zombie;
			newZombie.m_ClosestCrossing = m_ClosestCrossing.GetChild(0).position;
			m_SpawnTimer = 0;
			m_Zombies.Add(newZombie);
		}

		m_SpawnTimer += Time.deltaTime;
	}

	public void Clear()
	{
		// destroy all the zombies that aren't already destroyed
		foreach (Zombie zombie in m_Zombies)
		{
			if (zombie != null)
			{
				Destroy(zombie.gameObject);
			}
		}
		m_ZombiesSpawned = 0;
		m_Zombies.Clear();
	}

	Vector3 GetRandomSpawnPosition()
	{
		// get a random value in the bounds of the extents of the spawn area
		Vector3 random = new Vector3(Random.Range(-m_SpawnBounds.bounds.extents.x * 2f, m_SpawnBounds.bounds.extents.x * 2f), SPAWN_HEIGHT,
			Random.Range(-m_SpawnBounds.bounds.extents.z * 2f, m_SpawnBounds.bounds.extents.z * 2));
		random += m_SpawnBounds.bounds.center;
		return random;
	}
}
