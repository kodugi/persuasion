using System;
using System.Collections.Generic;
using SingletonUtils;
using UnityEngine;
using Random = System.Random;

namespace GamePlay
{
    public class BackgroundSuspicionView: SelfInitializingMonoBehaviourSingleton<BackgroundSuspicionView>
    {
        [SerializeField] private GameObject _suspicionPrefab;
        
        private List<GameObject> _spawnedSuspicionPrefabs;
        
        protected override bool InitializeCore()
        {
            if (GameStateManager.Instance == null)
            {
                Debug.LogError("GameStateManager is null");
                return false;
            }

            if (_suspicionPrefab == null)
            {
                Debug.LogError("SuspicionPrefab is null");
                return false;
            }
            
            _spawnedSuspicionPrefabs = new List<GameObject>();
            
            GameStateManager.Instance.RaiseSetGameStateEvent += HandleSetGameStateEvent;
            return true;
        }

        private void HandleSetGameStateEvent(object sender, SetGameStateEventArgs e)
        {
            if (e.gameState == GameState.Lost)
            {
                for (int i = 0; i < 20; i++)
                {
                    GameObject suspicionObject = Instantiate(_suspicionPrefab);
                    Random random = new Random();
                    suspicionObject.transform.position = new Vector3((float)random.NextDouble() * 16 - 8, (float)random.NextDouble() * 8 - 4, 0);
                    _spawnedSuspicionPrefabs.Add(suspicionObject);
                    BackgroundSuspicionPrefabView view = suspicionObject.AddComponent<BackgroundSuspicionPrefabView>();
                    view.Initialize();
                    view.PlayGameOverAnimation();
                }
            }
            else
            {
                foreach (GameObject suspicionObject in _spawnedSuspicionPrefabs)
                {
                    Destroy(suspicionObject);
                }
                _spawnedSuspicionPrefabs.Clear();
            }
        }
    }
}