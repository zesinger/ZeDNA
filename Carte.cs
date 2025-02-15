using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using static ZeDNA.GetData;
namespace ZeDNA
{
    /// <summary>
    /// Classe pour gérer l'affichage de la carte
    /// </summary>
    public static class Carte
    {
        /// <summary>
        /// Est-ce que les PNG sopnt chargés?
        /// </summary>
        public static bool bitmapLoaded = false;
        /// <summary>
        /// Bitmaps pour stocker les PNG
        /// </summary>
        private static Bitmap[] bitmaps = null;
        /// <summary>
        /// Opacité des cartes des zones [0,1]
        /// </summary>
        public static float[] opacity;
        /// <summary>
        /// charge les PNG et les convertit en Bitmap
        /// </summary>
        /// <returns>true si ça a réussi</returns>
        public static bool LoadCarte()
        {
            // on met les opacités à 1 par défaut
            opacity = new float[zoneList.Length];
            for (int j = 0; j < opacity.Length; j++) opacity[j] = 1;
            // on réserve les bitmaps pour le fond de carte et les zones
            bitmaps = new Bitmap[zoneList.Length + 1];
            // on charge le fond s'il existe sinon on retourne false
            if (!File.Exists("carte/fond.png"))
            {
                MessageBox.Show("La carte 'fond.png' n'a pas été trouvée");
                return false;
            }
            bitmaps[0] = new Bitmap("carte/fond.png");
            int i = 1;
            // on charge les cartes des zones si elles existent sinon on retourne false
            foreach (var zone in zoneList)
            {
                if (!File.Exists("carte/" + zone.name + ".png"))
                {
                    MessageBox.Show("La carte '" + zone.name + ".png' n'a pas été trouvée");
                    return false;
                }
                bitmaps[i] = new Bitmap("carte/" + zone.name + ".png");
                i++;
            }
            // et on dit que les cartes sont chargées avant de retourner true
            bitmapLoaded = true;
            return true;
        }
        /// <summary>
        /// Affiche les cartes: le fond puis les zones A, B, C avec fond bleu avant d'afficher les autres avec seulement le contour en rouge
        /// </summary>
        /// <param name="p">le Panel control où on va afficher</param>
        /// <param name="g">le Graphics de ce panel</param>
        public static void DrawCarte(Panel p, Graphics g)
        {
            // on vérifie que les bitmaps sont chargées, sinon on les charge
            bool loaded = true;
            if (!bitmapLoaded) loaded = LoadCarte();
            if (loaded)
            {
                // on affiche le fond
                g.DrawImage(bitmaps[0], 0, 0, fondDesc.w, fondDesc.h);
                // puis on prépare ce qu'il faut pour gérer l'opacité
                ColorMatrix matrix = new ColorMatrix();
                ImageAttributes attributes = new ImageAttributes();
                for (int i = 0; i < zoneList.Length; i++)
                {
                    //if (MainForm.zoneActive[i])
                    {
                        matrix.Matrix33 = opacity[i]; // Alpha value (0.0 = fully transparent, 1.0 = fully opaque)
                        attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        // avant d'afficher les zones
                        g.DrawImage(bitmaps[i + 1], new Rectangle(p.ClientRectangle.X + zoneList[i].x, p.ClientRectangle.Y + zoneList[i].y, zoneList[i].w, zoneList[i].h),
                            0, 0, zoneList[i].w, zoneList[i].h, GraphicsUnit.Pixel, attributes);
                    }
                }
            }
        }
    }
}
