using UnityEngine;
using RuralGames.Dice;
using RuralGames.Token;
using RuralGames.Rules;
using System.Collections.Generic;

namespace RuralGames.Core
{
    public enum GamePhase
    {
        Idle,           // Waiting for player to roll
        Rolling,        // Dice is rolling
        SelectingToken, // Player must pick which token to move
        Moving,         // Token is moving on board
        EndTurn         // Turn is over, switch player
    }

    public class GameManager : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private int currentPlayerId = 0;
        [SerializeField] private int totalPlayers = 2;

        [Header("References")]
        [SerializeField] private DiceRoller diceRoller;
        [SerializeField] private List<TokenController> playerTokens; // All tokens for current player

        [Header("State")]
        [SerializeField] private GamePhase currentPhase = GamePhase.Idle;
        [SerializeField] private int lastDiceRoll = 0;

        private void Start()
        {
            if (diceRoller == null)
                diceRoller = FindAnyObjectByType<DiceRoller>();

            diceRoller.OnDiceRolled += HandleDiceResult;

            Debug.Log($"=== Game Started | Player {currentPlayerId}'s Turn ===");
            Debug.Log("Press SPACE to roll dice.");
        }

        private void Update()
        {
            // Only allow rolling in Idle phase
            if (currentPhase == GamePhase.Idle && Input.GetKeyDown(KeyCode.Space))
            {
                currentPhase = GamePhase.Rolling;
                diceRoller.Roll();
            }
        }

        private void HandleDiceResult(int roll)
        {
            lastDiceRoll = roll;
            currentPhase = GamePhase.SelectingToken;

            Debug.Log($"[GameManager] Player {currentPlayerId} rolled {roll}");

            // For now: auto-move the first valid token (we'll add manual selection later)
            bool anyMoved = TryMoveFirstValidToken();

            if (anyMoved)
            {
                currentPhase = GamePhase.Moving;
                Invoke(nameof(EndTurn), 1.5f); // Wait for move to finish
            }
            else
            {
                Debug.Log($"<color=red>[GameManager] No valid moves for Player {currentPlayerId}</color>");
                EndTurn();
            }
        }

        private bool TryMoveFirstValidToken()
        {
            foreach (var token in playerTokens)
            {
                // TryMove returns true only if RuleManager allows it
                if (token.TryMove(lastDiceRoll))
                {
                    Debug.Log($"[GameManager] Moving token for Player {currentPlayerId}");
                    return true;
                }
            }
            return false;
        }

        private void EndTurn()
        {
            currentPhase = GamePhase.EndTurn;
            currentPlayerId = (currentPlayerId + 1) % totalPlayers;
            currentPhase = GamePhase.Idle;

            Debug.Log($"=== Player {currentPlayerId}'s Turn ===");
            Debug.Log("Press SPACE to roll dice.");
        }
    }
}
