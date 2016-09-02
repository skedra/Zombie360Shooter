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

	public Vector3 m_ClosestCrossing;

	Transform m_Trans;

	List<AStar.PathFindingNode> m_Path;

	private bool m_WaitForLight;

	EnemyState m_EnemyState;

	PlayerControler m_Player;

	float m_AttackTimer;

	public enum EnemyState
	{
		GoingToLights,
		GoingToPlayer,
	}

	// Use this for initialization
	void Start()
	{
		m_Trans = transform;
		m_Path = AStar.inst.GetPath(new Vector2(m_Trans.position.x, m_Trans.position.z), new Vector2(m_ClosestCrossing.x, m_ClosestCrossing.z));
		m_Path.RemoveAt(0);
		m_EnemyState = EnemyState.GoingToLights;
		m_Player = FindObjectOfType<PlayerControler>();
	}

	// Update is called once per frame
	void Update()
	{
		if (m_Path != null && m_Path.Count > 0)
		{
			if (!m_WaitForLight)
			{
				Vector3 nextPos = new Vector3(m_Path[0].m_Pos.x, m_Trans.position.y, m_Path[0].m_Pos.y);
				Vector3 dir = nextPos - m_Trans.position;
				m_Trans.position += dir.normalized * Time.deltaTime * m_InitSpeed;
				m_Trans.forward = dir.normalized;
				if ((m_Trans.position - nextPos).sqrMagnitude < 0.01f)
				{
					m_Path.RemoveAt(0);
					if (m_Path.Count == 0 && m_EnemyState == EnemyState.GoingToLights)
					{
						m_Path = AStar.inst.GetPath(new Vector2(m_Trans.position.x, m_Trans.position.z), AStar.inst.m_Center + Random.insideUnitCircle.normalized * 1.5f);
						m_EnemyState = EnemyState.GoingToPlayer;
					}
				}
			}
		}
		if ((m_Trans.position - m_Player.Trans.position).magnitude <= m_Range && m_AttackTimer >= m_AttackDelay)
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
		TrafficLight light = col.GetComponent<TrafficLight>();
		if (light && !light.IsRed())
		{
			m_WaitForLight = true;
			light.OnLightChanged += LightChanged;
		}
	}
}
