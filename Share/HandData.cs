using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNI;
using NITE;

namespace Share
{
    class HandData:PointControl
    {
        public HandData() :
            base()
        {
            Console.WriteLine("Constructing MyCanvas");
            pushDetector = new PushDetector();
            swipeDetector = new SwipeDetector();
            steadyDetector = new SteadyDetector();
            flowRouter = new FlowRouter();
            broadcaster = new Broadcaster();

            broadcaster.AddListener(pushDetector);
            broadcaster.AddListener(flowRouter);

            pushDetector.Push += new EventHandler<VelocityAngleEventArgs>(pushDetector_Push);
            steadyDetector.Steady += new EventHandler<SteadyEventArgs>(steadyDetector_Steady);
            swipeDetector.GeneralSwipe += new EventHandler<DirectionVelocityAngleEventArgs>(swipeDetector_GeneralSwipe);

            PrimaryPointCreate += new EventHandler<HandFocusEventArgs>(MyCanvas_PrimaryPointCreate);
            PrimaryPointDestroy += new EventHandler<IdEventArgs>(MyCanvas_PrimaryPointDestroy);
            PrimaryPointUpdate += new EventHandler<HandEventArgs>(MyCanvas_PrimaryPointUpdate);
            OnUpdate += new EventHandler<UpdateMessageEventArgs>(MyCanvas_OnUpdate);
        }

        void MyCanvas_PrimaryPointUpdate(object sender, HandEventArgs e)
        {
            Console.WriteLine("Point updated");
            Console.WriteLine(e.Hand.Position.X);
        }

        void MyCanvas_OnUpdate(object sender, UpdateMessageEventArgs e)
        {
            Console.WriteLine("Canvas updated");
            //Console.WriteLine(e.Message);
            broadcaster.UpdateMessage(e.Message);
            broadcaster.AddListener(steadyDetector);
        }

        void MyCanvas_PrimaryPointDestroy(object sender, IdEventArgs e)
        {
            Console.WriteLine("Point destroyed");
        }

        void MyCanvas_PrimaryPointCreate(object sender, HandFocusEventArgs e)
        {
            Console.WriteLine("Point created");
            Console.WriteLine(e.Hand.Position.X);
            flowRouter.ActiveListener = steadyDetector;
        }

        void swipeDetector_GeneralSwipe(object sender, DirectionVelocityAngleEventArgs e)
        {
            Console.WriteLine("Swipe detected");
            Console.WriteLine("{0}: Swipe", e.Direction);
        }

        void steadyDetector_Steady(object sender, SteadyEventArgs e)
        {
            Console.WriteLine("Steady {0} ({1})", e.ID, PrimaryID);
            if (e.ID == PrimaryID)
            {
                flowRouter.ActiveListener = swipeDetector;
            }
        }

        void pushDetector_Push(object sender, VelocityAngleEventArgs e)
        {
            Console.WriteLine("Push detected");
        }

        private PushDetector pushDetector;
        private SwipeDetector swipeDetector;
        private SteadyDetector steadyDetector;
        private FlowRouter flowRouter;
        private Broadcaster broadcaster;
    }
}
