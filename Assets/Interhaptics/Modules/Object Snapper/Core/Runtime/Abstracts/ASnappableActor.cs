using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    [RequireComponent(typeof(Animator))]
    public abstract class ASnappableActor : MonoBehaviour
    {
        #region Variables
        //[SerializeField] [Range(0.01f, 5f)] private float movementLerpSpeed = 0.5f;

        private Animator _animator = null;
        private SnappingObject _currentSnappingObject = null;
        #endregion

        #region Life Cycle
        protected virtual void Awake()
        {
            _animator = gameObject.GetComponent<Animator>();
        }
        #endregion

        #region Protecteds
        /// <summary>
        /// If the ASnappableActor interaction is starting or finishing, call this function will automatically subscribe to or unsubscribe from the SnappingObject.
        /// </summary>
        /// <param name="snappingObject">The SnappingObject that need to be handled by the hand</param>
        protected void OnInteractionStateChanging(SnappingObject snappingObject)
        {
            if (!_animator || (snappingObject == _currentSnappingObject))
                return;

            if (_currentSnappingObject)
            {
                this.OnUnsubscribeSnappingObject(_currentSnappingObject);

                _currentSnappingObject.UnsubscribeASnappableActor(this);
                _currentSnappingObject = null;
            }

            if (snappingObject)
            {
                snappingObject.SubscribeASnappableActor(this);
                this.OnSubscribeSnappingObject(snappingObject);
            }

            _currentSnappingObject = snappingObject;
        }

        /// <summary>
        /// If the ASnappableActor already subscribed to a SnappingObject, it will refresh the snapping position and rotation of the ASnappableActor.
        /// </summary>
        protected void RefreshSnapping()
        {
            if (!_currentSnappingObject || !_animator)
                return;

            _currentSnappingObject.SetObjectMasterSpatialRepresentation(this);
        }
        #endregion

        #region Publics
        /// <summary>
        /// Linked Animator.
        /// </summary>
        public virtual Animator Animator { get { return _animator; } protected set { _animator = value; } }
        #endregion

        #region Virtuals
        /// <summary>
        /// Called when the ASnappableActor is subscribing to the SnappingObject.
        /// </summary>
        /// <param name="snappingObject">The SnappingObject requested by the ASnappableActor</param>
        protected virtual void OnSubscribeSnappingObject(SnappingObject snappingObject) { }

        /// <summary>
        /// Called when the ASnappableActor is unsubscribing from the SnappingObject.
        /// </summary>
        /// <param name="snappingObject">The SnappingObject requested by the ASnappableActor</param>
        protected virtual void OnUnsubscribeSnappingObject(SnappingObject snappingObject) { }
        #endregion
    }
}
