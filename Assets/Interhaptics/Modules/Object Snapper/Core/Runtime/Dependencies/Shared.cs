using Interhaptics.InteractionsEngine.Shared.Types;
using System;
using System.Collections.Generic;

namespace Interhaptics.ObjectSnapper.core
{
    #region Serializables
    [Serializable]
    public struct SerializableVector
    {
        float vx, vy, vz;

        public SerializableVector(System.Numerics.Vector3 vector)
        {
            vx = vector.X;
            vy = vector.Y;
            vz = vector.Z;
        }

        public System.Numerics.Vector3 ToNumericsVector3()
        {
            return new System.Numerics.Vector3()
            {
                X = vx,
                Y = vy,
                Z = vz
            };
        }
    }

    [Serializable]
    public struct SerializableSpatialRepresentation
    {
        SerializableVector vector;
        float qx, qy, qz, qw;

        public SerializableSpatialRepresentation(SpatialRepresentation spatialRepresentation)
        {
            vector = new SerializableVector(spatialRepresentation.Position);

            qx = spatialRepresentation.Rotation.X;
            qy = spatialRepresentation.Rotation.Y;
            qz = spatialRepresentation.Rotation.Z;
            qw = spatialRepresentation.Rotation.W;
        }

        public SpatialRepresentation ToSpatialRepresentation()
        {
            return new SpatialRepresentation()
            {
                Position = vector.ToNumericsVector3(),
                Rotation = new System.Numerics.Quaternion(qx, qy, qz, qw)
            };
        }
    }

    [Serializable]
    public struct Pose
    {
        public string transformName;
        public SerializableSpatialRepresentation spatialRepresentation;
        public List<Pose> childrenPose;
    }


    [Serializable]
    public struct SnappableActorData
    {
        public SerializableVector forward;
        public SerializableVector upward;
        public Pose rootPose;
    }
    #endregion

}
