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

        #endregion

        #region State

        private readonly List<GridPosition> highlightedPath = new List<GridPosition>();
        private GridPosition? selectedPosition;
        private IGridPathfinder pathfinder = new DijkstraGridPathfinder();

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (gridView == null) gridView = GetComponent<GridView>();
            if (inputCamera == null) inputCamera = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) || gridView == null || gridView.Map == null) return;
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

        #endregion
    }
}
