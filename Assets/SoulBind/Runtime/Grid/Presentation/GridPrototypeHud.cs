using UnityEngine;

namespace SoulBind.Grid.Presentation
{
    /// <summary>Temporary readable HUD for validating the gameplay loop before final UI work.</summary>
    public sealed class GridPrototypeHud : MonoBehaviour
    {
        #region Configuration

        [SerializeField] private GridInteractionController controller;
        private Texture2D cardBackground;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (controller == null) controller = FindObjectOfType<GridInteractionController>();
            cardBackground = Resources.Load<Texture2D>("SoulBindArt/CardBackgroundTexture");
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

            var cardWidth = 150f;
            var cardHeight = 190f;
            var handY = Screen.height - cardHeight - 24f;
            for (var i = 0; i < controller.CardsInHand; i++)
            {
                var cardRect = new Rect(24f + i * (cardWidth + 14f), handY, cardWidth, cardHeight);
                if (cardBackground != null) GUI.DrawTexture(cardRect, cardBackground, ScaleMode.StretchToFill);
                else GUI.Box(cardRect, GUIContent.none);
                GUI.Label(new Rect(cardRect.x + 12f, cardRect.y + 18f, cardWidth - 24f, 28f), i == 0 ? "STRIKE" : i == 1 ? "GUARD" : "FOCUS");
                GUI.Label(new Rect(cardRect.x + 12f, cardRect.y + 64f, cardWidth - 24f, 64f), i == 0 ? "Deal damage to an enemy." : i == 1 ? "Prepare for the next hit." : "Restore tactical momentum.");
                GUI.Label(new Rect(cardRect.x + 12f, cardRect.yMax - 34f, cardWidth - 24f, 22f), "COST 1 AP");
                if (GUI.Button(new Rect(cardRect.x + 12f, cardRect.yMax - 62f, cardWidth - 24f, 24f), "PLAY CARD"))
                    controller.TryPlayCard(i);
            }
        }

        #endregion
    }
}
