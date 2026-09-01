using UnityEngine;
using RuralGames.Board;
using RuralGames.Rules;

namespace RuralGames.Token
{
    public class TokenController : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private int playerId = 0;
        public int PlayerId => playerId;

        [SerializeField] private Color playerColor = Color.red;

        [Header("State")]
        [SerializeField] private TokenState currentState = TokenState.InBase;
        public TokenState CurrentState => currentState;

        [SerializeField] private int currentBoardIndex = -1;
        public int CurrentBoardIndex => currentBoardIndex;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        private BoardManager _board;
        private RuleManager _rules;
        private Vector3 _targetPosition;
        private bool _isMoving = false;

        private void Awake()
        {
            _board = UnityEngine.Object.FindAnyObjectByType<BoardManager>();
            _rules = UnityEngine.Object.FindAnyObjectByType<RuleManager>();

            if (_board == null) Debug.LogError("[Token] BoardManager not found!");
            if (_rules == null) Debug.LogError("[Token] RuleManager not found!");

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
                sr.sprite = CreateCircleSprite();
            }
            sr.color = playerColor;

            Debug.Log($"[Token] Awake complete. Position: {transform.position}");
        }

        private void Update()
        {
            if (_isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
                {
                    transform.position = _targetPosition;
                    _isMoving = false;
                    Debug.Log($"[Token] Arrived at position: {transform.position}, Index: {currentBoardIndex}");
                }
            }
        }

        public bool CanMove(int diceValue)
        {
            if (_board == null || _rules == null) return false;

            int targetIndex = _board.CalculateTargetIndex(playerId, currentBoardIndex, diceValue, currentState);
            if (targetIndex == -1) return false;

            var context = new RuleContext(playerId, 0, diceValue, currentState, currentBoardIndex, targetIndex);
            return _rules.IsMoveValid(context);
        }

        public bool TryMove(int diceValue)
        {
            if (!CanMove(diceValue)) return false;

            int targetIndex = _board.CalculateTargetIndex(playerId, currentBoardIndex, diceValue, currentState);
            ExecuteMove(targetIndex, diceValue);
            return true;
        }

        private void ExecuteMove(int targetIndex, int diceValue)
        {
            if (currentState == TokenState.InBase && diceValue == 6)
            {
                currentState = TokenState.OnBoard;
                currentBoardIndex = 0;
            }
            else if (targetIndex >= 52)
            {
                currentState = TokenState.InHomePath;
                currentBoardIndex = targetIndex;
            }
            else
            {
                currentState = TokenState.OnBoard;
                currentBoardIndex = targetIndex;
            }

            _targetPosition = _board.GetPosition(playerId, currentBoardIndex, currentState);
            _targetPosition.z = 0;
            _isMoving = true;

            Debug.Log($"[Token] Moving to index {currentBoardIndex}, target pos: {_targetPosition}");

            // Check for capture
            var allTokens = UnityEngine.Object.FindObjectsByType<TokenController>();
            foreach (var other in allTokens)
            {
                if (other.PlayerId != this.playerId
                    && other.CurrentState == TokenState.OnBoard
                    && other.CurrentBoardIndex == this.currentBoardIndex)
                {
                    other.SetState(TokenState.InBase, -1);
                    Debug.Log($"<color=red>[CAPTURE] Player {playerId} captured Player {other.PlayerId}'s token at index {currentBoardIndex}!</color>");
                }
            }
        }

        public void SetState(TokenState state, int boardIndex)
        {
            currentState = state;
            currentBoardIndex = boardIndex;
            if (_board != null)
            {
                transform.position = _board.GetPosition(playerId, boardIndex, state);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            }
        }

        private Sprite CreateCircleSprite()
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            Vector2 center = new Vector2(32, 32);
            float radius = 30f;

            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                    pixels[y * 64 + x] = Vector2.Distance(new Vector2(x, y), center) < radius ? Color.white : Color.clear;

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }
    }
}