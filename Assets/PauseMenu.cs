using System;
using UnityEngine;
using UnityEngine.UI;

namespace BalloonsGame
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject resumeButton;
        [SerializeField] GameObject gameOverLabel;

        public void Show(bool gameOver)
        {
            //if game is over hide 'resume' and show 'game over' label
            gameOverLabel.SetActive(gameOver);
            resumeButton.SetActive(!gameOver);
            gameObject.SetActive(true);
        }
            
    }
}