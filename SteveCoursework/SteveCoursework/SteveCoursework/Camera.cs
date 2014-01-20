using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace SteveCoursework
{
    class Camera
    {
        // Added for the creation of a camera 
        public Matrix camViewMatrix, camRotationMatrix, projectionMatrix, worldMatrix;
        //Cameras matrices
        public Vector3 camPosition, camLookat, camTransform;
        //Position, Lookat and transform of Camera in world 
       public float camPitch, camYaw;
        //Defines the amount of rotation and position on Y 
       public GraphicsDeviceManager graphics;
        public String tag;

        public Camera(GraphicsDeviceManager g, Vector3 pos, Vector3 look, float pitch, float yaw)
        {
            //worldMatrix = wMatrix;
            graphics = g;
            camPosition = pos;
            camLookat = look;
            InitializeTransform(g);
            camPitch = pitch; 
            camYaw = yaw;
        }

        public void InitializeTransform(GraphicsDeviceManager g)
        {
            //Create initial camera view 
            camViewMatrix = Matrix.CreateLookAt(camPosition, camLookat, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)g.GraphicsDevice.Viewport.Width / (float)g.GraphicsDevice.Viewport.Height, 1.0f, 1000.0f);
            worldMatrix = Matrix.Identity;
        }
        
        // Updates camera view
        public void camUpdate()
        {
            camRotationMatrix = Matrix.CreateRotationX(camPitch) * Matrix.CreateRotationY(camYaw); camTransform = Vector3.Transform(Vector3.Forward, camRotationMatrix);
            camLookat = camPosition + camTransform;
            camViewMatrix = Matrix.CreateLookAt(camPosition, camLookat, Vector3.Up);
        }
    }
}
