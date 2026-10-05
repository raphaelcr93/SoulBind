using System.Collections.Generic;
using UnityEngine;
using SoulBind.Grid;

namespace SoulBind.Grid.Presentation
{
    /// <summary>
    /// Owns the Unity presentation of a domain grid. It intentionally creates
    /// primitive visuals so the system can be tested before art or prefabs exist.
    /// </summary>
    public sealed class GridView : MonoBehaviour
    {
        #region Configuration

        [Header("Grid Size")]
        [Min(1)] [SerializeField] private int width = 8;
        [Min(1)] [SerializeField] private int height = 8;
        [Min(0.1f)] [SerializeField] private float tileSize = 1f;
        [SerializeField] private bool buildOnAwake = true;

        #endregion

        #region State

        private readonly List<GridTileView> tileViews = new List<GridTileView>();
        private GridMap map;

        public GridMap Map => map;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (buildOnAwake) Build();
        }

        #endregion

        #region Construction

        [ContextMenu("Rebuild Grid")]
        public void Build()
        {
            Clear();
            map = new GridMap(width, height, new GridTileDefinition("default", 1, true));

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tileObject.transform.SetParent(transform, false);
                tileObject.transform.localPosition = new Vector3(x * tileSize, 0f, y * tileSize);
                tileObject.transform.localScale = new Vector3(tileSize * 0.95f, 0.1f, tileSize * 0.95f);

                var view = tileObject.AddComponent<GridTileView>();
                view.Bind(map.Get(new GridPosition(x, y)));
                tileViews.Add(view);
            }
        }

        private void Clear()
        {
            for (var i = tileViews.Count - 1; i >= 0; i--)
                if (tileViews[i] != null) DestroyImmediate(tileViews[i].gameObject);
            tileViews.Clear();
        }

        #endregion
    }
}
