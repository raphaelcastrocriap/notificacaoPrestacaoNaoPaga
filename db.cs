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
        public static List<objCursos> htCursos = new List<objCursos>();
        public static List<objSalas> Salas = new List<objSalas>();
        public static List<objAlojamentos> Alojamentos = new List<objAlojamentos>();
        public static List<objColaboradores> listaColaboradores = new List<objColaboradores>();

        public static List<objCursos> htCursosselected = new List<objCursos>();

        public static List<objMoodleContratos> moodleContontratos = new List<objMoodleContratos>();

        public static List<objMoodleUser> moodleUser = new List<objMoodleUser>();

        public static List<objFormandos> htobjFormandosMain = new List<objFormandos>();

        public static List<objSinc> htobjSinc = new List<objSinc>();
        public static FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
    }
}
