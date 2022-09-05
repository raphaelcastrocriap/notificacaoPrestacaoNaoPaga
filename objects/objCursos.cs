using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalasZoomNotificationFormadores.objects
{
    class objCursos
    {
        public string InicioFim { get; set; }
        public string Codigo_Projeto { get; set; }
        public string Codigo_Curso { get; set; }
        public string NomeCurso { get; set; }
        public string Ref_Accao { get; set; }
        public string Tecnica { get; set; }
        public string Estado { get; set; }
        public DateTime Hora_Inicio { get; set; }
        public DateTime Hora_Fim { get; set; }
        public string versao_rowid { get; set; }
        public string Modulo { get; set; }
        public decimal TotalHoras { get; set; }
        public string Formador { get; set; }
        public string OrigemFormador { get; set; }
        public int TotalFormandos { get; set; }
        public string Local { get; set; }
        public DateTime Inicio_Curso { get; set; }
        public DateTime Fim_Curso { get; set; }
        public int cod_Alojamento { get; set; }
        public double preco_Alojamento { get; set; }
        public int cod_Transporte { get; set; }
        public double preco_Transporte { get; set; }
        public int cod_Sala { get; set; }
        public double preco_Sala { get; set; }
        public string notas { get; set; }
        public int FormacaoExternaID { get; set; }
        public string CodFormador2 { get; set; }
        public string CoFormador { get; set; }
        public int Rowid_Accao { get; set; }
        public string TM { get; set; }
        public string TSV { get; set; }
        public string FormadorDistribuirLinks { get; set; }
        public string CodCordenador { get; set; }
        public string CodFormador { get; set; }
    }

    public class objFormadores
    {
        public string formadorID { get; set; }
        public string NomeFormador { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Sexo { get; set; }
    }

    public class objColaboradores
    {
        public string codigo_Colaborador { get; set; }

        public string nome { get; set; }

        public bool gestaosalas { get; set; }

        public string email { get; set; }

        public string nomeemail { get; set; }

        public string sigla { get; set; }
    }

    class objSinc
    {
        public string Codigo_Curso { get; set; }
        public string Ref_Accao { get; set; }
        public string Codigo_Aluno { get; set; }
        public string Codigo_AlunoMoodle { get; set; }
        public string Nome { get; set; }
        public string NC { get; set; }
        public string Email { get; set; }
        public string Link { get; set; }
        public string desistente { get; set; }
        public string bloqueado { get; set; }
        public string ContratoFalta { get; set; }
        public int desistenteMoodle { get; set; }
        public int bloqueadoMoodle { get; set; }
        public int ContratoFaltaMoodle { get; set; }
        public int DCCFaltaMoodle { get; set; }
        public int CHFaltaMoodle { get; set; }
        public int CMFaltaMoodle { get; set; }
        public DateTime DataHora { get; set; }
    }

    class objFormandos
    {
        public string Formando { get; set; }
        public string Contribuinte { get; set; }
        public string Email { get; set; }
        public string Link { get; set; }
        public string Desistente { get; set; }
        public string Bloqueado { get; set; }
        public string Contrato { get; set; }
        public string DCC { get; set; }
        public string CH { get; set; }
        public string CM { get; set; }
        public int ID { get; set; }
        public int Rowid_Accao { get; set; }
        public string versao_rowid { get; set; }
    }
}