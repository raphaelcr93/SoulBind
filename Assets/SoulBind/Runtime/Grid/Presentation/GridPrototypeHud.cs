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
            DrawUnitHealthBar(controller.PlayerTransform, controller.PlayerHealth, 10, new Color(0.15f, 0.75f, 1f));
            if (!controller.EnemyDefeated)
                DrawUnitHealthBar(controller.EnemyTransform, controller.EnemyHealth, 6, new Color(1f, 0.25f, 0.35f));
            GUILayout.BeginArea(new Rect(18f, 18f, Screen.width - 36f, 92f), GUI.skin.box);
            GUILayout.Label($"SOUL BIND    |    TURN {controller.Round}    |    PLAYER PHASE");
            GUILayout.Label($"AP  {controller.ActionPoints}/3     PLAYER HP  {controller.PlayerHealth}/10     ENEMY HP  {(controller.EnemyDefeated ? "DEFEATED" : controller.EnemyHealth.ToString())}     HAND  {controller.CardsInHand}");
            var outcome = controller.EnemyDefeated ? "Victory! Press R to restart." : controller.PlayerDefeated ? "Defeat! Press R to restart." : "Click to move/attack. Press C for a card. Press R to restart.";
            GUILayout.Label(outcome);
            GUILayout.Label(controller.LastActionMessage);
            GUILayout.EndArea();

            var controlY = 118f;
            if (GUI.Button(new Rect(Screen.width - 190f, controlY, 170f, 34f), "END TURN"))
                controller.EndTurnFromUi();
            if (GUI.Button(new Rect(Screen.width - 190f, controlY + 42f, 170f, 30f), "RESET BATTLE"))
                SendMessage("ResetPrototype", SendMessageOptions.DontRequireReceiver);

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

        private void DrawUnitHealthBar(Transform unit, int health, int maxHealth, Color fillColor)
        {
            if (unit == null || Camera.main == null) return;
            var screenPoint = Camera.main.WorldToScreenPoint(unit.position + Vector3.up * 1.6f);
            if (screenPoint.z <= 0f) return;
            var x = screenPoint.x - 48f;
            var y = Screen.height - screenPoint.y;
            var background = new Rect(x, y, 96f, 10f);
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(background, Texture2D.whiteTexture);
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(x + 2f, y + 2f, 92f * Mathf.Clamp01((float)health / maxHealth), 6f), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        #endregion
    }
}
