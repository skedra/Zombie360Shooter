using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ManGame : MonoBehaviour
{
	[Header("Cars")]
	[SerializeField]
	Car[] m_Cars;

	[Header("Traffic Lights")]
	[SerializeField]
	TrafficLight[] m_TrafficLights;

	[Header("Spawn Areas")]
	[SerializeField]
	SpawnArea[] m_Areas;

	[Header("Levels")]
	[SerializeField]
	LevelSettings[] m_LevelSettings;

	[Header("Player")]
	[SerializeField]
	PlayerControler m_Player;

	[Header("Score")]
	[SerializeField]
	Text m_ScoreLabel;

	[Header("Main UI")]
	Menu m_Menu;

	[System.Serializable]
	public class LevelSettings
	{
		[Header("Lights")]
		public float[] m_RedTimes;
		public float[] m_GreenTimes;

		[Header("Spawn Areas")]
		public float[] m_SpawnTimes;
		public int m_RedZombieAfter;

		[Header("Player")]
		public int m_PlayerLives;
		public int m_PlayerClipBullets;
		public float m_TimeBetweenBullets;

		[Header("Level settings")]
		public int m_ZombiePoints;
		public int m_PointsToProceed;
	}

	int m_Score;

	int m_CurrentLevel = 0;

	public static ManGame inst;

	void Awake()
	{
		// Lazy singleton
		inst = this;

		// send one car right and one left randomly
		Car.Direction dirFirst = Random.Range(0, 2) == 1 ? Car.Direction.Left : Car.Direction.Right;
		m_Cars[0].SetDirection(dirFirst);
		m_Cars[1].SetDirection(dirFirst == Car.Direction.Left ? Car.Direction.Right : Car.Direction.Left);
	}

	void StartLevel(int id)
	{
		// if we're out of bounds
		if (m_LevelSettings.Length >= id)
		{
			Debug.LogError("Level doesn't exist");
			return;
		}

		m_Score = 0;

		// setup the lights
		for (int i = 0; i < m_TrafficLights.Length; i++)
		{
			m_TrafficLights[i].SetTimeChange(m_LevelSettings[id].m_GreenTimes[i], m_LevelSettings[id].m_RedTimes[i]);
		}

		// setup spawns
		for (int i = 0; i < m_Areas.Length; i++)
		{
			m_Areas[i].Clear();
			m_Areas[i].Set(m_LevelSettings[id].m_SpawnTimes[i], m_LevelSettings[id].m_RedZombieAfter);
		}

		// setup player
		m_Player.Set(m_LevelSettings[id].m_PlayerLives, m_LevelSettings[id].m_PlayerClipBullets,
			m_LevelSettings[id].m_TimeBetweenBullets);

		m_CurrentLevel = id;

		m_Menu.gameObject.SetActive(false);
	}

	public void ZombieKilled()
	{
		// add points and show stop the game if the player finished the leve
		m_Score += m_LevelSettings[m_CurrentLevel].m_ZombiePoints;
		if (m_Score >= m_LevelSettings[m_CurrentLevel].m_PointsToProceed)
		{
			m_Menu.gameObject.SetActive(true);
			// setup the menu
			m_Menu.Set(m_CurrentLevel == 0, m_CurrentLevel == m_LevelSettings.Length - 1, true);
		}
	}

	public void StarNextLevel()
	{
		StartLevel(m_CurrentLevel + 1);
	}

	public void StartFirstLevel()
	{
		StartLevel(0);
	}

	public void RestartLevel()
	{
		StartLevel(m_CurrentLevel);
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
