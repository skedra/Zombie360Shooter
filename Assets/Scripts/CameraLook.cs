using UnityEngine;
using System.Collections;

public class CameraLook : MonoBehaviour
{
	[SerializeField]
	float m_Speed;

	[SerializeField]
	bool m_InvertY = true;

	Transform m_Trans;
	// Use this for initialization
	void Start()
	{
		m_Trans = transform;
	}

	// Update is called once per frame
	void Update()
	{
		m_Trans.localEulerAngles += new Vector3(Input.GetAxis("Mouse Y") * (m_InvertY ? -1 : 1), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * m_Speed;
	}
}
