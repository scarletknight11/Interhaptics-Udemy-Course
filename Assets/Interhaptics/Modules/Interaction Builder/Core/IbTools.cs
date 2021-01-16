using Interhaptics.InteractionsEngine.Shared.Types;


namespace Interhaptics.Modules.Interaction_Builder.Core
{
    public static class IbTools
    {

        #region Consts
        public const float FLOATING_PRECISION = 0.0001f;
        #endregion


        #region Convert Types
        /// <summary>
        ///     Convert a BodyPart into a BodyPartInteractionStrategy 
        /// </summary>
        /// <param name="bodyPart">A body part</param>
        /// <returns>Return the matching strategy</returns>
        public static BodyPartInteractionStrategy Convert(BodyPart bodyPart)
        {
            switch (bodyPart)
            {
                case BodyPart.RightHand:
                    return BodyPartInteractionStrategy.RightHand;
                case BodyPart.LeftHand:
                    return BodyPartInteractionStrategy.LeftHand;
                case BodyPart.Head:
                    return BodyPartInteractionStrategy.Head;
                case BodyPart.RightFingerThumb:
                    return BodyPartInteractionStrategy.RightFingerThumb;
                case BodyPart.RightFingerIndex:
                    return BodyPartInteractionStrategy.RightFingerIndex;
                case BodyPart.RightFingerMiddle:
                    return BodyPartInteractionStrategy.RightFingerMiddle;
                case BodyPart.RightFingerRing:
                    return BodyPartInteractionStrategy.RightFingerRing;
                case BodyPart.RightFingerLittle:
                    return BodyPartInteractionStrategy.RightFingerLittle;
                case BodyPart.LeftFingerThumb:
                    return BodyPartInteractionStrategy.LeftFingerThumb;
                case BodyPart.LeftFingerIndex:
                    return BodyPartInteractionStrategy.LeftFingerIndex;
                case BodyPart.LeftFingerMiddle:
                    return BodyPartInteractionStrategy.LeftFingerMiddle;
                case BodyPart.LeftFingerRing:
                    return BodyPartInteractionStrategy.LeftFingerRing;
                case BodyPart.LeftFingerLittle:
                    return BodyPartInteractionStrategy.LeftFingerLittle;
                default:
                    return BodyPartInteractionStrategy.None;
            }
        }

        /// <summary>
        ///     Convert a Quaternion from Unity into a Quaternion from System 
        /// </summary>
        /// <param name="q">A quaternion</param>
        /// <returns>Return the matching Quaternion</returns>
        public static System.Numerics.Quaternion Convert(UnityEngine.Quaternion q)
        {
            return new System.Numerics.Quaternion
            {
                X = q.x,
                Y = q.y,
                Z = q.z,
                W = q.w
            };
        }

        /// <summary>
        ///     Convert a Quaternion from System into a Quaternion from Unity 
        /// </summary>
        /// <param name="q">A quaternion</param>
        /// <returns>Return the matching Quaternion</returns>
        public static UnityEngine.Quaternion Convert(System.Numerics.Quaternion q)
        {
            return new UnityEngine.Quaternion
            {
                x = q.X,
                y = q.Y,
                z = q.Z,
                w = q.W
            };
        }

        /// <summary>
        ///     Convert a Vector3 from Unity into a Vector3 from System 
        /// </summary>
        /// <param name="v">A vector</param>
        /// <returns>Return the matching vector</returns>
        public static System.Numerics.Vector3 Convert(UnityEngine.Vector3 v)
        {
            return new System.Numerics.Vector3
            {
                X = v.x,
                Y = v.y,
                Z = v.z
            };
        }

        /// <summary>
        ///     Convert a Vector3 from System into a Vector3 from Unity 
        /// </summary>
        /// <param name="v">A vector</param>
        /// <returns>Return the matching vector</returns>
        public static UnityEngine.Vector3 Convert(System.Numerics.Vector3 v)
        {
            return new UnityEngine.Vector3
            {
                x = v.X,
                y = v.Y,
                z = v.Z
            };
        }

        public static int ToHumanBodyBonesValue(
            this BodyPartInteractionStrategy bodyPartInteractionStrategy)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (bodyPartInteractionStrategy)
            {
                case BodyPartInteractionStrategy.RightHand:
                    return (int) UnityEngine.HumanBodyBones.RightHand;
                case BodyPartInteractionStrategy.LeftHand:
                    return (int) UnityEngine.HumanBodyBones.LeftHand;
                case BodyPartInteractionStrategy.Head:
                    return (int) UnityEngine.HumanBodyBones.Head;
                case BodyPartInteractionStrategy.RightFingerThumb:
                    return (int) UnityEngine.HumanBodyBones.RightThumbDistal;
                case BodyPartInteractionStrategy.RightFingerIndex:
                    return (int) UnityEngine.HumanBodyBones.RightIndexDistal;
                case BodyPartInteractionStrategy.RightFingerMiddle:
                    return (int) UnityEngine.HumanBodyBones.RightMiddleDistal;
                case BodyPartInteractionStrategy.RightFingerRing:
                    return (int) UnityEngine.HumanBodyBones.RightRingDistal;
                case BodyPartInteractionStrategy.RightFingerLittle:
                    return (int) UnityEngine.HumanBodyBones.RightLittleDistal;
                case BodyPartInteractionStrategy.LeftFingerThumb:
                    return (int) UnityEngine.HumanBodyBones.LeftThumbDistal;
                case BodyPartInteractionStrategy.LeftFingerIndex:
                    return (int) UnityEngine.HumanBodyBones.LeftIndexDistal;
                case BodyPartInteractionStrategy.LeftFingerMiddle:
                    return (int) UnityEngine.HumanBodyBones.LeftMiddleDistal;
                case BodyPartInteractionStrategy.LeftFingerRing:
                    return (int) UnityEngine.HumanBodyBones.LeftRingDistal;
                case BodyPartInteractionStrategy.LeftFingerLittle:
                    return (int) UnityEngine.HumanBodyBones.LeftLittleDistal;
                default:
                    return -1;
            }
        }
        #endregion

    }
}