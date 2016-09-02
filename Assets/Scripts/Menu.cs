using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	// buttons
	[SerializeField]
	GameObject m_NextLevel, m_Restart, m_BackToStart;

	[SerializeField]
	Text m_Title;

	public void Set(bool firstLevel, bool lastLevel, bool win)
	{
		m_NextLevel.SetActive(win && !lastLevel);
		m_BackToStart.SetActive(!firstLevel);
		m_Title.text = win ? "You won" : "You died";
	}

	// Pause the game
	public void OnEnable()
	{
		Time.timeScale = 0;
	}

	// unpause the game
	public void OnDisable()
	{
		Time.timeScale = 1;
	}
}
