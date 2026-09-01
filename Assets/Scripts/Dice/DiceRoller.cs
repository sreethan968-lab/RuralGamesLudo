using System;
using UnityEngine;

namespace RuralGames.Dice
{
    public class DiceRoller : MonoBehaviour
    {
        public event Action<int> OnDiceRolled;

        [SerializeField] private float rollDelay = 0.5f;
        private bool _isRolling = false;

        /// <summary>
        /// Rolls the dice and returns the result via OnDiceRolled event.
        /// </summary>
        public void Roll()
        {
            if (_isRolling) return;
            StartCoroutine(RollRoutine());
        }

        private System.Collections.IEnumerator RollRoutine()
        {
            _isRolling = true;
            Debug.Log("[Dice] Rolling...");

            // Simulate animation delay
            yield return new WaitForSeconds(rollDelay);

            int result = UnityEngine.Random.Range(1, 7); // 1 to 6 inclusive
            Debug.Log($"[Dice] Rolled: {result}");

            OnDiceRolled?.Invoke(result);
            OnDiceRolled?.Invoke(result);
            _isRolling = false;
        }

        /// <summary>
        /// Instant roll without delay. Returns value directly.
        /// </summary>
        public int RollInstant()
        {
            int result = UnityEngine.Random.Range(1, 7);
            Debug.Log($"[Dice] Instant roll: {result}");
            return result;
        }
    }
}