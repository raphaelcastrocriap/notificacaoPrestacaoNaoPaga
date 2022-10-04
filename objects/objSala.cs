using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalasZoomNotificationFormadores.objects
{
    class objSala
    {
        public int id { get; set; }
        public string descricao { get; set; }
        public string nome { get; set; }
        public string local { get; set; }
        public int capacidade { get; set; }
        public int capacidadeU { get; set; }
        public int capacidadeEscola { get; set; }
        public int capacidadeAuditorio { get; set; }
        public string link { get; set; }
        public bool zoom { get; set; }
        public string loginAdmin { get; set; }
        public string zoomFirstName { get; set; }
        public string zoomLastName { get; set; }
    }
}
