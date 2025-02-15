using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace ZeDNA
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// liste des zones actives sur la journée parsées depuis ce qui a été trouvé sur internet
        /// cette liste ne contient QUE les zones actives sur la journée et peut donc être nulle
        /// </summary>
        private List<Zone> todayZones = null;
        /// <summary>
        /// 2 timers:
        /// - timer appelé toutes les secondes pour vérifier si on doit afficher des alertes de début et de fin d'activité des zones
        /// - timerop appelé tous les dixièmes de seconde pour changer l'affichage des cartes (cette fréquence est surtout due au
        ///   clignotement selon une sinusoïde de la zone survolée par la souris dans le tableau)
        /// </summary>
        private System.Windows.Forms.Timer timer, timerop;
        /// <summary>
        /// liste des zones actives, cette liste contient tous les éléments surveillés, même ceux qui ne sont pas actifs sur la journée,
        /// un élément par élement du tableau `zoneList` de la classe GetData (donc pas le même nombre d'éléments que todayZones)
        /// </summary>
        public static bool[] zoneActive;
        /// <summary>
        /// Numéro de l'élément survolé dans le tableau par la souris dans le tableau todayZones
        /// </summary>
        private int hoveredZone = -1;
        /// <summary>
        /// Initialize les controls de la fenêtre, les arrays ainsi que les timers
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            // rend les colonnes de la list view à largeur fixe
            listZones.ColumnWidthChanging += (sender, e) =>
            {
                e.NewWidth = listZones.Columns[e.ColumnIndex].Width; // Keep the original width
                e.Cancel = true; // Prevent resizing
            };
            // owner draw pour pouvoir changer le status gras et la couleur de ce qui est affiché dans la list view des zones
            listZones.OwnerDraw = true;
            // function d'affichage des en-têtes de colonne
            listZones.DrawColumnHeader += listZones_DrawColumnHeader;
            // function d'affichage des éléments de la list view des zones 
            listZones.DrawSubItem += listZones_DrawSubItem;
            // on surveille la position de la souris dans cette list-view pour faire clignoter la zone survolée sur la carte
            listZones.MouseMove += listZones_MouseMove;
            // on initialise le timer des alertes
            timer = new Timer { Interval = 1000 }; // 1 second
            timer.Tick += (s, e) => CheckNewAlert(false);
            // puis celui de l'affichage de la carte
            timerop = new Timer { Interval = 100 }; // 0.1 second
            timerop.Tick += (s, e) => SetOpacities();
            // on charge les zones sauvegardées à la dernière utilisation
            todayZones = LoadZonesFromFile("zones2.json");
            // on définit la fonction à appeler pour dessiner le Panel de dessin de la carte
            panelMap.Paint += AffCarte;
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
                    for (int i = 0; i < 4; i++)
                    {
                        if (zone.Deb[i] != default) item.SubItems.Add(zone.Deb[i].ToString("HH:mm"));
                        else item.SubItems.Add("");
                        if (zone.Deb[i] != default) item.SubItems.Add(zone.Fin[i].ToString("HH:mm"));
                        else item.SubItems.Add("");
                    }
                    listZones.Items.Add(item);
                }
            }
            // on définit les zones comme inactives par défaut, la function listZones_DrawSubItem se chargera de les mettre à la bonne valeur
            zoneActive = new bool[GetData.zoneList.Length];
            for (int i = 0; i < zoneActive.Length; i++) zoneActive[i] = false;
            // on fixe les valeurs initiales des surveillance d'alerte et on affiche les zones déjà ouvertes
            CheckNewAlert(true);
            // on démarre les timers
            timer.Start();
            timerop.Start();
        }
        /// <summary>
        /// Convertit un nom de zone en indice dans le array `zoneList`
        /// Attention à ne fournir que la zone sous la forme "R45A" mais sans la description géographique genre " BOURGOGNE"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private int GetZoneIdx(string name)
        {
            // retourne -1 si on ne trouve pas cette zone dans la liste
            int found = -1;
            for (int i = 0; i < GetData.zoneList.Length; i++)
            {
                // si on trouve une zone dans zoneList qui porte le même nom, on rend son indice
                if (GetData.zoneList[i].name == name)
                {
                    found = i;
                }
            }
            return found;
        }
        /// <summary>
        /// Cette fonction est appelée tous les dixièmes de seconde par le timer `timerop`
        /// Définit l'opacité des zones sur la carte en fonction de leur présence dans la liste todayZones et de leur activité
        /// et réaffiche la carte (panelMap.Invalidate();) si quelque chose a changé
        /// </summary>
        private void SetOpacities()
        {
            // on met la valeur par défaut de toutes les zones (1 si elles sont actives, 0 si elles ne le sont pas)
            for (int i = 0; i < GetData.zoneList.Length; i++)
            {
                var zn = GetData.zoneList[i];
                bool found = false;
                foreach (var z in todayZones)
                {
                    if (z.Name.Contains(zn.name) && zoneActive[i])
                    {
                        Carte.opacity[i] = 1;
                        found = true;
                        break;
                    }
                }
                // les zones qui ne sont pas dans la list todayZones ou celles qui ne sont pas actives pour l'instant ne sont pas affichées
                if (!found) Carte.opacity[i] = 0;
            }
            // puis on gère la zone survolée dans le tableau pour la faire clignoter
            if (hoveredZone >= 0)
            {
                // on revérifie que le curseur est toujours sur la list view listZones car il peut y avoir un loupé
                Point cursorPos = Cursor.Position;
                Point relativePos = listZones.PointToClient(cursorPos);
                if (relativePos.X >= 0 && relativePos.Y >= 0 &&
                    relativePos.X < listZones.Width && relativePos.Y < listZones.Height)
                {
                    // si c'est le cas, on regarde si on trouve la zone survolée dans le tableau GetData.zoneList grâce à GetZoneIdx()
                    int idx = todayZones[hoveredZone].Name.IndexOf(" ");
                    string name;
                    if (idx == -1) name = todayZones[hoveredZone].Name;
                    else name = todayZones[hoveredZone].Name.Substring(0, idx);
                    int found = GetZoneIdx(name);
                    if (found >= 0)
                    {
                        float period = 1.5f;
                        float time = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds; // Get current time in seconds
                        // on fait varier l'opacité de 0.5 à 1 à l'aide d'une sinusoïde de période `period` secondes
                        float value = 0.5f + (0.5f * (float)Math.Sin(2 * (float)Math.PI / period * time)); // Compute sine wave
                        Carte.opacity[found] = value;
                    }
                }
                // si on est en dehors du tableau, la hoveredZone est remise à -1 (ça peut arriver)
                else
                {
                    hoveredZone = -1;
                }
            }
            // On réaffiche la carte
            panelMap.Invalidate();
        }
        /// <summary>
        /// Fonction appelée quand la souris se déplacer sur le tableau des zones listZones
        /// Vérifie si le curseur de la souris survole une ligne contenant vraiment une zone
        /// et définit la valeur de hoveredZone utilisée dans SetOpacities() en fonction (-1 si aucune colonne n'est survolée)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listZones_MouseMove(object sender, MouseEventArgs e)
        {
            // on récupère la colonne dans item, s'il n'y a pas de contenu dans la ligne, item est null
            ListViewHitTestInfo hitTestInfo = listZones.HitTest(e.Location);
            ListViewItem item = hitTestInfo.Item;
            // s'il y a bien un item dans la colonne survolée on définit hoveredZone, sinon -1
            if (item != null) hoveredZone = item.Index; else hoveredZone = -1;
        }
        /// <summary>
        /// Juste une fonction locale pour appeler la vrai fonction d'affichage de la carte qui est dans la classe Carte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AffCarte(object sender, PaintEventArgs e)
        {
            Carte.DrawCarte(panelMap, e.Graphics);
        }
        /// <summary>
        /// Function appelée par le timer `timer` toutes les secondes
        /// Vérifie si de nouvelles alertes doivent être affichées
        /// </summary>
        /// <param name="firsttime"></param>
        private void CheckNewAlert(bool firsttime)
        {
            List<Alert> al = new List<Alert>();
            if (todayZones == null) return;
            TimeSpan now = DateTime.UtcNow.TimeOfDay;
            // au lancement, on définit l'état des alertes et on prévient si des zones sont déjà actives
            if (firsttime)
            {
                for (int i = 0; i < todayZones.Count; i++)
                {
                    Zone z = todayZones[i];
                    for (int j = 0; j < 4; j++)
                    {
                        // si la période complète est déjà finie, on désactive la vérification des alertes de début et de fin
                        if (z.Fin[j].TimeOfDay <= now)
                        {
                            z.alertD[j] = z.alertF[j] = true;
                        }
                        // si on est dans la période, on crée une alerte de zone active et on désactive la vérification d'alerte de début
                        else if (z.Deb[j].TimeOfDay <= now)
                        {
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
                    for (int j = 0; j < 4; j++)
                    {
                        // si l'alerte de début est encore vérifiée et qu'on a passé l'horaire de début, nouvelle alerte de début et on désactive la vérification
                        if (z.Deb[j].TimeOfDay <= now && z.alertD[j] == false)
                        {
                            z.alertD[j] = true;
                            al.Add(new Alert(z.Name, true));
                        }
                        // si l'alerte de fin est encore vérifiée et qu'on a passé l'horaire de fin, nouvelle alerte de fin et on désactive la vérification
                        if (z.Fin[j].TimeOfDay <= now && z.alertF[j] == false)
                        {
                            z.alertF[j] = true;
                            al.Add(new Alert(z.Name, false));
                        }
                    }
                }
                // s'il y a au moins une alerte, un ou plusieurs items du tableau doivent être redessinées, on redessine tout
                if (al.Count > 0) listZones.Invalidate();
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
                        var reader = new WaveFileReader("sound.wav");
                        output.Init(reader);
                        sound = true;
                    }
                    // s'il n'existe pas, on recherche un fichier "noptif.mp3"
                    else if (File.Exists("notif.mp3"))
                    {
                        var reader = new AudioFileReader("sound.mp3");
                        output.Init(reader);
                        sound = true;
                    }
                    // si un son est trouvé, on le joue
                    if (sound) output.Play();
                }
                // et on affiche le message avec les alertes
                MessageBox.Show(mes);
            }
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
            TimeSpan now = DateTime.UtcNow.TimeOfDay;
            // si on est dans la colonne des noms
            if (ci == -1)
            {
                // on vérifie si la zone est active
                bool active = false;
                for (int i = 0; i < 4; i++)
                {
                    if (now >= z.Deb[i].TimeOfDay && now <= z.Fin[i].TimeOfDay)
                    {
                        active = true;
                        break;
                    }
                }
                // on en profite pour remplir le array des zones actives `zoneActive`
                int idx = z.Name.IndexOf(" ");
                string name;
                if (idx == -1) name = z.Name;
                else name = z.Name.Substring(0, idx);
                idx = GetZoneIdx(name);
                if (idx >=0) zoneActive[idx] = active;
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
                if (fin.TimeOfDay < now) e.Graphics.DrawString(e.SubItem.Text, listZones.Font, Brushes.LightGray, e.Bounds);
                // si on est dans la période, on affiche en rouge et gras
                else if (deb.TimeOfDay <= now) e.Graphics.DrawString(e.SubItem.Text, new Font(e.SubItem.Font, FontStyle.Bold), Brushes.Red, e.Bounds);
                // sinon on est dans une période à venir, on l'affiche en noir
                else e.DrawDefault = true;
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
                Zones = listZones
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
                GetData.dateData = null;
                return new List<Zone>();
            }

            string json = File.ReadAllText(filePath);
            var zoneFile = JsonSerializer.Deserialize<ZoneFile>(json);

            GetData.dateData = zoneFile?.Date;
            if (GetData.dateData != null) textDate.Text = "Données affichées pour la journée du : " + GetData.dateData;
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
            // on récupère les données dans le NOTAM sur internet et on l'analyse
            List<Zone> tz = GetData.GetRtbaData();
            // s'il n'y a pas de nouvelles données, on garde les anciennes, on redémarre les alertes et on quitte
            if (tz == null)
            {
                MessageBox.Show("Aucune donnée concernant les zones RTBA pour aujourd'hui n'a été trouvée dans les NOTAMs sur sofia-briefing, les anciennes valeurs ont été gardées");
                timer.Start();
                return;
            }
            // sinon, on définit toutes les zones comme inactives et listZones_DrawSubItem() se chargera de le remplir comme il faut
            for (int i = 0; i < zoneActive.Length; i++) zoneActive[i] = false;
            todayZones = tz;
            // on sauvegarde immédiatement les données recueillies sur internet pour éviter de les perdre en cas de plantage ou de quittage inopiné
            SaveZonesToFile(tz, "zones2.json");
            // on affiche la date des données
            if (GetData.dateData != "") textDate.Text = "Données affichées pour la journée du : " + GetData.dateData;
            else textDate.Text = "Pas de données chargées encore...";
            // puis on remplit le tableau
            listZones.Items.Clear();
            foreach (var zone in todayZones)
            {
                ListViewItem item = new ListViewItem(zone.Name);
                for (int i = 0; i < 4; i++)
                {
                    if (zone.Deb[i] != default) item.SubItems.Add(zone.Deb[i].ToString("HH:mm"));
                    else item.SubItems.Add("");
                    if (zone.Deb[i] != default) item.SubItems.Add(zone.Fin[i].ToString("HH:mm"));
                    else item.SubItems.Add("");
                }
                listZones.Items.Add(item);
            }
            // on lance le test initial des alertes
            CheckNewAlert(true);
            // puis on redémarre le timer d'appel à CheckNewAlert()
            timer.Start();
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
    }
}
