using System;
using UnityEngine;
using UnityEngine.UI;

namespace BalloonsGame
{
    public class WinResult : MonoBehaviour
    {
        [SerializeField] Text lostCount;
        [SerializeField] Text conclusion;
        public const string bestResult = "Perfect.";
        public const string goodResult = "Not bad.";
        public const string middleResult = "Could be better.";
        public const string badResult = "Weak.";
        float badResultLimit = 0.25f;
        float middleResultLimit = 0.1f;

        public void SetResult(int totalBallonsLost, int maxLevel, int maxBalloonLost)
        {
            lostCount.text = totalBallonsLost.ToString() + " balloons lost";
            float missCoef = (float)totalBallonsLost / (maxLevel * (maxBalloonLost - 1));
            if (missCoef == 0)
                conclusion.text = bestResult;
            else
            {
                if (missCoef > badResultLimit)
                    conclusion.text = badResult;
                else if (missCoef <= badResultLimit && missCoef > middleResultLimit)
                    conclusion.text = middleResult;
                else
                    conclusion.text = goodResult;
            }
        }
    }
}