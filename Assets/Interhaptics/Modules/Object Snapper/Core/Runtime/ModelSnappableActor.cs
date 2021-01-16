using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    public class ModelSnappableActor : ASnappableActor
    {
        #region Variables
        [SerializeField] private Animator animator = null;
        #endregion

        #region Override
        protected override void Awake() { }

        public override Animator Animator { get => animator; protected set => base.Animator = value; }
        #endregion
    }
}
