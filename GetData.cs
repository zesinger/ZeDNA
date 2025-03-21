using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace ZeDNA
{
    /// <summary>
    /// Classe de récupération des NOTAM qui donnent la description de l'ouverture des zones
    /// Utilise Selenium qui lance Microsoft Edge et le contrôle pour naviguer et remplir le formulaire
    /// </summary>
    public static class GetData
    {
        /// <summary>
        /// Timeout en secondes pour les opérations de récupération sur internet
        /// Ne pas hésiter à mettre une valeur élevée, l'internet est assez irrégulier à la tour et
        /// l'application n'a pas besoin d'être hyper réactive
        /// </summary>
        private const int OVA_TIMEOUT = 60;
        /// <summary>
        /// nombre maximum de créneaux sur la journée
        /// si modifier, il faut aussi modifier le nombre de colonnes de la list view des zones
        /// </summary>
        public const int nMaxList = 4;
        /// <summary>
        /// Taille de l'image de fond de la carte (devrait être de la même taille que le panelMap du MainForm)
        /// </summary>
        public static readonly (int w, int h) fondDesc = (469, 450);
        /// <summary>
        /// Description des zones qu'on recherche dans le NOTAM: nom, position de l'image au dessus du fond (x,y) et dimension (width, height)
        /// </summary>
        public static readonly string[] zoneList = { "R45A", "R45B", "R45C","R45NS",
            "R45S1","R45S2","R45S3","R45S4","R45S5","R45S6.2","R45S6.1","R45S7","R152","R69", "LSR18" };
        public static readonly string[] zonePlanchers = {"SFC","SFC","SFC", "800AGL",
        "800AGL","800AGL","800AGL","800AGL","800AGL","800AGL","800AGL","800AGL","800AGL","800AGL", "SFC"};
        public static readonly string[] zonePlafonds = {"800AGL","800AGL","800AGL", "2800AGL",
        "4500AMSL","FL65","FL65","FL65","5000AMSL","6700AMSL","FL85","FL65","2800AGL","2700AGL", "4500AMSL"};
        /// <summary>
        /// Date du NOTAM affiché tel qu'affiché dans le NOTAM (JJ MM YYYY)
        /// </summary>
        public static string dateData = "";
        public static List<NotamChecker> notamCheckers = new List<NotamChecker>();
        public static List<Zone> GetRtbaData()
        {
            string notamresult;
            IWebDriver driver = null;
            try
            {
                // Démarrage de Microsoft Edge (browser obligatoirement présent sur tous les PC Windows)
                var options = new EdgeOptions();
                driver = new EdgeDriver(options);

                // Etape 1: Affichage de la page précédente (on ne peut pas afficher directement le formulaire)
                driver.Navigate().GoToUrl("https://sofia-briefing.aviation-civile.gouv.fr/sofia/pages/notamareamenu.html");

                // Etape 2: fixe un timeout puis attend que le bouton "FIR" soit clickable
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(OVA_TIMEOUT));
                IWebElement firButton = wait.Until(
                    ExpectedConditions.ElementToBeClickable(By.CssSelector("a.aside-link.d-block.ajax-link[href='/sofia/pages/notamsearchfir.html']"))
                );
                // Clic sur le bouton
                firButton.Click();

                // Etape 3: attend que la page soit affichée puis remplit le formulaire
                wait.Until(ExpectedConditions.UrlContains("notamsearchfir.html"));

                // première valeur à remplir:
                // l'heure qui doit être au delà que l'heure actuelle (UTC), donc on prend l'heure UTC et on ajoute 5 minutes
                string timeNowPlus5 = DateTime.UtcNow.AddMinutes(5).ToString("HHmm");
                // on repète le champ de l'heure
                IWebElement timeInput = driver.FindElement(By.Id("id_departureTime_notam_fir_val"));
                // on lui enlève son status readonly qui fait qu'il faut cliquer sur des boutons pour le changer
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].removeAttribute('readonly');", timeInput);
                // puis on fixe la valeur
                js.ExecuteScript("arguments[0].value = arguments[1];", timeInput, timeNowPlus5);

                // deuxième valeur à remplir:
                // les FIRs qui nous intéressent sont "LFEE" et "LSAS"
                // on repère le champ
                IWebElement firInput = driver.FindElement(By.Id("id_fir1"));
                // on le vide
                firInput.Clear();
                // et on rentre "LFEE"
                firInput.SendKeys("LFEE");
                // On déclenche un event pour être sûr que le changement a été pris en compte
                js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", firInput);
                // on rajoute une FIR avec le bouton "id_button_add_fir"
                IWebElement addfirButton = driver.FindElement(By.Id("id_button_add_FIR"));
                // et on le clique
                addfirButton.Click();
                // puis on rentre "LSAS" dans id_fir2
                IWebElement firInput2 = driver.FindElement(By.Id("id_fir2"));
                firInput2.Clear();
                firInput2.SendKeys("LSAS");
                js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", firInput2);

                // Etape 4: on soumet le formulaire en appuyant sur le bouton
                // on repère le bouton
                IWebElement searchButton = driver.FindElement(By.CssSelector("a.btn.btn-primary.btn-sm.notamsearchfir"));
                // et on le clique
                searchButton.Click();

                // Etape 6: On attend que les NOTAM soient chargés et on récupère le contenu
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id("id_notam_fir"))); // Adjust based on the results container
                IWebElement resultsDiv = driver.FindElement(By.Id("id_notam_fir"));
                notamresult = resultsDiv.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                notamresult = "";
            }
            finally
            {
                // quoi qu'il arrive, on ferme la fenêtre de Edge
                if (driver != null) driver.Quit();
            }

            // Si aucun résultat, on retourne null
            if (notamresult == "") return null;
            // exemple à retirer (utile pour les tests):
            //notamresult = "LFFA-Z0099/23\r\nDU: 01 06 2023 07:30 AU: 01 06 2023 23:59\r\nA)LFEE LFFF LFMM\r\nQ) LFXX / QRRCA / IV / BO / W / 000/085 / 4801N00521E099\r\nE) ZONES AIRFORCE RTBA ACT\r\nZONE R45A BOURGOGNE\r\n2007-2259:ACTIVE\r\nZONE R45B AUTUNOIS\r\n0800-1000:ACTIVE\r\n2007-2259:ACTIVE\r\nZONE R45C ARBOIS\r\n0800-1000:ACTIVE\r\n2007-2259:ACTIVE\r\nZONE R45S1 FRANCHE COMTE\r\n1500-2007:ACTIVE\r\nZONE R45S2 LANGRES\r\n1130-2359:ACTIVE\r\nZONE R45S3 YONNE\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S4 MACONNAIS OUEST\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S5 MACONNAIS CENTRE\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S6.1 MACONNAIS NORD EST\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S6.2 MACONNAIS SUD EST\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S7 JURA\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R69 CHAMPAGNE\r\n1130-1500:ACTIVE\r\n2007-2359:ACTIVE\r\nZONE R45NS\r\n1130-1500:ACTIVE\r\n2007-2359:ACTIVE\r\nF) SFC\r\nG) FL085\r\n";

            // I/ On va maintenant chercher le/s NOTAMs qui nous intéresse/nt pour les RTBA françaises, il/s commence/nt par "LFAA-Z"
            string finalresult = "";
            int offsNotam = notamresult.IndexOf("lffa-z", StringComparison.OrdinalIgnoreCase);
            // un "while" car si le message est trop long, il est réparti sur plusieurs notams
            bool messagefound = false;
            while (offsNotam >= 0)
            {
                // on obtient la date d'activité du notam pour être sûr qu'on ne récupère pas les ouvertures pour un autre jour
                bool notamToIgnore = false;
                DateTime notamDate;
                Match match = Regex.Match(notamresult.Substring(offsNotam), @"DU:\s*(\d{2} \d{2} \d{4})");
                string dateStr = "";
                if (match.Success)
                {
                    dateStr = match.Groups[1].Value;

                    // convertit de texte en DateTime
                    if (!DateTime.TryParseExact(dateStr, "dd MM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out notamDate))
                        notamToIgnore = true;
                    else
                    {
                        if (notamDate.Date != DateTime.UtcNow.Date) notamToIgnore = true;
                        else
                            dateData = dateStr;
                    }
                }
                else notamToIgnore = true;
                // 2 lignes suivantes à commenter pour les tests:
                // une fois qu'on a trouvé le NOTAM, on va à la section "E" de ce NOTAM, celle qui nous intéresse à partir de la seconde ligne
                offsNotam += notamresult.Substring(offsNotam).IndexOf("E)", StringComparison.OrdinalIgnoreCase);
                if (offsNotam == -1) notamToIgnore = true;
                // puis on passe la ligne
                offsNotam += notamresult.Substring(offsNotam).IndexOfAny(new char[] { '\r', '\n' });
                if (offsNotam == -1) notamToIgnore = true;
                while (offsNotam < notamresult.Length && (notamresult.Substring(offsNotam, 1) == "\r" || notamresult.Substring(offsNotam, 1) == "\n")) offsNotam++;
                if (offsNotam == notamresult.Length) notamToIgnore = true;
                // et on repère la fin de la partie qui nous intéresse, avant la section "F"
                int lenNotam = notamresult.Substring(offsNotam).IndexOf("F)", StringComparison.OrdinalIgnoreCase);
                if (lenNotam == -1) notamToIgnore = true;
                // si le NOTAM n'est pas ignoré à cause d'une date qui ne correspond pas, on l'ajoute au texte qui va être analysé
                if (!notamToIgnore)
                {
                    finalresult += notamresult.Substring(offsNotam, lenNotam);
                    messagefound = true;
                }
                // et on relance la recherche pour les NOTAM suivants contenant ce qui nous intéresse, s'il y en a plusieurs
                offsNotam = notamresult.IndexOf("lffa-z", offsNotam + lenNotam, StringComparison.OrdinalIgnoreCase);
            }
            // si aucun NOTAM correspondant n'a été trouvé, on retour null pour que les anciennes valeurs soient conservées
            if (!messagefound) return null;
            // Parsing du résultat zone par zone
            List<Zone> zones = ParseZones(finalresult);

            // II/ On cherche le notam pour la zone R174 Meyenheim
            notamCheckers.Add(new NotamChecker("R174", notamresult));
            notamCheckers[notamCheckers.Count - 1].IsActive("R174", DateTime.Today,ref zones);
            // III/ On cherche le notam pour la zone R171 Belfort
            notamCheckers.Add(new NotamChecker("R171", notamresult));
            notamCheckers[notamCheckers.Count - 1].IsActive("R171", DateTime.Today, ref zones);
            // IV/ On cherche le notam pour la zone suisse R18
            //notamCheckers.Add(new NotamChecker("LS-R18", notamresult));
            //notamCheckers[notamCheckers.Count - 1].IsActive("LS-R18", DateTime.Today, ref zones);
            notamCheckers.Add(new NotamChecker("LSR18", notamresult));
            notamCheckers[notamCheckers.Count - 1].IsActive("LSR18", DateTime.Today, ref zones);
            // on regarde si la zone est présente dans les notams
            // et tri dans l'ordre alphabétique des noms
            zones = zones.OrderBy(z => z.Name).ToList();
            return zones;
        }
        /// <summary>
        /// Analyse le contenu du/es NOTAM pour créer des blocs 
        /// </summary>
        /// <param name="finalresult">le contenu du/es NOTAM à analyser</param>
        /// <returns>la liste des zones avec leurs horaires d'ouverture de la journée</returns>
        public static List<Zone> ParseZones(string finalresult)
        {
            List<Zone> zones = new List<Zone>();
            // on obtient une liste de lignes, le contenu du/es NOTAM se présentant sous la forme:
            //ZONE R45A BOURGOGNE
            //2007-2259:ACTIVE
            //ZONE R45B AUTUNOIS
            //0800-1000:ACTIVE
            //2007-2259:ACTIVE
            // ...
            string[] lines = finalresult.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // on analyse le contenu en faisant un bloc de lignes pour une zone à la fois
            List<string> currentBlock = new List<string>();
            string currentZoneName = null;

            foreach (string line in lines)
            {
                // si ça commence par "ZONE", on est en début de bloc de zone
                if (line.StartsWith("ZONE "))
                {
                    // si on a créé un bloc complet de zone, on le convertit en `Zone` à l'aide de ParseZoneBlock() et on le sauvegarde avant de passer au suivant
                    if (currentZoneName != null && zoneList.Any(z => currentZoneName.Contains(z)))
                    {
                        zones.Add(ParseZoneBlock(currentZoneName, currentBlock));
                    }

                    // et on peut commmencer la nouvelle zone en récupérant son nom après le "ZONE "
                    currentZoneName = line.Replace("ZONE ", "").Trim();
                    // on vide le contenu de currentBlock
                    currentBlock.Clear();
                }
                // sinon, si on a déjà commencé une nouvelle zone, on ajoute cette ligne qui contient sans doute des horaires au bloc
                else if (currentZoneName != null)
                {
                    currentBlock.Add(line);
                }
            }

            // on convertit en `Zone` à l'aide de ParseZoneBlock() et on sauvegarde la dernière zone
            if (currentZoneName != null && zoneList.Any(z => currentZoneName.Contains(z)))
            {
                Zone decZone = ParseZoneBlock(currentZoneName, currentBlock);
                if (decZone != null) zones.Add(decZone);
            }
            // on retourne la List<Zone> créée
            return zones;
        }

        /// <summary>
        /// Convertit un nom de zone + blocs de strings représentant des ouvertures en `Zone`
        /// </summary>
        /// <param name="zoneName">le nom de la zone</param>
        /// <param name="blockLines">le bloc de string représentant ses ouvertures</param>
        /// <returns></returns>
        private static Zone ParseZoneBlock(string zoneName, List<string> blockLines)
        {
            // on crée une zone
            Zone zone = new Zone(zoneName);
            // on crée un index des périodes d'ouvertures trouvées
            int timeIndex = 0;

            // pour chaque ligne, on vérifie qu'elle contient ":ACTIVE"
            bool zonefound = false;
            foreach (string line in blockLines)
            {
                if (line.Contains(":ACTIVE") && timeIndex < nMaxList)
                {
                    // et on convertit ce qu'il y a avant en fonction des séparations ':' (séparation entre les horaires et "ACTIVE") et
                    // '-' (séparation entre l'heure d'ouverture et l'heure de fin)
                    string[] parts = line.Split(':')[0].Split('-');
                    if (parts.Length == 2 &&
                        // le premier horaire est l'ouverture
                        DateTime.TryParseExact(parts[0], "HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deb) &&
                        // le deuxième horaire est la fermeture
                        DateTime.TryParseExact(parts[1], "HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fin))
                    {
                        // et on ajoute l'intervalle d'ouverture à la liste disponible aujourd'hui
                        deb = DateTime.Today.AddHours(deb.Hour).AddMinutes(deb.Minute);
                        fin = DateTime.Today.AddHours(fin.Hour).AddMinutes(fin.Minute);
                        // on s'assure que la fin n'est pas le jour suivant
                        if (fin <= deb) fin = fin.AddDays(1);
                        zone.SetTime(timeIndex, deb, fin);
                        timeIndex++;
                        zonefound = true;
                    }
                }
            }
            // on retourne la zone créée
            if (zonefound) return zone;
            return null;
        }
        /// <summary>
        /// Va sur la page d'Eurocontrol https://www.public.nm.eurocontrol.int/PUBPORTAL/ pour récupérer l'activité
        /// des zones supérieures 
        /// </summary>
        /// <returns></returns>
        public static readonly (string docName, string ctrName)[] uzoneList = { ("euc25fw","EUC25FW"),("euc25fc","EUC25FC"),("euc25fe","EUC25FE"),
            ("euc25sl", "EUC25SL"), ("lft22a1", "TRA22A1"),("lft22b", "TRA22B"),
            ("lfr124","R124"), ("lfr322","R322"),("lfr323","R323"), ("lfr158a", "R158A"), ("lfr158b","R158B") };
        public static readonly string[] uzonePlanchers = {"FL65", "FL115", "FL115",
        "FL100","FL195", "FL195","FL115", "FL155", "FL145", "5000AGL", "1500AGL"};
        public static readonly string[] uzonePlafonds = {"FL195", "FL195", "FL195",
        "FL230","FL999", "FL285","FL195", "FL195", "FL195", "FL115", "5000AGL"};

        public static List<Zone> GetUpperZones()
        {
            List<Zone> UZones = new List<Zone>();
            IWebDriver driver = null;
            try
            {
                // on opuvre Edge
                var options = new EdgeOptions();
                driver = new EdgeDriver(options);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(OVA_TIMEOUT));
                // on va à la page d'Eurocontrol
                driver.Navigate().GoToUrl("https://www.public.nm.eurocontrol.int/PUBPORTAL/");

                // Etape 1: On cherche le premier élément cliquable correspondant à la date d'aujourd'hui
                // dans la div qui nous intéresse
                string today = DateTime.UtcNow.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                var parentElement = wait.Until(d => d.FindElement(By.Id("EAUP_CONTENTS_ELEMENT_ID")));
                var elements = parentElement.FindElements(By.ClassName("eurocontrol_gwt_ext_linkEnabled"));

                // le premier qu'on trouve, on le clique
                bool mesfound = false;
                foreach (var element in elements)
                {
                    if (element.Text.Length == "01/01/2000 00:00".Length && element.Text.StartsWith(today))
                    {
                        mesfound = true;
                        element.Click();
                        break;
                    }
                }
                if (!mesfound) return null;
                // Etape 2: on attend que la fenêtre avec les zones s'affiche et on la met au premier plan
                wait.Until(d => d.WindowHandles.Count > 1);
                driver.SwitchTo().Window(driver.WindowHandles.Last());

                // on récupère la date de validité de la page
                var labelElement = wait.Until(d => d.FindElement(By.XPath("//td[@class='portal_datalayoutLabel' and text()='Valid WEF']")));
                var dateElement = wait.Until(d => labelElement.FindElement(By.XPath("following-sibling::td[@class='portal_datalayoutValue']"))); string dateText = dateElement.Text;
                DateTime validWef = DateTime.ParseExact(dateText, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                labelElement = wait.Until(d => driver.FindElement(By.XPath("//td[@class='portal_datalayoutLabel' and text()='Valid TIL']")));
                dateElement = wait.Until(d => labelElement.FindElement(By.XPath("following-sibling::td[@class='portal_datalayoutValue']")));
                dateText = dateElement.Text;
                DateTime validTil = DateTime.ParseExact(dateText, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                // Etape 3: récupérer les allocations d'espace prévues
                var rsaElements = driver.FindElements(By.XPath("//td[starts-with(@id, 'RESULTS_AREA.RSA_AREA.RSA_ALLOCATION_TABLE-RSA_COLUMN-')]"));

                foreach (var rsaElement in rsaElements)
                {
                    string rsaName = rsaElement.Text.Trim();
                    if (string.IsNullOrEmpty(rsaName)) continue;

                    // est-ce que la RSA fait partie des zones qu'on recherche, sinon on continue
                    var match = uzoneList.FirstOrDefault(uz => rsaName.ToLower() == uz.docName);
                    if (match == default) continue;
                    // Si on a trouvé quelque chose, on récupère le nom à afficher
                    string ctrName = match.ctrName;

                    int x = int.Parse(rsaElement.GetAttribute("id").Split('-').Last()); // récupération de l'index

                    // On cherche l'heure de début "WEF" et de fin "UNT"
                    string wefXPath = $"//td[@id='RESULTS_AREA.RSA_AREA.RSA_ALLOCATION_TABLE-WEF_COLUMN-{x}']";
                    string untXPath = $"//td[@id='RESULTS_AREA.RSA_AREA.RSA_ALLOCATION_TABLE-UNT_COLUMN-{x}']";

                    var wefElement = driver.FindElements(By.XPath(wefXPath)).FirstOrDefault();
                    var untElement = driver.FindElements(By.XPath(untXPath)).FirstOrDefault();

                    // si l'un manque on continue
                    if (wefElement == null || untElement == null) continue;

                    // sinon on convertit en DateTime
                    string wefTime = wefElement.Text.Trim();
                    string untTime = untElement.Text.Trim();

                    DateTime wefDateTime = ConvertToDateTime(validWef, validTil, wefTime);
                    DateTime untDateTime = ConvertToDateTime(validWef, validTil, untTime);

                    // Et on finit en ajoutant les horaires si la zone existe déjà dans la liste ou
                    // en créant une zone à partir de ces données
                    Zone foundZone = UZones.FirstOrDefault(z => z.Name == ctrName);
                    if (foundZone != default)
                    {
                        int i = Array.FindIndex(foundZone.Deb, d => d == new DateTime(1753, 1, 1));
                        if (i == -1) continue; // On ignore l'activité si il y a déjà trop de créneaux d'ouverture prévus (nMaxList)
                        foundZone.Deb[i] = wefDateTime;
                        foundZone.Fin[i] = untDateTime;
                        int index = UZones.FindIndex(z => z.Name == ctrName);
                        if (index != -1) UZones[index] = foundZone;
                    }
                    else
                    {
                        foundZone = new Zone(ctrName);
                        foundZone.Deb[0] = wefDateTime;
                        foundZone.Fin[0] = untDateTime;
                        UZones.Add(foundZone);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
            finally
            {
                if (driver != null) driver.Quit();
            }

            return UZones.OrderBy(z => z.Name).ToList();
        }
        private static DateTime ConvertToDateTime(DateTime dtStart, DateTime dtEnd, string timeStr)
        {
            TimeSpan time;
            if (!TimeSpan.TryParseExact(timeStr, @"hh\:mm", CultureInfo.InvariantCulture, out time)) return new DateTime(1753, 1, 1);

            // Assume the input time is on dtStart's day
            DateTime result = dtStart.Date + time;

            // If result is before dtStart, it must belong to the next day
            if (result < dtStart)
                result = result.AddDays(1);

            return result;
        }
        public static (string, string) GetZoneHeights(string zn)
        {
            for (int i = 0; i < zoneList.Length; i++)
            {
                if (zoneList[i] == zn) return (zonePlanchers[i], zonePlafonds[i]);
            }
            for (int i = 0; i < uzoneList.Length; i++)
            {
                if (uzoneList[i].ctrName == zn) return (uzonePlanchers[i], uzonePlafonds[i]);
            }
            return ("", "");
        }
    }
}
