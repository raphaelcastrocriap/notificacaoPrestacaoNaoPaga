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
using System.Threading;
using System.IO;
using RestSharp;
using Newtonsoft.Json;
using System.Globalization;
using System.Transactions;



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
        DateTime dateNow;
        string mensagemInfo = "";

        public Form1()
        {
            InitializeComponent();

        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
           
            teste = true;

            if (!teste)
                dateNow = DateTime.Now;
            else
                dateNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 8);

            Security.remote();
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Text += " V." + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString();

            string dt = @"Controlo de versão: " + " V." + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString() + " Assembly built date: " + System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location) + " by rc";
            mensagemInfo += " \r\n" + dt;
            string[] passedInArgs = Environment.GetCommandLineArgs();

            // Certifica que é dia 9 do mes
            if (DateTime.Now.Day == 9) 
            {
                if (passedInArgs.Contains("-a") || passedInArgs.Contains("-A"))
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ExecuteSync();
                    Cursor.Current = Cursors.Default;
                    System.Windows.Forms.Application.Exit();
                }
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
                Get_Formandos();
                
                SendEmail(richTextBox1.Text, "Notification Pendências // Formadores - Emails enviados", "informatica@criap.com");
            }
            catch (Exception e)
            {
                error = true;
                TimeSpan delay = new TimeSpan(0, 5, 0);
                if (e.Message.Contains("Foi estabelecida ligação com êxito ao servidor, mas, em seguida, ocorreu um erro durante o handshake anterior ao início de sessão"))
                {
                    SendEmail(richTextBox1.Text + "\n\n" + e.ToString() + "\n" + "IMPORTANTE: Em 5 min será executada a rotina outra vez...", "Notificação Pendência de Pagamento // Formadores - HANDSHAKE ERROR", "informatica@criap.com");

                    Thread.Sleep(delay);
                    ExecuteSync();
                }
                else if (e.Message.Contains("Não foi possível criar um canal seguro SSL/TLS"))
                {
                    SendEmail(richTextBox1.Text + "\n\n" + e.ToString() + "\n" + "IMPORTANTE: Em 5 min será executada a rotina outra vez...", "Notificação Pendência de Pagamento // Formadores - canal seguro SSL/TLS", "informatica@criap.com");

                    Thread.Sleep(delay);
                    ExecuteSync();
                }
                else {
                    SendEmail(richTextBox1.Text + "\n\n" + e.ToString(), "Notificação Pendência de Pagamento // Formadores - ERROR ", "informatica@criap.com");
                }

            }
        }
        
        public void Get_Formandos()
        {
            richTextBox1.Text = richTextBox1.Text + "\nFORMANDOS:\n<br>";
            try
            {
                db.listaFormandos.Clear();

                string subQuery = $@"DECLARE @DATAHOJE DATE;
                            SET @DATAHOJE = '{dateNow:yyyy-MM-dd}';
                            SELECT *,
                            CASE WHEN ta.Codigo_Estado = 1 THEN 'Confirmado' WHEN ta.Codigo_Estado = 4 THEN 'Em análise' END AS Estado, 
                            tf.Formando AS 'Formando', tp.Valor_desconto AS 'Valor', tp.Descricao, tp.data_prestacao ,
                            (DATEDIFF(month,data_prestacao,@DATAHOJE)+1) * 25 as coima, tp.Valor_desconto+(DATEDIFF(month,data_prestacao,@DATAHOJE)+1) * 25 as valor_total
                            FROM TBForFinOrdensFaturacaoPlano tp 
                            INNER JOIN TBForFormandos tf ON tp.Rowid_entidade = tf.versao_rowid 
                            INNER JOIN TBForAccoes ta ON ta.versao_rowid = tp.Rowid_Opcao
                            INNER JOIN TBForInscricoes ti on tp.rowid_opcao=ti.Rowid_Accao and ti.Codigo_Formando=tf.Codigo_Formando
                            INNER JOIN TBForCursos c ON ta.Codigo_Curso = c.Codigo_Curso 
                            INNER JOIN TBGerContactos Cc ON tf.versao_rowid = Cc.Codigo_Entidade AND Cc.Tipo_Entidade=3 
                            WHERE tp.pago = 0 AND tp.data_prestacao between DATEADD(MONTH,-1,DATEADD(DAY,-1,@DATAHOJE))  and @DATAHOJE 
                            AND (ta.Codigo_Estado=1 or ta.Codigo_Estado=5)
                            AND tp.Descricao LIKE '%ª prestação%' 
                            AND CAST(SUBSTRING(tp.Descricao, 1, CHARINDEX('ª', tp.Descricao) - 1) AS INT)>2
                            AND ti.Confirmado=1 and ti.Desistente=0 AND tp.Codigo_Forma_Pagmto !='ADD' AND tp.Codigo_Forma_Pagmto !='TF'
                            ORDER BY tf.formando";

                dbConnect.ht2.ConnInit();
                SqlDataAdapter adapter = new SqlDataAdapter(subQuery, dbConnect.ht2.Conn);
                DataTable subData = new DataTable();
                adapter.Fill(subData);
                dbConnect.ht2.ConnEnd();

                if (subData.Rows.Count > 0)
                {
                    // Agrupamento por Codigo_Formando
                    var formandosAgrupados = subData.AsEnumerable()
                        .GroupBy(row => row["Codigo_Formando"].ToString())
                        .Select(grupo => new
                        {
                            Codigo_Formando = grupo.Key,
                            Nome = grupo.First()["Formando"].ToString(),
                            NC = grupo.First()["NC"].ToString(),
                            Telefone = grupo.First()["telefone1"].ToString(),
                            Nome_Abreviado = grupo.First()["Nome_Abreviado"].ToString(),
                            Email1 = grupo.First()["Email1"].ToString(),
                            Sexo = grupo.First()["Sexo"].ToString(),
                            RefAcao = grupo.First()["Ref_Accao"].ToString(),

                            // ParcelaDetalhes contém os valores das parcelas
                            ParcelaDetalhes = grupo.Select(row => new
                            {
                                Descricao = row["Descricao"].ToString(),
                                Ref_Accao = row["Ref_Accao"].ToString(),
                                DataPrestacao = Convert.ToDateTime(row["data_prestacao"]),
                                ValorParcela = Convert.ToDecimal(row["Valor_desconto"]),
                                ValorMulta = Convert.ToDecimal(row["coima"]),
                                ValorComMulta = Convert.ToDecimal(row["valor_total"])  // ja com multa
                            }).ToList(),
                            
                            // Valor total com multa calculado aqui em decimal
                            ValorTotalComMulta = grupo.Select(row => Convert.ToDecimal(row["valor_total"])).Sum()
                        }).ToList();

                    foreach (var formando in formandosAgrupados)
                    {
                        // Carregar o HTML base
                        string htmlTemplate = @"<!DOCTYPE html>
                                <html lang='pt-BR'>
                                <head>
                                    <meta charset='UTF-8'>
                                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                    <style>body {font-family: Arial, sans-serif; line-height: 1.3;} h1 {text-align: center; color: #003366;} table {border-collapse: collapse; margin: 20px 0; border: 1px solid #ddd;} table th {border: 1px solid #ddd; padding: 2px; text-align: center; background-color: #2F5496; color: white;} table td {border: 1px solid #ddd; padding: 5px; text-align: left;}</style>
                                </head>
                                <body>
                                    <p>{{nome}},</p>
                                    <p>De acordo com os nossos registos, verificamos a existência de mensalidades em atraso relativas ao seu plano de pagamentos.</p>
                                    <p>Segue a informação detalhada:</p>

                                    <table>
                                        <tr>
                                            <th>Curso</th>
                                            <th>Mensalidade</th>
                                            <th>Data de Vencimento</th>
                                            <th>Valor</th>
                                        </tr>
                                    {{tabela}}
                                    </table>

                                    <p>
                                        Neste sentido, e tendo em conta o exposto, solicitamos a regularização do(s) pagamento(s) em falta no prazo máximo de
                                        <strong>48 horas</strong>, de modo a evitar o bloqueio automático no acesso à ação de formação e respetivas plataformas colaborativas,
                                        conforme estipulado no <strong>ponto 1., Cláusula VIII do Contrato de Formação</strong>.
                                    </p>
                                    <p>
                                        Mais se informa que o bloqueio é um processo automático, impossibilitando o acesso às aulas, testes de avaliação e material de apoio ao autoestudo,
                                        pelo que a sua colaboração é essencial para minimizar qualquer tipo de transtorno.
                                    </p>

                                    <div>
                                        <table>
                                            <thead>
                                                <tr>
                                                    <th colspan='3'>Pagamento por Multibanco ou Homebanking</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr>
                                                    <td rowspan='3' style='text-align: center;'>
                                                          <img src='https://img.icons8.com/?size=100&id=HWbgAihjYdU_&format=png&color=000000'>
                                                    </td>
                                                    <td>ENTIDADE:</td>
                                                    <td><strong>{{entidade}}</strong></td>
                                                </tr>
                                                <tr>
                                                    <td>REFERÊNCIA:</td>
                                                    <td><strong>{{referencia}}</strong></td>
                                                </tr>
                                                <tr>
                                                    <td>VALOR:</td>
                                                    <td><strong>{{valor}}</strong></td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>

                                    <p>
                                        Se a situação apresentada, foi, entretanto, resolvida, agradecemos que nos envie os comprovativos de pagamento para validação.
                                    </p>

                                    <p>
                                        Em caso de esclarecimentos ou informações adicionais, estamos ao seu dispor através do e-mail:
                                        <a href='mailto:departamentojuridico@criap.com'>departamentojuridico@criap.com</a> ou através do contacto telefónico <strong>+351 225 492 190</strong>.
                                    </p>

                                    <p>Com os melhores cumprimentos,</p>
                                    <p><strong>Instituto CRIAP</strong></p>
                                </body>
                                </html>
                                ";

                        // Gerar as linhas da tabela
                        string linhasTabela = string.Join("", formando.ParcelaDetalhes.Select(parcela =>
                            $"<tr><td>{(string.IsNullOrEmpty(GetNomeCurso(parcela.Ref_Accao)) ? parcela.Ref_Accao : GetNomeCurso(parcela.Ref_Accao))}</td> " +
                            $"<td>({parcela.Descricao})</td>" +
                            $"<td>{parcela.DataPrestacao:dd-MM-yyyy}</td>" +
                            $"<td>{parcela.ValorParcela:F2}€ + Taxa administrativa: {parcela.ValorMulta:F2}€</td></tr>"));

                        DateTime expirationDate = DateTime.Now.AddDays(2);
                        ReferenceMB reference = GenerateReferenceAPI(formando.RefAcao, formando.NC, Math.Round(formando.ValorTotalComMulta, 2), expirationDate);

                        // Validar se a referência ou seus campos são nulos/vazios
                        if (reference?.Method == null || string.IsNullOrEmpty(reference.Method.Reference))
                        {
                            // Passar para o próximo formando caso seja inválido
                            richTextBox1.Text += $"Formando {formando.Nome} | {formando.Email1} | referência não gerada, ignorado.\n";
                            continue;
                        }

                        // Guarda na base de dados a referência gerada OrdemFaturacao e OrdemFaturacaoAcao 
                        if (dbConnect.secretariaVirtual.Conn.State != ConnectionState.Open)
                        {
                            dbConnect.secretariaVirtual.Conn.Open();
                        }
                        using (var transaction = dbConnect.secretariaVirtual.Conn.BeginTransaction())
                        {
                            try
                            {
                                string subQueryInsert = @"
                                    INSERT INTO [dbo].[OrdemFaturacao] 
                                    (Status, Type, Value, Entity, Reference, CreatedAt, ExpirationDate, CodigoFormando, NC) 
                                    OUTPUT INSERTED.Id
                                    VALUES 
                                    (@Status, @Type, @Value, @Entity, @Reference, @CreatedAt, @ExpirationDate, @CodigoFormando, @NC)";

                                int ordemFaturacaoId;
                                using (SqlCommand cmd = new SqlCommand(subQueryInsert, dbConnect.secretariaVirtual.Conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Status", reference.Method.Status);
                                    cmd.Parameters.AddWithValue("@Type", reference.Method.Type);
                                    cmd.Parameters.AddWithValue("@Value", Math.Round(formando.ValorTotalComMulta, 2));
                                    cmd.Parameters.AddWithValue("@Entity", reference.Method.Entity);
                                    cmd.Parameters.AddWithValue("@Reference", reference.Method.Reference);
                                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@ExpirationDate", expirationDate);
                                    cmd.Parameters.AddWithValue("@CodigoFormando", formando.Codigo_Formando);
                                    cmd.Parameters.AddWithValue("@NC", formando.NC);

                                    ordemFaturacaoId = (int)cmd.ExecuteScalar();
                                }

                                string insertAcoesQuery = @"
                                    INSERT INTO [dbo].[OrdemFaturacaoAcao] 
                                    (OrdemFaturacaoId, RefAcao, Descricao, ValorParcela, ValorComMulta, DataPrestacao) 
                                    VALUES 
                                    (@OrdemFaturacaoId, @RefAcao, @Descricao, @ValorParcela, @ValorComMulta, @DataPrestacao)";

                                using (SqlCommand cmd = new SqlCommand(insertAcoesQuery, dbConnect.secretariaVirtual.Conn, transaction))
                                {
                                    foreach (var parcela in formando.ParcelaDetalhes)
                                    {
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("@OrdemFaturacaoId", ordemFaturacaoId);
                                        cmd.Parameters.AddWithValue("@RefAcao", parcela.Ref_Accao);
                                        cmd.Parameters.AddWithValue("@Descricao", parcela.Descricao);
                                        cmd.Parameters.AddWithValue("@ValorParcela", parcela.ValorParcela);
                                        cmd.Parameters.AddWithValue("@ValorComMulta", parcela.ValorComMulta);
                                        cmd.Parameters.AddWithValue("@DataPrestacao", parcela.DataPrestacao);

                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                // Commit da transação
                                transaction.Commit();

                                // Log no RichTextBox
                                richTextBox1.Text += $"Ordem de faturação gerada para o formando {formando.Nome} com referência {reference.Method.Reference}.\n";
                            }
                            catch (Exception ex)
                            {
                                // Caso ocorra um erro, da rollback
                                transaction.Rollback();
                                richTextBox1.Text += $"Erro ao gerar ordem de faturação: {ex.Message}\n";
                            }
                        }


                        // Substituir placeholders no HTML
                        string emailBody = htmlTemplate
                            .Replace("{{nome}}", (formando.Sexo == "F") ? " Estimada formanda " + formando.Nome_Abreviado : " Estimado formando " + formando.Nome_Abreviado)
                            .Replace("{{entidade}}", reference.Method.Entity)
                            .Replace("{{referencia}}", reference.Method.Reference)
                            .Replace("{{valor}}", formando.ValorTotalComMulta.ToString("F2"))
                            .Replace("<tr><td>{{prestacao}}</td>", linhasTabela)
                            .Replace("{{tabela}}", linhasTabela);

                        // Enviar o e-mail
                        SendEmail(emailBody, "Notificação de Mensalidade Pendente", formando.Email1);

                        richTextBox1.Text += (formando.Sexo == "F" ? "Formanda " : "Formando ") + formando.Nome + " | " + formando.Email1 + " | enviado email no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") + "<br>";

                        // Envia sms
                        recipientWithName newSms = new recipientWithName();
                        Obj_logSend logSender = new Obj_logSend();

                        string numValidate = null;
                        if (formando.Telefone != null)
                        {
                            if (!teste)
                                numValidate = ValidarNum(formando.Telefone);
                            else
                                numValidate = ValidarNum("965787317");

                            if (numValidate != null)
                            {
                                newSms.msisdn = numValidate;
                                newSms.name = formando.Nome_Abreviado;
                            }
                        }

                        if (newSms.msisdn != "")
                        {
                            if (newSms.msisdn != null)
                            {
                                string bodysms = "" + ((formando.Sexo == "F") ? "Estimada formanda " + formando.Nome_Abreviado : "Estimado formando " + formando.Nome_Abreviado ) + "\r\n" +
                                    "Informamos que se encontra por liquidar uma mensalidade associada ao seu plano de pagamento, em conformidade com a comunicação enviada por e-mail.\r\n" +
                                    "Tendo em conta o exposto, aguarda-se a efetivação do pagamento por parte de " + ((formando.Sexo == "F") ? "V.ª Exa " + formando.Nome_Abreviado : "V.º Exo " + formando.Nome_Abreviado) + " , no prazo máximo de 48 horas.\r\n" +
                                    "Se a situação foi, entretanto, resolvida, agradecemos que nos envie os comprovativos de pagamento para email: tesouraria@criap.com.\r\n" +
                                    "Para mais informações, por favor consulte o seu email.\r\n" +
                                    "Desejamos uma excelente formação e uma boa continuação.\r\n" +
                                    "Departamento Jurídico,\r\n" +
                                    "Instituto CRIAP";

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

                                richTextBox1.Text += (formando.Sexo == "F" ? "Formanda " : "Formando ") + formando.Nome + " | " + formando.Telefone + (res.ToUpper().Contains("OK") ? " | enviado sms no dia " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToString("HH:mm") : "| sms NÃO ENVIADO") + "<br>";

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "<br><br>Erro: " + ex.Message + "\n";
            }
        }
        


        public static string GetNomeCurso(string refAcao)
        {
            string query = $"SELECT c.Descricao FROM TBForAccoes a INNER JOIN TBForCursos c ON a.Codigo_Curso = c.Codigo_Curso WHERE Ref_Accao = '{refAcao}'";

            using (SqlCommand command = new SqlCommand(query, dbConnect.ht2.Conn))
            {
                if (dbConnect.ht2.Conn.State == ConnectionState.Closed)
                {
                    dbConnect.ht2.Conn.Open();
                }

                object result = command.ExecuteScalar();

                return result?.ToString() ?? string.Empty;
            }
        }

        private string RetornaImgBase64(string caminhoImagem)
        {
            if (File.Exists(caminhoImagem))
            {
                byte[] imageBytes = File.ReadAllBytes(caminhoImagem);
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
            else
            {
                MessageBox.Show($"Arquivo não encontrado: {caminhoImagem}");
                return string.Empty;
            }
        }

        // Método para gerar a referência utilizando a API
        private static ReferenceMB GenerateReferenceAPI(string refAcao, string nif, decimal valor, DateTime expirationDate)
        {
            ReferenceMB reference = new ReferenceMB();

            try
            {
                var options = new RestClientOptions("https://server1.criap.com/api/easypay")
                {
                    ThrowOnAnyError = false,
                    MaxTimeout = 10000,
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };

                var client = new RestClient(options);
                var req = new RestRequest();

                req.RequestFormat = DataFormat.Json;
                req.AddHeader("Authorization", "Basic YWRtaW5BUEk6eFdlMjU1NWthZ3h4biF5WA=="); // Substitua pela chave real
                req.AddQueryParameter("refAcao", refAcao);
                req.AddQueryParameter("idNumber", nif);
                req.AddQueryParameter("valor", valor);
                req.AddQueryParameter("expiration_date", expirationDate.ToString("MM-dd-yyyy"));

                var resp = client.ExecuteGet(req);

                if (resp.IsSuccessStatusCode)
                {
                    string response = resp.Content;
                    reference = JsonConvert.DeserializeObject<ReferenceMB>(response);
                }
                else
                {
                    Console.WriteLine($"Erro ao chamar a API: {resp.StatusDescription}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar referência: {ex.Message}");
            }

            return reference;
        }

        
        private void SendEmail(string body, string assunto = "", string toEmail = "")
        {
            NetworkCredential basicCredential = new NetworkCredential(Properties.Settings.Default.emailenvio, Properties.Settings.Default.passwordemail);
            SmtpClient client = new SmtpClient
            {
                Port = 25,
                Host = "mail.criap.com",
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = basicCredential
            };

            MailMessage mm = new MailMessage
            {
                From = new MailAddress("Instituto CRIAP <" + Properties.Settings.Default.emailenvio + "> "),
                Subject = assunto + " // " + DateTime.Now.ToShortDateString() + " às " + DateTime.Now.ToShortTimeString(),
                IsBodyHtml = true, // Habilita suporte a HTML
                BodyEncoding = UTF8Encoding.UTF8,
                DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
            };

            if (!teste)
            {
                mm.To.Add(toEmail);
            }
            else
            {
                mm.To.Add("raphaelcastro@criap.com");
                mm.To.Add("rcastro.br@gmail.com");
            }

            // Ajuste o corpo para incluir o conteúdo HTML
            mm.Body = $"<html><body>{body}{(teste ? mensagemInfo : string.Empty)}</body></html>";

            try
            {
                client.Send(mm);
            }
            catch (Exception ex)
            {
                // Trate possíveis erros de envio aqui
                Console.WriteLine("Erro ao enviar e-mail: " + ex.Message);
            }
        }

        private void Button2_Click_1(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ExecuteSync();
            Cursor.Current = Cursors.Default;
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