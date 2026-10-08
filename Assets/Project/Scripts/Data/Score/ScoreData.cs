using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yudiz.VRAwarenessExperience.Data
{
    [CreateAssetMenu(fileName = "ScoreData", menuName = "Score/ScoreData")]
    public class ScoreData : ScriptableObject
    {
        [SerializeField] private int score;

        public void AddScore(int scoreToAdd)
        {
            score += scoreToAdd;
        }

        public void RemoveScore(int scoreToRemove)
        {
            score -= scoreToRemove;
        }
        
        public void ResetScore()
        {
            score = 0;
        }
        
        public int GetScore()
        {
            return score;
        }
    }
}

