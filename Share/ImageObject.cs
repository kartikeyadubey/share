using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Drawing;


namespace Share
{
    class ImageObject
    {
        private BitmapImage img;
        private int x;
        private int y;
        private bool selected;

        public ImageObject(BitmapImage img, int x, int y)
        {
            this.img = img;
            this.x = x;
            this.y = y;
            this.selected = false;
        }

        public BitmapImage getImage()
        {
            return this.img;
        }

        public int getX()
        {
            return this.x;
        }

        public int getY()
        {
            return this.y;
        }

        public Point getCoordinates()
        {
            return new Point(this.x, this.y);
        }
        public void moveImage(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.selected = false;
        }

        public void selectImage()
        {
            if (!this.selected)
            {
                this.selected = true;
            }
        }
    }
}
