using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace SteveCoursework
{
    struct Laser
    {
        //Variables for laser. 
        public Vector3 position;
        public Vector3 direction;
        public float speed;
        public bool isActive;

        public void Update(float delta)
        {
            position += direction * speed *
                        GameConstants.LaserSpeedAdjustment * delta;

            if (position.Z < -100.0f || position.Z > 100.0f)
            {
                isActive = false;
            }
        }
    }
}
