using System.Collections.Generic;
using UnityEngine;
using SoulBind.Grid;

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

        #endregion

        #region State

        private readonly List<GridPosition> highlightedPath = new List<GridPosition>();
        private GridPosition? selectedPosition;
        private IGridPathfinder pathfinder = new DijkstraGridPathfinder();
        private Transform activeUnit;
        private bool isMoving;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (gridView == null) gridView = GetComponent<GridView>();
            if (inputCamera == null) inputCamera = Camera.main;
            CreatePrototypeUnit();
        }

        private void Update()
        {
            if (isMoving || !Input.GetMouseButtonDown(0) || gridView == null || gridView.Map == null) return;
            var ray = inputCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return;
            var local = gridView.transform.InverseTransformPoint(hit.point);
            var position = new GridPosition(Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.z));
            if (!gridView.Map.Contains(position)) return;

            if (!selectedPosition.HasValue)
            {
                selectedPosition = position;
                gridView.GetView(position).SetHighlighted(true, selectedColor);
                return;
            }

            ClearPreview();
            highlightedPath.AddRange(pathfinder.FindPath(gridView.Map, selectedPosition.Value, position));
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
            isMoving = false;
        }

        #endregion
    }
}
