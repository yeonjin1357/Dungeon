using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;
using System.Collections;

namespace Dungeon.Managers
{
    [Serializable]
    public class GameStateChangedEvent : UnityEvent<GameManager.GameState> { }
    
    [Serializable]
    public class PlayerDeathEvent : UnityEvent { }

    public class GameManager : Singleton<GameManager>
    {
        #region Game States
        public enum GameState
        {
            Menu,
            Playing,
            Paused,
            GameOver,
            Loading
        }

        private GameState _currentState = GameState.Menu;
        public GameState CurrentState => _currentState;
        #endregion

        #region Events
        [Header("Game Events")]
        public GameStateChangedEvent OnGameStateChanged = new GameStateChangedEvent();
        public PlayerDeathEvent OnPlayerDeath = new PlayerDeathEvent();
        public UnityEvent OnGameSaved = new UnityEvent();
        public UnityEvent OnGameLoaded = new UnityEvent();
        #endregion

        #region Fields
        [Header("Game Settings")]
        [SerializeField] private int _currentFloor = 1;
        [SerializeField] private int _playerScore = 0;
        [SerializeField] private float _playTime = 0f;
        
        [Header("Player Data")]
        [SerializeField] private int _playerLevel = 1;
        [SerializeField] private int _playerExperience = 0;
        [SerializeField] private int _playerGold = 0;
        
        private const string SCENE_MAINMENU = "MainMenu";
        private const string SCENE_GAME = "Game";
        private const string SCENE_LOADING = "Loading";
        private const string SAVE_KEY = "DungeonSaveData";
        
        private bool _isLoading = false;
        #endregion

        #region Properties
        public int CurrentFloor => _currentFloor;
        public int PlayerScore => _playerScore;
        public float PlayTime => _playTime;
        public int PlayerLevel => _playerLevel;
        public int PlayerExperience => _playerExperience;
        public int PlayerGold => _playerGold;
        public bool IsLoading => _isLoading;
        #endregion

        #region Unity Lifecycle
        protected override void OnAwakeInitialization()
        {
            InitializeGame();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (_currentState == GameState.Playing)
            {
                _playTime += Time.deltaTime;
            }
        }
        #endregion

        #region Public Methods
        public void ChangeState(GameState newState)
        {
            GameState previousState = _currentState;
            _currentState = newState;
            
            Debug.Log($"Game State Changed from {previousState} to {newState}");
            OnGameStateChanged?.Invoke(newState);
            
            HandleStateChange(previousState, newState);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void StartNewGame()
        {
            ResetGameData();
            ChangeState(GameState.Playing);
            LoadScene(SCENE_GAME);
        }

        public void ContinueGame()
        {
            if (LoadGame())
            {
                ChangeState(GameState.Playing);
                LoadScene(SCENE_GAME);
            }
            else
            {
                Debug.LogWarning("No save data found. Starting new game.");
                StartNewGame();
            }
        }

        public void ReturnToMenu()
        {
            ChangeState(GameState.Menu);
            LoadScene(SCENE_MAINMENU);
        }

        public void PauseGame()
        {
            if (_currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            if (_currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }

        public void GameOver()
        {
            ChangeState(GameState.GameOver);
            Time.timeScale = 0f;
        }

        public void PlayerDied()
        {
            OnPlayerDeath?.Invoke();
            GameOver();
        }

        public void AddScore(int points)
        {
            _playerScore += points;
        }

        public void AddGold(int amount)
        {
            _playerGold += amount;
        }

        public void AddExperience(int exp)
        {
            _playerExperience += exp;
            CheckLevelUp();
        }

        public void NextFloor()
        {
            _currentFloor++;
            SaveGame();
        }
        #endregion

        #region Save/Load System
        public void SaveGame()
        {
            try
            {
                SaveData saveData = new SaveData
                {
                    currentFloor = _currentFloor,
                    playerScore = _playerScore,
                    playTime = _playTime,
                    playerLevel = _playerLevel,
                    playerExperience = _playerExperience,
                    playerGold = _playerGold,
                    saveDateTime = DateTime.Now.ToString()
                };
                
                string jsonData = JsonUtility.ToJson(saveData);
                PlayerPrefs.SetString(SAVE_KEY, jsonData);
                PlayerPrefs.Save();
                
                Debug.Log("Game saved successfully.");
                OnGameSaved?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }

        public bool LoadGame()
        {
            try
            {
                if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(SAVE_KEY);
                    SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
                    
                    _currentFloor = saveData.currentFloor;
                    _playerScore = saveData.playerScore;
                    _playTime = saveData.playTime;
                    _playerLevel = saveData.playerLevel;
                    _playerExperience = saveData.playerExperience;
                    _playerGold = saveData.playerGold;
                    
                    Debug.Log($"Game loaded successfully from {saveData.saveDateTime}");
                    OnGameLoaded?.Invoke();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return false;
            }
        }

        public void DeleteSaveData()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                PlayerPrefs.DeleteKey(SAVE_KEY);
                PlayerPrefs.Save();
                Debug.Log("Save data deleted.");
            }
        }

        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(SAVE_KEY);
        }
        #endregion

        #region Private Methods
        private void InitializeGame()
        {
            Application.targetFrameRate = 60;
            Time.timeScale = 1f;
        }

        private void HandleStateChange(GameState previousState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
                case GameState.Loading:
                    Time.timeScale = 1f;
                    break;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"Scene loaded: {scene.name}");
            
            if (scene.name == SCENE_GAME && _currentState != GameState.Playing)
            {
                ChangeState(GameState.Playing);
            }
            else if (scene.name == SCENE_MAINMENU && _currentState != GameState.Menu)
            {
                ChangeState(GameState.Menu);
            }
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            _isLoading = true;
            ChangeState(GameState.Loading);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SCENE_LOADING);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            yield return new WaitForSeconds(0.5f);
            
            asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                Debug.Log($"Loading progress: {progress * 100}%");
                yield return null;
            }
            
            _isLoading = false;
        }

        private void ResetGameData()
        {
            _currentFloor = 1;
            _playerScore = 0;
            _playTime = 0f;
            _playerLevel = 1;
            _playerExperience = 0;
            _playerGold = 0;
        }

        private void CheckLevelUp()
        {
            int requiredExp = _playerLevel * 100;
            while (_playerExperience >= requiredExp)
            {
                _playerExperience -= requiredExp;
                _playerLevel++;
                Debug.Log($"Level up! Now level {_playerLevel}");
                requiredExp = _playerLevel * 100;
            }
        }
        #endregion

        #region Save Data Structure
        [Serializable]
        private class SaveData
        {
            public int currentFloor;
            public int playerScore;
            public float playTime;
            public int playerLevel;
            public int playerExperience;
            public int playerGold;
            public string saveDateTime;
        }
        #endregion
    }
}