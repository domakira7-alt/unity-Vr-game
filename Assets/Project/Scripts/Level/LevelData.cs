using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yudiz.VRAwarenessExperience.Core;

namespace Yudiz.VRAwarenessExperience.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Level/LevelData")]
    public class LevelData : ScriptableObject
    {
        public List<LevelContainer> levels = new List<LevelContainer>();

        public LevelContainer GetLevelContainer(LevelName levelName)
        {
            return levels.Find(level => level.levelName == levelName);
        }

        public int GetLevelCount() =>  levels.Count;
    }
}

