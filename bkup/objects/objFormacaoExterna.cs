using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalasZoomNotificationFormadores.objects
{
    class objFormacaoExterna
    {
        public string CodCurso { get; set; }
        public string RefAccao { get; set; }
        public DateTime Hora_Inicio { get; set; }
        public DateTime Hora_Fim { get; set; }
        public string Estado { get; set; }
        public string Formacao { get; set; }
        public decimal nHoras { get; set; }
        public int nFormandos { get; set; }
        public string Formador { get; set; }
        public string Local { get; set; }
    }
}
