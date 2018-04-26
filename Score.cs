using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour {

    public Text scoreText;
    public int m_currentScore = 0;

	void Update () {
        
        scoreText.text = "Score: " + m_currentScore;
	}

    public void UpdateScore(int score)
    {
        m_currentScore += score;
    }
}
