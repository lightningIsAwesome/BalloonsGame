using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

namespace BalloonsGame
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject loadButton;
        public static bool loadGame{ get; private set;}

        void Awake()
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + GameController.GameData.SaveName))
                loadButton.SetActive(false);
        }

        public void StartNewGame()
        {
            loadGame = false;
            SceneManager.LoadScene("Game");
        }
            
        public void LoadGame()
        {
            loadGame = true;
            SceneManager.LoadScene("Game");
        }

        public void Quit()
        {
            Application.Quit();
        }

    }
}