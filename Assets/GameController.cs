using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

namespace BalloonsGame
{
    public class GameController : MonoBehaviour 
    {
        [Serializable]
        internal class GameData
        {
            public GameData(int level, int balloonsLost)
            {
                this.level = level;
                this.balloonsLost = balloonsLost;
            }

            public int level;
            public int balloonsLost;
            static string saveName = "balloonData.dat";
            public static string SaveName
            {
                get
                {
                    return saveName;
                }
            }
        }

        [SerializeField] BalloonBehaviour Balloon;
        [SerializeField] CollisionDestroy balloonDestroyerPrefab;
        [SerializeField] Button PauseButton;
        [SerializeField] Text score;
        [SerializeField] Text level;
        [SerializeField] Text scoreMissed;
        [SerializeField] Text maxBalloonsLost;
        [SerializeField] Text Status;
        [SerializeField] GameObject HUD;
        [SerializeField] PauseMenu pauseMenu;
        [SerializeField] WinResult winResult;
        [SerializeField] [Range(0.01f, 5f)] float spawnFreq = 0.5f;
        [SerializeField] [Range(0.01f, 5f)] float startBalloonSpeed = 0.08f;
        [SerializeField] [Range(1, 99)] int balloonsLostLimit = 10;
        [SerializeField] [Range(1, 99)] int balloonsHitForNewLevel = 10;
        [SerializeField] [Range(0.001f, 0.1f)] float speedIncreaseCoef = 0.01f;//level multiply speed increase
        [SerializeField] [Range(1, 999)] int maxLevel = 25;
        [SerializeField] [Range(0, 5)] int gameStartDelay = 3;
        public bool IsGameStopped{ get; private set; }
        Func<bool> checkHitPosition;
        Vector3 hitPosition;
        bool gameOver;
        int balloonsHited;
        int balloonsLost;
        int currentLevel;
        int totalBalloonsLost;
        float defaultPadding = 1f;//default padding from screen edge for instantiating(should fit balloon size)
        float defaultOrthoSize = 5f;//default camera ortho size
        Coroutine spawnCoroutine;
        public int CurrentLevel
        {
            get
            {
                return currentLevel;
            }
            private set
            {
                currentLevel = value;
                level.text = value.ToString();
            }
        }
        public int BalloonsHited
        {
            get
            {
                return balloonsHited;  
            }
            private set
            {
                balloonsHited = value;
                score.text = value.ToString();
            }
        }
        public int BalloonsLost
        {
            get
            {
                return balloonsLost;
            }
            private set
            {
                balloonsLost = value;
                scoreMissed.text = value.ToString();
            }
        }

    	void Start() 
        {	
            SetupDestroyer();
            maxBalloonsLost.text = "/" + balloonsLostLimit.ToString();
            if (Input.touchSupported)
                checkHitPosition = TouchScreenCheck;
            else
                checkHitPosition = MouseCheck;
            if (MainMenu.loadGame)
                LoadGame();
            StartCoroutine(StartGame());
    	}
    	
        void Update()
        {                
            if (checkHitPosition())
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(hitPosition), Vector2.zero);
                if (hit.collider)
                {
                    Destroy(hit.collider.gameObject);
                    BalloonsHited++;
                    if (balloonsHited == balloonsHitForNewLevel)
                        LevelUp();
                }  
            }
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void TogglePause()
        {
            HUD.SetActive(IsGameStopped);
            IsGameStopped = !IsGameStopped;
            if (IsGameStopped)
            {
                StopGame();
                pauseMenu.Show(gameOver);
            }
            else
            {
                ResumeGame();
                pauseMenu.gameObject.SetActive(false);//resume game and hide pause menu
            }
        }

        public void TryAgain()
        {
            foreach (var balloon in FindObjectsOfType<BalloonBehaviour>())
            {
                Destroy(balloon.gameObject);
            }
            RecetScore();
            totalBalloonsLost = 0;
            gameOver = false;
            StopCoroutine(spawnCoroutine);
            TogglePause();
            StartCoroutine(StartGame());
        }

