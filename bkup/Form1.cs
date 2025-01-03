﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SalasZoomNotificationFormadores.connects;
using System.Reflection;
using System.Net.Mail;
using System.Net;
using System.Data.SqlClient;
using SalasZoomNotificationFormadores.objects;
using System.Text.RegularExpressions;
using SMSbyMail.SMSbyMailWS;
using System.Diagnostics;

namespace SalasZoomNotificationFormadores
{
    public partial class Form1 : Form
    {
        public bool error = false;
        public RichTextBox errorTextBox = new RichTextBox(), newCursoTextBox = new RichTextBox();
        public static List<objFormadores> Formadores = new List<objFormadores>();
        authentication credenciais = new authentication();
        List<Obj_logSend> logsSenders = new List<Obj_logSend>();
        SMSByMailSEIService smsByMailSEIService = new SMSByMailSEIService();

        public Form1()
        {
            InitializeComponent();  
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Security.remote();
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text += " V." + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString();

            string[] passedInArgs = Environment.GetCommandLineArgs();

            if (passedInArgs.Contains("-a") || passedInArgs.Contains("-A"))
            {
                Cursor.Current = Cursors.WaitCursor;
                ExecuteSync();
                Cursor.Current = Cursors.Default;
                Application.Exit();
            }
        }
        private void StartSmsService()
        {
            smsByMailSEIService.UseDefaultCredentials = false;
            smsByMailSEIService.Url = "https://smsws.vodafone.pt:443/SmsByMailWs/serviceSMSByMail.web?wsdl";
            //credenciais.email = "filipepereira@criap.com";
            //credenciais.msisdn = "911145239";
            credenciais.password = "92458734";
            credenciais.email = "paulonascimento@criap.com";
            credenciais.msisdn = "914339725";

            string msg = smsByMailSEIService.testLogin(credenciais).resultMessage.ToString();
        }
        private void ExecuteSync()
        {
            try
            {
                //DateTime horasyncman = new DateTime(2022, 1, 11, 6, 0, 0);
                DateTime horasyncman = DateTime.Now;

                error = false;
                richTextBox1.Clear();
                errorTextBox.Clear();
                newCursoTextBox.Clear();
                StartSmsService();

                Get_Colaboradores();
                Get_Salas();
                GetHT_data(horasyncman, horasyncman);
                GetFormacaoExterna(horasyncman, horasyncman);
                GetSecretariaData();

                if (horasyncman.DayOfWeek == DayOfWeek.Saturday || horasyncman.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (horasyncman.Hour < 12)
                    {
                        Envia_emails_formadores_sabado();
                    }
                }
                else
                {
                    Envia_emails_formadores();
                }
                SendEmail(richTextBox1.Text, "Notification Salas Zoom // Formadores - Emails enviados", true);
            }
            catch (Exception e)
            {
                error = true;
                SendEmail(richTextBox1.Text + Environment.NewLine + e.Message, "Notification Salas Zoom // Formadores - ERROR", true);
            }
        }
        private void GetHT_data(DateTime inicio, DateTime fim)
        {
            Formadores.Clear();
            List<DataRow> dataList2 = criapQuerys.Query.getFormadores(dbConnect.ht2.Conn);
            if (dataList2.Count != 0)
            {
                for (int i = 0; i < dataList2.Count; i++)
                {
                    var obj = new objFormadores()
                    {
                        formadorID = dataList2[i][0].ToString(),
                        NomeFormador = dataList2[i][1].ToString()
                    };
                    Formadores.Add(obj);
                }
            }

            string subQuery = criapQuerys.Query.gestaoSalas.getHTData() + "WHERE(TBForAccoes.Codigo_Estado = 1 OR TBForAccoes.Codigo_Estado = 4) AND (TBForSessoes.Hora_Inicio >= '" + inicio.ToString("yyyy-MM-dd") + "') AND (TBForSessoes.Hora_Fim <= '" + fim.ToString("yyyy-MM-dd") + " 23:59') ORDER BY TBForSessoes.Hora_Inicio ";

            dbConnect.ht.ConnInit();
            SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.ht2.Conn);
            DataTable subData = new DataTable();
            adapter.Fill(subData);
            List<DataRow> dataList = subData.AsEnumerable().ToList();
            dbConnect.ht.ConnEnd();

            if (dataList.Count != 0)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    string formador2txt = "";
                    if (dataList[i][17].ToString() != "")
                    {
                        formador2txt = (from d in Formadores where d.formadorID == dataList[i][17].ToString() select d.NomeFormador).Distinct().First().ToString();
                    }

                    string nometecnicoass = dataList[i][19].ToString();
                    int index = db.listaColaboradores.FindIndex(x => x.codigo_Colaborador == dataList[i][19].ToString());
                    if (index > 0)
                    {
                        nometecnicoass = db.listaColaboradores.Find(x => x.codigo_Colaborador == dataList[i][19].ToString()).sigla;
                    }

                    string CODCOLABORADOR = "";
                    string existQuery = "SELECT CODIGO_COLABORADOR FROM DEST_Destacamentos where rowid_Sessao=" + int.Parse(dataList[i][20].ToString());
                    dbConnect.secretariaVirtual.ConnInit();
                    SqlCommand exist2 = new SqlCommand(existQuery, dbConnect.secretariaVirtual.Conn);
                    SqlDataReader existReader2 = exist2.ExecuteReader();
                    while (existReader2.Read())
                    {
                        CODCOLABORADOR = existReader2[0].ToString();
                    }
                    dbConnect.secretariaVirtual.ConnEnd();

                    string tecnicosalavirtual = "";
                    int index2 = db.listaColaboradores.FindIndex(x => x.codigo_Colaborador == CODCOLABORADOR);
                    if (index2 > 0)
                    {
                        tecnicosalavirtual = db.listaColaboradores.Find(x => x.codigo_Colaborador == CODCOLABORADOR).sigla;
                    }
                    var obj = new objCursos()
                    {
                        Codigo_Projeto = dataList[i][0].ToString(),
                        Codigo_Curso = dataList[i][1].ToString(),
                        Ref_Accao = dataList[i][2].ToString(),
                        Tecnica = dataList[i][3].ToString(),
                        Estado = dataList[i][4].ToString(),
                        Hora_Inicio = DateTime.Parse(dataList[i][5].ToString()),
                        Hora_Fim = DateTime.Parse(dataList[i][6].ToString()),
                        Modulo = dataList[i][7].ToString(),
                        TotalHoras = decimal.Parse(dataList[i][8].ToString(), System.Globalization.NumberStyles.Float),
                        Formador = dataList[i][9].ToString(),
                        TotalFormandos = int.Parse(dataList[i][10].ToString()),
                        Local = dataList[i][11].ToString(),
                        OrigemFormador = dataList[i][12].ToString(),
                        Inicio_Curso = DateTime.Parse(dataList[i][13].ToString()),
                        Fim_Curso = DateTime.Parse(dataList[i][14].ToString()),
                        InicioFim = null,
                        versao_rowid = dataList[i][15].ToString(),
                        NomeCurso = dataList[i][16].ToString(),
                        CodFormador2 = dataList[i][17].ToString(),
                        CoFormador = formador2txt,
                        Rowid_Accao = int.Parse(dataList[i][18].ToString()),
                        TM = nometecnicoass,
                        TSV = tecnicosalavirtual,
                        FormadorDistribuirLinks = dataList[i][9].ToString(),
                        CodCordenador = dataList[i][21].ToString(),
                        CodFormador = dataList[i][22].ToString()
                    };

                    if (obj.Inicio_Curso.Date == obj.Hora_Inicio.Date) obj.InicioFim = "I";
                    if (obj.Fim_Curso.Date == obj.Hora_Fim.Date) obj.InicioFim = "T";

