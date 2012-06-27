using System.Windows.Controls;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;

namespace KinectControls
{
    /// <summary>
    /// Interaction logic for HandCursor.xaml
    /// </summary>
    public partial class HandCursor : UserControl
    {
        public HandCursor()
        {
            InitializeComponent();
        }

        public void SetPosition(Joint joint)
        {
            Joint scaledJoint = joint.ScaleTo(1270 - 88, 750 - 38, 0.5f, 0.5f);

            Canvas.SetLeft(this, scaledJoint.Position.X);
            Canvas.SetTop(this, scaledJoint.Position.Y);
        }
    }
}
