using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using StarterKit;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Core;
using Yudiz.VRAwarenessExperience.Data;

namespace Yudiz.VRAwarenessExperience.Manager
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Level Data")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] private LevelData levelData;
        private int currentLevelIndex = -1;
        private Level currentLevel;

        [Header("Level Setup")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] private Transform levelParent;

        [Header("Level Prefab")]
        [HorizontalLine(color: EColor.Green)]
        [SerializeField] private Level levelPrefab;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [EnableIf("debugMode")] [SerializeField] private LevelName debugLevelName = LevelName.NormalMovementLevel;

        public void LoadNextLevel()
        {
            RemoveCurrentLevel();
            currentLevelIndex++;
            currentLevelIndex = currentLevelIndex % levelData.GetLevelCount();
            
            Debug.Log("LevelManager -> Loading level index : " + currentLevelIndex + " Level Name : " + ((LevelName)currentLevelIndex).ToString());
            
            if (debugMode)
            {
                LevelContainer levelContainer = levelData.GetLevelContainer(debugLevelName);
                if (levelContainer != null)
                {
                    currentLevel = Instantiate(levelPrefab, levelParent);
                    currentLevel.StartLevel(levelContainer);
                }
            }
            else
            {
                 LevelContainer levelContainer = levelData.GetLevelContainer((LevelName)currentLevelIndex);
                if (levelContainer != null)
                {
                    currentLevel = Instantiate(levelPrefab, levelParent);
                    currentLevel.StartLevel(levelContainer);
                }
            }
        }

        public void RemoveCurrentLevel()
        {
            if (currentLevelIndex >= 0 && currentLevel != null)
            {
                Debug.Log("LevelManager -> Remove level index : " + currentLevelIndex);
                Destroy(currentLevel.gameObject);
                currentLevel = null;
            }
        }

        public void ResetLevelData()
        {
            currentLevelIndex = -1;
        }

        public void ExitGameLevel()
        {
            RemoveCurrentLevel();
            ResetLevelData();
        }
    }
}