                    db.htCursos.Add(obj);
                }
            }
        }
        private void GetSecretariaData()
        {
            if (db.htCursos.Count > 0)
            {
                string subQuery = "SELECT Versao_rowid, Hora_Inicio, Hora_Fim, TransporteID, TransportePreco, AlojamentoID, AlojamentoPreco, SalaID, SalaPreco, Notas FROM cor_Dados WHERE (";
                for (int i = 0; i < db.htCursos.Count; i++)
                {
                    if (i != db.htCursos.Count - 1)
                        subQuery += "Versao_rowid = '" + db.htCursos[i].versao_rowid.ToString() + "' or ";
                    else
                    {
                        subQuery += "Versao_rowid = '" + db.htCursos[i].versao_rowid.ToString() + "')";
                    }
                }

                dbConnect.secretariaVirtual.ConnInit();
                SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.secretariaVirtual.Conn);
                DataTable subData = new DataTable();
                adapter.Fill(subData);
                List<DataRow> dataList = subData.AsEnumerable().ToList();
                dbConnect.secretariaVirtual.ConnEnd();

                if (dataList.Count != 0)
                {
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        int index = db.htCursos.FindIndex(x => x.versao_rowid == dataList[i][0].ToString());

                        if (dataList[i][3] != DBNull.Value) db.htCursos[index].cod_Transporte = int.Parse(dataList[i][3].ToString());
                        db.htCursos[index].preco_Transporte = double.Parse(dataList[i][4].ToString());
                        if (dataList[i][5] != DBNull.Value) db.htCursos[index].cod_Alojamento = int.Parse(dataList[i][5].ToString());
                        db.htCursos[index].preco_Alojamento = double.Parse(dataList[i][6].ToString());
                        if (dataList[i][7] != DBNull.Value) db.htCursos[index].cod_Sala = int.Parse(dataList[i][7].ToString());
                        db.htCursos[index].preco_Sala = double.Parse(dataList[i][8].ToString());
                        db.htCursos[index].notas = dataList[i][9].ToString();
                    }
                    subData.Dispose();
                }
            }
        }
        private void GetFormacaoExterna(DateTime inicio, DateTime fim)
        {
            string subQuery = "SELECT cor_FormacaoExterna.Id, cor_FormacaoExterna.NomeFormacao, cor_FormacaoExterna.Estado, cor_FormacaoExterna.NHoras, cor_FormacaoExterna.NFormandos, cor_FormacaoExterna.Formador, cor_FormacaoExterna.Local, cor_Dados.Codigo_Curso, cor_Dados.Ref_Accao, cor_Dados.Hora_Inicio, cor_Dados.Hora_Fim, cor_Dados.Versao_rowid FROM cor_Dados INNER JOIN cor_FormacaoExterna ON cor_Dados.FormacaoExterna = cor_FormacaoExterna.Id where (cor_Dados.Hora_Inicio >= '" + inicio.ToString("yyyy-MM-dd") + "') AND (cor_Dados.Hora_Fim <= '" + fim.ToString("yyyy-MM-dd") + " 23:59') ";

            dbConnect.secretariaVirtual.ConnInit();
            SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.secretariaVirtual.Conn);
            DataTable subData = new DataTable();
            adapter.Fill(subData);
            List<DataRow> dataList = subData.AsEnumerable().ToList();
            dbConnect.secretariaVirtual.ConnEnd();

            if (dataList.Count != 0)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    var obj = new objCursos
                    {
                        FormacaoExternaID = int.Parse(dataList[i][0].ToString()),
                        Modulo = dataList[i][1].ToString(),
                        Estado = dataList[i][2].ToString(),
                        TotalHoras = decimal.Parse(dataList[i][3].ToString()),
                        TotalFormandos = int.Parse(dataList[i][4].ToString()),
                        Formador = dataList[i][5].ToString(),
                        Local = dataList[i][6].ToString(),
                        Codigo_Curso = dataList[i][7].ToString(),
                        Ref_Accao = dataList[i][8].ToString(),
                        Hora_Inicio = DateTime.Parse(dataList[i][9].ToString()),
                        Hora_Fim = DateTime.Parse(dataList[i][10].ToString()),
                        versao_rowid = dataList[i][11].ToString()
                    };
                    db.htCursos.Add(obj);
                }
                subData.Dispose();
            }
        }
        public static void Get_Colaboradores()
        {
            db.listaColaboradores.Clear();
            string subQuery = "SELECT DEST_Colaboradores.codigo_Colaborador, nome, email, sigla FROM DEST_Colaboradores";
            dbConnect.secretariaVirtual.ConnInit();
            SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.secretariaVirtual.Conn);
            DataTable subData = new DataTable();
            adapter.Fill(subData);
            List<DataRow> dataList = subData.AsEnumerable().ToList();
            dbConnect.secretariaVirtual.ConnEnd();

            if (dataList.Count != 0)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    var obj = new objColaboradores()
                    {
                        codigo_Colaborador = dataList[i][0].ToString(),
                        nome = dataList[i][1].ToString(),
                        email = dataList[i][2].ToString(),
                        nomeemail = dataList[i][1].ToString() + " - " + dataList[i][2].ToString(),
                        sigla = dataList[i][3].ToString()
                    };
                    db.listaColaboradores.Add(obj);
                }
            }
        }
        private void Get_Salas()
        {
            db.Salas.Clear();
            string subQuery = "SELECT id, (CONCAT (numero, ' - ', descricao)) as sala, descricao, local, capacidade, ISNULL(capacidadeU, 0), ISNULL(capacidadeEscola, 0), ISNULL(capacidadeAuditorio, 0), link, Zoom FROM cor_Salas order by len(numero), numero";
            dbConnect.secretariaVirtual.ConnInit();
            SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.secretariaVirtual.Conn);
            DataTable subData = new DataTable();
            adapter.Fill(subData);
            List<DataRow> dataList = subData.AsEnumerable().ToList();
            dbConnect.secretariaVirtual.ConnEnd();

            if (dataList.Count != 0)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    if (dataList[i][1].ToString() != "1 - Sala Virtual")
                    {
                        if (dataList[i][9].ToString() == "True")
                        {
                            var obj = new objSalas()
                            {
                                id = dataList[i][0].ToString(),
                                descricao = dataList[i][2].ToString(),
                                nome = dataList[i][2].ToString(),
                                local = dataList[i][3].ToString(),
                                capacidade = int.Parse(dataList[i][4].ToString()),
                                capacidadeU = int.Parse(dataList[i][5].ToString()),
                                capacidadeEscola = int.Parse(dataList[i][6].ToString()),
                                capacidadeAuditorio = int.Parse(dataList[i][7].ToString()),
                                link = dataList[i][8].ToString()
                            };
                            db.Salas.Add(obj);
                        }
                        else
                        {
                            var obj = new objSalas()
                            {
                                id = dataList[i][0].ToString(),
                                descricao = dataList[i][1].ToString(),
                                nome = dataList[i][2].ToString(),
                                local = dataList[i][3].ToString(),
                                capacidade = int.Parse(dataList[i][4].ToString()),
                                capacidadeU = int.Parse(dataList[i][5].ToString()),
                                capacidadeEscola = int.Parse(dataList[i][6].ToString()),
                                capacidadeAuditorio = int.Parse(dataList[i][7].ToString()),
                                link = dataList[i][8].ToString()
                            };
                            db.Salas.Add(obj);
                        }
                    }
                }
            }
        }
        //public void getMoodleUsers()
        //{
        //    dbConnect.MoodleConnect.ConnInit();

        //    MySqlDataAdapter adapter = new MySqlDataAdapter();
        //    List<DataRow> dataList;

        //    adapter.SelectCommand = new MySqlCommand("SELECT id,idnumber,username,firstname,lastname,email from user", dbConnect.MoodleConnect.Conn);
        //    DataTable subData = new DataTable();
        //    adapter.Fill(subData);
        //    dataList = subData.AsEnumerable().ToList();
        //    adapter.Dispose();

        //    db.moodleUser.Clear();
        //    if (dataList.Count != 0)
        //    {
        //        for (int i = 0; i < dataList.Count; i++)
        //        {
        //            var obj = new objMoodleUser()
        //            {
        //                id = dataList[i][0].ToString(),
        //                idnumber = dataList[i][1].ToString(),
        //                username = dataList[i][2].ToString(),
        //                firstname = dataList[i][3].ToString(),
        //                lastname = dataList[i][4].ToString(),
        //                email = dataList[i][5].ToString()
        //            };
        //            db.moodleUser.Add(obj);
        //        }
        //        subData.Dispose();
        //    }
        //}

        //public void getMoodleData(string codCursoMoodle)
        //{
        //    dbConnect.MoodleConnect.ConnInit();

        //    MySqlDataAdapter adapter = new MySqlDataAdapter();

        //    string subQuery = "SELECT course_id, user_idnumber, download_key, data_aceitacao, versao, limpo, id FROM contratos where course_id = '" + codCursoMoodle + "'";

        //    adapter.SelectCommand = new MySqlCommand(subQuery, dbConnect.MoodleConnect.Conn);
        //    DataTable subData = new DataTable();
        //    adapter.Fill(subData);
        //    List<DataRow> dataList = subData.AsEnumerable().ToList();
        //    adapter.Dispose();

        //    db.moodleContontratos.Clear();
        //    if (dataList.Count != 0)
        //    {
        //        for (int i = 0; i < dataList.Count; i++)
        //        {
        //            var obj = new objMoodleContratos()
        //            {
        //                course_id = dataList[i][0].ToString(),
        //                user_idnumber = dataList[i][1].ToString(),
        //                download_key = dataList[i][2].ToString(),
        //                data_aceitacao = dataList[i][3].ToString(),
        //                versao = dataList[i][4].ToString(),
        //                limpo = int.Parse(dataList[i][5].ToString()),
        //                id = int.Parse(dataList[i][6].ToString())
        //            };
        //            db.moodleContontratos.Add(obj);
        //        }
        //        subData.Dispose();
        //    }

        //    dbConnect.MoodleConnect.ConnEnd();
        //}

        //public void logWrite(string text)
        //{
        //    richTextBox1.AppendText(DateTime.Now.ToString("dd-MM-yyyy H:mm") + text);
        //    richTextBox1.ScrollToCaret();
        //}

        //public void logERRORWrite(string text)
        //{
        //    errorTextBox.AppendText(DateTime.Now.ToString() + text);
        //    errorTextBox.ScrollToCaret();
        //}
        private void SendEmail(string body, string assunto = "", bool error = false)
        {
            NetworkCredential basicCredential = new NetworkCredential(Properties.Settings.Default.emailenvio, Properties.Settings.Default.passwordemail);
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.Host = "mail.criap.com";
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = basicCredential;

            MailMessage mm = new MailMessage();
            mm.From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> ");

            mm.To.Add("geral@isoft.pt");
            mm.To.Add("ritagoncalves@criap.com");
            mm.To.Add("geral@criap.com");
            mm.To.Add("luisgraca@criap.com");
            //mm.To.Add("joaoferreira@criap.com");

            mm.Subject = assunto + " // " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToShortTimeString();
            mm.IsBodyHtml = false;
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            mm.Body = body;
            client.Send(mm);
        }
        private void Button2_Click_1(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ExecuteSync();
            Cursor.Current = Cursors.Default;
        }
        private void Envia_emails_formadores()
        {
            richTextBox1.Text = richTextBox1.Text + Environment.NewLine + "FORMADORES:" + Environment.NewLine;
            string subQuery = criapQuerys.Query.formadoresNotification.getModulosFormadoresALL();
            Formadores.Clear();
            dbConnect.ht.ConnInit();
            SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.ht2.Conn);
            DataTable subData = new DataTable();
            adapter.Fill(subData);
            List<DataRow> dataList = subData.AsEnumerable().ToList();
            dbConnect.ht.ConnEnd();

            if (dataList.Count != 0)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    var obj = new objFormadores()
                    {
                        formadorID = dataList[i][0].ToString(),
                        NomeFormador = dataList[i][1].ToString(),
                        Email = dataList[i][2].ToString(),
                        Telefone = dataList[i][3].ToString(),
                        Sexo = dataList[i][4].ToString()
                    };
                    Formadores.Add(obj);
                }
            }

            NetworkCredential basicCredential = new NetworkCredential(Properties.Settings.Default.emailenvio, Properties.Settings.Default.passwordemail);
            //NetworkCredential basicCredential = new NetworkCredential("geral@isoft.com.pt", "Isoft@28092010");
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.Host = "mail.criap.com";
            //client.Host = "smtp.isoft.com.pt";
            client.Timeout = 100000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = basicCredential;

            //ENVIAR PARA TODOS OS FORMADORES
            var consolidatedChildren2 =
            from c in db.htCursos
            group c by new
            {
                c.cod_Sala,
                c.Formador,
                c.Hora_Inicio,
                c.Hora_Fim,
                c.Modulo,
                c.Ref_Accao,
                c.CodFormador
            } into gcs
            select new Result()
            {
                Formador = gcs.Key.Formador,
                HoraInicio = gcs.Key.Hora_Inicio,
                HoraFim = gcs.Key.Hora_Fim,
                RefAccao = gcs.Key.Ref_Accao,
                Modulo = gcs.Key.Modulo,
                CodSala = gcs.Key.cod_Sala,
                FormadorID = gcs.Key.CodFormador
            };

            foreach (var hora in consolidatedChildren2)
            {
                DateTime horasyncman = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                //DateTime horasyncman = new DateTime(2022, 1, 11, 6, 0, 0);
                bool CCP = hora.RefAccao.Contains("CFPIF");

                DateTime horasync = horasyncman.AddHours(9);

                if (hora.HoraInicio >= horasyncman && hora.HoraInicio <= horasync)
                {
                    int existe = (from d in Formadores where d.formadorID == hora.FormadorID select d.Email).Distinct().Count();

                    if (existe > 0)
                    {
                        if(CCP){continue;}
                        string email1 = (from d in Formadores where d.formadorID == hora.FormadorID select d.Email).Distinct().First().ToString();

                        string sexo = (from d in Formadores where d.formadorID == hora.FormadorID select d.Sexo).Distinct().First().ToString();

                        string editcurso = (from a in db.htCursos where (a.Ref_Accao == hora.RefAccao) select a.NomeCurso).First();

                        string codcoordenador = (from a in db.htCursos where (a.Ref_Accao == hora.RefAccao) select a.CodCordenador).First();

                        string telefone = (from d in Formadores where d.formadorID == hora.FormadorID select d.Telefone).Distinct().First().ToString();

                        string emailtxtcoordenador = "geral@criap.com";
                        if (codcoordenador != "")
                        {
                            emailtxtcoordenador = (from a in db.listaColaboradores where (a.codigo_Colaborador == codcoordenador) select a.email).First();
                        }

                        if (email1 != "")
                        {
                            if (ValidarEmail(email1.Trim()))
                            {
                                MailMessage mm = new MailMessage();
                                mm.From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> ");
                                //mm.From = new MailAddress("ISOFT <" + "geral@isoft.com.pt" + "> ");

                                mm.To.Add(email1.Trim());
                                //mm.To.Add("isoftbusiness@gmail.com");
                                mm.Subject = hora.RefAccao + " || " + editcurso + " // aula: " + hora.HoraInicio.ToShortDateString();
                                mm.IsBodyHtml = true;
                                mm.BodyEncoding = UTF8Encoding.UTF8;
                                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                                string horamodulo = hora.HoraInicio.ToShortTimeString() + " às " + hora.HoraFim.ToShortTimeString();

                                objSalas sala = null;
                                int existesala = (from d in db.Salas where d.id.ToString() == hora.CodSala.ToString() select d.id).Distinct().Count();
                                if (existesala > 0)
                                {
                                    sala = db.Salas.Where(x => x.id.ToString() == hora.CodSala.ToString()).First();
                                }

                                if (sala != null)
                                {
                                    string body = (sexo == "F" ? "Exma. Senhora" : "Exmo. Senhor ") + "<br/><br/>";
                                    body = body + (sexo == "F" ? "Professora " : "Professor ") + hora.Formador + "<br/><br/>";
                                    body = body + "Estimamos que se encontre bem.<br/>";
                                    body = body + "Serve o presente e-mail para relembrar o link de acesso à sala virtual da sessão de formação do módulo <b>" + hora.Modulo + " </b> das <b>" + horamodulo + " </b> (horário de Portugal Continental).<br/>";
                                    body = body + "Para aceder à sala virtual, deverá entrar através do seguinte link:<br/>";
                                    body = body + "<br/><a href=" + sala.link + ">Link de acesso à sala virtual</a>" + "<br/><br/>";
                                    body = body + "<a href=" + "https://ead.institutocriap.com/Zoomisoft/help.php?id=" + sala.id + "&formador="  + Uri.EscapeDataString(hora.Formador) + "&ref=" + hora.RefAccao + " style =\"font-face:arial;font-weight:bold;color:#fff;background-color:#1882d9;font-size:18px;text-decoration:none;line-height:2em;display:inline-block;text-align:center;border-radius:10px;border-color:#1882d9;border-style:solid;border-width:10px 20px\"> Solicitar ajuda ou apoio</a> " + "<br/><br/>";
                                    body = body + "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso responsável pela ação, através deste endereço de e-mail <a href='mailto:" + emailtxtcoordenador + "'>" + emailtxtcoordenador + "</a> e do seguinte contacto telefónico <strong>22 549 21 90.</strong>";
                                    body = body + "<br/><br/><h6>Data de envio: " + DateTime.Now.ToShortDateString() + " Hora: " + DateTime.Now.ToString("HH:mm") + "</h6>";

                                    mm.Body = body;

                                    client.Send(mm);
                                    mm.Dispose();

                                    richTextBox1.Text = richTextBox1.Text + (sexo == "F" ? "Professora " : "Professor ") + hora.Formador + " | " + email1 + " | enviado email no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + hora.RefAccao + " | " + horamodulo + System.Environment.NewLine;

                                    //Envia sms
                                    recipientWithName newSms = new recipientWithName();
                                    Obj_logSend logSender = new Obj_logSend();

                                    string numValidate = null;
                                    if (telefone != null)
                                    {
                                        numValidate = ValidarNum(telefone);
                                        if (numValidate != null)
                                        {
                                            newSms.msisdn = numValidate;
                                            newSms.name = hora.Formador;
                                        }
                                    }

                                    if (newSms.msisdn != "")
                                    {
                                        if (newSms.msisdn != null)
                                        {
                                            string bodysms = (sexo == "F" ? "Exma. Senhora " : "Exmo. Senhor ");
                                            bodysms = bodysms + (sexo == "F" ? "Professora " : "Professor ") + hora.Formador + "\n\n";
                                            bodysms = bodysms + "Estimamos que se encontre bem" + "\n\n";
                                            bodysms = bodysms + "Serve o resente e-mail para relembrar o link de acesso à sala virtual da sessão de formação do módulo " + hora.Modulo + " das " + horamodulo + " (horário de Portugal Continental)" + "\n\n";
                                            bodysms = bodysms + "Para aceder à sala virtual, deverá entrar através do seguinte link: ";
                                            bodysms = bodysms + sala.link + "\n\n";
                                            bodysms = bodysms + "Para solicitar apoio ou ajuda, deverá entrar através do seguinte link: ";
                                            bodysms = bodysms + "http://criapva.com/?id=" + sala.id + "&formador=" + Uri.EscapeDataString(hora.Formador) + "&ref=" + hora.RefAccao + "\n\n";
                                            bodysms = bodysms + "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso responsável pela ação, através deste endereço de e-mail " + emailtxtcoordenador + " e do seguinte contacto telefónico 22 549 21 90";

                                            logsSenders.Clear();
                                            if (newSms.msisdn != null)
                                            {
                                                logSender.Mensagem = bodysms;
                                                logSender.Nome = newSms.name;
                                                logSender.Numero = newSms.msisdn;
                                                logSender.smsSenders.Add(newSms);
                                                logsSenders.Add(logSender);
                                            }

                                            List<scheduleAlarm> smsS = new List<scheduleAlarm>();
                                            scheduleAlarm envio = new scheduleAlarm();
                                            envio.sendDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
                                            envio.text = logsSenders[0].Mensagem;
                                            envio.recipientsWithName = logsSenders[0].smsSenders.ToArray();
                                            smsS.Add(envio);
                                            smsByMailSEIService.sendShortScheduledMessage(credenciais, smsS.ToArray()).resultMessage.ToString();

                                            richTextBox1.Text = richTextBox1.Text + (sexo == "F" ? "Professora " : "Professor ") + hora.Formador + " | " + telefone + " | enviado sms no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + hora.RefAccao + " | " + horamodulo + System.Environment.NewLine;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //ENVIAR PARA CO-FORMADOR
            var consolidatedChildren3 =
            from c in db.htCursos.Where(x => x.CodFormador2 != "")
            group c by new
            {
                c.cod_Sala,
                c.CodFormador2,
                c.Hora_Inicio,
                c.Hora_Fim,
                c.Modulo,
                c.Ref_Accao
            } into gcs
            select new Result()
            {
                CodFormador2 = gcs.Key.CodFormador2,
                HoraInicio = gcs.Key.Hora_Inicio,
                HoraFim = gcs.Key.Hora_Fim,
                RefAccao = gcs.Key.Ref_Accao,
                Modulo = gcs.Key.Modulo,
                CodSala = gcs.Key.cod_Sala
            };

            foreach (var hora in consolidatedChildren3)
            {
                //DateTime horasyncman = new DateTime(2021, 7, 1,14, 0, 0);
                DateTime horasyncman = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                bool CCP = hora.RefAccao.Contains("CFPIF");

                DateTime horasync = horasyncman.AddHours(9);

                if (hora.HoraInicio >= horasyncman && hora.HoraInicio <= horasync)
                {
                    int existe = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.Email).Distinct().Count();

                    if (existe > 0)
                    {
                        if (CCP) { continue; }

                        string nomeformador = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.NomeFormador).Distinct().First().ToString();

                        string email1 = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.Email).Distinct().First().ToString();

                        string sexo = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.Sexo).Distinct().First().ToString();

                        string telefone = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.Telefone).Distinct().First().ToString();

                        string editcurso = (from a in db.htCursos where (a.Ref_Accao == hora.RefAccao) select a.NomeCurso).First();

                        string codcoordenador = (from a in db.htCursos where (a.Ref_Accao == hora.RefAccao) select a.CodCordenador).First();

                        string emailtxtcoordenador = "geral@criap.com";
                        if (codcoordenador != "")
                        {
                            emailtxtcoordenador = (from a in db.listaColaboradores where (a.codigo_Colaborador == codcoordenador) select a.email).First();
                        }

                        if (email1 != "")
                        {
                            if (ValidarEmail(email1.Trim()))
                            {
                                MailMessage mm = new MailMessage();
                                mm.From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> ");
                                //mm.From = new MailAddress("ISOFT <" + "geral@isoft.com.pt" + "> ");

                                mm.To.Add(email1.Trim());
                                //mm.To.Add("isoftbusiness@gmail.com");

                                mm.Subject = hora.RefAccao + " || " + editcurso + " // aula: " + hora.HoraInicio.ToShortDateString();
                                mm.IsBodyHtml = true;
                                mm.BodyEncoding = UTF8Encoding.UTF8;
                                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                                string horamodulo = hora.HoraInicio.ToShortTimeString() + " às " + hora.HoraFim.ToShortTimeString();

                                objSalas sala = null;
                                int existesala = (from d in db.Salas where d.id.ToString() == hora.CodSala.ToString() select d.id).Distinct().Count();
                                if (existesala > 0)
                                {
                                    sala = db.Salas.Where(x => x.id.ToString() == hora.CodSala.ToString()).First();
                                }

                                if (sala != null)
                                {
                                    string body = (sexo == "F" ? "Exma. Senhora" : "Exmo. Senhor ") + "<br/><br/>" +
                                                  (sexo == "F" ? "Professora " : "Professor ") + nomeformador + "<br/><br/>" +
                                                  "Estimamos que se encontre bem.<br/>" +
                                                  "Serve o presente e-mail para relembrar o link de acesso à sala virtual da sessão de formação do módulo <b>" +
                                                  hora.Modulo + " </b> das <b>" + horamodulo + " </b> (horário de Portugal Continental).<br/>" +
                                                  "Para aceder à sala virtual, deverá entrar através do seguinte link:<br/><br/>" +
                                                  "<a href='" + sala.link + "'>Link de acesso à sala virtual</a>" + "<br/><br/>" +
                                                  "<a href='http://criapva.com/?id=" + sala.id +
                                                  "&formador=" + Uri.EscapeDataString(nomeformador) +
                                                  "&ref=" + hora.RefAccao +
                                                  "&linkRegistrant=" + Uri.EscapeDataString(sala.link) +
                                                  "' style='font-face:arial;font-weight:bold;color:#fff;background-color:#1882d9;font-size:18px;text-decoration:none;line-height:2em;display:inline-block;text-align:center;border-radius:10px;border-color:#1882d9;border-style:solid;border-width:10px 20px'>" +
                                                  "Solicitar ajuda ou apoio</a> <br/><br/>" +
                                                  "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso responsável pela ação, através deste endereço de e-mail " +
                                                  "<a href='mailto:" + emailtxtcoordenador + "'>" + emailtxtcoordenador + "</a> e do seguinte contacto telefónico <strong>22 549 21 90.</strong><br/><br/>" +
                                                  "<h6>Data de envio: " + DateTime.Now.ToShortDateString() + " Hora: " + DateTime.Now.ToString("HH:mm") + "</h6>";

                                    mm.Body = body;
                                    client.Send(mm);
                                    mm.Dispose();

                                    richTextBox1.Text = richTextBox1.Text + (sexo == "F" ? "Professora " : "Professor ") + nomeformador + " | " + email1 + " | enviado email no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + hora.RefAccao + " | " + horamodulo + Environment.NewLine;

                                    //Envia sms
                                    recipientWithName newSms = new recipientWithName();
                                    Obj_logSend logSender = new Obj_logSend();

                                    string numValidate = null;
                                    if (telefone != null)
                                    {
                                        numValidate = ValidarNum(telefone);
                                        if (numValidate != null)
                                        {
                                            newSms.msisdn = numValidate;
                                            newSms.name = nomeformador;
                                        }
                                    }

                                    if (newSms.msisdn != "")
                                    {
                                        if (newSms.msisdn != null)
                                        {
                                            string bodysms = sessao.RefAccao + " // aula: " + DateTime.Now.ToShortDateString() + "\n\n" +
                                                            (sexo == "F" ? "Exma. Senhora " : "Exmo. Senhor ") +
                                                            (sexo == "F" ? "Professora " : "Professor ") + nomeformador + "\n\n" +
                                                            "Estimamos que se encontre bem" + "\n\n" +
                                                            "Serve o presente e-mail para relembrar o link de acesso à sala virtual da sessão de formação do módulo " +
                                                            sessao.Modulo + " das " + horamodulo + " (horário de Portugal Continental)" + "\n\n" +
                                                            "Para aceder à sala virtual, deverá entrar através do seguinte link: " + linkRegistrant + "\n\n" +
                                                            "Para solicitar apoio ou ajuda, deverá entrar através do seguinte link: " +
                                                            "http://criapva.com/?id=" + itemSala.id +
                                                            "&formador=" + Uri.EscapeDataString(nomeformador) +
                                                            "&ref=" + sessao.RefAccao +
                                                            "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) + "\n\n" +
                                                            "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso responsável pela ação, através deste endereço de e-mail " +
                                                            emailtxtcoordenador + " e do seguinte contacto telefónico 22 549 21 90";

                                            logsSenders.Clear();
                                            if (newSms.msisdn != null)
                                            {
                                                logSender.Mensagem = bodysms;
                                                logSender.Nome = newSms.name;
                                                logSender.Numero = newSms.msisdn;
                                                logSender.smsSenders.Add(newSms);
                                                logsSenders.Add(logSender);
                                            }

                                            List<scheduleAlarm> smsS = new List<scheduleAlarm>();
                                            scheduleAlarm envio = new scheduleAlarm();
                                            envio.sendDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
                                            envio.text = logsSenders[0].Mensagem;
                                            envio.recipientsWithName = logsSenders[0].smsSenders.ToArray();
                                            smsS.Add(envio);
                                            smsByMailSEIService.sendShortScheduledMessage(credenciais, smsS.ToArray()).resultMessage.ToString();

                                            richTextBox1.Text = richTextBox1.Text + (sexo == "F" ? "Professora " : "Professor ") + nomeformador + " | " + telefone + " | enviado sms no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + hora.RefAccao + " | " + horamodulo + System.Environment.NewLine;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void Envia_emails_formadores_sabado()
        {
            richTextBox1.Text = richTextBox1.Text + Environment.NewLine + "FORMADORES:" + Environment.NewLine;
            string subQuery = criapQuerys.Query.formadoresNotification.getModulosFormadoresALL();
            Formadores.Clear();
            dbConnect.ht.ConnInit();
            SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.ht2.Conn);
            DataTable subData = new DataTable();
            adapter.Fill(subData);
            List<DataRow> dataList = subData.AsEnumerable().ToList();
            dbConnect.ht.ConnEnd();

            if (dataList.Count != 0)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    var obj = new objFormadores()
                    {
                        formadorID = dataList[i][0].ToString(),
                        NomeFormador = dataList[i][1].ToString(),
                        Email = dataList[i][2].ToString(),
                        Telefone = dataList[i][3].ToString(),
                        Sexo = dataList[i][4].ToString()
                    };
                    Formadores.Add(obj);
                }
            }

            NetworkCredential basicCredential = new NetworkCredential(Properties.Settings.Default.emailenvio, Properties.Settings.Default.passwordemail);
            //NetworkCredential basicCredential = new NetworkCredential("geral@isoft.com.pt", "Isoft@28092010");
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.Host = "mail.criap.com";
            //client.Host = "smtp.isoft.com.pt";
            client.Timeout = 100000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = basicCredential;

            //ENVIAR PARA TODOS OS FORMADORES
            var consolidatedChildren2 =
            from c in db.htCursos
            group c by new
            {
                c.cod_Sala,
                c.Formador,
                c.Modulo,
                c.Ref_Accao,
                c.CodFormador
            } into gcs
            select new Result()
            {
                Formador = gcs.Key.Formador,
                RefAccao = gcs.Key.Ref_Accao,
                Modulo = gcs.Key.Modulo,
                CodSala = gcs.Key.cod_Sala,
                FormadorID = gcs.Key.CodFormador 
            };

            foreach (var hora in consolidatedChildren2)
            {
                DateTime horasyncman = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                //DateTime horasyncman = new DateTime(2021, 9, 18, 6, 0, 0);

                bool CCP = hora.RefAccao.Contains("CFPIF");

                int existe = (from d in Formadores where d.formadorID == hora.FormadorID select d.Email).Distinct().Count();

                if (existe > 0)
                {
                    if(CCP){continue;}

                    string email1 = (from d in Formadores where d.formadorID == hora.FormadorID select d.Email).Distinct().First().ToString();

                    string sexo = (from d in Formadores where d.formadorID == hora.FormadorID select d.Sexo).Distinct().First().ToString();

                    string telefone = (from d in Formadores where d.formadorID == hora.FormadorID select d.Telefone).Distinct().First().ToString();

                    string editcurso = (from a in db.htCursos where (a.Ref_Accao == hora.RefAccao) select a.NomeCurso).First();

                    string codcoordenador = (from a in db.htCursos where (a.Ref_Accao == hora.RefAccao) select a.CodCordenador).First();

                    string emailtxtcoordenador = "geral@criap.com";
                    if (codcoordenador != "")
                    {
                        emailtxtcoordenador = (from a in db.listaColaboradores where (a.codigo_Colaborador == codcoordenador) select a.email).First();
                    }

                    if (email1 != "" && hora.CodSala > 0)
                    {
                        if (ValidarEmail(email1.Trim()))
                        {
                            MailMessage mm = new MailMessage();
                            mm.From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> ");
                            //mm.From = new MailAddress("ISOFT <" + "geral@isoft.com.pt" + "> ");

                            mm.To.Add(email1.Trim());
                            //mm.To.Add("isoftbusiness@gmail.com");

                            mm.Subject = hora.RefAccao + " || " + editcurso + " // aula: " + horasyncman.ToShortDateString();
                            mm.IsBodyHtml = true;
                            mm.BodyEncoding = UTF8Encoding.UTF8;
                            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                            string horamodulo = "";
                            //VERIFICA QUANTAS SESSÕES TEM
                            List<objCursos> formadoreslista = (from a in db.htCursos select a).Where(X => X.CodFormador == hora.FormadorID && X.cod_Sala == hora.CodSala && X.Ref_Accao == hora.RefAccao).ToList();
                            foreach (var FORMADOR1 in formadoreslista.ToList())
                            {
                                if (horamodulo != "")
                                {
                                    horamodulo = horamodulo + " e das " + FORMADOR1.Hora_Inicio.ToShortTimeString() + " às " + FORMADOR1.Hora_Fim.ToShortTimeString();
                                }
                                else
                                {
                                    horamodulo = FORMADOR1.Hora_Inicio.ToShortTimeString() + " às " + FORMADOR1.Hora_Fim.ToShortTimeString();
                                }
                            }

                            objSalas sala = null;
                            int existesala = (from d in db.Salas where d.id.ToString() == hora.CodSala.ToString() select d.id).Distinct().Count();
                            if (existesala > 0)
                            {
                                sala = db.Salas.Where(x => x.id.ToString() == hora.CodSala.ToString()).First();
                            }

                            if (sala != null)
                            {
                                string body = (sexo == "F" ? "Exma. Senhora" : "Exmo. Senhor ") + "<br/><br/>" +
                                              (sexo == "F" ? "Professora " : "Professor ") + hora.Formador + "<br/><br/>" +
                                              "Estimamos que se encontre bem.<br/>" +
                                              "Serve o presente e-mail para relembrar o link de acesso à sala virtual da sessão de formação do módulo <b>" +
                                              hora.Modulo + " </b> das <b>" + horamodulo + " </b> (horário de Portugal Continental).<br/>" +
                                              "Para aceder à sala virtual, deverá entrar através do seguinte link:<br/><br/>" +
                                              "<a href='" + sala.link + "'>Link de acesso à sala virtual</a><br/><br/>" +
                                              "<a href='http://criapva.com/?id=" + sala.id +
                                              "&formador=" + Uri.EscapeDataString(hora.Formador) +
                                              "&ref=" + hora.RefAccao +
                                              "&linkRegistrant=" + Uri.EscapeDataString(sala.link) +
                                              "' style='font-face:arial;font-weight:bold;color:#fff;background-color:#1882d9;font-size:18px;text-decoration:none;line-height:2em;display:inline-block;text-align:center;border-radius:10px;border-color:#1882d9;border-style:solid;border-width:10px 20px'>" +
                                              "Solicitar ajuda ou apoio</a><br/><br/>" +
                                              "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso responsável pela ação, através deste endereço de e-mail " +
                                              "<a href='mailto:" + emailtxtcoordenador + "'>" + emailtxtcoordenador + "</a> e do seguinte contacto telefónico <strong>22 549 21 90.</strong><br/><br/>" +
                                              "<h6>Data de envio: " + DateTime.Now.ToShortDateString() + " Hora: " + DateTime.Now.ToString("HH:mm") + "</h6>";

                                mm.Body = body;
                                client.Send(mm);
                                mm.Dispose();
                                richTextBox1.Text = richTextBox1.Text + (sexo == "F" ? "Professora " : "Professor ") + hora.Formador + " | " + email1 + " | enviado email no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + hora.RefAccao + " | " + horamodulo + Environment.NewLine;

                                //Envia sms
                                recipientWithName newSms = new recipientWithName();
                                Obj_logSend logSender = new Obj_logSend();

                                string numValidate = null;
                                if (telefone != null)
                                {
                                    numValidate = ValidarNum(telefone);
                                    if (numValidate != null)
                                    {
                                        newSms.msisdn = numValidate;
                                        newSms.name = hora.Formador;
                                    }
                                }

                                if (newSms.msisdn != "")
                                {
                                    if (newSms.msisdn != null)
                                    {
                                        string body = (sexo == "F" ? "Exma. Senhora" : "Exmo. Senhor ") + "<br/><br/>" +
                                                      (sexo == "F" ? "Professora " : "Professor ") + hora.Formador + "<br/><br/>" +
                                                      "Estimamos que se encontre bem.<br/>" +
                                                      "Serve o presente e-mail para relembrar o link de acesso à sala virtual da sessão de formação do módulo <b>" +
                                                      hora.Modulo + " </b> das <b>" + horamodulo + " </b> (horário de Portugal Continental).<br/>" +
                                                     
                                                      "<a href='" + sala.link + "'>Link de acesso à sala virtual</a><br/><br/>" +
                                                      "<a href='http://criapva.com/?id=" + sala.id +
                                                      "&formador=" + Uri.EscapeDataString(hora.Formador) +
                                                      "&ref=" + hora.RefAccao +
                                                      "&linkRegistrant=" + Uri.EscapeDataString(sala.link) +
                                                      "' style='font-face:arial;font-weight:bold;color:#fff;background-color:#1882d9;font-size:18px;text-decoration:none;line-height:2em;display:inline-block;text-align:center;border-radius:10px;border-color:#1882d9;border-style:solid;border-width:10px 20px'>" +
                                                      "Solicitar ajuda ou apoio</a><br/><br/>" +
                                                      "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso responsável pela ação, através deste endereço de e-mail " +
                                                      "<a href='mailto:" + emailtxtcoordenador + "'>" + emailtxtcoordenador + "</a> e do seguinte contacto telefónico <strong>22 549 21 90.</strong><br/><br/>" +
                                                      "<h6>Data de envio: " + DateTime.Now.ToShortDateString() + " Hora: " + DateTime.Now.ToString("HH:mm") + "</h6>";

                                        logsSenders.Clear();
                                        if (newSms.msisdn != null)
                                        {
                                            logSender.Mensagem = bodysms;
                                            logSender.Nome = newSms.name;
                                            logSender.Numero = newSms.msisdn;
                                            logSender.smsSenders.Add(newSms);
                                            logsSenders.Add(logSender);
                                        }

                                        List<scheduleAlarm> smsS = new List<scheduleAlarm>();
                                        scheduleAlarm envio = new scheduleAlarm();
                                        envio.sendDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
                                        envio.text = logsSenders[0].Mensagem;
                                        envio.recipientsWithName = logsSenders[0].smsSenders.ToArray();
                                        smsS.Add(envio);
                                        smsByMailSEIService.sendShortScheduledMessage(credenciais, smsS.ToArray()).resultMessage.ToString();

                                        richTextBox1.Text = richTextBox1.Text + (sexo == "F" ? "Professora " : "Professor ") + hora.Formador + " | " + telefone + " | enviado sms no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + hora.RefAccao + " | " + horamodulo + Environment.NewLine;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //ENVIAR PARA CO-FORMADOR
            var consolidatedChildren3 =
            from c in db.htCursos.Where(x => x.CodFormador2 != "")
            group c by new
            {
                c.cod_Sala,
                c.CodFormador2,
                c.Hora_Inicio,
                c.Hora_Fim,
                c.Modulo,
                c.Ref_Accao
            } into gcs
            select new Result()
            {
                CodFormador2 = gcs.Key.CodFormador2,
                HoraInicio = gcs.Key.Hora_Inicio,
                HoraFim = gcs.Key.Hora_Fim,
                RefAccao = gcs.Key.Ref_Accao,
                Modulo = gcs.Key.Modulo,
                CodSala = gcs.Key.cod_Sala
            };

            foreach (var hora in consolidatedChildren3)
            {
                //DateTime horasyncman = new DateTime(2021, 9, 18, 6, 0, 0);
                DateTime horasyncman = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);

                int existe = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.Email).Distinct().Count();
                bool CCP = hora.RefAccao.Contains("CFPIF");

                if (existe > 0)
                {
                    if(CCP){continue;}

                    string nomeformador = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.NomeFormador).Distinct().First().ToString();

                    string email1 = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.Email).Distinct().First().ToString();

                    string sexo = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.Sexo).Distinct().First().ToString();

                    string telefone = (from d in Formadores where d.formadorID == hora.CodFormador2 select d.Telefone).Distinct().First().ToString();

                    string editcurso = (from a in db.htCursos where (a.Ref_Accao == hora.RefAccao) select a.NomeCurso).First();

                    string codcoordenador = (from a in db.htCursos where (a.Ref_Accao == hora.RefAccao) select a.CodCordenador).First();

                    string emailtxtcoordenador = "geral@criap.com";
                    if (codcoordenador != "")
                    {
                        emailtxtcoordenador = (from a in db.listaColaboradores where (a.codigo_Colaborador == codcoordenador) select a.email).First();
                    }

                    if (email1 != "" && hora.CodSala > 0)
                    {
                        if (ValidarEmail(email1.Trim()))
                        {
                            MailMessage mm = new MailMessage();
                            mm.From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> ");
                            //mm.From = new MailAddress("ISOFT <" + "geral@isoft.com.pt" + "> ");

                            mm.To.Add(email1.Trim());
                            //mm.To.Add("isoftbusiness@gmail.com");

                            mm.Subject = hora.RefAccao + " || " + editcurso + " // aula: " + hora.HoraInicio.ToShortDateString();
                            mm.IsBodyHtml = true;
                            mm.BodyEncoding = UTF8Encoding.UTF8;
                            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                            string horamodulo = hora.HoraInicio.ToShortTimeString() + " às " + hora.HoraFim.ToShortTimeString();

                            objSalas sala = null;
                            int existesala = (from d in db.Salas where d.id.ToString() == hora.CodSala.ToString() select d.id).Distinct().Count();
                            if (existesala > 0)
                            {
                                sala = db.Salas.Where(x => x.id.ToString() == hora.CodSala.ToString()).First();
                            }

                            if (sala != null)
                            {
                                string body = (sexo == "F" ? "Exma. Senhora" : "Exmo. Senhor ") + "<br/><br/>" +
                                              (sexo == "F" ? "Professora " : "Professor ") + nomeformador + "<br/><br/>" +
                                              "Estimamos que se encontre bem.<br/>Serve o presente e-mail para relembrar o link de acesso à sala virtual da sessão de formação do módulo <b>" +
                                              sessao.Modulo + " </b> das <b>" + horamodulo + " </b> (horário de Portugal Continental).<br/>Para aceder à sala virtual, deverá entrar através do seguinte link:<br/><br/>" +
                                              "<a href='http://criapva.com/?id=" + itemSala.id +
                                              "&formador=" + Uri.EscapeDataString(nomeformador) +
                                              "&ref=" + sessao.RefAccao +
                                              "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) +
                                              "&linkRegistrant=" + Uri.EscapeDataString(linkRegistrant) + "' " +
                                              "style='font-face:arial;font-weight:bold;color:#fff;background-color:#1882d9;font-size:18px;text-decoration:none;line-height:2em;display:inline-block;text-align:center;border-radius:10px;border-color:#1882d9;border-style:solid;border-width:10px 20px'>" +
                                              "Solicitar ajuda ou apoio</a> <br/><br/>Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso responsável pela ação, através deste endereço de e-mail <a href='mailto:" + emailtxtcoordenador + "'>" + emailtxtcoordenador + "</a> e do seguinte contacto telefónico <strong>22 549 21 90.</strong><br/><br/><h6>Data de envio: " + DateTime.Now.ToShortDateString() + " Hora: " + DateTime.Now.ToString("HH:mm") + "</h6>";

                                mm.Body = body;
                                client.Send(mm);
                                mm.Dispose();

                                richTextBox1.Text = richTextBox1.Text + (sexo == "F" ? "Professora " : "Professor ") + nomeformador + " | " + email1 + " | enviado email no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + hora.RefAccao + " | " + horamodulo + Environment.NewLine;

                                //Envia sms
                                recipientWithName newSms = new recipientWithName();
                                Obj_logSend logSender = new Obj_logSend();

                                string numValidate = null;
                                if (telefone != null)
                                {
                                    numValidate = ValidarNum(telefone);
                                    if (numValidate != null)
                                    {
                                        newSms.msisdn = numValidate;
                                        newSms.name = nomeformador;
                                    }
                                }

                                if (newSms.msisdn != "")
                                {
                                    if (newSms.msisdn != null)
                                    {
                                        string bodysms = sessao.RefAccao + " // aula: " + DateTime.Now.ToShortDateString() + "\n\n" +
                                                        (sexo == "F" ? "Exma. Senhora " : "Exmo. Senhor ") +
                                                        (sexo == "F" ? "Professora " : "Professor ") + nomeformador + "\n\n" +
                                                        "Estimamos que se encontre bem" + "\n\n" +
                                                        "Serve o presente e-mail para relembrar o link de acesso à sala virtual da sessão de formação do módulo " +
                                                        sessao.Modulo + " das " + horamodulo + " (horário de Portugal Continental)" + "\n\n" +
                                                        "Para aceder à sala virtual, deverá entrar através do seguinte link: " + linkRegistrant + "\n\n" +
                                                        "Para solicitar apoio ou ajuda, deverá entrar através do seguinte link: " +
                                                        "http://criapva.com/?id=" + itemSala.id +
                                                        "&formador=" + Uri.EscapeDataString(nomeformador) +
                                                        "&ref=" + sessao.RefAccao +
                                                        "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) + "\n\n" +
                                                        "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso responsável pela ação, através deste endereço de e-mail " +
                                                        emailtxtcoordenador + " e do seguinte contacto telefónico 22 549 21 90";
                                        logsSenders.Clear();
                                        if (newSms.msisdn != null)
                                        {
                                            logSender.Mensagem = bodysms;
                                            logSender.Nome = newSms.name;
                                            logSender.Numero = newSms.msisdn;
                                            logSender.smsSenders.Add(newSms);
                                            logsSenders.Add(logSender);
                                        }

                                        List<scheduleAlarm> smsS = new List<scheduleAlarm>();
                                        scheduleAlarm envio = new scheduleAlarm();
                                        envio.sendDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
                                        envio.text = logsSenders[0].Mensagem;
                                        envio.recipientsWithName = logsSenders[0].smsSenders.ToArray();
                                        smsS.Add(envio);
                                        smsByMailSEIService.sendShortScheduledMessage(credenciais, smsS.ToArray()).resultMessage.ToString();

                                        richTextBox1.Text = richTextBox1.Text + (sexo == "F" ? "Professora " : "Professor ") + nomeformador + " | " + telefone + " | enviado sms no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + hora.RefAccao + " | " + horamodulo + Environment.NewLine;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private string ValidarNum(string num)
        {
            Regex regex = new Regex(@"^(9[1236][0-9]{7})");
            Match match = regex.Match(CleanSpace(num));

            if (match.Success) return CleanSpace(match.Value);
            else return null;
        }
        private string CleanSpace(string num)
        {
            return num.Replace(" ", string.Empty);
        }
        public static bool ValidarEmail(string email)
        {
            //Regex rg = new Regex(@"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$");

            //if (rg.IsMatch(email))
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

            if (email.Trim().EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //private void getalunos(int Rowidaccao, string versao_rowid)
        //{
        //    var curso = db.htCursos.Where(x => x.Rowid_Accao == Rowidaccao).First();

        //    MySqlDataAdapter adapter = new MySqlDataAdapter();
        //    List<DataRow> dataList;

        //    adapter.SelectCommand = new MySqlCommand("SELECT idLinksZoom, Bloqueado, Desistente, ContratoFalta, DCC, CH, CM, Codigo_Aluno,Link FROM LinksZoom WHERE refaccao='" + curso.Ref_Accao + "' AND date(DataSessao)='" + curso.Hora_Inicio.Date.ToString("yyyy-MM-dd") + "'", dbConnect.MoodleConnect.Conn);
        //    DataTable subData = new DataTable();
        //    adapter.Fill(subData);
        //    dataList = subData.AsEnumerable().ToList();
        //    adapter.Dispose();

        //    if (dataList.Count != 0)
        //    {
        //        for (int i = 0; i < dataList.Count; i++)
        //        {
        //            if (dataList[i][2].ToString() == "0" && dataList[i][1].ToString() == "0" && dataList[i][3].ToString() == "0" && dataList[i][8].ToString() != "")
        //            {
        //                var item = db.moodleUser.Where(x => x.id == dataList[i][7].ToString()).First();

        //                var obj = new objFormandos()
        //                {
        //                    ID = int.Parse(dataList[i][0].ToString()),
        //                    Formando = item.firstname + " " + item.lastname,
        //                    Email = item.email,
        //                    Contribuinte = item.idnumber,
        //                    Desistente = (dataList[i][2].ToString() == "1" ? "X" : ""),
        //                    Bloqueado = (dataList[i][1].ToString() == "1" ? "X" : ""),
        //                    Contrato = (dataList[i][3].ToString() == "1" ? "Em falta" : ""),
        //                    DCC = (dataList[i][4].ToString() == "1" ? "Em falta" : ""),
        //                    CH = (dataList[i][5].ToString() == "1" ? "Em falta" : ""),
        //                    CM = (dataList[i][6].ToString() == "1" ? "Em falta" : ""),
        //                    Link = dataList[i][8].ToString(),
        //                    versao_rowid = versao_rowid,
        //                    Rowid_Accao = Rowidaccao
        //                };
        //                db.htobjFormandosMain.Add(obj);
        //            }
        //        }
        //        subData.Dispose();
        //    }
        //}

        //private void getalunos(int Rowidaccao)
        //{
        //    var curso = db.htCursos.Where(x => x.Rowid_Accao == Rowidaccao).First();
        //    db.htobjFormandosMain.Clear();

        //    MySqlDataAdapter adapter = new MySqlDataAdapter();
        //    List<DataRow> dataList;

        //    adapter.SelectCommand = new MySqlCommand("SELECT idLinksZoom, Bloqueado, Desistente, ContratoFalta, DCC, CH, CM, Codigo_Aluno,Link FROM LinksZoom WHERE refaccao='" + curso.Ref_Accao + "' AND date(DataSessao)='" + curso.Hora_Inicio.Date.ToString("yyyy-MM-dd") + "'", dbConnect.MoodleConnect.Conn);
        //    DataTable subData = new DataTable();
        //    adapter.Fill(subData);
        //    dataList = subData.AsEnumerable().ToList();
        //    adapter.Dispose();

        //    if (dataList.Count != 0)
        //    {
        //        for (int i = 0; i < dataList.Count; i++)
        //        {
        //            if (dataList[i][2].ToString() == "0" && dataList[i][1].ToString() == "0" && dataList[i][3].ToString() == "0" && dataList[i][8].ToString() != "")
        //            {
        //                var item = db.moodleUser.Where(x => x.id == dataList[i][7].ToString()).First();

        //                var obj = new objFormandos()
        //                {
        //                    ID = int.Parse(dataList[i][0].ToString()),
        //                    Formando = item.firstname + " " + item.lastname,
        //                    Email = item.email,
        //                    Contribuinte = item.idnumber,
        //                    Desistente = (dataList[i][2].ToString() == "1" ? "X" : ""),
        //                    Bloqueado = (dataList[i][1].ToString() == "1" ? "X" : ""),
        //                    Contrato = (dataList[i][3].ToString() == "1" ? "Em falta" : ""),
        //                    DCC = (dataList[i][4].ToString() == "1" ? "Em falta" : ""),
        //                    CH = (dataList[i][5].ToString() == "1" ? "Em falta" : ""),
        //                    CM = (dataList[i][6].ToString() == "1" ? "Em falta" : ""),
        //                    Link = dataList[i][8].ToString()
        //                };
        //                db.htobjFormandosMain.Add(obj);
        //            }
        //        }
        //        subData.Dispose();
        //    }
        //}
        private void Button1_Click(object sender, EventArgs e)
        {
            //WebClient client1 = new WebClient();
            //string path = "https://ead.institutocriap.com/Zoomisoft/enviarmsg.php";//your php path
            //NameValueCollection formData = new NameValueCollection();
            //byte[] responseBytes2 = null;
            //formData.Add("number", "351914744908");
            //formData.Add("msg", "Serveteste.");
            //try
            //{
            //    responseBytes2 = client1.UploadValues(path, "POST", formData);
            //}
            //catch (WebException web)
            //{
            //    MessageBox.Show("Check network connection.\n"+web.Message);
            //}

            //string URI = "https://ead.institutocriap.com/Zoomisoft/enviarmsg.php";
            //string myParameters = "number=351914744908&msg=Hello2";

            string msg2 = Uri.EscapeDataString("Hello 2");
            ProcessStartInfo p1 = new ProcessStartInfo("https://ead.institutocriap.com/Zoomisoft/enviarmsg.php?text=Olá&phone=170123456789", "");
            Process.Start(p1);
        }
    }
}
class Result
{
    public DateTime HoraInicio, HoraFim;
    public string RefAccao, Formador, CodFormador2, Modulo, Link, Formando, FormadorID;
    public int Rowid_Accao, CodSala;
}
class Obj_logSend
{
    public string ColaboradorID { get; set; }
    public string Mensagem { get; set; }
    public string Nome { get; set; }
    public string Numero { get; set; }

    public List<recipientWithName> smsSenders = new List<recipientWithName>();
}