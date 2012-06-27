using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using Microsoft.Kinect;
using Kinect.Toolbox;

namespace KinectControls
{
    public class NotreGestureDetecteur : SwipeGestureDetector
    {
        const float SwipeMinimalLength = 0.2f;//0.4
        const float SwipeMaximalHeight = 1.0f;//0.2
        const int SwipeMininalDuration = 150;//250
        const int SwipeMaximalDuration = 1300;

        public NotreGestureDetecteur(int windowSize = 20)
            : base(windowSize)
        {

        }

        bool ScanPositions(Func<Vector3, Vector3, bool> heightFunction, Func<Vector3, Vector3, bool> directionFunction, Func<Vector3, Vector3, bool> lengthFunction, int minTime, int maxTime)
        {
            int start = 0;

            for (int index = 1; index < Entries.Count - 1; index++)
            {
                if (!heightFunction(Entries[0].Position, Entries[index].Position) || !directionFunction(Entries[index].Position, Entries[index + 1].Position))
                {
                    start = index;
                }

                if (lengthFunction(Entries[index].Position, Entries[start].Position))
                {
                    double totalMilliseconds = (Entries[index].Time - Entries[start].Time).TotalMilliseconds;
                    if (totalMilliseconds >= minTime && totalMilliseconds <= maxTime)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void LookForGesture()
        {

            // Swipe to front to back
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight,  // Height
                (p1, p2) => p2.Z - p1.Z > -0.01f, // Progression to depth
                (p1, p2) => Math.Abs(p2.Z - p1.Z) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration))// Duration
            {

                RaiseGestureDetected("SwipeFrontBack");
                return;

            }

            // Swipe to back to front
            if (ScanPositions((p1, p2) => Math.Abs(p2.Y - p1.Y) < SwipeMaximalHeight, // Height
                (p1, p2) => p2.Z - p1.Z < 0.01f, // Progression to depth
                (p1, p2) => Math.Abs(p2.Z - p1.Z) > SwipeMinimalLength , // Length
                SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            {
                RaiseGestureDetected("SwipeBackFront");
                return;
            }



            // Swipe to up
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < SwipeMaximalHeight, 
                (p1, p2) => p2.Y - p1.Y > -0.001f, // Progression to up
                (p1, p2) => Math.Abs(p2.Y - p1.Y) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration)) // Duration
            {
                RaiseGestureDetected("SwipeUp");
                return;
            }

            // Swipe to bottom
            if (ScanPositions((p1, p2) => Math.Abs(p2.X - p1.X) < SwipeMaximalHeight,  
                (p1, p2) => p2.Y - p1.Y < 0.001f, // Progression to right
                (p1, p2) => Math.Abs(p2.Y - p1.Y) > SwipeMinimalLength, // Length
                SwipeMininalDuration, SwipeMaximalDuration))// Duration
            {
                RaiseGestureDetected("SwipeBottom");
                return;
            }


         

        }
    }
}