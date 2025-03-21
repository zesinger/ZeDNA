using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static ZeDNA.GetData;

namespace ZeDNA
{
    /// <summary>
    /// Classe du contenu du fichier à sauvegarder sur le disque pour s'assurer de ne pas avoir
    /// de pertes en cas de crash ou de quittage inopiné
    /// </summary>
    public class ZoneFile
    {
        /// <summary>
        /// La date du contenu des zones
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// les zones trouvées
        /// </summary>
        public List<Zone> Zones { get; set; }
        /// <summary>
        /// nombre de zones Rtba = début des zones supérieures dans la list
        /// </summary>
        public int? NRtba { get; set; }
    }
    /// <summary>
    /// Classe de stockage des zones trouvées dans le NOTAM
    /// </summary>
    public class Zone
    {
        /// <summary>
        /// nom de la zone
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// tableau des heures d'ouverture
        /// </summary>
        public DateTime[] Deb { get; set; }
        /// <summary>
        /// Tableau des: L'alerte de début a-t-elle déjà été déclenchée? false avant et true une fois déclenchée
        /// </summary>
        public bool[] alertD { get; set; }
        /// <summary>
        /// tableau des heures de fermeture
        /// </summary>
        public DateTime[] Fin { get; set; }
        /// <summary>
        /// Tableau des: L'alerte de fin a-t-elle déjà été déclenchée? false avant et true une fois déclenchée
        /// </summary>
        public bool[] alertF { get; set; }

        /// <summary>
        /// Constructeur avec mise à zéro des horaires et déclenchement de la surveillance des alertes
        /// </summary>
        /// <param name="name"></param>
        public Zone(string name)
        {
            Name = name;
            Deb = new DateTime[nMaxList];
            Fin = new DateTime[nMaxList];
            alertD = new bool[nMaxList];
            alertF = new bool[nMaxList];
            for (int i = 0; i < nMaxList; i++)
            {
                // on met les dates à la première possible affichable dans les date-time controls
                Deb[i] = Fin[i] = new DateTime(1753, 1, 1);
                // on met toutes les alertes comme n'ayant pas encore été déclenchées
                alertD[i] = alertF[i] = false;
            }
        }
        /// <summary>
        /// Appelé par la procédure de parsing du NOTAM pour enregistrer une nouvelle période d'ouverture
        /// </summary>
        /// <param name="index"></param>
        /// <param name="deb"></param>
        /// <param name="fin"></param>
        public void SetTime(int index, DateTime deb, DateTime fin)
        {
            if (index < nMaxList)
            {
                Deb[index] = deb;
                Fin[index] = fin;
            }
            else MessageBox.Show($"Cette zone contient trop de périodes d'ouverture, seules les {nMaxList.ToString()} premières sont récupérées!");
        }
    }
}
