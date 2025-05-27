namespace ApiTarefas.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Tarefas
{
    [JsonIgnore]
    public int Id { get; set; }
    [Required(ErrorMessage = "Campo Obrigatório", AllowEmptyStrings = false)]
    public string? Descricao { get; set; }

    [Range(0, 1, ErrorMessage = "Status deve ser 0 (Pendente) ou 1 (Concluído)")]
    public int Status { get; set; }
    public int UsuarioId { get; set; }

    public virtual Usuario? Usuario {get; set;}
}