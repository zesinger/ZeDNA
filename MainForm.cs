using NAudio.Wave;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using static ZeDNA.GetData;


namespace ZeDNA
{
    /// <summary>
    /// Classe de la fenêtre principale
    /// </summary>
    public partial class MainForm : Form
    {
        public static int M_version = 2;
        public static int m_version = 2;
        public static int p_version = 1;

        /// <summary>
        /// liste des zones actives sur la journée parsées depuis ce qui a été trouvé sur internet
        /// cette liste ne contient QUE les zones actives sur la journée et peut donc être nulle
        /// </summary>
        public static List<Zone> todayZones = null;
        /// <summary>
        /// timer appelé toutes les secondes pour vérifier si on doit afficher des alertes de début et de fin d'activité des zones
        /// </summary>
        private Timer timer;
        /// <summary>
        /// timer appelé toutes les 0.2 secondes pour mettre à jour la carte
        /// </summary>
        public static Timer timerop;
        /// <summary>
        /// liste des zones actives, cette liste contient tous les éléments surveillés, même ceux qui ne sont pas actifs sur la journée,
        /// un élément par élement du tableau `zoneList` de la classe GetData (donc pas le même nombre d'éléments que todayZones)
        /// </summary>
        public static bool[] zoneActive;
        /// <summary>
        /// liste des zones actives, cette liste contient tous les éléments surveillés, même ceux qui ne sont pas actifs sur la journée,
        /// un élément par élement du tableau `uzoneList` de la classe GetData (donc pas le même nombre d'éléments que todayZones)
        /// </summary>
        public static bool[] uzoneActive;
        /// <summary>
        /// Numéro de l'élément survolé dans le tableau par la souris dans le tableau todayZones
        /// </summary>
        private int hoveredZone = -1;
        /// <summary>
        /// Nombre de zones RTBA (= début des zones supérieures)
        /// </summary>
        public static int? nRtba = 0;
        /// <summary>
        /// value kept from one call to CheckAlerts() to the other to check if we have changed the day
        /// </summary>
        private DateTime lastNow = DateTime.UtcNow;
        private ContextMenuStrip contextMenu;
        /// <summary>
        /// Indice de l'élément qui a été cliqué droit pour ouvrir le contect menu
        /// </summary>
        private int rightclickedIdx;
        /// <summary>
        /// Initialize les controls de la fenêtre, les arrays ainsi que les timers
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            zoneActive = new bool[GetData.zoneList.Length];
            for (int i = 0; i < zoneActive.Length; i++) zoneActive[i] = false;
            uzoneActive = new bool[GetData.uzoneList.Length];
            for (int i = 0; i < uzoneActive.Length; i++) uzoneActive[i] = false;
            Text = "ZeDNA v" + M_version.ToString() + "." + m_version.ToString() + "." + p_version.ToString() + ": Activités des espaces militaires";
            // rend les colonnes de la list view à largeur fixe
            listZones.ColumnWidthChanging += (sender, e) =>
            {
                e.NewWidth = listZones.Columns[e.ColumnIndex].Width; // Keep the original width
                e.Cancel = true; // Prevent resizing
            };
            Carte.LoadCarte();
            // owner draw pour pouvoir changer le status gras et la couleur de ce qui est affiché dans la list view des zones
            listZones.OwnerDraw = true;
            // function d'affichage des en-têtes de colonne
            listZones.DrawColumnHeader += listZones_DrawColumnHeader;
            // function d'affichage des éléments de la list view des zones 
            listZones.DrawSubItem += listZones_DrawSubItem;
            // on surveille la position de la souris dans cette list-view pour faire clignoter la zone survolée sur la carte
            listZones.MouseMove += listZones_MouseMove;
            listZones.MouseLeave += listZones_MouseLeave;
            // créer un context menu popur pouvoir ajouter/retirer des créneaux
            contextMenu = new ContextMenuStrip();

            // Add menu items
            ToolStripMenuItem option1 = new ToolStripMenuItem("Ajouter une activité pour une zone non listée", null, Creer_new_Click);
            ToolStripMenuItem option2 = new ToolStripMenuItem("Ajouter une activité pour cette zone", null, Add_new_Click);
            ToolStripMenuItem option3 = new ToolStripMenuItem("Supprimer une activité pour cette zone", null, Del_new_Click);

