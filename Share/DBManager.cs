using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;

namespace Share
{
    class DBManager
    {
        private MySqlConnection galleryConnection;
        private MySqlConnection activeImagesConnection;

        string galleryString = @"server=localhost;userid=root;
            password=;database=sharegallery";

        string activeString = @"server=localhost;userid=root;
            password=;database=sharegallery";

        public DBManager()
        {
            try
            {
                galleryConnection = new MySqlConnection(galleryString);
                activeImagesConnection = new MySqlConnection(activeString);

                galleryConnection.Open();
                activeImagesConnection.Open();
                Console.WriteLine("MySQL version : {0}", galleryConnection.ServerVersion);
                Console.WriteLine("MySQL version : {0}", activeImagesConnection.ServerVersion);

            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());

            }
        }

        public void removeFromGallery()
        {

        }


        internal void updateImageCoordinatesWithId(int Id, OpenNI.Point3D handPoint)
        {
            using (MySqlConnection c = new MySqlConnection(activeString))
            {
                c.Open();
                string query = string.Format("UPDATE images SET x=" + (int)handPoint.X + ", y=" + (int)handPoint.Y +
                " WHERE id=" + Id);
                MySqlCommand mCmd = new MySqlCommand(query, c);
                mCmd.ExecuteReader();
            }
            
        }

        public List<ImageObject> getUpdatedImages(int latestId)
        {
            List<ImageObject> retVal = new List<ImageObject>();
            using (MySqlConnection c = new MySqlConnection(galleryString))
            {
                c.Open();
                MySqlCommand mCmd = new MySqlCommand(string.Format("SELECT * FROM images  WHERE id > " + latestId +  " ORDER BY id ASC;"), c);
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

                            ImageObject obj = new ImageObject(bitmap, r.GetInt32("x"), r.GetInt32("y"),
                                                                r.GetInt32("id"));
                            retVal.Add(obj);
                        }

                        streamBitmap.Close();
                    }

                }
            }

            return retVal;
        }

        public Tuple<int,int> getUpdatedImageWithId(int Id)
        {
            Tuple<int,int> retVal = new Tuple<int,int>(0,0);
            using (MySqlConnection c = new MySqlConnection(activeString))
            {
                c.Open();
                string query = string.Format("SELECT x,y FROM images WHERE id=" + Id);
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

        public List<ImageObject> getGalleryImages()
        {
            List<ImageObject> retVal = new List<ImageObject>();
            using (MySqlConnection c = new MySqlConnection(galleryString))
            {
                c.Open();
                MySqlCommand mCmd = new MySqlCommand(string.Format("SELECT * FROM images ORDER BY id ASC;"), c);

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

                            ImageObject obj = new ImageObject(bitmap, r.GetInt32("x"), r.GetInt32("y"),
                                                                r.GetInt32("id"));
                            retVal.Add(obj);
                        }

                        streamBitmap.Close();
                    }

                }               
            }
            return retVal;
        }



        public string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty);
            sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }

        public List<ImageObject> getActiveImages()
        {
            return null;
        }
    }
}
