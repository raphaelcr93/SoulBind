using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private readonly List<GridPosition> highlightedAttackRange = new List<GridPosition>();
        private GridPosition? selectedPosition;
        private IGridPathfinder pathfinder = new DijkstraGridPathfinder();
        private Transform activeUnit;
        private Transform enemyUnit;
        private UnitState playerState;
        private UnitState enemyState;
        private readonly CombatService combatService = new CombatService();
        private readonly CardHand cardHand = new CardHand();
        private LineRenderer pathArrow;
        private GameObject pathArrowHead;

        public int ActionPoints => turnState.ActionPoints;
        public int Round => turnState.Round;
        public int PlayerHealth => playerState == null ? 0 : playerState.Health;
        public int EnemyHealth => enemyState == null ? 0 : enemyState.Health;
        public bool EnemyDefeated => enemyState != null && enemyState.IsDefeated;
        public bool PlayerDefeated => playerState != null && playerState.IsDefeated;
        public int CardsInHand => cardHand.Cards.Count;
        public Transform PlayerTransform => activeUnit;
        public Transform EnemyTransform => enemyUnit;
        public string LastActionMessage { get; private set; } = "Select a tile to move.";

        public void EndTurnFromUi()
        {
            if (PlayerDefeated || EnemyDefeated || isMoving) return;
            turnState.End();
            turnState.Begin(0, actionPointsPerTurn);
            LastActionMessage = "Turn ended. Action points restored.";
            ClearPreview();
            selectedPosition = null;
        }
        private bool isMoving;
        private TurnState turnState = new TurnState();

        public bool TryPlayCard(int index)
        {
            if (PlayerDefeated || EnemyDefeated || isMoving) return false;
            if (!cardHand.TryPlay(index, turnState))
            {
                LastActionMessage = "Not enough action points for that card.";
                return false;
            }
            switch (index)
            {
                case 0:
                    enemyState.ReceiveDamage(2);
                    if (!enemyState.IsDefeated) StartCoroutine(FlashEnemy());
                    LastActionMessage = enemyState.IsDefeated ? "Strike defeated the enemy!" : "Strike dealt 2 damage.";
                    if (enemyState.IsDefeated && enemyUnit != null) Destroy(enemyUnit.gameObject);
                    break;
                case 1:
                    var restored = playerState.RestoreHealth(3);
                    LastActionMessage = $"Guard restored {restored} health.";
                    break;
                case 2:
                    turnState.AddActionPoints(2);
                    LastActionMessage = "Focus restored 2 action points.";
                    break;
            }
            EndTurnIfSpent();
            return true;
        }

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
            pathArrow = gameObject.AddComponent<LineRenderer>();
            pathArrow.widthMultiplier = 0.08f;
            pathArrow.material = new Material(Shader.Find("Sprites/Default"));
            pathArrow.material.color = new Color(1f, 0.82f, 0.12f);
            pathArrow.positionCount = 0;
            cardHand.Add(new CardState("strike", "Strike", 1));
            cardHand.Add(new CardState("guard", "Guard", 1));
            cardHand.Add(new CardState("focus", "Focus", 1));
        }

        private void Update()
        {
            if (isMoving || gridView == null || gridView.Map == null) return;
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetPrototype();
                return;
            }
            if (PlayerDefeated || EnemyDefeated) return;
            if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame) TryPlayCard(0);
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
            var ray = inputCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
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
                    StartCoroutine(FlashEnemy());
                    LastActionMessage = enemyState.IsDefeated ? "Enemy defeated!" : "Attack landed. The enemy counterattacks.";
                    if (enemyState.IsDefeated)
                        Destroy(enemyUnit.gameObject);
                    else
                    {
                        playerState.ReceiveDamage(enemyState.Attack);
                        StartCoroutine(FlashPlayer());
                    }
                    EndTurnIfSpent();
                }
                else if (distance > attackRange)
                {
                    LastActionMessage = $"Target out of range ({distance}/{attackRange}).";
                }
                ClearPreview();
                selectedPosition = null;
                return;
            }

            if (!selectedPosition.HasValue)
            {
                selectedPosition = position;
                gridView.GetView(position).SetSelected(true);
                var playerPosition = new GridPosition(Mathf.RoundToInt(activeUnit.localPosition.x), Mathf.RoundToInt(activeUnit.localPosition.z));
                if (position == playerPosition)
                {
                    for (var x = 0; x < gridView.Width; x++)
                    for (var y = 0; y < gridView.Height; y++)
                    {
                        var candidate = new GridPosition(x, y);
                        var distance = Mathf.Abs(candidate.X - playerPosition.X) + Mathf.Abs(candidate.Y - playerPosition.Y);
                        if (distance > 0 && distance <= attackRange && gridView.Map.Get(candidate).IsWalkable)
                        {
                            highlightedAttackRange.Add(candidate);
                            gridView.GetView(candidate)?.SetHighlighted(true, new Color(0.85f, 0.25f, 0.25f));
                        }
                    }
                    LastActionMessage = "Attack range shown. Select an enemy or destination.";
                }
                return;
            }

            ClearPreview();
            highlightedPath.AddRange(pathfinder.FindPath(gridView.Map, selectedPosition.Value, position));
            if (highlightedPath.Count < 2 || !turnState.TrySpend(highlightedPath.Count - 1))
            {
                LastActionMessage = "That route costs more action points than remain.";
                highlightedPath.Clear();
                selectedPosition = null;
                return;
            }
            foreach (var pathPosition in highlightedPath)
                gridView.GetView(pathPosition)?.SetHighlighted(true, pathColor);
            DrawPathArrow(highlightedPath);
            if (highlightedPath.Count > 1) StartCoroutine(MoveUnit(highlightedPath));
            selectedPosition = null;
            LastActionMessage = "Moving along the highlighted route.";
        }

        #endregion

        #region Presentation State

        private void ClearPreview()
        {
            if (selectedPosition.HasValue)
                gridView.GetView(selectedPosition.Value)?.SetSelected(false);
            foreach (var position in highlightedPath)
                gridView.GetView(position)?.SetHighlighted(false, Color.clear);
            highlightedPath.Clear();
            foreach (var position in highlightedAttackRange)
                gridView.GetView(position)?.SetHighlighted(false, Color.clear);
            highlightedAttackRange.Clear();
            if (pathArrow != null) pathArrow.positionCount = 0;
            if (pathArrowHead != null) pathArrowHead.SetActive(false);
        }

        private void CreatePrototypeUnit()
        {
            var unitObject = new GameObject("Prototype Unit");
            unitObject.transform.SetParent(transform, false);
            unitObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            activeUnit = unitObject.transform;
            var chestPrefab = Resources.Load<GameObject>("SoulBindUnits/ChestMonster");
            if (chestPrefab != null)
            {
                var visual = Instantiate(chestPrefab, unitObject.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one * 0.35f;
            }
            else
            {
                var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallback.transform.SetParent(unitObject.transform, false);
                fallback.GetComponent<Renderer>().material.color = new Color(0.15f, 0.55f, 0.95f);
            }
        }

        private void CreatePrototypeEnemy()
        {
            var enemyObject = new GameObject("Prototype Enemy");
            enemyObject.transform.SetParent(transform, false);
            enemyObject.transform.localPosition = new Vector3(5f, 0.6f, 5f);
            var beholderPrefab = Resources.Load<GameObject>("SoulBindUnits/Beholder");
            if (beholderPrefab != null)
            {
                var visual = Instantiate(beholderPrefab, enemyObject.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one * 0.35f;
            }
            else
            {
                var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallback.transform.SetParent(enemyObject.transform, false);
                fallback.GetComponent<Renderer>().material.color = Color.magenta;
            }
            enemyUnit = enemyObject.transform;
        }

        private void ResetPrototype()
        {
            if (enemyUnit == null)
            {
                CreatePrototypeEnemy();
            }
            enemyUnit.gameObject.SetActive(true);
            TintEnemy(Color.magenta);
            activeUnit.localPosition = new Vector3(0f, 0.6f, 0f);
            playerState.Reset();
            enemyState.Reset();
            turnState.Begin(0, actionPointsPerTurn);
            selectedPosition = null;
            ClearPreview();
            LastActionMessage = "Battle reset. Select your unit to begin.";
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
            EndTurnIfSpent();
            isMoving = false;
        }

        private void EndTurnIfSpent()
        {
            if (turnState.ActionPoints > 0) return;
            turnState.End();
            turnState.Begin(0, actionPointsPerTurn);
            LastActionMessage = "New turn. Your action points are restored.";
        }

        private void DrawPathArrow(IReadOnlyList<GridPosition> path)
        {
            if (pathArrow == null || path == null || path.Count < 2) return;
            pathArrow.positionCount = path.Count;
            for (var i = 0; i < path.Count; i++)
                pathArrow.SetPosition(i, gridView.transform.TransformPoint(new Vector3(path[i].X, 0.16f, path[i].Y)));
            var headTexture = Resources.Load<Texture2D>("SoulBindArt/GridTilePathArrowHead");
            if (headTexture == null) return;
            if (pathArrowHead == null)
            {
                pathArrowHead = new GameObject("Path Arrow Head");
                pathArrowHead.transform.SetParent(gridView.transform, false);
                var spriteRenderer = pathArrowHead.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = Sprite.Create(headTexture, new Rect(0f, 0f, headTexture.width, headTexture.height), new Vector2(0.5f, 0.5f), headTexture.width);
            }
            pathArrowHead.SetActive(true);
            var finalPosition = path[path.Count - 1];
            pathArrowHead.transform.localPosition = new Vector3(finalPosition.X, 0.22f, finalPosition.Y);
            pathArrowHead.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            pathArrowHead.transform.localScale = Vector3.one * 0.012f;
        }

        private void TintEnemy(Color color)
        {
            if (enemyUnit == null) return;
            foreach (var renderer in enemyUnit.GetComponentsInChildren<Renderer>())
                renderer.material.color = color;
        }

        private System.Collections.IEnumerator FlashEnemy()
        {
            TintEnemy(Color.red);
            yield return new WaitForSeconds(0.18f);
            if (enemyUnit != null && enemyUnit.gameObject.activeInHierarchy)
                TintEnemy(Color.magenta);
        }

        private System.Collections.IEnumerator FlashPlayer()
        {
            TintPlayer(Color.red);
            yield return new WaitForSeconds(0.18f);
            if (activeUnit != null && activeUnit.gameObject.activeInHierarchy)
                TintPlayer(Color.white);
        }

        private void TintPlayer(Color color)
        {
            if (activeUnit == null) return;
            foreach (var renderer in activeUnit.GetComponentsInChildren<Renderer>())
                renderer.material.color = color;
        }

        #endregion
    }
}
