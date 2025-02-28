using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ZeDNA
{
    /// <summary>
    /// Classe qui représente le contenu du fichier à sauvegarder sur le disque pour s'assurer de ne pas avoir de pertes en cas
    /// de crash ou de quittage inopiné
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
        /// Tableau des: faut-il encore surveiller l'heure de début pour déclencher l'alerte?
        /// </summary>
        public bool[] alertD { get; set; }
        /// <summary>
        /// tableau des heures de fermeture
        /// </summary>
        public DateTime[] Fin { get; set; }
        /// <summary>
        /// Tableau des: faut-il encore surveiller l'heure de fin pour déclencher l'alerte?
        /// </summary>
        public bool[] alertF { get; set; }

        /// <summary>
        /// Constructeur avec mise à zéro des horaires et déclenchement de la surveillance des alertes
        /// </summary>
        /// <param name="name"></param>
        public Zone(string name)
        {
            Name = name;
            Deb = new DateTime[4];
            Fin = new DateTime[4];
            alertD = new bool[4];
            alertF = new bool[4];
            for (int i = 0; i < 4; i++)
            {
                Deb[i] = Fin[i] = default;
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
            if (index < 4)
            {
                Deb[index] = deb;
                Fin[index] = fin;
            }
            else MessageBox.Show("Cette zone contient trop de périodes d'ouverture, seules les 4 premières sont récupérées!");
        }
    }
}
