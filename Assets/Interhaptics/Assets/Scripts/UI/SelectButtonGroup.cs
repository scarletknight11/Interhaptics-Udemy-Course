namespace Interhaptics.Assets.UI
{

    public sealed class SelectButtonGroup : UnityEngine.MonoBehaviour
    {

        #region NESTED TYPES
        [System.Serializable]
        public struct SSelectButtonItem
        {

            #region PUBLIC FIELDS
            [UnityEngine.Tooltip("SwitchButton for this item")]
            public SwitchButton button;
            [UnityEngine.Range(0, 1)]
            [UnityEngine.Tooltip("State in which the item is selected")]
            public int selectedState;
            [UnityEngine.Tooltip("Event called on this item selection")]
            public UnityEngine.Events.UnityEvent selectionEvent;
            #endregion


            #region PUBLIC MEMBERS
            public int NotSelectedState => UnityEngine.Mathf.Abs(selectedState - 1);
            #endregion

        }
        #endregion


        #region SERIALIZED FIELDS
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("The item selected by default at the beginning (-1 if you don't want one)")]
        private int initialSelectedItem = -1;
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("List of select button items used for this group (each button is a switch button with only 2 states)")]
        private System.Collections.Generic.List<SSelectButtonItem> selectButtonItems =
            new System.Collections.Generic.List<SSelectButtonItem>();
        #endregion


        #region PUBLIC MEMBERS
        // ReSharper disable once MemberCanBePrivate.Global
        public SSelectButtonItem? SelectedItem { get; private set; } = null;
        public SwitchButton SelectedButton => SelectedItem?.button;
        #endregion


        #region LIFE CYCLES
        private void Awake()
        {
            foreach (SSelectButtonItem selectItem in selectButtonItems)
            {
                int stateCount = selectItem.button.SwitchStates.Count;
                if (stateCount < 2)
                {
                    UnityEngine.Color buttonColor = selectItem.button.ButtonRenderer.material.color;
                    if (stateCount == 0)
                        selectItem.button.SwitchStates.Add(new SwitchButton.SwitchState
                        {
                            color = buttonColor,
                            snappingPoint = selectItem.button.transform,
                            onStateEnter = new SwitchButton.SwitchEvent(),
                            onStateExit = new SwitchButton.SwitchEvent()
                        });

                    UnityEngine.Color.RGBToHSV(buttonColor, out float hue, out float saturation, out float value);
                    buttonColor = UnityEngine.Color.HSVToRGB((hue + .5f) % 1, saturation, value);

                    selectItem.button.SwitchStates.Add(new SwitchButton.SwitchState
                    {
                        color = buttonColor,
                        snappingPoint = selectItem.button.transform,
                        onStateEnter = new SwitchButton.SwitchEvent(),
                        onStateExit = new SwitchButton.SwitchEvent()
                    });
                }

                selectItem.button.SwitchTo(selectItem.NotSelectedState);
            }

            if (initialSelectedItem < 0 || initialSelectedItem >= selectButtonItems.Count)
                return;

            SSelectButtonItem defaultSelection = selectButtonItems[initialSelectedItem];
            defaultSelection.button.SwitchTo(defaultSelection.selectedState);
        }

        private void OnEnable()
        {
            foreach (SSelectButtonItem item in selectButtonItems)
                foreach (SwitchButton.SwitchState state in item.button.SwitchStates)
                    state.onStateEnter.AddListener(i => CheckButtons(item.button));
        }

        private void OnDisable()
        {
            foreach (SSelectButtonItem item in selectButtonItems)
                foreach (SwitchButton.SwitchState state in item.button.SwitchStates)
                    state.onStateEnter.RemoveListener(i => CheckButtons(item.button));
        }
        #endregion


        #region PRIVATE METHODS
        private void CheckButtons(UnityEngine.Object caller)
        {
            foreach (SSelectButtonItem item in selectButtonItems)
            {
                bool isSelected = item.button == caller;
                item.button.SwitchTo(isSelected ? item.selectedState : item.NotSelectedState, false);
                if (isSelected)
                    SelectedItem = item;
            }

            SelectedItem?.selectionEvent.Invoke();
        }
        #endregion

    }

}