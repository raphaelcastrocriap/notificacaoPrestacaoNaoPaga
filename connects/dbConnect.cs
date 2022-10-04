using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalasZoomNotificationFormadores.connects
{
    class dbConnect
    {
        public static connectHT ht = new connectHT(Security.settings.ht_HName, Security.settings.ht_DBName, Security.settings.ht_UName, Security.settings.ht_Pass);

        public static connectHT2 ht2 = new connectHT2(Security.settings.ht_HName, Security.settings.ht_DBName, Security.settings.ht_UName, Security.settings.ht_Pass);

        public static connectHT secretariaVirtual = new connectHT(Security.settings.ht_HName, "secretariaVirtual", Security.settings.ht_UName, Security.settings.ht_Pass);

        public static Connect_Moodle_Server MoodleConnect = new Connect_Moodle_Server(Security.settings.m_HName, Security.settings.m_UName, Security.settings.m_Pass, Security.settings.m_DBName);
        //public static string nomeTableCorDados = "cor_Dados_test";
        public static String nomeTableCorDados = "cor_Dados";

        
        public static string nomeTableMeetings = "cor_meetings";
        public static string nomeTableRegistrants = "cor_meeting_registrant";
    }
}
