using ImageMagick;
using OpenQA.Selenium.DevTools.V130.DOM;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ZeDNA
{
    /// <summary>
    /// Classe pour gérer la carte
    /// </summary>
    public static class Carte
    {
        /// <summary>
        /// Est-ce que les cartes sont chargées?
        /// </summary>
        public static bool bitmapLoaded = false;
        /// <summary>
        /// Bitmaps pour stocker les layers séparément
        /// </summary>
        private static (string Name, Bitmap bm)[] bitmaps = null;
        /// <summary>
        /// Bitmap actuelle avec l'ensemble des couches actives
        /// </summary>
        private static Bitmap curBitmap = null;
        /// <summary>
        /// Chemin de la carte
        /// </summary>
        private static string cartePath = "carte\\carte.xcf";
        /// <summary>
        /// Bool qui permet de savoir si on a trouvé le fichier de carte et qu'on a réussi à le charger (sinon on n'affiche pas les cartes)
        /// </summary>
        private static bool isCarteFile = false;
        /// <summary>
        /// permet d'ouvrir le fichier de carte et de le garder ouvert tout au long du fonctionnement du programme
        /// </summary>
        private static MagickImageCollection micCarte = null;
        /// <summary>
        /// on garde en mémoire les couches affichées lors du dernier affichage pour comparer et recalculer la bitmap si elle a changé
        /// </summary>
        private static string[] prevLayers = new string[] { "" };
        /// <summary>
        /// Convertit tous les pixels non transparents d'un layer d'une image xcf dans une couleur souhaitée et crée une bitmap
        /// Utilisé pour afficher la zone qui clignote dans une couleur différente
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="targetColor"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Bitmap ConvertTransparentToColor(MagickImage layer, Color targetColor)
        {
            // On force la transparence
            layer.Alpha(AlphaOption.On);

            // on force le mode 8 bit par pixel
            if (layer.Depth != 8) layer.Depth = 8;

            int width = (int)layer.Width;
            int height = (int)layer.Height;

            // et on convertit en 8 bits rgba
            layer.Format = MagickFormat.Rgba;
            byte[] pixelData = layer.ToByteArray(MagickFormat.Rgba);

            // on crée une bitmap et on mlock ses bits pour poiuvoir copier directement
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = width * height * 4;

            // On change la couleur des pixels
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte alpha = pixelData[i + 3];
                if (alpha > 0)
                {
                    pixelData[i] = targetColor.B;
                    pixelData[i + 1] = targetColor.G;
                    pixelData[i + 2] = targetColor.R;
                    pixelData[i + 3] = alpha;
                }
            }

            // Et on copie sur la bitmap
            Marshal.Copy(pixelData, 0, ptr, bytes);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }
        /// <summary>
        /// charge les PNG et les convertit en Bitmap
        /// </summary>
        /// <returns>true si ça a réussi</returns>
        public static void LoadCarte()
        {
            // On vérifie que le fichier des cartes existe
            if (!File.Exists(cartePath))
            {
                MessageBox.Show("Le fichier de carte n'a pas été trouvé, les cartes ne seront pas affichées.");
                return;
            }
            try
            {
                // le cas échéant, on le charge
                micCarte = new MagickImageCollection(cartePath);
                // et on crée les bitmaps individuelles pour le clignotement
                bitmaps = new (string Name, Bitmap bm)[micCarte.Count];
                for (int i = 0; i < micCarte.Count; i++)
                {
                    MagickImage miCarte = new MagickImage(MagickColors.Transparent, micCarte[0].Width, micCarte[0].Height);
                    miCarte.Composite(micCarte[i], 0, 0, CompositeOperator.Over);
                    //bitmaps[i].bm = new Bitmap(new MemoryStream(miCarte.ToByteArray(MagickFormat.Png)));
                    bitmaps[i].bm = ConvertTransparentToColor(miCarte, Color.Black);
                    bitmaps[i].Name = micCarte[i].Label;
                }
                isCarteFile = true;
            }
            catch
            {
                MessageBox.Show("Impossible de traiter le fichier carte, les cartes ne seront pas affichées.");
            }
            // on charge les couches de la carte séparément
        }
        private static Bitmap GetCarte(string name)
        {
            for (int i = 0; i < bitmaps.Length; i++)
            {
                if (bitmaps[i].Name == name) return bitmaps[i].bm;
            }
            return null;
        }
        /// <summary>
        /// crée une bitmap avec toutes les couches de la liste layerlist
        /// </summary>
        /// <param name="layerList">les couches sous forme du nom de la zone comme sauvegardé dans le fichier GIMP xcf</param>
        private static void LoadSelectedLayers(string[] layerList)
        {
            // on crée une magickimage qui servira à composer les layers les uns au dessus des autres
            MagickImage miCarte = new MagickImage(MagickColors.Transparent, micCarte[0].Width, micCarte[0].Height);
            // on rajoute le fond à la liste des couches qu'on veut afficher
            string[] llist = new string[layerList.Length + 1];
            llist[0] = "fond";
            for (int i = 0; i < layerList.Length; i++) llist[i + 1] = layerList[i];
            // et on compose l'image à partir des couches qui nous itnéressent
            foreach (var layer in micCarte)
            {
                if (llist.Contains(layer.Label))
                {
                    miCarte.Composite(layer, 0, 0, CompositeOperator.Over);
                }
            }
            // On finit en convertissant l'image en bitmap
            curBitmap = new Bitmap(new MemoryStream(miCarte.ToByteArray(MagickFormat.Png)));
            bitmapLoaded = true;
        }
        /// <summary>
        /// Affiche les cartes
        /// </summary>
        /// <param name="pea">le PaintEventArgs  du panel control où on va afficher</param>
        /// <param name="layers">le nom des zones à afficher</param>
        public static void DrawCarte(PaintEventArgs pea, string[] layers, string hoveredlayer)
        {
            if (!isCarteFile) return;
            if (!layers.SequenceEqual(prevLayers))
            {
                bitmapLoaded = false;
                prevLayers = layers;
            }
            // on vérifie que les bitmaps sont chargées, sinon on les charge
            if (!bitmapLoaded) LoadSelectedLayers(layers);
            // puis on affiche la carte complète
            pea.Graphics.DrawImage(curBitmap, pea.ClipRectangle);
            // et la carte à faire clignoter s'il y en a une
            //if (hoveredlayer != "")
            {
                Bitmap hoveredbitmap = GetCarte(hoveredlayer);
                if (hoveredbitmap != null)
                {
                    float period = 1.0f; // Période en secondes du clignotement
                    float time = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds; // obtenir l'heure en secondes
                    float value = (float)(0.5 + 0.5 * Math.Sin((2 * Math.PI / period) * time)); // en déduire l'opacité du clignotement
                    ColorMatrix matrix = new ColorMatrix();
                    matrix.Matrix33 = value;
                    ImageAttributes attributes = new ImageAttributes();
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    // Draw the image with opacity
                    pea.Graphics.DrawImage(hoveredbitmap, pea.ClipRectangle, 0, 0, micCarte[0].Width, micCarte[0].Height, GraphicsUnit.Pixel, attributes);
                }
            }
        }
    }
}
