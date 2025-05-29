namespace ApiTarefas.Models;

using System.ComponentModel.DataAnnotations;

public class Login
{
    [Required(ErrorMessage = "Campo obrigatório", AllowEmptyStrings = false)]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Campo obrigatório", AllowEmptyStrings = false)]
    public string? Senha { get; set; }
}