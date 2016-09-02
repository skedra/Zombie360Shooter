using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Zombie : MonoBehaviour
{
	[SerializeField]
	float m_InitSpeed = 5;

	[SerializeField]
	float m_Range = 2f;

	[SerializeField]
	float m_AttackDelay = 2f;

	[SerializeField]
	bool m_IgnoreLights = false;

	public Vector3 m_ClosestCrossing;

	Transform m_Trans;

	List<AStar.PathFindingNode> m_Path;

	private bool m_WaitForLight;

	EnemyState m_EnemyState;

	PlayerControler m_Player;

	float m_AttackTimer;

	float m_CurrentSpeed;

	public enum EnemyState
	{
		GoingToLights,
		GoingToPlayer,
		Attacking,
	}

	// Use this for initialization
	void Start()
	{
		m_Trans = transform;

		if (!m_IgnoreLights)
			m_EnemyState = EnemyState.GoingToLights;
		else
			m_EnemyState = EnemyState.GoingToPlayer;

		PickNextPath();
		m_Player = FindObjectOfType<PlayerControler>();
		m_CurrentSpeed = m_InitSpeed;
	}

	void PickNextPath()
	{
		if (m_EnemyState == EnemyState.GoingToLights)
		{
			m_Path = AStar.inst.GetPath(new Vector2(m_Trans.position.x, m_Trans.position.z),
					new Vector2(m_ClosestCrossing.x, m_ClosestCrossing.z));
		}
		else
		{
			m_Path = AStar.inst.GetPath(new Vector2(m_Trans.position.x, m_Trans.position.z), AStar.inst.m_Center);
		}

		if (m_Path.Count > 0)
			m_Path.RemoveAt(0);
	}

	// Update is called once per frame
	void Update()
	{
		if (m_Path != null && m_Path.Count > 0 && m_EnemyState != EnemyState.Attacking)
		{
			if (!m_WaitForLight)
			{
				// pick the direction
				Vector3 nextPos = new Vector3(m_Path[0].m_Pos.x, m_Trans.position.y, m_Path[0].m_Pos.y);
				Vector3 dir = nextPos - m_Trans.position;

				// update position and orientation
				m_Trans.position += dir.normalized * Time.deltaTime * m_CurrentSpeed;
				m_Trans.forward = -dir;
				m_Trans.localEulerAngles += new Vector3(0, 0, 180);

				// if we reached the position by certain treshold 
				if ((m_Trans.position - nextPos).sqrMagnitude < 0.01f)
				{
					m_Path.RemoveAt(0);
					if (m_Path.Count == 0 && m_EnemyState == EnemyState.GoingToLights)
					{
						m_EnemyState = EnemyState.GoingToPlayer;
						PickNextPath();
					}
				}

				// if we're close enough to the player
				if ((m_Trans.position - m_Player.Trans.position).magnitude <= m_Range)
				{
					m_EnemyState = EnemyState.Attacking;
				}
			}
		}

		// if the enemy can attack
		if (m_EnemyState == EnemyState.Attacking && m_AttackTimer >= m_AttackDelay)
		{
			m_Player.TakeDamage();
			m_AttackTimer = 0;
		}
		m_AttackTimer += Time.deltaTime;
	}

	void LightChanged(TrafficLight light)
	{
		m_WaitForLight = false;
		light.OnLightChanged -= LightChanged;
	}

	public void Kill()
	{
		ManGame.inst.ZombieKilled();
		Destroy(this.gameObject);
	}

	// if the light is green before going beneath it wait, otherwise continue 
	void OnTriggerEnter(Collider col)
	{
		// if we've hit a light trigger and the light for the cars isn't red setup for the light change event
		TrafficLight light = col.GetComponent<TrafficLight>();
		if (light && !light.IsRed() && !m_IgnoreLights)
		{
			m_WaitForLight = true;
			light.OnLightChanged += LightChanged;
		}
		else if (col.tag.Equals("CenterTrigger"))
		{
			m_CurrentSpeed = m_InitSpeed * (Random.Range(1.5f, 2.0f));
		}
	}

	void OnDrawGizmos()
	{
		if (m_Path.Count >= 2)

			for (int i = 0; i < m_Path.Count - 1; i++)
			{
				Gizmos.DrawLine(i == 0 ? m_Trans.position : new Vector3(m_Path[i].m_Pos.x, 1, m_Path[i].m_Pos.y), new Vector3(m_Path[i + 1].m_Pos.x, 1, m_Path[i + 1].m_Pos.y));
			}
	}
}
