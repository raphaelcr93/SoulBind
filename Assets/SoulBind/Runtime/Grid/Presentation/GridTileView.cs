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
        private Texture2D highlightTexture;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            tileRenderer = GetComponent<Renderer>();
            highlightTexture = Resources.Load<Texture2D>("SoulBindArt/GridTileHighlight");
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
            if (!tile.IsWalkable)
                baseColor = new Color(0.16f, 0.14f, 0.12f);
            else if ((tile.Position.X + tile.Position.Y) % 5 == 0)
                baseColor = new Color(0.24f, 0.38f, 0.22f);
            else if (tile.Position.Y == 2 || tile.Position.Y == 6)
                baseColor = new Color(0.30f, 0.46f, 0.25f);
            else
                baseColor = new Color(0.20f, 0.34f, 0.19f);
            tileRenderer.material.color = baseColor;
        }

        public void SetHighlighted(bool highlighted, Color highlightColor)
        {
            if (tileRenderer == null) return;
            tileRenderer.material.color = highlighted ? highlightColor : baseColor;
            tileRenderer.material.mainTexture = highlighted ? highlightTexture : null;
        }

        #endregion
    }
}
