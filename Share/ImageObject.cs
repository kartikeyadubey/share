﻿using System;
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
        private int canvasId;

        public ImageObject(BitmapImage img, int x, int y, int id)
        {
            this.img = img;
            this.x = x;
            this.y = y;
            this.selected = false;
            this.id = id;
            this.canvasId = -1;
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

        public void setCanvasId(int cId)
        {
            this.canvasId = cId;
        }

        public int getCanvasId()
        {
            return this.canvasId;
        }
    }
}
