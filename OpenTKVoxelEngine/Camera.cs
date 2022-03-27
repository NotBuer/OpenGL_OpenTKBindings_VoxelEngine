using System;
using OpenTK.Mathematics;

namespace OpenTKVoxelEngine_Camera
{
    public class Camera
    {

        private Vector3 _forward = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        public const float CAMERA_DEFAULT_SPEED = 4f;
        public float _cameraSpeed = CAMERA_DEFAULT_SPEED;
        public float _sensitivity = 0.2f;

        // Rotation around the X axis (radians)
        private float _pitch;

        // Rotation arounde the Y axis (radians)
        private float _yaw = -MathHelper.PiOver2; // Without this, would be started rotated 90 degrees right.

        // The camera field of view (radians)
        private float _fov = MathHelper.PiOver2 / 2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        public Vector3 Position { get; set; }
        public float AspectRatio { get; set; }
        public Vector3 Forward => _forward;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        // Convert from degrees to radians as soon as the property is set to improve performance.
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                float angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // Convert from degrees to radians as soon as the property is set to improve performance.
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set 
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                float angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _forward, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        private void UpdateVectors()
        {
            // First, the front matrix is calculated using some basic trigonometry
            _forward.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _forward.Y = MathF.Sin(_pitch);
            _forward.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            // Make sure the vectors are all normalized, as otherwise would get some funky results.
            _forward = Vector3.Normalize(_forward);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up, this behaviour might
            // not be what you need for all cameras so keep in mind if you do not want a FPS camera.
            _right = Vector3.Normalize(Vector3.Cross(_forward, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _forward));
        }

    }
}
