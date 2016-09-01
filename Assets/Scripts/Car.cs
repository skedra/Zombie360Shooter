using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour
{
	[SerializeField]
	Vector3[] m_Path;

	[SerializeField]
	float m_Speed = 5f;

	Transform m_Trans;
	int m_NextTarget;

	Direction m_Direction;

	bool m_WaitForLight;

	public enum Direction
	{
		Left = -1,
		Right = 1,
	}

	public void SetDirection(Direction dir)
	{
		m_Direction = dir;
	}

	void Start()
	{
		m_NextTarget = 0;
		m_Trans = transform;
		m_Trans.position = m_Path[0];
		PickNextTarget();
	}

	void PickNextTarget()
	{
		m_NextTarget += (int)m_Direction;
		if (m_NextTarget >= m_Path.Length)
			m_NextTarget = 0;
		else if (m_NextTarget == -1)
			m_NextTarget = m_Path.Length - 1;
	}

	// Update is called once per frame
	void Update()
	{
		if (!m_WaitForLight)
		{
			// move toward the next waypoint
			Vector3 dir = m_Path[m_NextTarget] - m_Trans.position;
			m_Trans.position += dir.normalized*Time.deltaTime* m_Speed;
			m_Trans.right = dir.normalized;

			// if close enough get next waypoint 
			if ((m_Trans.position - m_Path[m_NextTarget]).sqrMagnitude < 0.01f)
				PickNextTarget();
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (UnityEditor.Selection.activeGameObject != this.gameObject)
			return;

		// Visualize the path => help setting up
		for (int i = 0; i < m_Path.Length; i++)
		{
			Gizmos.DrawWireSphere(m_Path[i], 1f);
			if (i == m_Path.Length - 1)
				Gizmos.DrawLine(m_Path[i], m_Path[0]);
			else
				Gizmos.DrawLine(m_Path[i], m_Path[i + 1]);

		}
	}
#endif

	void LightChanged(TrafficLight light)
	{
		m_WaitForLight = false;
		light.OnLightChanged -= LightChanged;
	}

	// if the light is red before going beneath it wait, otherwise continue 
	void OnTriggerEnter(Collider col)
	{
		TrafficLight light = col.GetComponent<TrafficLight>();
		if (light && light.IsRed())
		{
			m_WaitForLight = true;
			light.OnLightChanged += LightChanged;
		}
	}
}
