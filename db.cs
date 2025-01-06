using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalasZoomNotificationFormadores.objects;
using System.Diagnostics;
using System.Reflection;

namespace SalasZoomNotificationFormadores
{
    class db
    {
        public static List<objSessao> htSessoes = new List<objSessao>();
        public static List<objSala> Salas = new List<objSala>();
        public static List<objAlojamento> Alojamentos = new List<objAlojamento>();
        public static List<objColaborador> listaFormandos = new List<objColaborador>();

        public static List<objSessao> htSessoesSelected = new List<objSessao>();

        public static List<objMoodleContrato> moodleContontratos = new List<objMoodleContrato>();

        public static List<objMoodleUser> moodleUsers = new List<objMoodleUser>();

        public static List<objFormando> htobjFormandosMain = new List<objFormando>();

        public static List<objSinc> htobjSinc = new List<objSinc>();
        public static FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
    }
}
