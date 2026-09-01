using RuralGames.Rules;
using RuralGames.Token;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuralGames.Core
{
    public class WinChecker : MonoBehaviour
    {
        /// <summary>
        /// Returns true if all tokens for the given player have reached home.
        /// </summary>
        public bool HasPlayerWon(int playerId, List<TokenController> playerTokens)
        {
            if (playerTokens == null || playerTokens.Count == 0)
                return false;

            bool allHome = playerTokens.All(t => t.CurrentState == TokenState.ReachedHome);

            if (allHome)
            {
                Debug.Log($"<color=yellow>[WinChecker] Player {playerId} has WON!</color>");
            }

            return allHome;
        }
    }
}
