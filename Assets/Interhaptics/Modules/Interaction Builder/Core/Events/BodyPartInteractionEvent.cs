using Interhaptics.Modules.Interaction_Builder.Core.Abstract;

using UnityEngine.Events;


namespace Interhaptics.Modules.Interaction_Builder.Core.Events
{

    [System.Serializable]
    public abstract class BodyPartInteractionEvent : UnityEvent<AInteractionBodyPart, InteractionObject>
    {}

}