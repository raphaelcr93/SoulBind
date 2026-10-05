using UnityEngine;

namespace SoulBind.Grid.Presentation
{
    /// <summary>Temporary readable HUD for validating the gameplay loop before final UI work.</summary>
    public sealed class GridPrototypeHud : MonoBehaviour
    {
        #region Configuration

        [SerializeField] private GridInteractionController controller;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (controller == null) controller = FindObjectOfType<GridInteractionController>();
        }

        private void OnGUI()
        {
            if (controller == null) return;
            GUILayout.BeginArea(new Rect(16f, 16f, 300f, 140f), GUI.skin.box);
            GUILayout.Label("SOUL BIND - GRID PROTOTYPE");
            GUILayout.Label($"Action Points: {controller.ActionPoints}");
            GUILayout.Label($"Player Health: {controller.PlayerHealth}");
            GUILayout.Label($"Enemy Health: {(controller.EnemyDefeated ? "DEFEATED" : controller.EnemyHealth.ToString())}");
            GUILayout.Label($"Cards in Hand: {controller.CardsInHand}");
            GUILayout.Label(controller.EnemyDefeated ? "Victory! Press R to restart." : "Click to move/attack. Press C for a card. Press R to restart.");
            GUILayout.EndArea();
        }

        #endregion
    }
}
