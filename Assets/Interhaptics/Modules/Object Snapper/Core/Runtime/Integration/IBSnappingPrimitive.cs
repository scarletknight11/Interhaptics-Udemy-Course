using UnityEngine;

namespace Interhaptics.ObjectSnapper.core
{
    [UnityEngine.AddComponentMenu("Interhaptics/Object Snapper/IBSnappingPrimitive")]
    public class IBSnappingPrimitive : SnappingPrimitive
    {
        #region Constants
        //Shape
        private const float VALUE_PrimaryRadius = 0.05f;
        private const float VALUE_SecondaryRadius = 0.02f;
        private const float VALUE_Length = 0.1f;

        //Snapping
        private const float VALUE_TrackingRadius = 0.0125f;
        private const float VALUE_TrackingDistance = 0.08f;
        private const float VALUE_AxisLength = 1.5f;
        #endregion

        #region Life Cycle
        protected override void OnReset()
        {
            //Shape
            primaryColor = new Color(0.310f, 0.2f, 1f, 0.5f);
            secondaryColor = new Color(1f, 0.69f, 1f, 0.5f);

            //Snapping
            forwardAxis = Axis.X;
            upwardAxis = Axis.minusY;

            trackingColor = new Color(1f, 1f, 1f, 0.5f);
            trackingRadius = VALUE_TrackingRadius;
            trackingDistance = VALUE_TrackingDistance;
            trackingAxisLength = VALUE_AxisLength;
        }

        protected override void OnResetShapeValues()
        {
            //Shape
            primaryRadius = VALUE_PrimaryRadius;
            secondaryRadius = VALUE_SecondaryRadius;
            length = VALUE_Length;
        }
        #endregion
    }
}
