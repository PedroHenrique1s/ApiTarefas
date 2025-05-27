namespace ApiTarefas.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


public class Usuario
{
    //Se quisesse uma outra primary key [Key]
    [JsonIgnore]
    public int Id { get; set; }
    [Required(ErrorMessage = "Campo obrigatório", AllowEmptyStrings = false)]
    public string? Nome { get; set; }
    [Required(ErrorMessage = "Campo obrigatório", AllowEmptyStrings = false)]
    [EmailAddress(ErrorMessage = "E-mail invalido")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Campo obrigatório", AllowEmptyStrings = false)]
    public string? Senha { get; set; }
    
    public virtual List<Tarefas>? ListaTarefas { get; set; } 
}