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

        public DBManager()
        {
            string galleryString = @"server=localhost;userid=root;
            password=;database=sharegallery";

            string activeString = @"server=localhost;userid=root;
            password=;database=shareactive";

            galleryConnection = null;
            activeImagesConnection = null;

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

        public List<ImageObject> getGalleryImages()
        {
            List<ImageObject> retVal = new List<ImageObject>();
            MySqlCommand mCmd = new MySqlCommand(string.Format("SELECT * FROM images;"), this.galleryConnection);
            MySqlDataReader mReader = mCmd.ExecuteReader();

            while (mReader.Read())
            {
                String s = mReader.GetString("image");
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

                    ImageObject obj = new ImageObject(bitmap, mReader.GetInt32("x"), mReader.GetInt32("y"));
                    retVal.Add(obj);
                }

                streamBitmap.Close();
                // Save the image as a GIF.
                //bitImage.Save("image" + i + ".jpeg");
                //BitmapImage b = new BitmapImage(new Uri(@"C:\Users\Kartikeya\Documents\Visual Studio 2010\Projects\Share\Share\bin\Debug\image.gif"));
                
            }
            mReader.Close();
            mReader.Dispose();
            mReader = null;
            mCmd.Dispose();
            mCmd = null;

            return retVal;
        }

        public string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty);
            sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }


        public void updateActiveImage()
        {

        }

        public List<ImageObject> getActiveImages()
        {
            return null;
        }

    }
}
