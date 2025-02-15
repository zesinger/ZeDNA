using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZeDNA
{
    /// <summary>
    /// Classe de récupération des NOTAM qui donnent la description de l'ouverture des zones
    /// Utilise Selenium qui lance Microsoft Edge et le contrôle pour naviguer et remplir le formulaire
    /// </summary>
    public static class GetData
    {
        /// <summary>
        /// Taille de l'image de fond de la carte (devrait être de la même taille que le panelMap du MainForm)
        /// </summary>
        public static readonly (int w, int h) fondDesc = (469, 450);
        /// <summary>
        /// Description des zones qu'on recherche dans le NOTAM: nom, position de l'image au dessus du fond (x,y) et dimension (width, height)
        /// </summary>
        public static readonly (string name, int x, int y, int w, int h)[] zoneList =
            { ("R45A",99,174,88,81 ), ("R45B",33,274,124,99), ("R45C",189,311,91,105),
            ("R45NS",202,176,49,85), ("R45S1",237,189,95,41), ("R45S2",43,189,195,101),
            ("R45S3",32,274,103,101),("R45S4",78,362,71,59), ("R45S5",146,359,63,62),
            ("R45S6.2",195,374,80,67),("R45S6.1",234,311,108,89), ("R45S7",211,249,113,80),
            ("R152",229,104,145,91), ("R69",59,19,162,171) };
        /// <summary>
        /// Date du NOTAM affiché tel qu'affiché dans le NOTAM (JJ MM YYYY)
        /// </summary>
        public static string dateData = "";
        public static List<Zone> GetRtbaData()
        {
            // Démarrage de Microsoft Edge (browser obligatoirement présent sur tous les PC Windows)
            string notamresult;
            var options = new EdgeOptions();
            IWebDriver driver = new EdgeDriver(options);
            try
            {
                // Etape 1: Affichage de la page précédente (on ne peut pas afficher directement le formulaire)
                driver.Navigate().GoToUrl("https://sofia-briefing.aviation-civile.gouv.fr/sofia/pages/notamareamenu.html");

                // Etape 2: fixe un timeout puis attend que le bouton "FIR" soit clickable
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
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
                // la FIR qui nous intéresse est "LFEE"
                // on repère le champ
                IWebElement firInput = driver.FindElement(By.Id("id_fir1"));
                // on le vide
                firInput.Clear();
                // et on rentre "LFEE"
                firInput.SendKeys("LFEE");

                // On déclenche un event pour être sûr que le changement a été pris en compte
                js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", firInput);

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
                driver.Quit();
            }
            
            // Si aucun résultat, on retourne null
            if (notamresult == "") return null;
            // exemple à retirer (utile pour les tests):
            //notamresult = "LFFA-Z0099/23\r\nDU: 01 06 2023 07:30 AU: 01 06 2023 23:59\r\nA)LFEE LFFF LFMM\r\nQ) LFXX / QRRCA / IV / BO / W / 000/085 / 4801N00521E099\r\nE) ZONES AIRFORCE RTBA ACT\r\nZONE R45A BOURGOGNE\r\n2007-2259:ACTIVE\r\nZONE R45B AUTUNOIS\r\n0800-1000:ACTIVE\r\n2007-2259:ACTIVE\r\nZONE R45C ARBOIS\r\n0800-1000:ACTIVE\r\n2007-2259:ACTIVE\r\nZONE R45S1 FRANCHE COMTE\r\n1500-2007:ACTIVE\r\nZONE R45S2 LANGRES\r\n1130-2359:ACTIVE\r\nZONE R45S3 YONNE\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S4 MACONNAIS OUEST\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S5 MACONNAIS CENTRE\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S6.1 MACONNAIS NORD EST\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S6.2 MACONNAIS SUD EST\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R45S7 JURA\r\n0730-1000:ACTIVE\r\n1130-1330:ACTIVE\r\n1500-2359:ACTIVE\r\nZONE R69 CHAMPAGNE\r\n1130-1500:ACTIVE\r\n2007-2359:ACTIVE\r\nZONE R45NS\r\n1130-1500:ACTIVE\r\n2007-2359:ACTIVE\r\nF) SFC\r\nG) FL085\r\n";
            
            // On va maintenant chercher le/s NOTAMs qui nous intéresse/nt, il/s commence/nt par "LFAA-Z"
            string finalresult = "";
            int offsNotam = notamresult.IndexOf("lffa-z", StringComparison.OrdinalIgnoreCase);
            // un "while" car si le message est trop long, il est réparti sur plusieurs notams
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
                        return null;
                }
                else return null;
                // 2 lignes suivantes à commenter pour les tests:
                if (notamDate.Date != DateTime.UtcNow.Date) notamToIgnore = true;
                else
                    dateData = dateStr;
                // une fois qu'on a trouvé le NOTAM, on va à la section "E" de ce NOTAM, celle qui nous intéresse à partir de la seconde ligne
                offsNotam += notamresult.Substring(offsNotam).IndexOf("E)", StringComparison.OrdinalIgnoreCase);
                if (offsNotam == -1) return null;
                // puis on passe la ligne
                offsNotam += notamresult.Substring(offsNotam).IndexOfAny(new char[] { '\r', '\n' });
                if (offsNotam == -1) return null;
                while (offsNotam < notamresult.Length && (notamresult.Substring(offsNotam, 1) == "\r" || notamresult.Substring(offsNotam, 1) == "\n")) offsNotam++;
                if (offsNotam == notamresult.Length) return null;
                // et on repère la fin de la partie qui nous intéresse, avant la section "F"
                int lenNotam = notamresult.Substring(offsNotam).IndexOf("F)", StringComparison.OrdinalIgnoreCase);
                if (lenNotam == -1) return null;
                // si le NOTAM n'est pas ignoré à cause d'une date qui ne correspond pas, on l'ajoute au texte qui va être analysé
                if (!notamToIgnore) finalresult += notamresult.Substring(offsNotam, lenNotam);
                // et on relance la recherche pour les NOTAM suivants contenant ce qui nous intéresse, s'il y en a plusieurs
                offsNotam = notamresult.IndexOf("lffa-z", offsNotam + lenNotam, StringComparison.OrdinalIgnoreCase);
            }
            // si aucun NOTAM correspondant n'a été trouvé, on retour null
            if (finalresult == "") return null;
            // Parsing du résultat zone par zone
            List<Zone> zones = ParseZones(finalresult);
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
                    if (currentZoneName != null && zoneList.Any(z => currentZoneName.Contains(z.name)))
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
            if (currentZoneName != null && zoneList.Any(z => currentZoneName.Contains(z.name)))
            {
                zones.Add(ParseZoneBlock(currentZoneName, currentBlock));
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
            foreach (string line in blockLines)
            {
                if (line.Contains(":ACTIVE") && timeIndex < 4)
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
                        // et on ajoute l'intervalle d'ouverture à la liste disponible
                        zone.SetTime(timeIndex, deb, fin);
                        timeIndex++;
                    }
                }
            }
            // on retourne la zone créée
            return zone;
        }
    }
}
