using UnityEngine;
using RuralGames.Board;
using RuralGames.Rules;

namespace RuralGames.Token
{
    public class TokenController : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private int playerId = 0;
        [SerializeField] private Color playerColor = Color.red;

        [Header("State")]
        [SerializeField] private TokenState currentState = TokenState.InBase;
        [SerializeField] private int currentBoardIndex = -1;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        private BoardManager _board;
        private RuleManager _rules;
        private Vector3 _targetPosition;
        private bool _isMoving = false;

        private void Awake()
        {
            _board = Object.FindAnyObjectByType<BoardManager>();
            _rules = Object.FindAnyObjectByType<RuleManager>();

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

        public bool TryMove(int diceValue)
        {
            if (_board == null || _rules == null)
            {
                Debug.LogError("[Token] Board or Rules missing!");
                return false;
            }

            int targetIndex = _board.CalculateTargetIndex(playerId, currentBoardIndex, diceValue, currentState);
            Debug.Log($"[Token] TryMove: state={currentState}, dice={diceValue}, targetIndex={targetIndex}");

            if (targetIndex == -1)
            {
                Debug.Log("[Token] Target invalid (-1).");
                return false;
            }

            var context = new RuleContext(playerId, 0, diceValue, currentState, currentBoardIndex, targetIndex);
            bool valid = _rules.IsMoveValid(context);
            Debug.Log($"[Token] IsMoveValid returned: {valid}");

            if (!valid) return false;

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
        }

        public void SetState(TokenState state, int boardIndex)
        {
            currentState = state;
            currentBoardIndex = boardIndex;
            Vector3 pos = _board != null ? _board.GetPosition(playerId, boardIndex, state) : Vector3.zero;
            transform.position = new Vector3(pos.x, pos.y, 0);
            Debug.Log($"[Token] SetState: {state}, index {boardIndex}, pos: {transform.position}");
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