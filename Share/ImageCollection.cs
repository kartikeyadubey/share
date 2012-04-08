using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Share
{
    class ImageCollection
    {
        private List<ImageObject> images;

        public ImageCollection()
        {
            images = new List<ImageObject>();
        }

        public List<ImageObject> getAllImages()
        {
            return this.images;
        }

        public ImageObject getImageAtIndex(int index)
        {
            return this.images.ElementAt(index);
        }

        public void setImages(List<ImageObject> images)
        {
            this.images = images;
        }

        public int getImageCount()
        {
            return this.images.Count;
        }

        public int findImageAt(OpenNI.Point3D p)
        {
            int retVal = -1;
            for (int i = 0; i < this.images.Count; i++)
            {
                if (pointsOverlap(p, this.images.ElementAt(i).getCoordinates()))
                {
                    Console.WriteLine("Image underneath");
                    retVal = i;
                }
            }
            return retVal;
        }

        public bool pointsOverlap(OpenNI.Point3D pointOne, Point pointTwo)
        {
            bool retVal = false;

            double a = (double)(pointTwo.X - pointOne.X);
            double b = (double)(pointTwo.Y - pointOne.Y);

            if (Math.Sqrt(a * a + b * b) < 10)
            {
                retVal = true;
            }

            return retVal;
        }
    }
}
