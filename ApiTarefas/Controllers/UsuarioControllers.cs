namespace ApiTarefas.Controllers;

using Microsoft.AspNetCore.Mvc;
using ApiTarefas.Contexto;
using ApiTarefas.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("[controller]")]
public class UsuariosController(BancoDados Banco, IConfiguration Configuracoes) : ControllerBase
{
    [HttpGet]
    public ActionResult ListarUsuarios()
    {
        List<Usuario> lista = [.. Banco.TabelaUsuario.OrderBy(u => u.Nome)];
        if (lista.Count > 0) return Ok(lista);
        return NotFound();
    }

    [HttpPost]
    public ActionResult CriarUsuario(Usuario model)
    {
        Banco.TabelaUsuario.Add(model);
        Banco.SaveChanges();
        return Ok(new { Mensagem = "Usuário criado com sucesso" });
    }

    string GerarToken(Usuario user)
    {
        string token = "";
        DateTime iat = DateTime.UtcNow, exp = iat.AddMinutes(30);
        token = new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(
                audience: "ApiEstoque",
                claims: [
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(iat).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.Nome!)
                ],
                expires: exp,
                signingCredentials: new(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuracoes["ChaveSecreta"]!)), SecurityAlgorithms.HmacSha256
                    )
                )
        );
        return token;
    }

    [HttpPost("/login")]
    public ActionResult FazerLogin(Login model)
    {
        Usuario? user = Banco.TabelaUsuario.SingleOrDefault(u => u.Email == model.Email && u.Senha == model.Senha);
        if (user == null) return NotFound(new { Mensagem = "E=mail e/ou senha incorretos" });
        return Ok(new
        {
            Token = GerarToken(user)
        });
    }

    [HttpGet("/logout")]
    [Authorize]
    public ActionResult FazerLogout([FromServices] BlackList blacklist)
    {
        blacklist.RevogarToken(
            User.FindFirstValue(JwtRegisteredClaimNames.Jti)!,
            DateTimeOffset.FromUnixTimeSeconds(long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Exp)!)).DateTime
        );
        return Ok(new
        {
            Mensagem = "Sessão encerrada com sucesso"
        });
    }
}