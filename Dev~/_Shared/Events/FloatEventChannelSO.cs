using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "FloatEventChannelSO", menuName = "Events/Float")]
public class FloatEventChannelSO : DescriptionSO
{
    public UnityAction<float> OnEventRaised = delegate { };

    public void RaiseEvent(float value)
    {
        OnEventRaised?.Invoke(value);
    }
}