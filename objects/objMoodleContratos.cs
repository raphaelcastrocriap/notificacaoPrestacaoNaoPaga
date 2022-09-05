using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalasZoomNotificationFormadores.objects
{
    public class objMoodleContratos
    {
        public string course_id { get; set; }
        public string data_aceitacao { get; set; }
        public string download_key { get; set; }
        public int id { get; set; }
        public int limpo { get; set; }
        public string user_idnumber { get; set; }
        public string versao { get; set; }
    }

    public class objMoodleUser
    {
        public string id { get; set; }
        public string idnumber { get; set; }
        public string username { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
    }
}