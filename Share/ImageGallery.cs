using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Share
{
    class ImageGallery
    {
        private List<ImageObject> images;
        private DBManager dbManager;
        private int startIndex;
        private int latestId;
        public bool isSelected;
        public const int MOVE_LEFT = -1;
        public const int MOVE_RIGHT = -2;
        public const int LEFT_X = 50;
        public const int LEFT_Y = 25;
        public const int RIGHT_X = 300;
        public const int RIGHT_Y = 25;
        private const int DISPLAY_COUNT = 5;

        public ImageGallery()
        {
            dbManager = new DBManager();
            startIndex = 0;
            images = new List<ImageObject>(dbManager.getGalleryImages());
            latestId = getIdImageAtIndex(this.images.Count() - 1);
            isSelected = false;
        }

        public ImageObject getImageAtIndex(int index)
        {
            return this.images.ElementAt(index);
        }

        public List<ImageObject> getImagesToDraw()
        {
            updateCollection();
            if(startIndex + DISPLAY_COUNT >= this.images.Count())
            {
                return this.images.GetRange(startIndex, this.images.Count() - startIndex);
            }
            return this.images.GetRange(startIndex, DISPLAY_COUNT);
        }

        public bool updateCollection()
        {
            bool retVal = false;
            List<ImageObject> tempImages = dbManager.getGalleryUpdatedImages(this.latestId);
            if (tempImages.Count > 0)
            {
                this.images.AddRange(tempImages);
                updateLatestId();
                retVal = true;
            }

            return retVal;
        }

        public void updateLatestId()
        {
            latestId = getIdImageAtIndex(this.images.Count() - 1);
        }

        /// <summary>
        /// Return the Id of the image @index
        /// </summary>
        public int getIdImageAtIndex(int index)
        {
            int retVal = -1;

            if (index >= 0 && index < this.images.Count())
            {
                return this.images.ElementAt(index).getId();
            }

            return retVal;
        }

        /// <summary>
        /// Check if image in the collection
        /// exists within a specified region of the
        /// @param handpoint
        /// </summary>
        public int findImageAt(OpenNI.Point3D p)
        {
            int retVal = int.MinValue;

            if (pointsOverlap(p, new Point(LEFT_X, LEFT_Y)))
            {
                if (startIndex - DISPLAY_COUNT < 0)
                {
                    startIndex = 0;
                }
                else
                {
                    startIndex -= DISPLAY_COUNT;
                }
                return MOVE_LEFT;
            }

            if (pointsOverlap(p, new Point(RIGHT_X, RIGHT_Y)))
            {
                if (startIndex + DISPLAY_COUNT >= this.images.Count())
                {
                    if (images.Count() - DISPLAY_COUNT <= 0)
                    {
                        startIndex = 0;
                    }
                    else
                    {
                        startIndex = images.Count() - DISPLAY_COUNT;
                    }
                }
                else
                {
                    startIndex += DISPLAY_COUNT;
                }
                return MOVE_RIGHT;
            }

            for (int i = startIndex; i < DISPLAY_COUNT; i++)
            {
                if (i < this.images.Count() && pointsOverlap(p, this.images.ElementAt(i).getCoordinates()))
                {
                    retVal = i;
                    return retVal;
                }
            }

            return retVal;
        }


        /// <summary>
        /// Check if two points lie
        /// within 100 pixels of each other
        /// return true if they do else
        /// return false
        /// </summary>
        public bool pointsOverlap(OpenNI.Point3D pointOne, Point pointTwo)
        {
            bool retVal = false;

            double a = (double)(pointTwo.X - pointOne.X);
            double b = (double)(pointTwo.Y - pointOne.Y);

            if (Math.Sqrt(a * a + b * b) < 100)
            {
                retVal = true;
            }

            return retVal;
        }

        public void setCoordinatesAtIndex(int index, int x, int y)
        {
            this.images.ElementAt(index).moveImage(x, y);
        }
    }
}
