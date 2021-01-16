using Interhaptics.Modules.Interaction_Builder.Core;

using static Interhaptics.Assets.Scripts.Extensions.OperatorExtensions;


namespace Interhaptics.Assets.UI
{

    [UnityEngine.RequireComponent(typeof(InteractionObject))]
    public class SwitchButton : UnityEngine.MonoBehaviour
    {

        #region NESTED TYPES
        [System.Serializable]
        public class SwitchEvent : UnityEngine.Events.UnityEvent<int> { }

        [System.Serializable]
        public struct SwitchState
        {
            [UnityEngine.Tooltip("State color")]
            public UnityEngine.Color color;
            [UnityEngine.Tooltip("State transform")]
            public UnityEngine.Transform snappingPoint;
            [UnityEngine.Tooltip("Event called when the state is entered")]
            public SwitchEvent onStateEnter;
            [UnityEngine.Tooltip("Event called when the state is exited")]
            public SwitchEvent onStateExit;
        }
        #endregion


        #region SERIALIZED FIELDS
        [UnityEngine.Header("Visual settings")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Button renderer")]
        private UnityEngine.Renderer buttonRenderer = null;

        [UnityEngine.Header("Switch settings")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("Initial State to take")]
        private int initialState = 0;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("List of each state possible for this button")]
        private System.Collections.Generic.List<SwitchState> switchStates =
            new System.Collections.Generic.List<SwitchState>();
        #endregion


        #region PRIVATE FIELDS
        private InteractionObject _button;
        #endregion


        #region PUBLIC MEMBERS
        // ReSharper disable once MemberCanBePrivate.Global
        public int ActualButtonState { get; private set; } = 0;

        public System.Collections.Generic.List<SwitchState> SwitchStates => switchStates;
        public UnityEngine.Renderer ButtonRenderer => buttonRenderer;
        #endregion


        #region LIFE CYCLES
        private void Awake()
        {
            _button = GetComponent<InteractionObject>();
            SwitchTo(initialState.AbsMod(switchStates.Count));
        }

        private void OnEnable()
        {
            _button.OnInteractionFinish.AddListener(() => Switch());
        }

        private void OnDisable()
        {
            _button.OnInteractionFinish.RemoveListener(() => Switch());
        }
        #endregion


        #region PRIVATE METHODS
        // ReSharper disable once MemberCanBePrivate.Global
        public void Switch(bool invokeEvent = true)
        {
            SwitchTo(ActualButtonState + 1, invokeEvent);
        }

        public void SwitchTo(int state)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            SwitchTo(state, true);
        }

        public void SwitchTo(int state, bool invokeEvent)
        {
            int oldState = ActualButtonState;
            ActualButtonState = state % switchStates.Count;
            if (oldState == ActualButtonState)
                return;

            SwitchState actualState = switchStates[ActualButtonState];
            if (_button && actualState.snappingPoint)
                _button.SnapTo(actualState.snappingPoint);
            if (buttonRenderer && buttonRenderer.material)
                buttonRenderer.material.color = actualState.color;

            if (!invokeEvent)
                return;

            switchStates[oldState].onStateExit.Invoke(oldState);
            actualState.onStateEnter.Invoke(ActualButtonState);
        }
        #endregion

    }

}