        public void ToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
          
        void RecetScore()
        {
            BalloonsLost = 0;
            BalloonsHited = 0;
        }

        bool TouchScreenCheck()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    hitPosition = new Vector3(touch.position.x, touch.position.y);
                    return true;
                }
            }
            return false;
        }

        bool MouseCheck()
        {
            if (Input.GetMouseButtonDown(0))
            {
                hitPosition = Input.mousePosition;
                return true;
            }
            return false;
        }

        void LoadGame()
        {
            BinaryFormatter binForm = new BinaryFormatter();
            using (FileStream fs = new FileStream(GameData.SaveName, FileMode.Open))
            {
                var info = (GameData)binForm.Deserialize(fs);
                CurrentLevel = info.level;
                totalBalloonsLost = info.balloonsLost;
            }
        }

        void LevelUp()
        {
            RecetScore();
            CurrentLevel++;
            if (currentLevel == maxLevel)
                Win();
            else
                SaveData();
        }

        void SaveData()
        {
            GameData data = new GameData(CurrentLevel, totalBalloonsLost);
            BinaryFormatter binForm = new BinaryFormatter();
            using (FileStream fs = new FileStream(GameData.SaveName, FileMode.OpenOrCreate))
            {
                binForm.Serialize(fs, data);
            }
        }

        void LostHandle()
        {
            BalloonsLost++;
            totalBalloonsLost++;
            if (balloonsLost == balloonsLostLimit)
                GameOver();
        }

        void GameOver()
        {
            gameOver = true;
            TogglePause();
        }

        void StopGame()
        {
            enabled = false;
            Time.timeScale = 0f;
            HUD.SetActive(false);
        }

        void ResumeGame()
        {
            enabled = true;
            Time.timeScale = 1f;
            HUD.SetActive(true);
        }

        void Win()
        {
            StopGame();
            HUD.SetActive(false);
            winResult.gameObject.SetActive(true);
            winResult.SetResult(totalBalloonsLost, maxLevel, balloonsLostLimit);
        }

        void SetupDestroyer()
        {
            var balloonDestroyer = Instantiate(balloonDestroyerPrefab);
            float scale = Camera.main.orthographicSize / defaultOrthoSize;//calculating valid balloon destroyer position and scale
            balloonDestroyer.transform.localPosition = Camera.main.transform.localPosition + new Vector3(0f, (Camera.main.orthographicSize + defaultPadding));
            balloonDestroyer.transform.localScale = new Vector3(balloonDestroyer.transform.localScale.x * scale, balloonDestroyer.transform.localScale.y);
            balloonDestroyer.OnColliderDestroyed += LostHandle;
        }

        IEnumerator StartGame()
        {
            HUD.SetActive(false);
            yield return Countdown();
            ResumeGame();
            spawnCoroutine = StartCoroutine(Spawn());
        }

        IEnumerator Countdown()
        {
            Status.gameObject.SetActive(true);
            for (int i = gameStartDelay; i > 0; i--)
            {
                Status.text = i.ToString();
                yield return new WaitForSecondsRealtime(1f);
            }
            Status.gameObject.SetActive(false);
        }

        IEnumerator Spawn()
        {
            for (;;)
            {
                var newBalloon = Instantiate(Balloon);
                newBalloon.speed = startBalloonSpeed + currentLevel * speedIncreaseCoef;
                //add randomization
                var randomOffset = new Vector3(UnityEngine.Random.Range(-(Camera.main.orthographicSize + defaultPadding * 0.5f), Camera.main.orthographicSize + defaultPadding * 0.5f), -(Camera.main.orthographicSize + defaultPadding));
                newBalloon.transform.localPosition = Camera.main.transform.localPosition + randomOffset;
                yield return new WaitForSeconds(spawnFreq);
            }
        }
    }
}
