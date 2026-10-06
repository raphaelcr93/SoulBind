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
            GUILayout.BeginArea(new Rect(18f, 18f, Screen.width - 36f, 92f), GUI.skin.box);
            GUILayout.Label("SOUL BIND    |    TURN 1    |    PLAYER PHASE");
            GUILayout.Label($"AP  {controller.ActionPoints}/3     PLAYER HP  {controller.PlayerHealth}/10     ENEMY HP  {(controller.EnemyDefeated ? "DEFEATED" : controller.EnemyHealth.ToString())}     HAND  {controller.CardsInHand}");
            var outcome = controller.EnemyDefeated ? "Victory! Press R to restart." : controller.PlayerDefeated ? "Defeat! Press R to restart." : "Click to move/attack. Press C for a card. Press R to restart.";
            GUILayout.Label(outcome);
            GUILayout.EndArea();
        }

        #endregion
    }
}
