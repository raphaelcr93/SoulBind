using UnityEngine;
using SoulBind.Grid;

namespace SoulBind.Grid.Presentation
{
    /// <summary>Visual representation of one domain grid tile.</summary>
    public sealed class GridTileView : MonoBehaviour
    {
        #region Fields

        private Renderer tileRenderer;
        private GridTile tile;
        private Color baseColor = Color.white;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            tileRenderer = GetComponent<Renderer>();
        }

        #endregion

        #region Binding

        public void Bind(GridTile domainTile)
        {
            tile = domainTile;
            name = $"Tile_{tile.Position.X}_{tile.Position.Y}";
            Refresh();
        }

        public void Refresh()
        {
            if (tile == null || tileRenderer == null) return;
            baseColor = tile.IsWalkable ? Color.white : Color.gray;
            tileRenderer.material.color = baseColor;
        }

        public void SetHighlighted(bool highlighted, Color highlightColor)
        {
            if (tileRenderer == null) return;
            tileRenderer.material.color = highlighted ? highlightColor : baseColor;
        }

        #endregion
    }
}
