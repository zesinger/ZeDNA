using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ZeDNA
{
    /// <summary>
    /// Classe de la fenêtre d'ajout ou de suppression manuelle de créneaux d'activité
    /// </summary>
    public partial class AddCreneau : Form
    {
        /// <summary>
        /// nom de la zone sélectionnée dans la combobox avant de fermer la fenêtre
        /// </summary>
        public static string nomZoneSel;
        /// <summary>
        /// mode pour la boîte de dialogue: 0 - créer pour une nouvelle zone, 1 - créer un nouveau créneau pour une zone existante, 2 - enlever un creéneau existant
        /// </summary>
        private static int modeAdd;
        /// <summary>
        /// numéro du créneau sélectionné dans le updown pour être effacé
        /// </summary>
        public static int noCreneauSel;
        /// <summary>
        /// numéro de la zone sélectionnée à l'ouverture dans la todayZones list
        /// </summary>
        public static int noselZone;
        /// <summary>
        /// horaires sélectionnés à la fermeture de la fenêtre
        /// </summary>
        public static DateTime debNewActivity, finNewActivity;
        /// <summary>
        /// Fonction appelée par les éléments du context menu pour ajouter ou effacer des créneaux
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="sel"></param>
        public AddCreneau(int mode, int sel)
        {
            InitializeComponent();
            // On remplit la liste des zones en fonction du type d'ajout/suppression de créneau d'activité
            comboZones.Items.Clear();
            modeAdd = mode;
            if (mode == 0)
            {
                // si on est en mode ajout d'un créneau pour une zone pas encore existante
                // la liste des créneaux dispo n'est pas affichée, on peut modifier le créneau,
                // on met ce créneau à maintenant et le bouton affiche "Ajouter"
                labelCreneau.Visible = false;
                numericCreneau.Visible = false;
                timeStart.Enabled = true;
                timeEnd.Enabled = true;
                timeEnd.Value = timeStart.Value = DateTime.UtcNow;
                buttonAdd.Text = "Ajouter";
                // on liste dans la combobox toutes les zones qui ne font pas encore partie des zones affichées...
                int ni = 0;
                // ... pour les RTBA
                foreach (var zone in GetData.zoneList)
                {
                    bool isin = false;
                    for (int i = 0; i < MainForm.todayZones.Count; i++)
                    {
                        if (MainForm.todayZones[i].Name.Split(' ')[0] == zone) { isin = true; break; }
                    }
                    if (!isin) { comboZones.Items.Add(zone); ni++; }
                }
                // ... pour les autres zones militaires
                foreach (var zone in GetData.uzoneList)
                {
                    bool isin = false;
                    for (int i = 0; i < MainForm.todayZones.Count; i++)
                    {
                        if (MainForm.todayZones[i].Name.Split(' ')[0] == zone.ctrName) { isin = true; break; }
                    }
                    if (!isin) { comboZones.Items.Add(zone.ctrName); ni++; }
                }
                // on définit la zone sélectionnée comme la première dans la liste (s'il y en a au moins une)
                if (ni > 0) comboZones.SelectedIndex = 0;
                else
                {
                    MessageBox.Show("Toutes les zones sont listées, impossible d'en rajouter une nouvelle.");
                    debNewActivity = new DateTime(1753, 1, 1);
                    finNewActivity = debNewActivity;
                    Close();
                }
            }
            else
            {
                // dans les cas où on ajoute à une zone existante ou on supprime d'une zon existante,
                // on n'affiche que cette zone dans la combobox sans son complément type "DAMBLAIN"
                string nomZone = MainForm.todayZones[sel].Name.Split(' ')[0];
                comboZones.Items.Add(nomZone);
                comboZones.SelectedIndex = 0;
                comboZones.Enabled = false;
                if (mode == 1)
                {
                    // si on est en mode ajout d'un créneau pour une zone déjà existante
                    // la liste des créneaux dispo n'est pas affichée, on peut modifier le créneau,
                    // on met ce créneau à maintenant et le bouton affiche "Ajouter"
                    labelCreneau.Visible = false;
                    numericCreneau.Visible = false;
                    timeStart.Enabled = true;
                    timeEnd.Enabled = true;
                    buttonAdd.Text = "Ajouter";
                    timeEnd.Value = timeStart.Value = DateTime.UtcNow;
                }
                else if (mode == 2)
                {
                    // si on est en mode suppression d'un créneau pour une zone déjà existante
                    // la liste des créneaux dispo est affichée, on ne peut pas modifier les valeur du créneau,
                    // on met ce créneau à la valeur du premier de la zone et le bouton affiche "Supprimer"
                    labelCreneau.Visible = true;
                    numericCreneau.Visible = true;
                    timeStart.Enabled = false;
                    timeEnd.Enabled = false;
                    numericCreneau.Value = 0;
                    buttonAdd.Text = "Supprimer";
                    Zone aczone = MainForm.todayZones[noselZone];
                    int i = 1;
                    // on fixe la valeur maximale du numéro de créneau en fonction du nombre disponible
                    for (; i < GetData.nMaxList; i++)
                    {
                        if (aczone.Deb[i] == new DateTime(1753, 1, 1)) break;
                    }
                    numericCreneau.Maximum = i - 1;
                    // et on affiche les valeurs de ce premier créneau
                    DisplayCreneau();
                }
            }
        }
        /// <summary>
        /// affiche les horaires de début et de fin d'un créneau d'activité dans les contrôles DateTime
        /// </summary>
        private void DisplayCreneau()
        {
            // on récupère le numéro du créneau et la zone choisie
            int nocreneau = (int)numericCreneau.Value;
            Zone aczone = MainForm.todayZones[noselZone];
            // si ce créneau est par défaut, on met la valeur par défaut
            if (aczone.Deb[nocreneau] == new DateTime(1753, 1, 1)) timeStart.Value = timeEnd.Value = aczone.Deb[nocreneau];
            else
            {
                // sinon on remplit avec les valeurs actives
                timeStart.Value = aczone.Deb[nocreneau];
                timeEnd.Value = aczone.Fin[nocreneau];
            }
        }
        /// <summary>
        /// Procédure pour ne pas avoir de croix de fermeture de la fenêtre
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x200; // CS_NOCLOSE: Disables the close button (X)
                return cp;
            }
        }
        /// <summary>
        /// Procédure quand on valide la suppression ou l'ajout de créneau d'activité
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // on met une valeur pour debNewActivity pour que ça ne soit pas considéré comme une annulation
            debNewActivity = timeStart.Value;
            finNewActivity = timeEnd.Value;
            // dans les cas de création, on vérifie que la zone ouvre au delà de maintenant
            if (modeAdd != 2 && (debNewActivity < DateTime.UtcNow || debNewActivity > DateTime.Today.AddDays(1)))
            {
                // sinon on demande à corriger
                MessageBox.Show("Le début de l'activité doit se situer entre maintenant et minuit, veuillez corriger!");
                return;
            }
            // si l'horaire de fin est situé avant celui de début, on ajoute 1 jour à la fin
            if (finNewActivity < debNewActivity) finNewActivity = finNewActivity.AddDays(1);
            // et on met les valeurs de retour (nom de zone pour tous et numéro du créneau pour la suppression)
            nomZoneSel = comboZones.Text;
            noCreneauSel = (int)numericCreneau.Value;
            // on ferme la fenêtre
            Close();
        }
        /// <summary>
        /// procédure quand on annule la création/suppression d'un créneau d'activité
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // on retourne la valeur par défaut pour que l'appelant sache qu'on a annulé dans le cas d'une création
            debNewActivity = new DateTime(1753, 1, 1);
            finNewActivity = debNewActivity;
            // on ferme la fenêtre
            Close();
        }
        /// <summary>
        /// Crée une zone qui n'existe pas encore dans la liste et définit un premier créneau
        /// </summary>
        public static void CreateZoneAndInterval()
        {
            // on crée la nouvelle zone avec son premier créneau d'activité
            Zone tzone = new Zone(nomZoneSel);
            tzone.Deb[0] = debNewActivity;
            tzone.Fin[0] = finNewActivity;
            // et on met par défaut les autres créneaux de la zone
            for (int i = 1; i < GetData.nMaxList; i++)
            {
                tzone.Fin[i] = tzone.Deb[i] = new DateTime(1753, 1, 1);
            }
            // on récupère les zones existantes et on sépare les zones RTBA des autres zones militaires
            List<Zone> toz = Enumerable.Take(MainForm.todayZones, (int)MainForm.nRtba).ToList();
            List<Zone> touz = Enumerable.Skip(MainForm.todayZones, (int)MainForm.nRtba).ToList();
            // puis en fonction de si la nouvelle zone est une RTBA ou non, on l'ajoute au bon tableau
            // et on fait le tri
            (bool isrtba, int idx) = MainForm.GetZoneIdx(nomZoneSel);
            if (isrtba && toz != null)
            {
                toz.Add(tzone);
                toz = toz.OrderBy(z => z.Name).ToList();
            }
            else if (!isrtba && touz != null)
            {
                touz.Add(tzone);
                touz = touz.OrderBy(z => z.Name).ToList();
            }
            // enfin on reconstitue la liste todayZones
            if (toz != null) MainForm.todayZones = toz;
            else MainForm.todayZones = new List<Zone>();
            MainForm.nRtba = MainForm.todayZones.Count;
            if (touz != null) MainForm.todayZones.AddRange(touz);
        }
        /// <summary>
        /// Insère un intervalle de temps parmis plusieurs autres et les fusionne s'ils se recouvrent
        /// Les mets aussi dans l'ordre
        /// </summary>
        /// <param name="intervals"></param>
        /// <param name="newInterval"></param>
        /// <returns></returns>
        public static void InsertAndMergeIntervals()
        {
            Zone aczone = MainForm.todayZones[noselZone];
            // on crée une List<DateTime, DateTime> à partir des tableaux de départ et de fin de créneau d'activité
            List<(DateTime start, DateTime end)> intervals = new List<(DateTime start, DateTime end)>();
            for (int ti = 0; ti < GetData.nMaxList; ti++)
            {
                if (aczone.Deb[ti] == new DateTime(1753, 1, 1)) break;
                intervals.Add((aczone.Deb[ti], aczone.Fin[ti]));
            }
            // puis on met le créneau à ajouter au même format (DateTime, DateTime)
            (DateTime start, DateTime end) newInterval = (debNewActivity, finNewActivity);
            // on crée une seconde List<DateTime, DateTime> pour recevoir le résultat
            List<(DateTime start, DateTime end)> result = new List<(DateTime, DateTime)>();

            // teste si le nouveau créneau s'insère dans ceux déjà existants (en fonction de l'horaire de départ)...
            bool inserted = false;
            foreach (var interval in intervals)
            {
                // ... Si oui, on le met à la bonne place parmi les "interval" existants ...
                if (!inserted && newInterval.start < interval.start)
                {
                    result.Add(newInterval);
                    inserted = true;
                }
                result.Add(interval);
            }
            // ... Si non, on le met à la fin
            if (!inserted) result.Add(newInterval);

            // On traite maintenant les recouvrements, on crée encore une nouvelle liste pour recevoir les résultats
            List<(DateTime start, DateTime end)> merged = new List<(DateTime, DateTime)>();
            foreach (var interval in result)
            {
                if (merged.Count == 0 || merged[merged.Count - 1].end < interval.start)
                    // si la fin d'un créneau est avant le début du suivant, on laisse
                    // donc on copie le créneau tel quel
                    merged.Add(interval);
                else
                    // sinon on fusionne avec le précédent
                    merged[merged.Count - 1] = (merged[merged.Count - 1].start, Max(merged[merged.Count - 1].end, interval.end));
            }

            // une fois que c'est fait, on recopie la List<...> dans les tableaux
            int i = 0;
            for (; i < Math.Min(merged.Count, GetData.nMaxList); i++)
            {
                aczone.Deb[i] = merged[i].start;
                aczone.Fin[i] = merged[i].end;
            }
            // et on termine en mettant par défaut les éléments du tableaux vides
            for (; i < GetData.nMaxList; i++) aczone.Fin[i] = aczone.Deb[i] = new DateTime(1753, 1, 1);
            // et en copiant le tout dans les todayZones
            MainForm.todayZones[noselZone] = aczone;
        }
        /// <summary>
        /// Efface un créneau d'activité, si c'est le seul créneau pour la zone, on efface la zone
        /// </summary>
        public static void DeleteInterval()
        {
            Zone aczone = MainForm.todayZones[noselZone];
            // on décale toutes les données dans les tableaux des créneuax d'activité
            for (int i = noCreneauSel; i < GetData.nMaxList - 1; i++)
            {
                aczone.Deb[i] = aczone.Deb[i + 1];
                aczone.Fin[i] = aczone.Fin[i + 1];
                aczone.alertD[i] = aczone.alertD[i + 1];
                aczone.alertF[i] = aczone.alertF[i + 1];
            }
            // et on met la dernière valeur par défaut (01/01/1753)
            aczone.Fin[GetData.nMaxList - 1] = aczone.Deb[GetData.nMaxList - 1] = new DateTime(1753, 1, 1);
            MainForm.todayZones[noselZone] = aczone;
            // si le premier créneau est déjà par défaut, on peut effacer la zone
            if (aczone.Fin[0] == new DateTime(1753, 1, 1))
            {
                // s'il n'y a plus de créneau dispo pour cette zone, on retire la zone de la todayZones list
                if (noselZone < MainForm.nRtba) MainForm.nRtba--;
                MainForm.todayZones.RemoveAt(noselZone);
            }
        }
        /// <summary>
        /// Fonction appelée quand on change le numéro du créneau d'activité par les boutons haut ou bas
        /// Utilisé uniquement quand on veut effacer un créneau
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericCreneau_ValueChanged(object sender, EventArgs e)
        {
            DisplayCreneau();
        }

        static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
    }
}
