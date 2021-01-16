using Interhaptics.Modules.Interaction_Builder.Core;
using UnityEngine;

public class SimpleCar : MonoBehaviour
{
    #region Variables
    [Header("Front")]
    [SerializeField] private InteractionObject steeringWheel = null;
    [SerializeField] [Range(10, 60)] private float maxRotation = 30f;
    [SerializeField] private bool invertedSW = true;
    [SerializeField] private WheelCollider frontLeft = null;
    [SerializeField] private Transform visualFrontLeft = null;
    [SerializeField] private WheelCollider frontRight = null;
    [SerializeField] private Transform visualFrontRight = null;
    [Header("Back")]
    [SerializeField] private InteractionObject lever = null;
    [SerializeField] [Range(10, 500)] private float maxForwardSpeed = 200f;
    [SerializeField] [Range(10, 500)] private float maxBackwardSpeed = 100f;
    [SerializeField] private bool invertedL = true;
    [SerializeField] private WheelCollider backLeft = null;
    [SerializeField] private Transform visualBackLeft = null;
    [SerializeField] private WheelCollider backRight = null;
    [SerializeField] private Transform visualBackRight = null;
    #endregion

    #region Life cycle
    private void Update()
    {
        //Steering Wheel
        if(steeringWheel && frontLeft && frontRight)
        {
            float minimalConstraint = steeringWheel.InteractionPrimitive.minimalConstraint;
            float maximalConstraint = steeringWheel.InteractionPrimitive.maximalConstraint;

            float rotation = 0;
            if (minimalConstraint != maximalConstraint)
            {
                float constraintNormalization = (((steeringWheel.GetDistanceFromObjectOriginPoint() - minimalConstraint) / (maximalConstraint - minimalConstraint)) - 0.5f) * 2;
                if (invertedSW)
                    constraintNormalization *= -1;

                rotation = maxRotation * constraintNormalization;
                frontLeft.steerAngle = rotation;
                frontRight.steerAngle = rotation;
            }
        }

        //Motor
        if (lever && backLeft && backRight)
        {
            float minimalConstraint = lever.InteractionPrimitive.minimalConstraint;
            float maximalConstraint = lever.InteractionPrimitive.maximalConstraint;

            float rotation = 0;
            if (minimalConstraint != maximalConstraint)
            {
                float constraintNormalization = (((lever.GetDistanceFromObjectOriginPoint() - minimalConstraint) / (maximalConstraint - minimalConstraint)) - 0.5f) * 2;
                if (invertedL)
                    constraintNormalization *= -1;

                float speed = constraintNormalization < 0 ? maxBackwardSpeed : maxForwardSpeed;
                rotation = speed * constraintNormalization;

                bool rpmCheck = (backRight.rpm > 500 || backLeft.rpm > 500);
                backLeft.motorTorque = rpmCheck ? 0 : rotation;
                backRight.motorTorque = rpmCheck ? 0 : rotation;
                frontLeft.motorTorque = rpmCheck ? 0 : rotation;
                frontRight.motorTorque = rpmCheck ? 0 : rotation;

            }

            //Wheels
            Quaternion quaternion;
            Vector3 position;
            frontLeft.GetWorldPose(out position, out quaternion);
            if (visualFrontLeft) visualFrontLeft.SetPositionAndRotation(position, quaternion);
            frontRight.GetWorldPose(out position, out quaternion);
            if (visualFrontRight) visualFrontRight.SetPositionAndRotation(position, quaternion);
            backLeft.GetWorldPose(out position, out quaternion);
            if (visualBackLeft) visualBackLeft.SetPositionAndRotation(position, quaternion);
            backRight.GetWorldPose(out position, out quaternion);
            if (visualBackRight) visualBackRight.SetPositionAndRotation(position, quaternion);
        }
    }
    #endregion
}
