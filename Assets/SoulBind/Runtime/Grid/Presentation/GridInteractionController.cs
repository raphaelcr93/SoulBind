using System.Collections.Generic;
using UnityEngine;
using SoulBind.Grid;
using SoulBind.Turn;
using SoulBind.Units;
using SoulBind.Cards;

namespace SoulBind.Grid.Presentation
{
    /// <summary>Provides simple click-to-select and click-to-preview-path interaction.</summary>
    public sealed class GridInteractionController : MonoBehaviour
    {
        #region Configuration

        [SerializeField] private GridView gridView;
        [SerializeField] private Camera inputCamera;
        [SerializeField] private Color selectedColor = Color.yellow;
        [SerializeField] private Color pathColor = Color.cyan;
        [SerializeField] private float moveDurationPerTile = 0.15f;
        [SerializeField] private int actionPointsPerTurn = 3;
        [SerializeField] private int attackRange = 3;

        #endregion

        #region State

        private readonly List<GridPosition> highlightedPath = new List<GridPosition>();
        private GridPosition? selectedPosition;
        private IGridPathfinder pathfinder = new DijkstraGridPathfinder();
        private Transform activeUnit;
        private Transform enemyUnit;
        private UnitState playerState;
        private UnitState enemyState;
        private readonly CombatService combatService = new CombatService();
        private readonly CardHand cardHand = new CardHand();

        public int ActionPoints => turnState.ActionPoints;
        public int PlayerHealth => playerState == null ? 0 : playerState.Health;
        public int EnemyHealth => enemyState == null ? 0 : enemyState.Health;
        public bool EnemyDefeated => enemyState != null && enemyState.IsDefeated;
        public bool PlayerDefeated => playerState != null && playerState.IsDefeated;
        public int CardsInHand => cardHand.Cards.Count;
        private bool isMoving;
        private TurnState turnState = new TurnState();

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (gridView == null) gridView = GetComponent<GridView>();
            if (inputCamera == null) inputCamera = Camera.main;
            CreatePrototypeUnit();
            CreatePrototypeEnemy();
            playerState = new UnitState("player", "Player", 3, 10);
            enemyState = new UnitState("enemy", "Enemy", 2, 6);
            turnState.Begin(0, actionPointsPerTurn);
            cardHand.Add(new CardState("strike", "Strike", 1));
            cardHand.Add(new CardState("guard", "Guard", 1));
            cardHand.Add(new CardState("focus", "Focus", 1));
        }

        private void Update()
        {
            if (isMoving || gridView == null || gridView.Map == null) return;
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetPrototype();
                return;
            }
            if (PlayerDefeated || EnemyDefeated) return;
            if (Input.GetKeyDown(KeyCode.C)) cardHand.TryPlay(0, turnState);
            if (!Input.GetMouseButtonDown(0)) return;
            var ray = inputCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return;
            var local = gridView.transform.InverseTransformPoint(hit.point);
            var position = new GridPosition(Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.z));
            if (!gridView.Map.Contains(position)) return;

            if (position == new GridPosition(5, 5) && enemyUnit != null)
            {
                var playerGridPosition = new GridPosition(Mathf.RoundToInt(activeUnit.localPosition.x), Mathf.RoundToInt(activeUnit.localPosition.z));
                var distance = Mathf.Abs(playerGridPosition.X - position.X) + Mathf.Abs(playerGridPosition.Y - position.Y);
                if (distance <= attackRange && combatService.TryAttack(playerState, enemyState, turnState))
                {
                    enemyUnit.GetComponent<Renderer>().material.color = Color.red;
                    if (enemyState.IsDefeated)
                        Destroy(enemyUnit.gameObject);
                    else
                        playerState.ReceiveDamage(enemyState.Attack);
                }
                selectedPosition = null;
                ClearPreview();
                return;
            }

            if (!selectedPosition.HasValue)
            {
                selectedPosition = position;
                gridView.GetView(position).SetHighlighted(true, selectedColor);
                return;
            }

            ClearPreview();
            highlightedPath.AddRange(pathfinder.FindPath(gridView.Map, selectedPosition.Value, position));
            if (highlightedPath.Count < 2 || !turnState.TrySpend(highlightedPath.Count - 1))
            {
                highlightedPath.Clear();
                selectedPosition = null;
                return;
            }
            foreach (var pathPosition in highlightedPath)
                gridView.GetView(pathPosition)?.SetHighlighted(true, pathColor);
            if (highlightedPath.Count > 1) StartCoroutine(MoveUnit(highlightedPath));
            selectedPosition = null;
        }

        #endregion

        #region Presentation State

        private void ClearPreview()
        {
            foreach (var position in highlightedPath)
                gridView.GetView(position)?.SetHighlighted(false, Color.clear);
            highlightedPath.Clear();
        }

        private void CreatePrototypeUnit()
        {
            var unitObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unitObject.name = "Prototype Unit";
            unitObject.transform.SetParent(transform, false);
            unitObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            activeUnit = unitObject.transform;
        }

        private void CreatePrototypeEnemy()
        {
            var enemyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObject.name = "Prototype Enemy";
            enemyObject.transform.SetParent(transform, false);
            enemyObject.transform.localPosition = new Vector3(5f, 0.6f, 5f);
            enemyObject.GetComponent<Renderer>().material.color = Color.magenta;
            enemyUnit = enemyObject.transform;
        }

        private void ResetPrototype()
        {
            if (enemyUnit == null)
            {
                CreatePrototypeEnemy();
            }
            enemyUnit.gameObject.SetActive(true);
            enemyUnit.GetComponent<Renderer>().material.color = Color.magenta;
            playerState.Reset();
            enemyState.Reset();
            turnState.Begin(0, actionPointsPerTurn);
            selectedPosition = null;
            ClearPreview();
        }

        private System.Collections.IEnumerator MoveUnit(IReadOnlyList<GridPosition> path)
        {
            isMoving = true;
            foreach (var position in path)
            {
                var destination = gridView.transform.TransformPoint(new Vector3(position.X, 0.6f, position.Y));
                while (Vector3.Distance(activeUnit.position, destination) > 0.01f)
                {
                    activeUnit.position = Vector3.MoveTowards(activeUnit.position, destination, Time.deltaTime / moveDurationPerTile);
                    yield return null;
                }
            }
            ClearPreview();
            turnState.End();
            turnState.Begin(0, actionPointsPerTurn);
            isMoving = false;
        }

        #endregion
    }
}
