using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNI;
using NITE;

namespace Share
{
    class ImageTracker : PointControl
    {

        public ImageTracker(string name) :
            base()
        {
            Console.WriteLine("MyImage created");
            this.name = name;
            pushDetector = new PushDetector();
            circleDetector = new CircleDetector();
            circleDetector.MinimumPoints = 50;
            steadyDetector = new SteadyDetector();
            
            flowRouter = new FlowRouter();
            broadcaster = new Broadcaster();

            broadcaster.AddListener(pushDetector);
            broadcaster.AddListener(circleDetector);
            broadcaster.AddListener(flowRouter);

            pushDetector.Push += new EventHandler<VelocityAngleEventArgs>(pushDetector_Push);
            steadyDetector.Steady += new EventHandler<SteadyEventArgs>(steadyDetector_Steady);
            circleDetector.OnCircle += new EventHandler<CircleEventArgs>(circleDetector_OnCircle);

            PrimaryPointCreate += new EventHandler<HandFocusEventArgs>(MyBox_PrimaryPointCreate);
            PrimaryPointDestroy += new EventHandler<IdEventArgs>(MyBox_PrimaryPointDestroy);
            PrimaryPointUpdate += new EventHandler<HandEventArgs>(MyBox_PrimaryPointUpdate);
            OnUpdate += new EventHandler<UpdateMessageEventArgs>(MyBox_OnUpdate);
        }

        void circleDetector_OnCircle(object sender, CircleEventArgs e)
        {
            if (e.Confidence)
            {
                Update(new Point3D(), "circle");
                flowRouter.ActiveListener = pushDetector;
            }
        }

        void swipeDetector_GeneralSwipe(object sender, DirectionVelocityAngleEventArgs e)
        {
            Update(new Point3D(), "swipe" + e.Direction);
        }



        void MyBox_PrimaryPointUpdate(object sender, HandEventArgs e)
        {
            //Console.WriteLine("Point Updated" + e.Hand.Position.ToString());
            this.currentPoint = e.Hand.Position;
            Update(e.Hand.Position, "locationupdate");
        }


        void MyBox_OnUpdate(object sender, UpdateMessageEventArgs e)
        {
            broadcaster.UpdateMessage(e.Message);
        }

        void MyBox_PrimaryPointDestroy(object sender, IdEventArgs e)
        {
            Console.WriteLine("Point destroyed");
        }

        void MyBox_PrimaryPointCreate(object sender, HandFocusEventArgs e)
        {
            Console.WriteLine("PrimaryPointCreate");
            flowRouter.ActiveListener = pushDetector;
        }


        void steadyDetector_Steady(object sender, SteadyEventArgs e)
        {
            Update(this.currentPoint, "steady");
        }

        void pushDetector_Push(object sender, VelocityAngleEventArgs e)
        {
            Update(this.currentPoint, "push");
        }

        #region Update Event
        public delegate void UpdateHandler(Point3D p, String str);
        public event UpdateHandler Update;
        #endregion

        private PushDetector pushDetector;
        private CircleDetector circleDetector;
        private SteadyDetector steadyDetector;
        private FlowRouter flowRouter;
        private Broadcaster broadcaster;
        private string name;
        public Point3D currentPoint;
    }
}
