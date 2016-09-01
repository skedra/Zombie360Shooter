using UnityEngine;
using System.Collections;

public class TrafficLight : MonoBehaviour
{
	[SerializeField]
	float m_RedTime;

	[SerializeField]
	float m_GreenTime;

	public System.Action<TrafficLight> OnLightChanged;

	Renderer m_Renderer;

	bool m_IsRed;

	public void SetTimeChange(float timeGreen, float timeRed)
	{
		m_RedTime = timeRed;
		m_GreenTime = timeGreen;
	}

	void Start()
	{
		// cache the renderer and start the light switch
		m_Renderer = GetComponent<Renderer>();
		StartCoroutine(LightChange());
	}

	IEnumerator LightChange()
	{
		while (true)
		{
			// wait for the change
			yield return new WaitForSeconds(m_IsRed ? m_RedTime : m_GreenTime);

			// toggle colour
			m_IsRed = !m_IsRed;
			m_Renderer.material.color = m_IsRed ? Color.red : Color.green;

			// if there is a subscriber to the event call it
			if (OnLightChanged != null)
				OnLightChanged(this);
		}
	}

	public bool IsRed()
	{
		return m_IsRed;
	}
}
