using UnityEngine;
using UnityEngine.Events;

namespace Border.Events
{
    /// <summary>
    /// Floating world-space text event channel. Payload: (text, color, worldPosition). A HUD
    /// listener spawns a floating label at the given position — damage numbers, pickup notices,
    /// status callouts, etc. Carries no game-specific types.
    /// </summary>
    [CreateAssetMenu(fileName = "FloatingHudEventChannelSO", menuName = "Border/Events/Floating Hud")]
    public class FloatingHudEventChannelSO : ScriptableObject
    {
        public UnityAction<string, Color, Vector3> OnEventRaised = delegate { };

        public void RaiseEvent(string text, Color color, Vector3 position) => OnEventRaised?.Invoke(text, color, position);
    }
}
