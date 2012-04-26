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
        private int id;
        private String base64Image;
        private String text;
        private bool isText;


        public ImageObject(BitmapImage img, String base64Image,int x, int y, int id)
        {
            this.img = img;
            this.x = x;
            this.y = y;
            this.selected = false;
            this.id = id;
            this.base64Image = base64Image;
            this.isText = false;
            this.text = null;
        }

        public ImageObject(String text, int x, int y, int id)
        {
            this.text = text;
            this.x = x;
            this.y = y;
            this.id = id;
            this.isText = true;
            this.img = null;
            this.base64Image = null;
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

        public int getId()
        {
            return this.id;
        }

        public Point getCoordinates()
        {
            return new Point(this.x, this.y);
        }

        public String getText()
        {
            return this.text;
        }
        public void moveImage(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void selectImage()
        {
            this.selected = true;
        }

        public void deselectImage()
        {
            this.selected = false;
        }

        public bool isSelected()
        {
            return this.selected;
        }

        public string getBase64String()
        {
            return this.base64Image;
        }

        public bool isObjectText()
        {
            return this.isText;
        }

    }
}
