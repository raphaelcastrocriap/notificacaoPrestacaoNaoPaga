using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace criapQuerys
{
    public class Query
    {
        public class secretariaVirtual
        {
            public static string moodleUpdateRoleAssignments()
            {
                return "UPDATE role_assignments AS ra LEFT JOIN user_enrolments AS ue ON ra.userid = ue.userid " + "LEFT JOIN role AS r ON ra.roleid = r.id LEFT JOIN context AS c ON c.id = ra.contextid " + "LEFT JOIN enrol AS e ON e.courseid = c.instanceid AND ue.enrolid = e.id SET ue.timeend = ";
            }
        }

        public class webSite2017Middleware
        {
            public static string getAccoes()
            {
                return "SELECT TBForAccoes.Ref_Accao, TBForAccoes.Codigo_Curso, TBForAccoes.Numero_Accao, TBForAccoes.Data_Inicio, TBForAccoes.Data_Fim, TBForAccoes.Valor_Accao, TBForAccoes.Valor_1Prest_Accao, TBForAccoes.Codigo_Estado, " + "TBForAccoes.versao_rowid, TBForCandAccoes.versao_rowid AS cand_versao_rowid, TBForCandAccoes.Codigo_Local, (SELECT Max(v) FROM(VALUES(TBForAccoes.versao_data), (TBForCandAccoes.versao_data), (estadosWeb.versao_data), (TfaseCandidatura.versao_data), (SfaseCandidatura.versao_data), (Pfasecandidatura.versao_data)) AS value(v)) as versao_data, TBForAccoes.Cod_Tecn_Resp, PotalAluno.id, estadosWeb.estadoWeb,  " + "CONVERT(int, TBForAccoes.Valor_1Prest_Especial) AS Mensalidades, TfaseCandidatura.tfasedata, ISNULL(totalFormandos.totalFormandos, 0) AS nFormandos, ISNULL(totalFormandos.Tipo_Curso, 0) AS tipoCurso, " + "SfaseCandidatura.sfasedata, Pfasecandidatura.pfasedata, TBForAccoes.Valor_Especial FROM TBForCandAccoes INNER JOIN TBForAccoes ON TBForCandAccoes.Numero_Cand = TBForAccoes.Numero_Projecto AND TBForCandAccoes.Codigo_Curso = TBForAccoes.Codigo_Curso AND " + "TBForCandAccoes.Numero_Accao = TBForAccoes.Numero_Accao INNER JOIN (SELECT ISNULL(a.Numero_Projecto, ca.Numero_Cand) AS can_number, ISNULL(a.Codigo_Curso, ca.Codigo_Curso) AS course_code, ISNULL(a.Numero_Accao, ca.Numero_Accao) AS action_number, " + "ca.versao_rowid AS id FROM TBForCandAccoes AS ca LEFT OUTER JOIN TBPrjProjectos AS prj ON prj.Origem = 'C' AND prj.Numero_Origem = ca.Numero_Cand LEFT OUTER JOIN " + "TBForAccoes AS a ON a.Numero_Projecto = prj.Numero_Projecto AND a.Codigo_Curso = ca.Codigo_Curso AND a.Numero_Accao = ca.Numero_Accao AND ISNULL(a.Estado, '') <> 'N') AS PotalAluno ON " + "TBForAccoes.Numero_Projecto = PotalAluno.can_number AND TBForAccoes.Codigo_Curso = PotalAluno.course_code AND TBForAccoes.Numero_Accao = PotalAluno.action_number LEFT OUTER JOIN " + "(SELECT rowid_ecran, valor AS pfasedata, versao_data FROM TBGerCUValores AS TBGerCUValores_1 WHERE(nome_campo = 'Pfasecandidatura')) AS Pfasecandidatura ON TBForAccoes.versao_rowid = Pfasecandidatura.rowid_ecran LEFT OUTER JOIN " + "(SELECT rowid_ecran, valor AS sfasedata, versao_data FROM TBGerCUValores AS TBGerCUValores_1 WHERE(nome_campo = 'Sfasecandidatura')) AS SfaseCandidatura ON TBForAccoes.versao_rowid = SfaseCandidatura.rowid_ecran LEFT OUTER JOIN " + "(SELECT TBForAccoes_1.versao_rowid, COUNT(TBForAccoes_1.versao_rowid) AS totalFormandos, TBForCursos.Tipo_Curso FROM TBForAccoes AS TBForAccoes_1 INNER JOIN TBForInscricoes ON TBForAccoes_1.versao_rowid = TBForInscricoes.Rowid_Accao INNER JOIN " + "TBForCursos ON TBForAccoes_1.Codigo_Curso = TBForCursos.Codigo_Curso WHERE(TBForInscricoes.Confirmado = 1) GROUP BY TBForAccoes_1.versao_rowid, TBForAccoes_1.Ref_Accao, TBForCursos.Tipo_Curso) AS totalFormandos ON TBForAccoes.versao_rowid = totalFormandos.versao_rowid LEFT OUTER JOIN " + "(SELECT rowid_ecran, valor AS tfasedata, versao_data FROM TBGerCUValores AS TBGerCUValores_1 WHERE(nome_campo = 'Tfasecandidatura')) AS TfaseCandidatura ON TBForAccoes.versao_rowid = TfaseCandidatura.rowid_ecran LEFT OUTER JOIN " + "(SELECT rowid_ecran, valor AS estadoWeb, versao_data FROM TBGerCUValores WHERE(nome_campo = 'estadoweb')) AS estadosWeb ON TBForAccoes.versao_rowid = estadosWeb.rowid_ecran WHERE( ";
            }

            public static string getCursos()
            {
                return "SELECT TBForCursos.Codigo_Curso, TBForCursos.Codigo_Area_Web, TBForCursos.Tipo_Curso, TBForCursos.Descricao, TBForCursos.Duracao_Defeito, TBForCursos.Obs, TBForCursos.Enquadramento, TBForCursos.Destinatarios, TBForCursos.Conteudos_Prog, TBForCursos.Metodologias, TBForCursos.Objs_especificos, " + "(SELECT Max(v) FROM(VALUES(TBForCursos.versao_data), (SaidasProf.versao_data), (Vantagens.versao_data), (Sincronas.versao_data), (Assincronas.versao_data), (CertificacaoOPP.versao_data)) AS value(v)) as versao_data, SaidasProf.SaidasProf, TBForCursos.Data_Criacao, TBForCursos.Material_Apoio, Vantagens.Vatagens, " + "ISNULL(Assincronas.hAssincronas, 0) AS hAssincronas, ISNULL(Sincronas.hSincronas, 0) AS hSincronas, CertificacaoOPP.Certificacao FROM TBForCursos LEFT OUTER JOIN " + "(SELECT nome_campo, rowid_ecran, valor AS Certificacao, versao_data FROM TBGerCUValores AS TBGerCUValores_1 WHERE (nome_campo = 'z10_CertificacaoOPP')) AS CertificacaoOPP ON TBForCursos.versao_rowid = CertificacaoOPP.rowid_ecran LEFT OUTER JOIN " + "(SELECT nome_campo, rowid_ecran, valor AS hSincronas, versao_data FROM TBGerCUValores AS TBGerCUValores_1 WHERE(nome_campo = 'z_HorasSincronas')) AS Sincronas ON TBForCursos.versao_rowid = Sincronas.rowid_ecran LEFT OUTER JOIN " + "(SELECT nome_campo, rowid_ecran, valor AS hAssincronas, versao_data FROM TBGerCUValores AS TBGerCUValores_1 WHERE(nome_campo = 'z_HorasAssincronas')) AS Assincronas ON TBForCursos.versao_rowid = Assincronas.rowid_ecran LEFT OUTER JOIN " + "(SELECT nome_campo, rowid_ecran, valor AS Vatagens, versao_data FROM TBGerCUValores AS TBGerCUValores_1 WHERE(nome_campo = 'Vantagens')) AS Vantagens ON TBForCursos.versao_rowid = Vantagens.rowid_ecran LEFT OUTER JOIN " + "(SELECT nome_campo, rowid_ecran, valor AS SaidasProf, versao_data FROM TBGerCUValores AS TBGerCUValores_1 WHERE(nome_campo = 'Saida_prof')) AS SaidasProf ON TBForCursos.versao_rowid = SaidasProf.rowid_ecran WHERE(TBForCursos.Pub_Web = 1) AND (TBForCursos.Tipo_Curso IS NOT NULL) ";
            }

            public static string getTestemunhos()
            {
                return "SELECT TBGerCUValores.versao_rowid, TBForAccoes.Codigo_Curso, Inscricoes.Codigo_Formando, " + "ISNULL(anonimato.valor, 0) AS Anonimo, TBGerCUValores.valor, (SELECT Max(v) FROM(VALUES(TBGerCUValores.versao_data), (anonimato.versao_data), (Subsequente.versao_data)) AS value(v)) as versao_data, Subsequente.valor AS Subsequente " + "FROM TBGerCUValores INNER JOIN(SELECT Codigo_Formando, Rowid_Accao, versao_rowid FROM TBForInscricoes) AS Inscricoes ON TBGerCUValores.rowid_ecran = Inscricoes.versao_rowid INNER JOIN " + "TBForAccoes ON Inscricoes.Rowid_Accao = TBForAccoes.versao_rowid INNER JOIN TBForCursos ON TBForAccoes.Codigo_Curso = TBForCursos.Codigo_Curso LEFT OUTER JOIN " + "(SELECT rowid_ecran, valor, versao_data FROM TBGerCUValores AS TBGerCUValores_2 WHERE(nome_campo = 'z11_CodCursoSubsequente') AND(id_ecran = 3)) AS Subsequente ON TBForCursos.versao_rowid = Subsequente.rowid_ecran LEFT OUTER JOIN " + "(SELECT TBForAccoes_1.Codigo_Curso, Inscricoes_1.Codigo_Formando, TBGerCUValores_1.valor, TBGerCUValores_1.versao_data FROM TBGerCUValores AS TBGerCUValores_1 INNER JOIN " + "(SELECT Codigo_Formando, Rowid_Accao, versao_rowid FROM TBForInscricoes AS TBForInscricoes_1) AS Inscricoes_1 ON TBGerCUValores_1.rowid_ecran = Inscricoes_1.versao_rowid INNER JOIN " + "TBForAccoes AS TBForAccoes_1 ON Inscricoes_1.Rowid_Accao = TBForAccoes_1.versao_rowid WHERE(TBGerCUValores_1.id_ecran = 6) AND(TBGerCUValores_1.nome_campo = 'testemunhoAnonimo')) AS anonimato ON Inscricoes.Codigo_Formando = anonimato.Codigo_Formando AND " + "TBForAccoes.Codigo_Curso = anonimato.Codigo_Curso WHERE(TBGerCUValores.id_ecran = 6) AND(TBGerCUValores.nome_campo = 'testemunho') AND((TBForCursos.Pub_Web = 1) OR(Subsequente.valor is not null)) ";
            }
        }

        public class CRIAP_appDv
        {
            public static string getFirstData()
            {
                return "SELECT Accoes.idPortalAluno " + ", Accoes.codCurso " + ", Accoes.rowid_Accao " + ", Accoes.refAccao " + ", Cursos.tituloHT " + ", Accoes.numeroAccao " + ", Urls.url " + ", Locais.local " + ", AreasCurso.descricao " + ", TiposCurso.descricao " + ", COALESCE(Accoes.idEstadoWeb, 0) " + ", Accoes.dataInicio " + ", Accoes.dataFim " + "FROM Accoes " + "LEFT JOIN Cursos " + "ON(Accoes.codCurso = Cursos.codCurso) " + "LEFT JOIN Locais " + "ON(Accoes.idLocal = Locais.idLocal) " + "LEFT JOIN Urls " + "ON(Cursos.idUrl = Urls.idUrl) " + "LEFT JOIN AreasCurso " + "ON(Cursos.idArea = AreasCurso.idArea) " + "LEFT JOIN TiposCurso " + "ON(Cursos.idTipo = TiposCurso.idTipo) " + "order by Accoes.dataInicio";
            }

            public static string getLocalHT()
            {
                return "SELECT TBForAccoes.versao_rowid, TBForAccoes.Ref_Accao, TBForAccoes.Codigo_Curso, TBForAccoes.Numero_Accao, TBForAccoes.Data_Inicio, TBForAccoes.Data_Fim, TBForLocais.Local, TBForAreas.Descricao, " + "TbForTiposCurso.Descricao AS Expr1, TBForCursos.Data_Criacao, TBForCursos.versao_data, ISNULL(contaFormandos.totalFormandos, 0) AS TotalFormandos " + "FROM TBForAreas INNER JOIN TBForAccoes INNER JOIN TBForCursos ON TBForAccoes.Codigo_Curso = TBForCursos.Codigo_Curso ON TBForAreas.Codigo_Area = TBForCursos.Codigo_Area INNER JOIN " + "TbForTiposCurso ON TBForCursos.Tipo_Curso = TbForTiposCurso.Tipo INNER JOIN TBForCandAccoes INNER JOIN " + "TBForLocais ON TBForCandAccoes.Codigo_Local = TBForLocais.Codigo_Local ON TBForAccoes.Codigo_Curso = TBForCandAccoes.Codigo_Curso AND  " + "TBForAccoes.Numero_Accao = TBForCandAccoes.Numero_Accao LEFT OUTER JOIN (SELECT TBForAccoes_1.Ref_Accao, COUNT(TBForAccoes_1.versao_rowid) AS totalFormandos, TBForCursos_1.Codigo_Curso " + "FROM TBForAccoes AS TBForAccoes_1 INNER JOIN TBForInscricoes ON TBForAccoes_1.versao_rowid = TBForInscricoes.Rowid_Accao INNER JOIN " + "TBForCursos AS TBForCursos_1 ON TBForAccoes_1.Codigo_Curso = TBForCursos_1.Codigo_Curso " + "WHERE (TBForInscricoes.Confirmado = 1) GROUP BY TBForAccoes_1.versao_rowid, TBForAccoes_1.Ref_Accao, TBForCursos_1.Codigo_Curso) AS contaFormandos ON TBForAccoes.Codigo_Curso = contaFormandos.Codigo_Curso AND TBForAccoes.Ref_Accao = contaFormandos.Ref_Accao " + "WHERE (";
            }
        }

        public class gestaoSalas
        {
            public static string getHTData()
            {
                return "SELECT TBForAccoes.Numero_Projecto, TBForAccoes.Codigo_Curso, TBForAccoes.Ref_Accao, TBSysUsers.Nome AS Tecnica, CASE TBForAccoes.Codigo_Estado WHEN 1 THEN 'Confirmado' WHEN 4 THEN 'Em análise' END AS Estado, " + "TBForSessoes.Hora_Inicio, TBForSessoes.Hora_Fim, TBForModulos.Descricao, TBForSessoes.SC + TBForSessoes.CT + TBForSessoes.PS + TBForSessoes.PCT AS TotalHoras, TBForFormadores.Nome_Abreviado, " + "ISNULL(totalFormandos.contagem, 0) AS TotalFormandos, TBForLocais.Local, Moradas.Distrito, TBForAccoes.Data_Inicio, TBForAccoes.Data_Fim, TBForSessoes.versao_rowid AS ID, " + "TBForCursos.Descricao AS NomeCurso, Codigo_Formador2, TBForSessoes.Rowid_Accao, TBForAccoes.Cod_Tecn_Assist, TBForSessoes.versao_rowid, TBForAccoes.Cod_Tecn_Resp, TBForFormadores.Codigo_formador FROM TBForModulos FULL OUTER JOIN TBSysUsers RIGHT OUTER JOIN TBForCursos INNER JOIN " + "TBForAccoes ON TBForCursos.Codigo_Curso = TBForAccoes.Codigo_Curso LEFT OUTER JOIN TBForLocais INNER JOIN " + "TBForCandAccoes ON TBForLocais.Codigo_Local = TBForCandAccoes.Codigo_Local ON TBForAccoes.Numero_Projecto = TBForCandAccoes.Numero_Cand AND " + "TBForAccoes.Numero_Accao = TBForCandAccoes.Numero_Accao AND TBForAccoes.Codigo_Curso = TBForCandAccoes.Codigo_Curso ON TBSysUsers.Login = TBForAccoes.Cod_Tecn_Resp FULL OUTER JOIN " + "(SELECT COUNT(Codigo_Formando) AS contagem, Rowid_Accao FROM TBForInscricoes WHERE(Confirmado = 1) GROUP BY Rowid_Accao) AS totalFormandos RIGHT OUTER JOIN " + "(SELECT TBGerMoradas.Tipo_Entidade, TBGerMoradas.Codigo_Entidade, TBGerMoradas.Codigo_Pais, TBGerDistritos.Distrito FROM TBGerMoradas LEFT OUTER JOIN TBGerDistritos ON TBGerMoradas.Cod_Distrito = TBGerDistritos.Cod_Distrito " + "WHERE(TBGerMoradas.Tipo_Entidade = 4)) AS Moradas RIGHT OUTER JOIN TBForFormadores INNER JOIN " + "TBForSessoes ON TBForFormadores.Codigo_Formador = TBForSessoes.Codigo_Formador ON Moradas.Codigo_Entidade = TBForFormadores.versao_rowid ON totalFormandos.Rowid_Accao = TBForSessoes.Rowid_Accao ON " + "TBForAccoes.versao_rowid = TBForSessoes.Rowid_Accao ON TBForModulos.versao_rowid = TBForSessoes.Rowid_Modulo ";
            }
        }

        public class pendentesNotification
        {
            public static string getPendentes()
            {
                return "SELECT matriculations.created_at AS DataInscricao, users.name AS Nome, entities.phone1 AS Telefone1, entities.phone2 AS Telefone2, entities.email1 AS Email, " + "matriculations.matriculation_mb_ref as RefPagamento, " + "(case when(courses.course_type IN (1, 11)) " + "THEN courseactions.courseaction_1render ELSE " + "CASE " + "WHEN(matriculations.discount_code2 is not null) THEN courseactions.courseaction_coust - (courseactions.courseaction_coust * (SELECT discount FROM discounts where discount_code = matriculations.discount_code2)) " + "WHEN(matriculations.discount_code is not null) THEN courseactions.courseaction_coust - (courseactions.courseaction_coust * (SELECT discount FROM discounts where discount_code = matriculations.discount_code)) " + "ELSE courseactions.courseaction_coust " + "END END) AS Valor, " + "courses.course_code AS CodCurso, courses.course AS Curso, courseactions.courseaction_ref AS RefAccao, " + "courseactions.courseaction_start AS Inicio, courseactions.courseaction_end AS Fim, matriculations.id " + "FROM criapco_ensino.matriculations INNER JOIN criapco_ensino.users ON (matriculations.user_id = users.id) " + "INNER JOIN criapco_ensino.courseactions ON(matriculations.courseaction_id = courseactions.courseaction_id) " + "INNER JOIN criapco_ensino.entities ON(entities.user_id = users.id) " + "INNER JOIN criapco_ensino.courses ON(courseactions.course_code = courses.course_code) ";
            }
        }

        public class formadoresNotification
        {
            public static string getModulosFormadores(DateTime data)
            {
                return "SELECT DISTINCT TBForFormadores.Codigo_Formador, TBForFormadores.Nome_Abreviado, TBGerContactos.Email1, TBGerContactos.Telefone1, TBForFormadores.Sexo, TBForSessoes.Hora_Inicio, TBForSessoes.Hora_Fim, TBForCursos.Descricao, TBForLocais.Local, " + "TBForModulos.Descricao AS Módulo FROM TBForSessoes INNER JOIN " + "TBForAccoes ON TBForSessoes.Rowid_Accao = TBForAccoes.versao_rowid INNER JOIN " + "TBForFormadores ON TBForSessoes.Codigo_Formador = TBForFormadores.Codigo_Formador INNER JOIN " + "TBGerContactos ON TBForFormadores.versao_rowid = TBGerContactos.Codigo_Entidade INNER JOIN " + "TBForCandAccoes ON TBForAccoes.Numero_Projecto = TBForCandAccoes.Numero_Cand AND TBForAccoes.Codigo_Curso = TBForCandAccoes.Codigo_Curso AND " + "TBForAccoes.Numero_Accao = TBForCandAccoes.Numero_Accao INNER JOIN " + "TBForCursos ON TBForAccoes.Codigo_Curso = TBForCursos.Codigo_Curso INNER JOIN " + "TBForLocais ON TBForCandAccoes.Codigo_Local = TBForLocais.Codigo_Local INNER JOIN " + "TBForModulos ON TBForSessoes.Rowid_Modulo = TBForModulos.versao_rowid " + "WHERE TBForAccoes.Codigo_Estado = 1 AND (TBForSessoes.Data = '" + data.ToString("yyyy-MM-dd") + "') AND(TBGerContactos.Tipo_Entidade = 4) AND(TBForFormadores.Codigo_Formador NOT IN(699, 704, 827, 1046))" + "GROUP BY TBForFormadores.Codigo_Formador, TBForFormadores.Nome_Abreviado, TBGerContactos.Email1, TBGerContactos.Telefone1, TBForFormadores.Sexo, TBForSessoes.Hora_Inicio, TBForSessoes.Hora_Fim, TBForCursos.Descricao, TBForLocais.Local, TBForModulos.Descricao " + "ORDER BY Hora_Inicio";
            }

            public static string getModulosFormadoresALL()
            {
                return "SELECT DISTINCT TBForFormadores.Codigo_Formador, TBForFormadores.Nome_Abreviado, TBGerContactos.Email1, TBGerContactos.Telefone1, TBForFormadores.Sexo FROM TBForFormadores INNER JOIN TBGerContactos ON TBForFormadores.versao_rowid = TBGerContactos.Codigo_Entidade WHERE (TBGerContactos.Tipo_Entidade = 4) AND(TBForFormadores.Codigo_Formador NOT IN(699, 704, 827, 1046)) GROUP BY TBForFormadores.Codigo_Formador, TBForFormadores.Nome_Abreviado, TBGerContactos.Email1, TBGerContactos.Telefone1, TBForFormadores.Sexo";
            }
        }

        public class agendasIndesign
        {
            public static string getData()
            {
                return "SELECT Cursos.codCurso " + ", Accoes.refAccao " + ", Cursos.tituloHT " + ", Cursos.nomeWeb1 " + ", Cursos.nomeWeb2 " + ", Cursos.nomeWeb3 " + ", Accoes.dataInicio " + ", Accoes.dataFim " + ", Cursos.duracao " + ", Accoes.numeroAccao " + ", Locais.local " + ", TiposCurso.descricao " + ", AccoesEstadosWeb.descricao " + ", Accoes.preco " + ", Accoes.prestacao " + "FROM " + "Accoes " + "INNER JOIN Cursos " + "ON(Accoes.codCurso = Cursos.codCurso) " + "INNER JOIN Locais " + "ON(Accoes.idLocal = Locais.idLocal) " + "INNER JOIN TiposCurso " + "ON(Cursos.idTipo = TiposCurso.idTipo) " + "INNER JOIN AccoesEstadosWeb " + "ON (Accoes.idEstadoWeb = AccoesEstadosWeb.idEstadoWeb) order by Accoes.dataInicio";
            }
        }

        public static List<DataRow> getFormadores(SqlConnection connection)
        {
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("SELECT TBForFormadores.Codigo_Formador, TBForFormadores.Nome_Abreviado FROM TBForFormadores", connection);
            DataTable dataTable = new DataTable();
            sqlDataAdapter.Fill(dataTable);
            List<DataRow> list = dataTable.AsEnumerable().ToList<DataRow>();
            connection.Close();
            return list;
        }

    }
}
