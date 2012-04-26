using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Share
{
    class ActiveImageCollection
    {
        private List<ImageObject> images;
        private DBManager dbManager;
        private int latestId;

        public ActiveImageCollection()
        {
            dbManager = new DBManager();
            images = new List<ImageObject>(dbManager.getActiveImages());
            latestId = getIdImageAtIndex(getImageCount() - 1);
        }


        /// <summary>
        /// Return all the ImageObjects in the collection
        /// </summary>
        public List<ImageObject> getAllImages()
        {
            return this.images;
        }

        /// <summary>
        /// Return image at @param index
        /// </summary>
        public ImageObject getImageAtIndex(int index)
        {
            return this.images.ElementAt(index);
        }

        public void removeImageAtIndex(int index, int id)
        {
            if (index >= 0 && index < this.images.Count())
            {
                this.images.RemoveAt(index);
                dbManager.removeActiveImage(id);
            }
        }

        /// <summary>
        /// Return the total number of images in
        /// the collection
        /// </summary>
        public int getImageCount()
        {
            return this.images.Count;
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
        /// Set the List of images
        /// </summary>
        public void setImages(List<ImageObject> images)
        {
            this.images = images;
        }


        /// <summary>
        /// Check if image in the collection
        /// exists within a specified region of the
        /// @param handpoint
        /// </summary>
        public int findImageAt(OpenNI.Point3D p)
        {
            int retVal = -1;
            double minDistSoFar = Double.MaxValue;
            double currDist = 0;

            for (int i = 0; i < this.images.Count; i++)
            {
                currDist = pointsOverlap(p, this.images.ElementAt(i).getCoordinates());
                if (currDist < minDistSoFar)
                {
                    minDistSoFar = currDist;
                    retVal = i;
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
        public double pointsOverlap(OpenNI.Point3D pointOne, Point pointTwo)
        {
            double retVal = Double.MaxValue;

            double a = (double)(pointTwo.X - pointOne.X);
            double b = (double)(pointTwo.Y - pointOne.Y);

            double dist = Math.Sqrt(a * a + b * b);
            if (dist < 50)
            {
                retVal = dist;
            }

            return retVal;
        }

        /// <summary>
        /// Mark image at @param index
        /// as selected
        /// </summary>
        public void selectImageAtIndex(int index)
        {
            if (index > -1)
            {
                this.images.ElementAt(index).selectImage();
            }
        }

        /// <summary>
        /// Mark image at @param index
        /// as not selected
        /// </summary>
        public void deselectImageAtIndex(int index)
        {
            if (index > -1)
            {
                this.images.ElementAt(index).deselectImage();
            }
        }

        /// <summary>
        /// Update the coordinates of the 
        /// image at the @param imageSelectedIndex with
        /// the @param handPoint as the new location
        /// </summary>
        internal void updateImageAtIndex(int imageSelectedIndex, OpenNI.Point3D handPoint, int imageSelectedId)
        {
            dbManager.updateImageCoordinatesWithId(imageSelectedId, handPoint);
            this.images.ElementAt(imageSelectedIndex).moveImage((int)handPoint.X, (int)handPoint.Y);    
        }

        public void updateLatestId()
        {
            latestId = getIdImageAtIndex(getImageCount() - 1);
        }


        public bool updateCollection()
        {
            bool retVal = false;
            List<ImageObject> tempImages = dbManager.getUpdatedImages(this.latestId);
            if(tempImages.Count > 0)
            {
                this.images.AddRange(tempImages);
                updateLatestId();
                retVal = true;
            }

            return retVal;
        }

        public void addImage(ImageObject img)
        {
            dbManager.addToActiveImages(img);
        }
    }
}
