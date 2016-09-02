using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerControler : MonoBehaviour
{
	[SerializeField]
	int m_Lives;

	[SerializeField]
	int m_BulletsPerClip;

	[SerializeField]
	float m_TimeBetweenBullets;

	[SerializeField]
	Bullet m_BulletPrefab;

	[SerializeField]
	Text m_LivesLabel;

	[SerializeField]
	Text m_CurrentBulletsLabel;

	float m_TimeSinceLastBullet;

	int m_CurrentBullets;

	public void Set(int lives, int bulletsPerClip, float timeBetweenBullets)
	{
		m_Lives = lives;
		m_BulletsPerClip = bulletsPerClip;
		m_TimeBetweenBullets = timeBetweenBullets;
	}

	// Use this for initialization
	void Start()
	{
		// allow to shoot straight away
		m_TimeSinceLastBullet = m_TimeBetweenBullets;

		// full ammo
		m_CurrentBullets = m_BulletsPerClip;
	}

	public void TakeDamage()
	{
		m_Lives--;
	}

	void Fire()
	{
		m_CurrentBullets--;
		m_TimeSinceLastBullet = 0;

		// use the forward of the camera to shoot
		Transform cameraTrans = Camera.main.transform;
		Bullet newBullet = Instantiate(m_BulletPrefab, cameraTrans.position + cameraTrans.forward * 0.5f, Quaternion.identity) as Bullet;
		newBullet.GetComponent<Rigidbody>().AddForce(cameraTrans.forward * 800f);
	}

	void HandleButtonPresses()
	{
		// Allow holding space to fire
		if (Input.GetKey(KeyCode.Space) && m_TimeSinceLastBullet >= m_TimeBetweenBullets && m_CurrentBullets > 0)
		{
			Fire();
		}

		// reload
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			m_CurrentBullets = m_BulletsPerClip;
		}
		m_TimeSinceLastBullet += Time.deltaTime;
	}

	// Update is called once per frame
	void Update()
	{
		HandleButtonPresses();
	}
}
