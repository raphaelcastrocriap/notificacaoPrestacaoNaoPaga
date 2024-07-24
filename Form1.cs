using System;
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
using System.Threading;
using System.IO;
using System.Drawing;
using MySqlX.XDevAPI;

namespace SalasZoomNotificationFormadores
{
    public partial class Form1 : Form
    {



        public bool error = false, teste;
        public RichTextBox errorTextBox = new RichTextBox(), newCursoTextBox = new RichTextBox();
        public static List<objFormadores> Formadores = new List<objFormadores>();
        authentication credenciais = new authentication();
        List<Obj_logSend> logsSenders = new List<Obj_logSend>();
        SMSByMailSEIService smsByMailSEIService = new SMSByMailSEIService();
        DateTime horasyncman;
        string mensagemInfo = "";





        public Form1()
        {
            InitializeComponent();

        }
        // Função para obter dados do coordenador
        private (string Email, string Telemovel) GetCoordenadorInfo(string codcoordenador)
        {
            string emailtxtcoordenador = "geral@criap.com"; // Valor padrão caso não encontre
            string telemovelCoordenacao = "22 549 21 90"; // Valor padrão caso não encontre

            string queryCoordenador3 = $"SELECT email, telemovel FROM email_coordenadora WHERE Login='{codcoordenador}'";
            dbConnect.secretariaVirtual.ConnInit();
            DataTable dataTableCoordenador3 = new DataTable();

            try
            {
                SqlDataAdapter adapterCoordenador3 = new SqlDataAdapter(queryCoordenador3, dbConnect.secretariaVirtual.Conn);
                adapterCoordenador3.Fill(dataTableCoordenador3);

                if (dataTableCoordenador3.Rows.Count > 0)
                {
                    emailtxtcoordenador = dataTableCoordenador3.Rows[0]["email"].ToString().Trim();
                    telemovelCoordenacao = dataTableCoordenador3.Rows[0]["telemovel"].ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao obter dados do coordenador: {ex.Message}");
            }
            finally
            {
                dbConnect.secretariaVirtual.ConnEnd();
            }

            return (emailtxtcoordenador, telemovelCoordenacao);
        }

        public string RetornaImgBase64(string caminhoImagem, int novaLargura = 100, int novaAltura = 100)
        {
            Image RedimensionaImagem(Image imagem, int largura, int altura)
            {
                var bmp = new Bitmap(largura, altura);
                using (var graphics = Graphics.FromImage(bmp))
                {
                    graphics.DrawImage(imagem, 0, 0, largura, altura);
                }
                return bmp;
            }

            try
            {
                if (Uri.IsWellFormedUriString(caminhoImagem, UriKind.Absolute))
                {
                    using (WebClient client = new WebClient())
                    {
                        byte[] imageBytes = client.DownloadData(caminhoImagem);
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            var imagemOriginal = Image.FromStream(ms);
                            var imagemReduzida = RedimensionaImagem(imagemOriginal, novaLargura, novaAltura);
                            using (var msReduzido = new MemoryStream())
                            {
                                imagemReduzida.Save(msReduzido, imagemOriginal.RawFormat);
                                string base64String = Convert.ToBase64String(msReduzido.ToArray());
                                return base64String;
                            }
                        }
                    }
                }
                else if (File.Exists(caminhoImagem))
                {
                    using (var imagemOriginal = Image.FromFile(caminhoImagem))
                    {
                        var imagemReduzida = RedimensionaImagem(imagemOriginal, novaLargura, novaAltura);
                        using (var msReduzido = new MemoryStream())
                        {
                            imagemReduzida.Save(msReduzido, imagemOriginal.RawFormat);
                            string base64String = Convert.ToBase64String(msReduzido.ToArray());
                            return base64String;
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Arquivo não encontrado: {caminhoImagem}");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao processar a imagem: {ex.Message}");
                return string.Empty;
            }
        }
        private void Form1_Load(object sender, EventArgs e)

        {

            //horasyncman = new DateTime(2022, 9, 29, 14, 0, 0);
            //horasyncman = new DateTime(2022, 10, 8, 8, 0, 0);
            teste = true;
            if (!teste)
                horasyncman = DateTime.Now;
            else
                horasyncman = new DateTime(2024, 07, 24, 14, 0, 0);
            Security.remote();
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Text += " V." + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString();

            string dt = @"Controlo de versão: " + " V." + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString() + " Assembly built date: " + System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location) + " by sa";
            mensagemInfo += " \r\n" + dt;
            string[] passedInArgs = Environment.GetCommandLineArgs();

            if (passedInArgs.Contains("-a") || passedInArgs.Contains("-A"))
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
                TimeSpan delay = new TimeSpan(0, 5, 0);
                if (e.Message.Contains("Foi estabelecida ligação com êxito ao servidor, mas, em seguida, ocorreu um erro durante o handshake anterior ao início de sessão"))
                {
                    SendEmail(richTextBox1.Text + "\n\n" + e.ToString() + "\n" + "IMPORTANTE: Em 5 min será executada a rotina outra vez...", "Notification Salas Zoom // Formadores - HANDSHAKE ERROR", true);

                    Thread.Sleep(delay);
                    ExecuteSync();
                }
                else if (e.Message.Contains("Não foi possível criar um canal seguro SSL/TLS"))
                {
                    SendEmail(richTextBox1.Text + "\n\n" + e.ToString() + "\n" + "IMPORTANTE: Em 5 min será executada a rotina outra vez...", "Notification Salas Zoom // Formadores - canal seguro SSL/TLS", true);

                    Thread.Sleep(delay);
                    ExecuteSync();
                }
                else
                    SendEmail(richTextBox1.Text + "\n\n" + e.ToString(), "Notification Salas Zoom // Formadores - ERROR ", true);

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
                    string existQuery = "SELECT CODIGO_COLABORADOR FROM DEST_Destacamentos where rowid_Sessao = " + int.Parse(dataList[i][20].ToString());
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
                    var obj = new objSessao()
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
                        sessao_versao_rowid = dataList[i][15].ToString(),
                        NomeCurso = dataList[i][16].ToString(),
                        codigoFormador2 = dataList[i][17].ToString(),
                        Formador2 = formador2txt,
                        Rowid_Accao = int.Parse(dataList[i][18].ToString()),
                        TM = nometecnicoass,
                        TSV = tecnicosalavirtual,
                        FormadorDistribuirLinks = dataList[i][9].ToString(),
                        CodCordenador = dataList[i][21].ToString(),
                        codigoFormador = dataList[i][22].ToString()
                    };

                    if (obj.Inicio_Curso.Date == obj.Hora_Inicio.Date) obj.InicioFim = "I";
                    if (obj.Fim_Curso.Date == obj.Hora_Fim.Date) obj.InicioFim = "T";

                    db.htSessoes.Add(obj);

                }
            }
        }
        private void GetSecretariaData()
        {
            if (db.htSessoes.Count > 0)
            {
                string subQuery = "SELECT Versao_rowid, Hora_Inicio, Hora_Fim, TransporteID, TransportePreco, AlojamentoID, AlojamentoPreco, SalaID, SalaPreco, Notas, Sala2ID_Sinc FROM " + dbConnect.nomeTableCorDados + " WHERE (";
                for (int i = 0; i < db.htSessoes.Count; i++)
                {
                    if (i != db.htSessoes.Count - 1)
                        subQuery += "Versao_rowid = '" + db.htSessoes[i].sessao_versao_rowid.ToString() + "' or ";
                    else
                    {
                        subQuery += "Versao_rowid = '" + db.htSessoes[i].sessao_versao_rowid.ToString() + "')";
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
                        int index = db.htSessoes.FindIndex(x => x.sessao_versao_rowid == dataList[i][0].ToString());

                        if (dataList[i][3] != DBNull.Value)
                            db.htSessoes[index].cod_Transporte = int.Parse(dataList[i][3].ToString());
                        db.htSessoes[index].preco_Transporte = double.Parse(dataList[i][4].ToString());
                        if (dataList[i][5] != DBNull.Value)
                            db.htSessoes[index].cod_Alojamento = int.Parse(dataList[i][5].ToString());
                        db.htSessoes[index].preco_Alojamento = double.Parse(dataList[i][6].ToString());
                        if (dataList[i][7] != DBNull.Value)
                            db.htSessoes[index].cod_Sala = int.Parse(dataList[i][7].ToString());
                        db.htSessoes[index].preco_Sala = double.Parse(dataList[i][8].ToString());
                        db.htSessoes[index].notas = dataList[i][9].ToString();
                        if (dataList[i][10] != DBNull.Value)
                            db.htSessoes[index].cod_Sala2 = int.Parse(dataList[i][10].ToString());
                    }
                    subData.Dispose();
                }
            }
        }
        private void GetFormacaoExterna(DateTime inicio, DateTime fim)
        {
            string subQuery = "SELECT fe.Id, fe.NomeFormacao, fe.Estado, fe.NHoras, fe.NFormandos, fe.Formador, fe.Local, cd.Codigo_Curso, cd.Ref_Accao, cd.Hora_Inicio, cd.Hora_Fim, cd.Versao_rowid FROM " + dbConnect.nomeTableCorDados + " cd INNER JOIN cor_FormacaoExterna fe ON cd.FormacaoExterna = fe.Id where (cd.Hora_Inicio >= '" + inicio.ToString("yyyy-MM-dd") + "') AND (cd.Hora_Fim <= '" + fim.ToString("yyyy-MM-dd") + " 23:59') ";

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
                    var obj = new objSessao
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
                        sessao_versao_rowid = dataList[i][11].ToString()
                    };
                    db.htSessoes.Add(obj);
                }
                subData.Dispose();
            }
        }
        public static void Get_Colaboradores()
        {
            db.listaColaboradores.Clear();
            string subQuery = "SELECT codigo_Colaborador, nome, email, sigla FROM DEST_Colaboradores";
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
                    var obj = new objColaborador()
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
            string subQuery = "SELECT id, (CONCAT (numero, ' - ', descricao)) as sala, descricao, local, capacidade, ISNULL(capacidadeU, 0), ISNULL(capacidadeEscola, 0), ISNULL(capacidadeAuditorio, 0), link, Zoom, LoginAdmin, ZoomFirstName, ZoomLastName FROM cor_Salas order by len(numero), numero"; //, LoginAdmin, ZoomFirstName, ZoomLastName
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
                    if (dataList[i][1].ToString() != "1 - Sala Virtual") //outras salas virtuales no Zoom
                    {
                        var obj = new objSala()
                        {
                            id = int.Parse(dataList[i][0].ToString()),
                            nome = dataList[i][2].ToString(),
                            local = dataList[i][3].ToString(),
                            capacidade = int.Parse(dataList[i][4].ToString()),
                            capacidadeU = int.Parse(dataList[i][5].ToString()),
                            capacidadeEscola = int.Parse(dataList[i][6].ToString()),
                            capacidadeAuditorio = int.Parse(dataList[i][7].ToString()),
                            link = dataList[i][8].ToString(),
                            loginAdmin = dataList[i][10].ToString(),
                            zoomFirstName = dataList[i][11].ToString(),
                            zoomLastName = dataList[i][12].ToString()
                        };
                        if (dataList[i][9] != DBNull.Value)
                            obj.zoom = (bool)dataList[i][9];
                        else obj.zoom = false;
                        if (dataList[i][9].ToString() == "True")
                            obj.descricao = dataList[i][2].ToString();
                        else
                            obj.descricao = dataList[i][1].ToString();
                        db.Salas.Add(obj);
                    }
                }
            }


        }
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

            if (!teste)
            {
                mm.To.Add("ritagoncalves@criap.com");
                mm.To.Add("franciscaalmeida@criap.com");
                mm.To.Add("geral@criap.com");
                mm.To.Add("luisgraca@criap.com");
                mm.To.Add("informatica@criap.com");
            }
            else
                mm.To.Add("sandraaguilar@criap.com");
            mm.Subject = assunto + " // " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToShortTimeString();
            mm.IsBodyHtml = false;
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            mm.Body = body + mensagemInfo;
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
            richTextBox1.Text = richTextBox1.Text + "\nFORMADORES:\n\n";
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
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.Host = "mail.criap.com";
            client.Timeout = 100000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = basicCredential;

            //ENVIAR PARA TODOS OS FORMADORES
            var consolidatedSessoesByFormador =
            from c in db.htSessoes
            group c by new
            {
                c.cod_Sala,
                c.sessao_versao_rowid,
                c.Formador,
                c.Hora_Inicio,
                c.Hora_Fim,
                c.Modulo,
                c.Ref_Accao,
                c.codigoFormador
            } into gcs
            select new Result()
            {
                Formador = gcs.Key.Formador,
                HoraInicio = gcs.Key.Hora_Inicio,
                HoraFim = gcs.Key.Hora_Fim,
                RefAccao = gcs.Key.Ref_Accao,
                Modulo = gcs.Key.Modulo,
                CodSala = gcs.Key.cod_Sala,
                CodFormador = gcs.Key.codigoFormador,
                Sessao_versao_rowid = gcs.Key.sessao_versao_rowid
            };




            foreach (var sessao in consolidatedSessoesByFormador)
            {
                //DateTime horasyncman = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                ////DateTime horasyncman = new DateTime(2022, 9, 30, 14, 0, 0);
                bool CCP = sessao.RefAccao.Contains("CFPIF") || sessao.RefAccao.Contains("FM_");
                DateTime horasync = horasyncman.AddHours(9);

                if (sessao.HoraInicio >= horasyncman && sessao.HoraInicio <= horasync)
                {
                    int existe = (from d in Formadores where d.formadorID == sessao.CodFormador select d.Email).Distinct().Count();

                    if (existe > 0)
                    {
                        //if (CCP) { continue; }
                        string email1 = (from d in Formadores where d.formadorID == sessao.CodFormador select d.Email).Distinct().First().ToString();

                        string sexo = (from d in Formadores where d.formadorID == sessao.CodFormador select d.Sexo).Distinct().First().ToString();

                        string editcurso = (from a in db.htSessoes where (a.Ref_Accao == sessao.RefAccao) select a.NomeCurso).First();

                        string codcoordenador = (from a in db.htSessoes where (a.Ref_Accao == sessao.RefAccao) select a.CodCordenador).First();

                        string telefone = (from d in Formadores where d.formadorID == sessao.CodFormador select d.Telefone).Distinct().First().ToString();

                        var (emailtxtcoordenador, telemovelCoordenacao) = GetCoordenadorInfo(codcoordenador);


                        if (email1 != "")
                        {
                            if (ValidarEmail(email1.Trim()))
                            {
                                MailMessage mm = new MailMessage();
                                mm.From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> ");
                                if (!teste)
                                    mm.To.Add(email1.Trim());
                                else
                                    mm.To.Add("sandraaguilar@criap.com");
                                mm.Subject = sessao.RefAccao + " || " + editcurso + " // aula: " + sessao.HoraInicio.ToShortDateString();
                                mm.IsBodyHtml = true;
                                mm.BodyEncoding = UTF8Encoding.UTF8;
                                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                                string horamodulo = sessao.HoraInicio.ToShortTimeString() + " às " + sessao.HoraFim.ToShortTimeString();

                                objSala itemSala = null;
                                objSala itemSala1 = null;
                                objSala itemSala2 = null;
                                objSessao sessaosala2 = db.htSessoes.Find(x => x.sessao_versao_rowid == sessao.Sessao_versao_rowid);
                                if (sessao.CodSala > 0)
                                    itemSala1 = db.Salas.Find(x => x.id == sessao.CodSala);
                                if (sessaosala2.cod_Sala2 > 0)
                                {
                                    //hallar la session

                                    itemSala2 = db.Salas.Find(x => x.id == sessaosala2.cod_Sala2);
                                }
                                if (itemSala1 != null)
                                {
                                    if (itemSala1.zoom)
                                        itemSala = itemSala1;
                                }
                                if (itemSala2 != null)
                                {
                                    if (itemSala2.zoom)
                                        itemSala = itemSala2;
                                }



                                //objSala itemSala = db.Salas.Find(x => x.id == sessao.CodSala);
                                DataRow meeting = null;

                                if (itemSala != null)
                                {
                                    if (itemSala.zoom)
                                        meeting = GetMeetingDB(sessao.Sessao_versao_rowid);
                                }

                                if (itemSala != null)
                                {

                                    string linkRegistrant = "";
                                    //busco los links del alumnos
                                    if (meeting != null)
                                    {
                                        linkRegistrant = getLinksRegistrant(long.Parse(meeting[2].ToString()), sessao.CodFormador);
                                    }
                                    if (linkRegistrant != null && linkRegistrant != "")
                                    {

                                        string body = (sexo == "F" ? "Exma. Senhora" : "Exmo. Senhor ") + " " +
                                          (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + ",<br/><br/>" +
                                          "Estimamos que se encontre bem.<br/>" +
                                          "Serve o presente e-mail para relembrar o acesso à sala virtual da sessão de formação do módulo <b>" +
                                          sessao.Modulo + " </b> das <b>" + horamodulo + " </b> (horário de Portugal Continental).<br/>" +
                                          "Para aceder à sala virtual, deverá aceder ao " + "<a href='http://criapva.com/?id=" + itemSala.id +
                                          "&formador=" + Uri.EscapeDataString(sessao.Formador) +
                                          "&ref=" + sessao.RefAccao +
                                          "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) +
                                          "&linkRegistrant=" + Uri.EscapeDataString(linkRegistrant) + "'>" + "Link de acesso</a>" + " " + "e clicar no botão <b>“Entrar no Zoom”</b>.<br/><br/>" +

                                          "Aproveitamos para relembrar que, sempre que possível, o acesso à sala virtual deverá ser feito com 30 minutos de antecedência para teste de som e imagem.<br/><br/>" +
                                         "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso, através do endereço de e-mail " +
                                          "<a href='mailto:" + emailtxtcoordenador + "'>" + emailtxtcoordenador + "</a> e do contacto telefónico <strong> " + telemovelCoordenacao + "  </strong><br/><br/>";


                                        mm.Body = body;
                                        client.Send(mm);
                                        mm.Dispose();

                                        richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + " | " + email1 + " | enviado email no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo + "\n";

                                        //Envia sms
                                        recipientWithName newSms = new recipientWithName();
                                        Obj_logSend logSender = new Obj_logSend();

                                        string numValidate = null;
                                        if (telefone != null)
                                        {
                                            if (!teste)
                                                numValidate = ValidarNum(telefone);
                                            else
                                                numValidate = ValidarNum("912648984");

                                            if (numValidate != null)
                                            {
                                                newSms.msisdn = numValidate;
                                                newSms.name = sessao.Formador;
                                            }
                                        }

                                        if (newSms.msisdn != "")
                                        {
                                            if (newSms.msisdn != null)
                                            {
                                                string bodysms = sessao.RefAccao.ToString() + " // aula: " + DateTime.Now.ToShortDateString() + "\n\n" +
                                                (sexo == "F" ? "Exma. Senhora " : "Exmo. Senhor ") +
                                                (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + ",\n" +
                                                "Estimamos que se encontre bem.\n\n" +
                                                "Serve a presente SMS para relembrar o acesso à sala virtual da sessão de formação do modulo " +
                                                sessao.Modulo + " das " + horamodulo + " (horário de Portugal Continental).\n\n" +
                                                "Para aceder à sala virtual, deverá aceder ao link e clicar no botão “Entrar no Zoom”:\n" +
                                                "http://criapva.com/?id=" + itemSala.id +
                                                "&formador=" + Uri.EscapeDataString(sessao.Formador) +
                                                "&ref=" + sessao.RefAccao +
                                                "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) +
                                                "&linkRegistrant=" + Uri.EscapeDataString(linkRegistrant) + "\n\n" +

                                                 "Aproveitamos para relembrar que, sempre que possível, o acesso à sala virtual deverá ser feito com 30 minutos de antecedência para teste de som e imagem.\n\n" +
                                               "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso, através do endereço de e-mail " +
                                                emailtxtcoordenador + " e do contacto telefónico " + telemovelCoordenacao + "\n\n";



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
                                                //  if (!teste)
                                                var res = smsByMailSEIService.sendShortScheduledMessage(credenciais, smsS.ToArray()).resultMessage.ToString(); //principal

                                                richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + " | " + telefone + (res.ToUpper().Contains("OK") ? " | enviado sms no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo : "| sms NÃO ENVIADO") + "\n\n";
                                            }
                                        }
                                    }
                                    else
                                        richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + " | " + email1 + " | NÃO FOI NOTIFICADO, NÃO TEM REGISTO NA MEETING , ALTERAÇÃO DE FORMADOR " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo + "\n\n";

                                }
                            }
                        }
                    }
                }
            }

            //ENVIAR PARA CO-FORMADOR
            var consolidatedSessoesByFormador2 =
            from c in db.htSessoes.Where(x => x.codigoFormador2 != "")
            group c by new
            {
                c.cod_Sala,
                c.sessao_versao_rowid,
                c.codigoFormador2,
                c.Hora_Inicio,
                c.Hora_Fim,
                c.Modulo,
                c.Ref_Accao
            } into gcs
            select new Result()
            {
                CodFormador2 = gcs.Key.codigoFormador2,
                HoraInicio = gcs.Key.Hora_Inicio,
                HoraFim = gcs.Key.Hora_Fim,
                RefAccao = gcs.Key.Ref_Accao,
                Modulo = gcs.Key.Modulo,
                CodSala = gcs.Key.cod_Sala,
                Sessao_versao_rowid = gcs.Key.sessao_versao_rowid
            };

            foreach (var sessao in consolidatedSessoesByFormador2)
            {
                ////DateTime horasyncman = new DateTime(2022, 9, 30, 14, 0, 0);
                //DateTime horasyncman = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                bool CCP = sessao.RefAccao.Contains("CFPIF") || sessao.RefAccao.Contains("FM_");

                DateTime horasync = horasyncman.AddHours(9);

                if (sessao.HoraInicio >= horasyncman && sessao.HoraInicio <= horasync)
                {
                    int existe = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.Email).Distinct().Count();

                    if (existe > 0)
                    {
                        //if (CCP) { continue; }

                        string nomeformador = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.NomeFormador).Distinct().First().ToString();

                        string email1 = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.Email).Distinct().First().ToString();

                        string sexo = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.Sexo).Distinct().First().ToString();

                        string telefone = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.Telefone).Distinct().First().ToString();

                        string editcurso = (from a in db.htSessoes where (a.Ref_Accao == sessao.RefAccao) select a.NomeCurso).First();

                        string codcoordenador = (from a in db.htSessoes where (a.Ref_Accao == sessao.RefAccao) select a.CodCordenador).First();

                        var (emailtxtcoordenador, telemovelCoordenacao) = GetCoordenadorInfo(codcoordenador);

                        if (email1 != "")
                        {
                            if (ValidarEmail(email1.Trim()))
                            {
                                MailMessage mm = new MailMessage();
                                mm.From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> ");
                                if (!teste)
                                    mm.To.Add(email1.Trim());
                                else
                                    mm.To.Add("sandraaguilar@criap.com");
                                mm.Subject = sessao.RefAccao + " || " + editcurso + " // aula: " + sessao.HoraInicio.ToShortDateString();
                                mm.IsBodyHtml = true;
                                mm.BodyEncoding = UTF8Encoding.UTF8;
                                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                                string horamodulo = sessao.HoraInicio.ToShortTimeString() + " às " + sessao.HoraFim.ToShortTimeString();

                                objSala itemSala = null;
                                objSala itemSala1 = null;
                                objSala itemSala2 = null;
                                objSessao sessaosala2 = db.htSessoes.Find(x => x.sessao_versao_rowid == sessao.Sessao_versao_rowid);
                                if (sessao.CodSala > 0)
                                    itemSala1 = db.Salas.Find(x => x.id == sessao.CodSala);
                                if (sessaosala2.cod_Sala2 > 0)
                                {
                                    //hallar la session

                                    itemSala2 = db.Salas.Find(x => x.id == sessaosala2.cod_Sala2);
                                }
                                if (itemSala1 != null)
                                {
                                    if (itemSala1.zoom)
                                        itemSala = itemSala1;
                                }
                                if (itemSala2 != null)
                                {
                                    if (itemSala2.zoom)
                                        itemSala = itemSala2;
                                }



                                //objSala itemSala = db.Salas.Find(x => x.id == sessao.CodSala);
                                DataRow meeting = null;

                                if (itemSala != null)
                                {
                                    if (itemSala.zoom)
                                        meeting = GetMeetingDB(sessao.Sessao_versao_rowid);
                                }

                                if (itemSala != null)
                                {
                                    string linkRegistrant = "";
                                    //busco los links del alumno
                                    if (meeting != null)
                                    {
                                        linkRegistrant = getLinksRegistrant(long.Parse(meeting[2].ToString()), sessao.CodFormador2);
                                    }
                                    if (linkRegistrant != null && linkRegistrant != "")
                                    {
                                        string body = (sexo == "F" ? "Exma. Senhora" : "Exmo. Senhor ") + " " +
                                           (sexo == "F" ? "Professora " : "Professor ") + nomeformador + "<br/><br/>" +
                                           "Estimamos que se encontre bem.<br/>" +
                                           "Serve o presente e-mail para relembrar o acesso à sala virtual da sessão de formação do módulo <b>" +
                                           sessao.Modulo + " </b> das <b>" + horamodulo + " </b> (horário de Portugal Continental).<br/>" +
                                           "Para aceder à sala virtual, deverá aceder ao " + "<a href='http://criapva.com/?id=" + itemSala.id +
                                           "&formador=" + Uri.EscapeDataString(nomeformador) +
                                           "&ref=" + sessao.RefAccao +
                                           "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) +
                                           "&linkRegistrant=" + Uri.EscapeDataString(linkRegistrant) + "'>" + "Link de acesso</a>" + " " + "e clicar no botão <b>“Entrar no Zoom”</b>.<br/><br/>" +

                                         "Aproveitamos para relembrar que, sempre que possível, o acesso à sala virtual deverá ser feito com 30 minutos de antecedência para teste de som e imagem.<br/><br/>" +
                                         "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso, através do endereço de e-mail " +
                                         "<a href='mailto:" + emailtxtcoordenador + "'>" + emailtxtcoordenador + "</a> e do contacto telefónico <strong> " + telemovelCoordenacao + "  </strong><br/><br/>";


                                        mm.Body = body;
                                        client.Send(mm);
                                        mm.Dispose();

                                        richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + nomeformador + " | " + email1 + " | enviado email no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo + "\n";

                                        //Envia sms
                                        recipientWithName newSms = new recipientWithName();
                                        Obj_logSend logSender = new Obj_logSend();

                                        string numValidate = null;
                                        if (telefone != null)
                                        {
                                            if (!teste)
                                                numValidate = ValidarNum(telefone);
                                            else
                                                numValidate = ValidarNum("912648984");
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
                                                string bodysms = sessao.RefAccao.ToString() + " // aula: " + DateTime.Now.ToShortDateString() + "\n\n" +
                                                (sexo == "F" ? "Exma. Senhora " : "Exmo. Senhor ") +
                                                (sexo == "F" ? "Professora " : "Professor ") + nomeformador + ",\n" +
                                                "Estimamos que se encontre bem.\n\n" +
                                                "Serve a presente SMS para relembrar o acesso à sala virtual da sessão de formação do modulo " +
                                                sessao.Modulo + " das " + horamodulo + " (horário de Portugal Continental).\n\n" +
                                                 "Para aceder à sala virtual, deverá aceder ao link e clicar no botão “Entrar no Zoom”:\n" +
                                                "http://criapva.com/?id=" + itemSala.id +
                                                "&formador=" + Uri.EscapeDataString(nomeformador) +
                                                "&ref=" + sessao.RefAccao +
                                                "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) +
                                                "&linkRegistrant=" + Uri.EscapeDataString(linkRegistrant) + "\n\n" +

                                                "Aproveitamos para relembrar que, sempre que possível, o acesso à sala virtual deverá ser feito com 30 minutos de antecedência para teste de som e imagem.\n\n" +
                                                "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso, através do endereço de e-mail " +
                                                 emailtxtcoordenador + " e do contacto telefónico " + telemovelCoordenacao + "\n\n";


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
                                                //if (!teste)
                                                var res = smsByMailSEIService.sendShortScheduledMessage(credenciais, smsS.ToArray()).resultMessage.ToString();

                                                richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + nomeformador + " | " + telefone + (res.ToUpper().Contains("OK") ? " | enviado sms no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo : "| sms NÃO ENVIADO") + "\n\n";
                                            }
                                        }
                                    }
                                    else
                                        richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + " | " + email1 + " | NÃO FOI NOTIFICADO, NÃO TEM REGISTO NA MEETING , ALTERAÇÃO DE FORMADOR " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo + "\n\n";

                                }
                            }
                        }
                    }
                }
            }
        }

        

        private string getLinksRegistrant(long meetingID, string idFormandoHT)
        {
            if (idFormandoHT == null)
                idFormandoHT = "0";
            string subQuery = "select join_url from " + dbConnect.nomeTableRegistrants + " where meetingID = " + meetingID.ToString() + " and idHT = " + idFormandoHT;
            ;
            dbConnect.secretariaVirtual.ConnInit();
            SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.secretariaVirtual.Conn);
            DataTable subData = new DataTable();
            adapter.Fill(subData);
            dbConnect.secretariaVirtual.ConnEnd();
            try
            {
                DataRow link = subData.AsEnumerable().ToList().First();
                return link[0].ToString();
            }
            catch (Exception)
            {
                return null;
            }



        }
        public DataRow GetMeetingDB(string sessao_versao_rowid)
        {
            string subQuery = "SELECT * FROM " + dbConnect.nomeTableMeetings + " where sessao_versao_rowid=" + sessao_versao_rowid;
            dbConnect.secretariaVirtual.ConnInit();
            SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.secretariaVirtual.Conn);
            DataTable subData = new DataTable();
            adapter.Fill(subData);
            DataRow meeting = subData.AsEnumerable().ToList().First();
            dbConnect.secretariaVirtual.ConnEnd();
            return meeting;
        }
        private void Envia_emails_formadores_sabado()
        {
            richTextBox1.Text = richTextBox1.Text + "\nFORMADORES:" + "\n\n";
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
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.Host = "mail.criap.com";
            client.Timeout = 100000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = basicCredential;

            //ENVIAR PARA TODOS OS FORMADORES
            var consolidatedChildren2 =
            from c in db.htSessoes
            group c by new
            {
                c.cod_Sala,
                c.sessao_versao_rowid,
                c.Formador,
                c.Hora_Inicio,
                c.Hora_Fim,
                c.Modulo,
                c.Ref_Accao,
                c.codigoFormador
            } into gcs
            select new Result()
            {
                Formador = gcs.Key.Formador,
                HoraInicio = gcs.Key.Hora_Inicio,
                HoraFim = gcs.Key.Hora_Fim,
                RefAccao = gcs.Key.Ref_Accao,
                Modulo = gcs.Key.Modulo,
                CodSala = gcs.Key.cod_Sala,
                CodFormador = gcs.Key.codigoFormador,
                Sessao_versao_rowid = gcs.Key.sessao_versao_rowid
            };


            foreach (var sessao in consolidatedChildren2)
            {
                //DateTime horasyncman = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                ////DateTime horasyncman = new DateTime(2021, 9, 18, 6, 0, 0);

                bool CCP = sessao.RefAccao.Contains("CFPIF") || sessao.RefAccao.Contains("FM_");
                int existe = (from d in Formadores where d.formadorID == sessao.CodFormador select d.Email).Distinct().Count();

                if (existe > 0)
                {
                    //if (CCP) { continue; }

                    string email1 = (from d in Formadores where d.formadorID == sessao.CodFormador select d.Email).Distinct().First().ToString();

                    string sexo = (from d in Formadores where d.formadorID == sessao.CodFormador select d.Sexo).Distinct().First().ToString();

                    string telefone = (from d in Formadores where d.formadorID == sessao.CodFormador select d.Telefone).Distinct().First().ToString();

                    string editcurso = (from a in db.htSessoes where (a.Ref_Accao == sessao.RefAccao) select a.NomeCurso).First();

                    string codcoordenador = (from a in db.htSessoes where (a.Ref_Accao == sessao.RefAccao) select a.CodCordenador).First();
                    var (emailtxtcoordenador, telemovelCoordenacao) = GetCoordenadorInfo(codcoordenador);



                    if (email1 != "")
                    {
                        if (ValidarEmail(email1.Trim()))
                        {
                            MailMessage mm = new MailMessage();
                            mm.From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> ");
                            if (!teste)
                                mm.To.Add(email1.Trim());
                            else
                                mm.To.Add("sandraaguilar@criap.com");
                            mm.Subject = sessao.RefAccao + " || " + editcurso + " // aula: " + sessao.HoraInicio.ToShortDateString();
                            mm.IsBodyHtml = true;
                            mm.BodyEncoding = UTF8Encoding.UTF8;
                            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                            string horamodulo = "";
                            //VERIFICA QUANTAS SESSÕES TEM
                            List<objSessao> formadoreslista = (from a in db.htSessoes select a).Where(X => X.codigoFormador == sessao.CodFormador && X.cod_Sala == sessao.CodSala && X.Ref_Accao == sessao.RefAccao).ToList();
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

                            objSala itemSala = null;
                            objSala itemSala1 = null;
                            objSala itemSala2 = null;
                            objSessao sessaosala2 = db.htSessoes.Find(x => x.sessao_versao_rowid == sessao.Sessao_versao_rowid);
                            if (sessao.CodSala > 0)
                                itemSala1 = db.Salas.Find(x => x.id == sessao.CodSala);
                            if (sessaosala2.cod_Sala2 > 0)
                            {
                                //hallar la session

                                itemSala2 = db.Salas.Find(x => x.id == sessaosala2.cod_Sala2);
                            }
                            if (itemSala1 != null)
                            {
                                if (itemSala1.zoom)
                                    itemSala = itemSala1;
                            }
                            if (itemSala2 != null)
                            {
                                if (itemSala2.zoom)
                                    itemSala = itemSala2;
                            }

                            DataRow meeting = null;

                            if (itemSala != null)
                            {
                                if (itemSala.zoom)
                                    meeting = GetMeetingDB(sessao.Sessao_versao_rowid);
                            }

                            if (itemSala != null)
                            {
                                if (itemSala.zoom)
                                    meeting = GetMeetingDB(sessao.Sessao_versao_rowid);
                            }

                            if (itemSala != null)
                            {
                                string linkRegistrant = "";
                                //busco los links del alumno
                                if (meeting != null)
                                {

                                    linkRegistrant = getLinksRegistrant(long.Parse(meeting[2].ToString()), sessao.CodFormador);
                                }
                                if (linkRegistrant != null && linkRegistrant != "")
                                {

                                    string body = (sexo == "F" ? "Exma. Senhora" : "Exmo. Senhor ") + " " +
                                                   (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + ",<br/><br/>" +
                                                   "Estimamos que se encontre bem.<br/>" +
                                                   "Serve o presente e-mail para relembrar o acesso à sala virtual da sessão de formação do módulo <b>" +
                                                   sessao.Modulo + " </b> das <b>" + horamodulo + " </b> (horário de Portugal Continental).<br/>" +
                                                   "Para aceder à sala virtual, deverá aceder ao " + "<a href='http://criapva.com/?id=" + itemSala.id +
                                                   "&formador=" + Uri.EscapeDataString(sessao.Formador) +
                                                   "&ref=" + sessao.RefAccao +
                                                   "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) +
                                                   "&linkRegistrant=" + Uri.EscapeDataString(linkRegistrant) + "'>" + "Link de acesso</a>" + " " + "e clicar no botão <b>“Entrar no Zoom”</b>.<br/><br/>" +

                                                 "Aproveitamos para relembrar que, sempre que possível, o acesso à sala virtual deverá ser feito com 30 minutos de antecedência para teste de som e imagem.<br/><br/>" +
                                                 "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso, através do endereço de e-mail " +
                                                 "<a href='mailto:" + emailtxtcoordenador + "'>" + emailtxtcoordenador + "</a> e do contacto telefónico <strong> " + telemovelCoordenacao + "  </strong><br/><br/>";


                                    mm.Body = body;
                                    client.Send(mm);
                                    mm.Dispose();

                                    richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + " | " + email1 + " | enviado email no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo + "\n";

                                    //Envia sms
                                    recipientWithName newSms = new recipientWithName();
                                    Obj_logSend logSender = new Obj_logSend();

                                    string numValidate = null;
                                    if (telefone != null)
                                    {
                                        if (!teste)
                                            numValidate = ValidarNum(telefone);
                                        else
                                            numValidate = ValidarNum("912648984");

                                        if (numValidate != null)
                                        {
                                            newSms.msisdn = numValidate;
                                            newSms.name = sessao.Formador;
                                        }
                                    }

                                    if (newSms.msisdn != "")
                                    {
                                        if (newSms.msisdn != null)
                                        {
                                            string bodysms = sessao.RefAccao.ToString() + " // aula: " + DateTime.Now.ToShortDateString() + "\n\n" +
                                            (sexo == "F" ? "Exma. Senhora " : "Exmo. Senhor ") +
                                            (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador  +",\n" +
                                            "Estimamos que se encontre bem.\n\n" +
                                            "Serve a presente SMS para relembrar o acesso à sala virtual da sessão de formação do modulo " +
                                            sessao.Modulo + " das " + horamodulo + " (horário de Portugal Continental).\n\n" +
                                            "Para aceder à sala virtual, deverá aceder ao link e clicar no botão “Entrar no Zoom”:\n" +
                                            "http://criapva.com/?id=" + itemSala.id +
                                            "&formador=" + Uri.EscapeDataString(sessao.Formador) +
                                            "&ref=" + sessao.RefAccao +
                                            "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) +
                                            "&linkRegistrant=" + Uri.EscapeDataString(linkRegistrant) + "\n\n" +

                                             "Aproveitamos para relembrar que, sempre que possível, o acesso à sala virtual deverá ser feito com 30 minutos de antecedência para teste de som e imagem.\n\n" +
                                               "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso, através do endereço de e-mail " +
                                                emailtxtcoordenador + " e do contacto telefónico " + telemovelCoordenacao + "\n\n";



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
                                            //  if (!teste)
                                            var res = smsByMailSEIService.sendShortScheduledMessage(credenciais, smsS.ToArray()).resultMessage.ToString(); //principal

                                            richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + " | " + telefone + (res.ToUpper().Contains("OK") ? " | enviado sms no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo : "| sms NÃO ENVIADO") + "\n\n";
                                        }
                                    }
                                }
                                else
                                    richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + " | " + email1 + " | NÃO FOI NOTIFICADO, NÃO TEM REGISTO NA MEETING , ALTERAÇÃO DE FORMADOR " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo + "\n\n";


                            }
                        }
                    }
                }
            }

            //ENVIAR PARA CO-FORMADOR
            var consolidatedChildren3 =
            from c in db.htSessoes.Where(x => x.codigoFormador2 != "")
            group c by new
            {
                c.cod_Sala,
                c.sessao_versao_rowid,
                c.codigoFormador2,
                c.Hora_Inicio,
                c.Hora_Fim,
                c.Modulo,
                c.Ref_Accao
            } into gcs
            select new Result()
            {
                CodFormador2 = gcs.Key.codigoFormador2,
                HoraInicio = gcs.Key.Hora_Inicio,
                HoraFim = gcs.Key.Hora_Fim,
                RefAccao = gcs.Key.Ref_Accao,
                Modulo = gcs.Key.Modulo,
                CodSala = gcs.Key.cod_Sala,
                Sessao_versao_rowid = gcs.Key.sessao_versao_rowid
            };

            foreach (var sessao in consolidatedChildren3)
            {
                ////DateTime horasyncman = new DateTime(2021, 9, 18, 6, 0, 0);
                //DateTime horasyncman = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);

                int existe = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.Email).Distinct().Count();
                bool CCP = sessao.RefAccao.Contains("CFPIF") || sessao.RefAccao.Contains("FM_");

                if (existe > 0)
                {
                    //if (CCP) { continue; }

                    string nomeformador = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.NomeFormador).Distinct().First().ToString();

                    string email1 = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.Email).Distinct().First().ToString();

                    string sexo = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.Sexo).Distinct().First().ToString();

                    string telefone = (from d in Formadores where d.formadorID == sessao.CodFormador2 select d.Telefone).Distinct().First().ToString();

                    string editcurso = (from a in db.htSessoes where (a.Ref_Accao == sessao.RefAccao) select a.NomeCurso).First();

                    string codcoordenador = (from a in db.htSessoes where (a.Ref_Accao == sessao.RefAccao) select a.CodCordenador).First();
                    var (emailtxtcoordenador, telemovelCoordenacao) = GetCoordenadorInfo(codcoordenador);

                    if (email1 != "")
                    {
                        if (ValidarEmail(email1.Trim()))
                        {
                            MailMessage mm = new MailMessage();
                            mm.From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> ");
                            if (!teste)
                                mm.To.Add(email1.Trim());
                            else
                                mm.To.Add("sandraaguilar@criap.com");
                            mm.Subject = sessao.RefAccao + " || " + editcurso + " // aula: " + sessao.HoraInicio.ToShortDateString();
                            mm.IsBodyHtml = true;
                            mm.BodyEncoding = UTF8Encoding.UTF8;
                            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                            string horamodulo = sessao.HoraInicio.ToShortTimeString() + " às " + sessao.HoraFim.ToShortTimeString();

                            objSala itemSala = null;
                            objSala itemSala1 = null;
                            objSala itemSala2 = null;
                            objSessao sessaosala2 = db.htSessoes.Find(x => x.sessao_versao_rowid == sessao.Sessao_versao_rowid);
                            if (sessao.CodSala > 0)
                                itemSala1 = db.Salas.Find(x => x.id == sessao.CodSala);
                            if (sessaosala2.cod_Sala2 > 0)
                            {
                                //hallar la session

                                itemSala2 = db.Salas.Find(x => x.id == sessaosala2.cod_Sala2);
                            }
                            if (itemSala1 != null)
                            {
                                if (itemSala1.zoom)
                                    itemSala = itemSala1;
                            }
                            if (itemSala2 != null)
                            {
                                if (itemSala2.zoom)
                                    itemSala = itemSala2;
                            }




                            DataRow meeting = null;

                            if (itemSala != null)
                            {
                                if (itemSala.zoom)
                                    meeting = GetMeetingDB(sessao.Sessao_versao_rowid);
                            }

                            if (itemSala != null)
                            {
                                string linkRegistrant = "";
                                //busco los links del alumno
                                if (meeting != null)
                                {

                                    linkRegistrant = getLinksRegistrant(long.Parse(meeting[2].ToString()), sessao.CodFormador2);
                                }
                                if (linkRegistrant != null && linkRegistrant != "")
                                {
                                    string body = (sexo == "F" ? "Exma. Senhora" : "Exmo. Senhor ") + " " +
                                                 (sexo == "F" ? "Professora " : "Professor ") + nomeformador + "<br/><br/>" +
                                                 "Estimamos que se encontre bem.<br/>" +
                                                 "Serve o presente e-mail para relembrar o acesso à sala virtual da sessão de formação do módulo <b>" +
                                                 sessao.Modulo + " </b> das <b>" + horamodulo + " </b> (horário de Portugal Continental).<br/>" +
                                               "Para aceder à sala virtual, deverá aceder ao " + "<a href='http://criapva.com/?id=" + itemSala.id +
                                                 "&formador=" + Uri.EscapeDataString(nomeformador) +
                                                 "&ref=" + sessao.RefAccao +
                                                 "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) +
                                                 "&linkRegistrant=" + Uri.EscapeDataString(linkRegistrant) + "'>" + "Link de acesso</a>" + " " + "e clicar no botão <b>“Entrar no Zoom”</b>.<br/><br/>" +

                                               "Aproveitamos para relembrar que, sempre que possível, o acesso à sala virtual deverá ser feito com 30 minutos de antecedência para teste de som e imagem.<br/><br/>" +
                                               "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso, através do endereço de e-mail " +
                                               "<a href='mailto:" + emailtxtcoordenador + "'>" + emailtxtcoordenador + "</a> e do contacto telefónico <strong> " + telemovelCoordenacao + "  </strong><br/><br/>";


                                    mm.Body = body;
                                    client.Send(mm);
                                    mm.Dispose();

                                    richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + nomeformador + " | " + email1 + " | enviado email no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo + "\n";

                                    //Envia sms
                                    recipientWithName newSms = new recipientWithName();
                                    Obj_logSend logSender = new Obj_logSend();

                                    string numValidate = null;
                                    if (telefone != null)
                                    {
                                        if (!teste)
                                            numValidate = ValidarNum(telefone);
                                        else
                                            numValidate = ValidarNum("912648984");
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
                                            string bodysms = sessao.RefAccao.ToString() + " // aula: " + DateTime.Now.ToShortDateString() + "\n\n" +
                                                 (sexo == "F" ? "Exma. Senhora " : "Exmo. Senhor ") +
                                                 (sexo == "F" ? "Professora " : "Professor ") + nomeformador + ",\n" +
                                                 "Estimamos que se encontre bem.\n\n" +
                                                 "Serve a presente SMS para relembrar o acesso à sala virtual da sessão de formação do modulo " +
                                                 sessao.Modulo + " das " + horamodulo + " (horário de Portugal Continental).\n\n" +
                                                  "Para aceder à sala virtual, deverá aceder ao link e clicar no botão “Entrar no Zoom”:\n" +
                                                 "http://criapva.com/?id=" + itemSala.id +
                                                 "&formador=" + Uri.EscapeDataString(nomeformador) +
                                                 "&ref=" + sessao.RefAccao +
                                                 "&hsessao=" + Uri.EscapeDataString(sessao.HoraInicio.ToString("yyyy-MM-dd HH:mm")) +
                                                 "&linkRegistrant=" + Uri.EscapeDataString(linkRegistrant) + "\n\n" +

                                                 "Aproveitamos para relembrar que, sempre que possível, o acesso à sala virtual deverá ser feito com 30 minutos de antecedência para teste de som e imagem.\n\n" +
                                                 "Para qualquer questão adicional, estarei à sua inteira disposição, enquanto Coordenadora de Curso, através do endereço de e-mail " +
                                                  emailtxtcoordenador + " e do contacto telefónico " + telemovelCoordenacao + "\n\n";


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
                                            //if (!teste)
                                            var res = smsByMailSEIService.sendShortScheduledMessage(credenciais, smsS.ToArray()).resultMessage.ToString();
                                            
                                            richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + nomeformador + " | " + telefone + (res.ToUpper().Contains("OK")?" | enviado sms no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo: "| sms NÃO ENVIADO") + "\n\n";
                                        }
                                    }
                                }
                                else
                                    richTextBox1.Text += (sexo == "F" ? "Professora " : "Professor ") + sessao.Formador + " | " + email1 + " | NÃO FOI NOTIFICADO, NÃO TEM REGISTO NA MEETING , ALTERAÇÃO DE FORMADOR " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + " | " + sessao.RefAccao + " | " + horamodulo + "\n\n";


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
        private void Button1_Click(object sender, EventArgs e)
        {
            //string msg2 = Uri.EscapeDataString("Hello 2");
            //ProcessStartInfo p1 = new ProcessStartInfo("https://ead.institutocriap.com/Zoomisoft/enviarmsg.php?text=Olá&phone=170123456789", "");
            //Process.Start(p1);
        }
    }
}
class Result
{
    public DateTime HoraInicio, HoraFim;
    public string RefAccao, Formador, CodFormador2, Modulo, CodFormador;
    public int CodSala;
    public string Sessao_versao_rowid;
}
class Obj_logSend
{
    public string ColaboradorID { get; set; }
    public string Mensagem { get; set; }
    public string Nome { get; set; }
    public string Numero { get; set; }
    public List<recipientWithName> smsSenders = new List<recipientWithName>();
}