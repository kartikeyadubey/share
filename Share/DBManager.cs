using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Threading;

namespace Share
{
    class DBManager
    {
        private int prevX = 0;
        private int prevY = 100;

        private const string DBPATH = @"server=localhost;userid=root;
            password=;database=sharegallery";

        public DBManager()
        {

        }

        public void removeActiveImage(int id)
        {
            using (MySqlConnection c = new MySqlConnection(DBPATH))
            {
                c.Open();
                string query = string.Format("DELETE from activeimages WHERE id=" + id);
                MySqlCommand mCmd = new MySqlCommand(query, c);
                mCmd.ExecuteReader();
            }
        }

        internal void updateImageCoordinatesWithId(int Id, OpenNI.Point3D handPoint)
        {
            using (MySqlConnection c = new MySqlConnection(DBPATH))
            {
                c.Open();
                string query = string.Format("UPDATE activeimages SET x=" + (int)handPoint.X + ", y=" + (int)handPoint.Y +
                " WHERE id=" + Id);
                MySqlCommand mCmd = new MySqlCommand(query, c);
                mCmd.ExecuteReader();
            }

        }

        public List<ImageObject> getUpdatedImages(int latestId)
        {
            List<ImageObject> retVal = new List<ImageObject>();
            using (MySqlConnection c = new MySqlConnection(DBPATH))
            {
                c.Open();
                MySqlCommand mCmd = new MySqlCommand(string.Format("SELECT * FROM activeimages WHERE id > " + latestId + " ORDER BY id ASC;"), c);
                using (MySqlDataReader r = mCmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        switch (r.GetInt32("istext"))
                        {
                            //It is an image
                            case -1:
                                String s = r.GetString("image");
                                Byte[] bitmapData = new Byte[s.Length];
                                bitmapData = Convert.FromBase64String(FixBase64ForImage(s));
                                System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
                                streamBitmap.Seek(0, SeekOrigin.Begin);
                                Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));

                                using (var stream = new MemoryStream(bitmapData))
                                {
                                    var bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.StreamSource = stream;
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.EndInit();
                                    bitmap.Freeze();

                                    ImageObject imgObj = new ImageObject(bitmap, s, r.GetInt32("x"), r.GetInt32("y"),
                                                                        r.GetInt32("id"));
                                    retVal.Add(imgObj);
                                }

                                streamBitmap.Close();
                                break;
                            //It is a text string
                            case 1:
                                ImageObject textObj = new ImageObject(r.GetString("text"), r.GetInt32("x"), r.GetInt32("y"),
                                                                            r.GetInt32("id"));
                                retVal.Add(textObj);
                                break;
                            default: break;
                        }

                    }

                }
            }

            return retVal;
        }



        public List<ImageObject> getGalleryUpdatedImages(int latestId)
        {
            List<ImageObject> retVal = new List<ImageObject>();
            using (MySqlConnection c = new MySqlConnection(DBPATH))
            {
                c.Open();
                MySqlCommand mCmd = new MySqlCommand(string.Format("SELECT * FROM galleryimages  WHERE id > " + latestId + " ORDER BY id ASC;"), c);
                using (MySqlDataReader r = mCmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        String s = r.GetString("image");
                        Byte[] bitmapData = new Byte[s.Length];
                        bitmapData = Convert.FromBase64String(FixBase64ForImage(s));
                        System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
                        streamBitmap.Seek(0, SeekOrigin.Begin);
                        Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));

                        using (var stream = new MemoryStream(bitmapData))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            bitmap.Freeze();

                            ImageObject obj = new ImageObject(bitmap, s, r.GetInt32("x"), r.GetInt32("y"),
                                                                r.GetInt32("id"));
                            retVal.Add(obj);
                        }

                        streamBitmap.Close();
                    }

                }
            }

            return retVal;
        }


        public Tuple<int, int> getUpdatedImageWithId(int Id)
        {
            Tuple<int, int> retVal = new Tuple<int, int>(0, 0);
            using (MySqlConnection c = new MySqlConnection(DBPATH))
            {
                c.Open();
                string query = string.Format("SELECT x,y FROM activeimages WHERE id=" + Id);
                MySqlCommand mCmd = new MySqlCommand(query, c);
                using (MySqlDataReader r = mCmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        retVal = Tuple.Create(r.GetInt32("x"), r.GetInt32("y"));
                    }
                }
            }
            return retVal;

        }

        public List<ImageObject> getActiveImages()
        {
            List<ImageObject> retVal = new List<ImageObject>();
            using (MySqlConnection c = new MySqlConnection(DBPATH))
            {
                c.Open();
                MySqlCommand mCmd = new MySqlCommand(string.Format("SELECT * FROM activeimages ORDER BY id ASC;"), c);

                using (MySqlDataReader r = mCmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        switch (r.GetInt32("istext"))
                        {
                            case -1:
                                String s = r.GetString("image");
                                Byte[] bitmapData = new Byte[s.Length];
                                bitmapData = Convert.FromBase64String(FixBase64ForImage(s));
                                System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
                                streamBitmap.Seek(0, SeekOrigin.Begin);
                                Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));

                                using (var stream = new MemoryStream(bitmapData))
                                {
                                    var bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.StreamSource = stream;
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.EndInit();
                                    bitmap.Freeze();

                                    ImageObject imgObj = new ImageObject(bitmap, s, r.GetInt32("x"), r.GetInt32("y"),
                                                                        r.GetInt32("id"));
                                    retVal.Add(imgObj);
                                }

                                streamBitmap.Close();
                                break;

                            case 1:
                                ImageObject textObj = new ImageObject(r.GetString("text"), r.GetInt32("x"), r.GetInt32("y"),
                                                                        r.GetInt32("id"));
                                retVal.Add(textObj);
                                break;

                            default: break;
                        }

                    }

                }
            }
            return retVal;
        }


        public List<ImageObject> getGalleryImages()
        {
            List<ImageObject> retVal = new List<ImageObject>();
            using (MySqlConnection c = new MySqlConnection(DBPATH))
            {
                c.Open();
                MySqlCommand mCmd = new MySqlCommand(string.Format("SELECT * FROM galleryimages ORDER BY id ASC;"), c);

                using (MySqlDataReader r = mCmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        String s = r.GetString("image");
                        Byte[] bitmapData = new Byte[s.Length];
                        bitmapData = Convert.FromBase64String(FixBase64ForImage(s));
                        System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
                        streamBitmap.Seek(0, SeekOrigin.Begin);
                        Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));

                        using (var stream = new MemoryStream(bitmapData))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            bitmap.Freeze();

                            ImageObject obj = new ImageObject(bitmap, s, r.GetInt32("x"), r.GetInt32("y"),
                                                                r.GetInt32("id"));
                            retVal.Add(obj);
                        }

                        streamBitmap.Close();
                    }

                }
            }
            return retVal;
        }

        public void addToActiveImages(ImageObject img)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                using (MySqlConnection c = new MySqlConnection(DBPATH))
                {
                    c.Open();
                    string query = "INSERT into activeimages(image, x, y, istext) VALUES('" + img.getBase64String() +
                        "'," + prevX + ", " + prevY + ", -1)";
                    prevY += 50;
                    MySqlCommand mCmd = new MySqlCommand(query, c);
                    mCmd.ExecuteNonQuery();
                }
            });

        }

        public string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty);
            sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }

    }
}
