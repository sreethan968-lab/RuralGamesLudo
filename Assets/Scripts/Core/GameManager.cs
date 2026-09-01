using UnityEngine;
using RuralGames.Dice;
using RuralGames.Token;
using System.Collections.Generic;
using System.Linq;

namespace RuralGames.Core
{
    public enum GamePhase
    {
        Idle,
        Rolling,
        SelectingToken,
        Moving,
        EndTurn,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private int currentPlayerId = 0;
        [SerializeField] private int totalPlayers = 2;

        [Header("References")]
        [SerializeField] private DiceRoller diceRoller;
        [SerializeField] private List<TokenController> allTokens;

        [Header("State")]
        [SerializeField] private GamePhase currentPhase = GamePhase.Idle;
        [SerializeField] private int lastDiceRoll = 0;

        private List<TokenController> _validTokens = new();
        private WinChecker _winChecker;

        private void Start()
        {
            if (diceRoller == null)
                diceRoller = UnityEngine.Object.FindAnyObjectByType<DiceRoller>();

            _winChecker = UnityEngine.Object.FindAnyObjectByType<WinChecker>();

            diceRoller.OnDiceRolled += HandleDiceResult;

            Debug.Log($"=== Game Started | Player {currentPlayerId}'s Turn ===");
            Debug.Log("Press SPACE to roll dice.");
        }

        private void Update()
        {
            // Stop everything if game is over
            if (currentPhase == GamePhase.GameOver)
                return;

            // Roll dice
            if (currentPhase == GamePhase.Idle && Input.GetKeyDown(KeyCode.Space))
            {
                currentPhase = GamePhase.Rolling;
                diceRoller.Roll();
            }

            // Select token (press 1-4)
            if (currentPhase == GamePhase.SelectingToken)
            {
                for (int i = 1; i <= _validTokens.Count && i <= 4; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                    {
                        MoveSelectedToken(i - 1);
                        break;
                    }
                }
            }
        }

        private void HandleDiceResult(int roll)
        {
            lastDiceRoll = roll;

            // Find which tokens can legally move
            var currentPlayerTokens = allTokens.Where(t => t.PlayerId == currentPlayerId).ToList();
            _validTokens = currentPlayerTokens.Where(t => t.CanMove(roll)).ToList();

            if (_validTokens.Count == 0)
            {
                Debug.Log($"<color=red>[GameManager] No valid moves for Player {currentPlayerId} with roll {roll}</color>");
                EndTurn();
                return;
            }

            currentPhase = GamePhase.SelectingToken;

            Debug.Log($"[GameManager] Player {currentPlayerId} rolled {roll}. Select a token to move:");
            for (int i = 0; i < _validTokens.Count; i++)
            {
                int tokenNum = allTokens.IndexOf(_validTokens[i]);
                Debug.Log($"  Press {i + 1} for Token_{tokenNum}");
            }
        }

        private void MoveSelectedToken(int validListIndex)
        {
            if (validListIndex < 0 || validListIndex >= _validTokens.Count) return;

            var token = _validTokens[validListIndex];
            currentPhase = GamePhase.Moving;

            Debug.Log($"[GameManager] Moving Token...");
            token.TryMove(lastDiceRoll);

            Invoke(nameof(EndTurn), 1.5f);
        }

        private void EndTurn()
        {
            currentPhase = GamePhase.EndTurn;

            // Check win condition BEFORE switching player
            var currentPlayerTokens = allTokens.Where(t => t.PlayerId == currentPlayerId).ToList();
            if (_winChecker != null && _winChecker.HasPlayerWon(currentPlayerId, currentPlayerTokens))
            {
                Debug.Log($"<color=green>=== PLAYER {currentPlayerId} WINS ===</color>");
                currentPhase = GamePhase.GameOver;
                return;
            }

            _validTokens.Clear();

            currentPlayerId = (currentPlayerId + 1) % totalPlayers;
            currentPhase = GamePhase.Idle;

            Debug.Log($"=== Player {currentPlayerId}'s Turn ===");
            Debug.Log("Press SPACE to roll dice.");
        }
    }
}