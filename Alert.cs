using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeDNA
{
    /// <summary>
    /// Classe pour sauvegarder les alertes avant de les afficher
    /// </summary>
    public class Alert
    {
        /// <summary>
        /// Nom de la zone concernée par les alertes
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Est-ce que c'est pour signaler une ouverture (true) ou une fermeture (false) de zone?
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Constructeur qui remplit les valeurs
        /// </summary>
        /// <param name="name"></param>
        /// <param name="active"></param>
        public Alert(string name, bool active)
        {
            Name = name;
            Active = active;
        }
    }
}