            // Add items to context menu
            contextMenu.Items.Add(option1);
            contextMenu.Items.Add(option2);
            contextMenu.Items.Add(option3);

            // Attach menu to ListView
            listZones.ContextMenuStrip = contextMenu;

            // Handle right-click event
            listZones.MouseUp += ListZones_MouseUp;
            // on initialise le timer des alertes
            timer = new Timer { Interval = 1000 }; // 1 second
            timer.Tick += (s, e) => CheckNewAlert(false);
            // on charge les zones sauvegardées à la dernière utilisation
            todayZones = LoadZonesFromFile("zones2.json");
            // on passe ce Panel en dessin double buffering pour éviter les clignotements pénibles
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, panelMap, new object[] { true });
            // on remplit la list view des zones si du contenu a été sauvegardé
            if (todayZones != null)
            {
                foreach (var zone in todayZones)
                {
                    ListViewItem item = new ListViewItem(zone.Name);
                    for (int i = 0; i < nMaxList; i++)
                    {
                        if (zone.Deb[i] != new DateTime(1753, 1, 1)) item.SubItems.Add(zone.Deb[i].ToString("HH:mm"));
                        else item.SubItems.Add("");
                        if (zone.Deb[i] != new DateTime(1753, 1, 1)) item.SubItems.Add(zone.Fin[i].ToString("HH:mm"));
                        else item.SubItems.Add("");
                    }
                    listZones.Items.Add(item);
                }
            }
            CheckNewAlert(true);
            listZones.Invalidate();
            // on définit les zones comme inactives par défaut, la function listZones_DrawSubItem se chargera de les mettre à la bonne valeur
            panelMap.Paint += (s, e) => DessineCarte(e);
            timerop = new Timer { Interval = 100 }; // 0.1 second
            timerop.Tick += (s, e) => { panelMap.Invalidate(); };
            // on fixe les valeurs initiales des surveillance d'alerte et on affiche les zones déjà ouvertes
            // on démarre les timers
            timer.Start();
            timerop.Start();
        }
        /// Right-click handler
        private void Creer_new_Click(object sender, EventArgs e)
        {
            AddCreneau ac = new AddCreneau(0, -1);
            ac.ShowDialog();
            // Si on retourne new DateTime(1753, 1, 1), c'est qu'on a annulé
            if (AddCreneau.debNewActivity == new DateTime(1753, 1, 1)) return;
            AddCreneau.CreateZoneAndInterval();
            UpdateDisplayZone();
        }

        private void Add_new_Click(object sender, EventArgs e)
        {
            AddCreneau ac = new AddCreneau(1, rightclickedIdx);
            ac.ShowDialog();
            // Si on retourne new DateTime(1753, 1, 1), c'est qu'on a annulé
            if (AddCreneau.debNewActivity == new DateTime(1753, 1, 1)) return;
            AddCreneau.InsertAndMergeIntervals();
            SaveZonesToFile(todayZones, "zones2.json");
            UpdateDisplayZone();
        }

        private void Del_new_Click(object sender, EventArgs e)
        {
            AddCreneau ac = new AddCreneau(2, rightclickedIdx);
            ac.ShowDialog();
            // Si on retourne new DateTime(1753, 1, 1), c'est qu'on a annulé
            if (AddCreneau.debNewActivity == new DateTime(1753, 1, 1)) return;
            AddCreneau.DeleteInterval();
            UpdateDisplayZone();
        }
        private void ListZones_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListViewItem item = listZones.GetItemAt(e.X, e.Y);

                if (item != null)
                {
                    AddCreneau.noselZone = rightclickedIdx = listZones.Items.IndexOf(item); // Get position in the full list
                    contextMenu.Items[1].Enabled = true;  // Option 2
                    contextMenu.Items[2].Enabled = true; // Option 3
                }
                else
                {
                    contextMenu.Items[1].Enabled = false; // Option 2
                    contextMenu.Items[2].Enabled = false;  // Option 3
                }
            }
        }
        /// <summary>
        /// Fonction appelée régulièrement pour mettre à jour la carte
        /// </summary>
        /// <param name="e"></param>
        private void DessineCarte(PaintEventArgs e)
        {
            int nact = 0;
            for (int ti = 0; ti < zoneActive.Length; ti++)
            {
                if (zoneActive[ti]) nact++;
            }
            for (int ti = 0; ti < uzoneActive.Length; ti++)
            {
                if (uzoneActive[ti]) nact++;
            }
            string[] layers = new string[nact]; 
            int tj = 0;
            for (int ti = 0; ti < zoneActive.Length; ti++)
            {
                if (zoneActive[ti])
                {
                    layers[tj] = GetData.zoneList[ti];
                    tj++;
                }
            }
            for (int ti = 0; ti < uzoneActive.Length; ti++)
            {
                if (uzoneActive[ti])
                {
                    layers[tj] = GetData.uzoneList[ti].ctrName;
                    tj++;
                }
            }
            string hovzone = "";
            if (hoveredZone >= 0)
            {
                hovzone = todayZones[hoveredZone].Name.Split(' ')[0];
                textZone.Text = hovzone;
                (textPlancher.Text, textPlafond.Text) = GetData.GetZoneHeights(hovzone);
            }
            Carte.DrawCarte(e, layers, hovzone);
        }
        /// <summary>
        /// Convertit un nom de zone en indice dans le array `zoneList`
        /// Attention à ne fournir que la zone sous la forme "R45A" mais sans la description géographique genre " BOURGOGNE"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static (bool, int) GetZoneIdx(string name)
        {
            // retourne -1 si on ne trouve pas cette zone dans la liste
            int found = -1;
            bool isrtba = true;
            for (int i = 0; i < GetData.zoneList.Length; i++)
            {
                // si on trouve une zone dans zoneList qui porte le même nom, on rend son indice
                if (GetData.zoneList[i] == name)
                {
                    found = i;
                }
            }
            if (found == -1)
            {
                for (int i = 0; i < GetData.uzoneList.Length; i++)
                {
                    // si on trouve une zone dans zoneList qui porte le même nom, on rend son indice
                    if (GetData.uzoneList[i].ctrName == name)
                    {
                        found = i;
                        isrtba = false;
                    }
                }
            }
            return (isrtba, found);
        }
        /// <summary>
        /// Fonction appelée quand la souris se déplacer sur le tableau des zones listZones
        /// Vérifie si le curseur de la souris survole une ligne contenant vraiment une zone
        /// et définit la valeur de hoveredZone utilisée dans SetOpacities() en fonction (-1 si aucune colonne n'est survolée)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listZones_MouseLeave(object sender, EventArgs e)
        {
            hoveredZone = -1;
            textZone.Text = textPlancher.Text = textPlafond.Text = "";
        }
        private void listZones_MouseMove(object sender, MouseEventArgs e)
        {
            // on récupère la colonne dans item, s'il n'y a pas de contenu dans la ligne, item est null
            ListViewHitTestInfo hitTestInfo = listZones.HitTest(e.Location);
            ListViewItem item = hitTestInfo.Item;
            // s'il y a bien un item dans la colonne survolée on définit hoveredZone, sinon -1

            if (item != null) hoveredZone = item.Index; else hoveredZone = -1;
        }
        /// <summary>
        /// Function appelée par le timer `timer` toutes les secondes
        /// Vérifie si de nouvelles alertes doivent être affichées
        /// </summary>
        /// <param name="firsttime"></param>
        private void CheckNewAlert(bool firsttime)
        {
            timer.Stop();
            DateTime now = DateTime.UtcNow;
            if (todayZones == null || todayZones.Count == 0) 
            {
                lastNow = now;
                Invalidate();
                timer.Start();
                return;
            }
            List<Alert> al = new List<Alert>();
            if (now.Date != lastNow.Date)
            {
                todayZones = new List<Zone>();
                for (int i = 0; i < zoneActive.Length; i++) zoneActive[i] = false;
                for (int i = 0; i < uzoneActive.Length; i++) uzoneActive[i] = false;
                nRtba = 0;
                SaveZonesToFile(todayZones, "zones2.json");
                listZones.Items.Clear();
                MessageBox.Show("La date ayant changé, les valeurs ont été réinitialisées, veuillez mettre à jour.");
                lastNow = now;
                Invalidate();
                timer.Start();
                return;
            }
            // au lancement, on définit l'état des alertes et on prévient si des zones sont déjà actives
            if (firsttime)
            {
                for (int i = 0; i < zoneActive.Length; i++) zoneActive[i] = false;
                for (int i = 0; i < uzoneActive.Length; i++) uzoneActive[i] = false;
                for (int i = 0; i < todayZones.Count; i++)
                {
                    Zone z = todayZones[i];
                    for (int j = 0; j < nMaxList; j++)
                    {
                        DateTime fin = z.Fin[j];
                        // si la période complète est déjà finie, on désactive la vérification des alertes de début et de fin
                        if (z.Fin[j] <= now)
                        {
                            z.alertD[j] = z.alertF[j] = true;
                        }
                        // si on est dans la période, on crée une alerte de zone active et on désactive la vérification d'alerte de début
                        else if (z.Deb[j] <= now)
                        {
                            (bool isrtba, int idx) = GetZoneIdx(z.Name.Split(' ')[0]);
                            if (idx >= 0)
                            {
                                if (isrtba) zoneActive[idx] = true;
                                else uzoneActive[idx] = true;
                            }
                            z.alertD[j] = true;
                            z.alertF[j] = false;
                            al.Add(new Alert(z.Name, true));
                        }
                        // sinon les alertes de début et de fin continuent d'être vérifiées
                        else
                        {
                            z.alertD[j] = z.alertF[j] = false;
                        }
                    }
                }
            }
            // les fois suivantes, on regarde juste s'il y a des nouvelles zones qui ont activé ou désactivé
            else
            {
                for (int i = 0; i < todayZones.Count; i++)
                {
                    Zone z = todayZones[i];
                    for (int j = 0; j < nMaxList; j++)
                    {
                        // si l'alerte de début est encore vérifiée et qu'on a passé l'horaire de début, nouvelle alerte de début et on désactive la vérification
                        if (z.Deb[j] <= now && z.alertD[j] == false)
                        {
                            (bool isrtba, int idx) = GetZoneIdx(z.Name);
                            if (idx >= 0)
                            {
                                if (isrtba) zoneActive[idx] = true;
                                else uzoneActive[idx] = true;
                            }
                            z.alertD[j] = true;
                            al.Add(new Alert(z.Name, true));
                        }
                        // si l'alerte de fin est encore vérifiée et qu'on a passé l'horaire de fin, nouvelle alerte de fin et on désactive la vérification
                        if (z.Fin[j] <= now && z.alertF[j] == false)
                        {
                            (bool isrtba, int idx) = GetZoneIdx(z.Name);
                            if (idx >= 0)
                            {
                                if (isrtba) zoneActive[idx] = false;
                                else uzoneActive[idx] = false;
                            }
                            z.alertF[j] = true;
                            al.Add(new Alert(z.Name, false));
                        }
                    }
                }
                // s'il y a au moins une alerte, un ou plusieurs items du tableau doivent être redessinées, on redessine tout
                if (al.Count > 0) listZones.Invalidate();
                lastNow = now;
            }
            // on génère le messages à afficher dans la messagebox, 1 ligne par alerte
            string mes = "";
            foreach (var z in al)
            {
                string val = "active";
                if (z.Active == false) val = "inactive";
                mes += "La zone " + z.Name + " est devenue " + val + ".\r\n";
            }
            // on vérifie que l'affichage des notifications est active
            if (mes != "" && checkNotif.Checked)
            {
                // on vérifie que le son de notification est actif
                if (checkSound.Checked)
                {
                    bool sound = false;
                    var output = new WaveOutEvent();
                    // puis on recherche un fichier "notif.wav"
                    if (File.Exists("notif.wav"))
                    {
                        var reader = new WaveFileReader("notif.wav");
                        output.Init(reader);
                        sound = true;
                    }
                    // s'il n'existe pas, on recherche un fichier "noptif.mp3"
                    else if (File.Exists("notif.mp3"))
                    {
                        var reader = new AudioFileReader("notif.mp3");
                        output.Init(reader);
                        sound = true;
                    }
                    // si un son est trouvé, on le joue
                    if (sound) output.Play();
                }
                // et on affiche le message avec les alertes
                MessageBox.Show(mes, "Liste des changements:", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            timer.Start();
        }
        /// <summary>
        /// Permet de owner draw les headers de colonne de la list view des zones, mais on n'y fait rien de spécial
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listZones_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }
        /// <summary>
        /// Permet de owner draw les sous-items des lignes de la list view des zones
        /// Affiche le contenu des cases du tableau des zones avec des couleurs et du gras en fonction de l'activité
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listZones_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            int ci = e.ColumnIndex - 1;
            Zone z = todayZones[e.ItemIndex];
            DateTime now = DateTime.UtcNow;
            if (e.ItemIndex % 2 == 1)
            {
                // Fill the background with light gray
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 240, 240, 240)))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }
            else
            {
                // Fill with white for even rows
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }
            // si on est dans la colonne des noms
            if (ci == -1)
            {
                // on vérifie si la zone est active
                bool active = false;
                for (int i = 0; i < nMaxList; i++)
                {
                    if (now >= z.Deb[i] && now <= z.Fin[i])
                    {
                        active = true;
                        break;
                    }
                }
                // et on dessine le texte: si la zone est active, on l'affiche en gras et rouge
                if (active) e.Graphics.DrawString(e.SubItem.Text, new Font(e.SubItem.Font, FontStyle.Bold), Brushes.Red, e.Bounds);
                else e.Graphics.DrawString(e.SubItem.Text, new Font(e.SubItem.Font, FontStyle.Bold), Brushes.Black, e.Bounds);
            }
            // sinon on est dans la colonne des horaires
            else
            {
                DateTime deb = z.Deb[ci / 2];
                DateTime fin = z.Fin[ci / 2];
                // si toute la période est terminée on affiche en gris
                if (fin < now) e.Graphics.DrawString(e.SubItem.Text, listZones.Font, new SolidBrush(Color.FromArgb(255, 200, 200, 200)), e.Bounds);
                // si on est dans la période, on affiche en rouge et gras
                else if (deb <= now) e.Graphics.DrawString(e.SubItem.Text, new Font(e.SubItem.Font, FontStyle.Bold), Brushes.Red, e.Bounds);
                // sinon on est dans une période à venir, on l'affiche en noir
                else e.Graphics.DrawString(e.SubItem.Text, new Font(e.SubItem.Font, FontStyle.Regular), Brushes.Black, e.Bounds);
            }
            if (e.ItemIndex == nRtba)
            {
                e.Graphics.DrawLine(new Pen(Color.Green, 1), e.Bounds.Left, e.Bounds.Top, e.Bounds.Right, e.Bounds.Top);
            }
        }
        /// <summary>
        /// On sauvegarde les zones actives sur la journée récupérées sur internet dès qu'on les a analysées
        /// on lui adjoint la date du jour concerné par cette activité de zones
        /// </summary>
        /// <param name="listZones"></param>
        /// <param name="filePath"></param>
        void SaveZonesToFile(List<Zone> listZones, string filePath)
        {
            //sauver aussi la date de ces zones
            var zoneFile = new ZoneFile
            {
                Date = GetData.dateData,
                Zones = listZones,
                NRtba = nRtba
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(zoneFile, options);
            File.WriteAllText(filePath, json);
        }
        /// <summary>
        /// On charge les zones sauvegardées ci-dessus au démarrage du programme, comme ça, s'il y a eu plantage et relance, rien n'a été perdu
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        List<Zone> LoadZonesFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                nRtba = 0;
                GetData.dateData = null;
                return new List<Zone>();
            }

            string json = File.ReadAllText(filePath);
            var zoneFile = JsonSerializer.Deserialize<ZoneFile>(json);

            GetData.dateData = zoneFile?.Date;
            DateTime loaddt;
            if (DateTime.TryParseExact(dateData, "dd MM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out loaddt) && loaddt < DateTime.UtcNow.Date)
            {
                MessageBox.Show("Les données chargées correspondent à un jour passé, les données sont effacées. Veuillez mettre à jour.");
                nRtba = 0;
                GetData.dateData = null;
                return new List<Zone>();
            }
            if (GetData.dateData != null) textDate.Text = "Données affichées pour la journée du : " + GetData.dateData;
            nRtba = zoneFile?.NRtba;
            if (nRtba == null) nRtba = 0;
            return zoneFile?.Zones ?? new List<Zone>();
        }
        /// <summary>
        /// Action provoquée par l'appui sur le bouton de "Mise à jour"
        /// - Arrête le timer de vérification des alertes
        /// - Appelle GetData.GetRtbaData() qui va chercher les données sur internet et les analyse
        /// - Sauvegarde immédiatement ces données récupérées
        /// - Met à jour la list view des zones
        /// - Appelle CheckNewAlert() comme si on venait de démarer le logiciel en mode appel initial
        /// - Redémarre le timer de vérification des alertes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            // on arrête d'analyser les alertes
            timer.Stop();
            // on garde les anciennes valeurs au cas où la récupération sur internet ne marcherait pas
            List<Zone> toz = Enumerable.Take(todayZones, (int)nRtba).ToList();
            List<Zone> touz = Enumerable.Skip(todayZones, (int)nRtba).ToList();
            // on récupère les données dans le NOTAM sur internet et on l'analyse
            // - si les zones retournées sont null, il y a eu un souci pendant la récupération
            // - si c'est une liste vide, il n'y a pas de zone active dans ce qui a été téléchargé
            // - sinon on a une liste avec des éléments
            List<Zone> tz = GetData.GetRtbaData();
            int npareil = 0;
            if (tz.Count == 0)
            {
                // si non null (notams chargés) mais pas de résultat, on garde les anciennes valeurs
                tz = toz;
                npareil++;
            }
            else if (tz != null)
                // si non null et des résultats trouvés, on met à jour avec les nouvelles valeurs
                nRtba = tz.Count;
            else
            {
                // si null, on affiche une erreur et on garde les vieilles valeurs
                MessageBox.Show("Il y a eu un souci dans la récupération des RTBA, les anciennes valeurs sont conservées.");
                tz = toz;
                npareil++;
            }
            List<Zone> tuz = GetData.GetUpperZones();
            if (tuz == null)
            {
                MessageBox.Show("Il y a eu un souci dans la récupération des autres zones militaires (hors RTBA), les anciennes valeurs sont conservées.");
                tuz = touz;
                npareil++;
            }
            // impossible de télécharger quoi que ce soit, on ne change rien
            if (npareil == 2)
            {
                timer.Start();
                return; // évite de recréer le tableau pour rien
            }
            // s'il n'y a pas de nouvelles données, on garde les anciennes, on redémarre les alertes et on quitte
            // sinon, on définit toutes les zones comme inactives et listZones_DrawSubItem() se chargera de le remplir comme il faut
            for (int i = 0; i < zoneActive.Length; i++) zoneActive[i] = false;
            if (tz != null)
            {
                todayZones = tz;
                if (tuz != null) todayZones.AddRange(tuz);
            }
            else todayZones = tuz;
            // on sauvegarde immédiatement les données recueillies sur internet pour éviter de les perdre en cas de plantage ou de quittage inopiné
            SaveZonesToFile(todayZones, "zones2.json");
            // on affiche la date des données
            if (GetData.dateData != "") textDate.Text = "Données affichées récupérées le " + DateTime.UtcNow.ToString("dd/MM/yyyy")+" à "+ DateTime.UtcNow.ToString("HH:mm");
            else textDate.Text = "Pas de données chargées encore...";
            // puis on remplit le tableau
            UpdateDisplayZone();
            // on lance le test initial des alertes
            CheckNewAlert(true);
            // puis on redémarre le timer d'appel à CheckNewAlert()
            timer.Start();
            Invalidate();
        }
        /// <summary>
        /// Bouton pour imprimer le contenu du tablea
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPrint_Click(object sender, EventArgs e)
        {
            int startX, startY;
            startY = startX = 50;
            int cellHeight = 30;
            int[] columnWidths = { 210, 65, 65, 65, 65, 65, 65, 65, 65 }; // Adjust based on ListView columns

            PdfDocument document = new PdfDocument();
            document.Info.Title = "Table Export";

            // Add a landscape page
            PdfPage page = document.AddPage();
            page.Orientation = PdfSharp.PageOrientation.Landscape;

            // Create a graphics object for the page
            XGraphics gfx = XGraphics.FromPdfPage(page);
            double pageHeight = page.Height - startX; // Available height excluding margins

            // Define fonts and pens
            XFont font = new XFont("Arial", 10, XFontStyleEx.Regular);
            XPen pen = new XPen(XColors.Black, 1);

            // Draw table header
            int x = startX;
            int i = 0;
            foreach (ColumnHeader column in listZones.Columns)
            {
                gfx.DrawRectangle(pen, x, startY, columnWidths[i], cellHeight);
                gfx.DrawString(column.Text, font, XBrushes.Black, x + 5, startY + (cellHeight / 2) + 5);
                x += columnWidths[i++];
            }

            // Draw table rows
            startY += cellHeight;
            while (rowIndex < listZones.Items.Count)
            {
                x = startX;
                // Check if page is full
                if (startY + cellHeight > pageHeight)
                {
                    page = document.AddPage();
                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                    gfx = XGraphics.FromPdfPage(page);
                    startY = startX; // Reset Y position for the new page
                }
                for (int col = 0; col < listZones.Columns.Count; col++)
                {
                    if (rowIndex % 2 == 1)
                    {
                        gfx.DrawRectangle(new XSolidBrush(XColors.LightGray), x, startY, columnWidths[col], cellHeight);
                    }
                    gfx.DrawRectangle(pen, x, startY, columnWidths[col], cellHeight);
                    gfx.DrawString(listZones.Items[rowIndex].SubItems[col].Text, font, XBrushes.Black, x + 5, startY + 5 + (cellHeight / 2));
                    x += columnWidths[col];
                }
                startY += cellHeight;
                rowIndex++;
            }
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "zones_militaires.pdf");
            document.Save(filePath);
        }
        private int rowIndex = 0; // Track printed rows
        private void PrintDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            int startX = 50, startY = 50, cellHeight = 30;
            int[] columnWidths = { 300, 95, 95, 95, 95, 95, 95, 95, 95 }; // Adjust based on ListView columns

            using (Pen pen = new Pen(Color.Black, 1))
            using (Font font = new Font("Arial", 10))
            {
                // Draw table header
                int x = startX;
                int i = 0;
                foreach (ColumnHeader column in listZones.Columns)
                {
                    e.Graphics.DrawRectangle(pen, x, startY, columnWidths[i], cellHeight);
                    e.Graphics.DrawString(column.Text, font, Brushes.Black, x + 5, startY + 5);
                    x += columnWidths[i++];
                }

                // Draw table rows
                startY += cellHeight;
                while (rowIndex < listZones.Items.Count)
                {
                    x = startX;
                    for (int col = 0; col < listZones.Columns.Count; col++)
                    {
                        if (rowIndex % 2 == 1)
                        {
                            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), x, startY, columnWidths[col], cellHeight);
                        }
                        e.Graphics.DrawRectangle(pen, x, startY, columnWidths[col], cellHeight);
                        e.Graphics.DrawString(listZones.Items[rowIndex].SubItems[col].Text, font, Brushes.Black, x + 5, startY + 5);
                        x += columnWidths[col];
                    }
                    startY += cellHeight;
                    rowIndex++;

                    // Check if page is full
                    if (startY + cellHeight > e.MarginBounds.Bottom)
                    {
                        e.HasMorePages = true;
                        return;
                    }
                }
                e.HasMorePages = false;
            }
        }

        /// <summary>   
        /// Si la case des alertes messages n'est pas cochée, la case des alertes son est désactivée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkNotif_CheckedChanged(object sender, EventArgs e)
        {
            checkSound.Enabled = checkNotif.Checked;
        }

        private void UpdateDisplayZone()
        {
            listZones.Items.Clear();
            foreach (var zone in todayZones)
            {
                ListViewItem item = new ListViewItem(zone.Name);
                for (int i = 0; i < nMaxList; i++)
                {
                    if (zone.Deb[i] != new DateTime(1753, 1, 1)) item.SubItems.Add(zone.Deb[i].ToString("HH:mm"));
                    else item.SubItems.Add("");
                    if (zone.Deb[i] != new DateTime(1753, 1, 1)) item.SubItems.Add(zone.Fin[i].ToString("HH:mm"));
                    else item.SubItems.Add("");
                }
                listZones.Items.Add(item);
            }
        }
    }
}
