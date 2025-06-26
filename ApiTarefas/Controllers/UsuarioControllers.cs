namespace ApiTarefas.Controllers;

using Microsoft.AspNetCore.Mvc;
using ApiTarefas.Contexto;
using ApiTarefas.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

[ApiController]
[Route("[controller]")]
public class UsuariosController(BancoDados Banco, IConfiguration Configuracoes) : ControllerBase
{
    [HttpGet]
    public ActionResult ListarUsuarios()
    {
        var lista = Banco.TabelaUsuario
            .Include(u => u.ListaTarefas) // traz as tarefas associadas
            .OrderBy(u => u.Nome)
            .ToList();

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

    string GerarRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    [HttpPost("/login")]
    public ActionResult FazerLogin(Login model)
    {
        var user = Banco.TabelaUsuario.SingleOrDefault(u => u.Email == model.Email && u.Senha == model.Senha);

        if (user == null) return NotFound(new { Mensagem = "E-mail e/ou senha incorretos" });

        string accessToken = GerarToken(user);
        string refreshToken = GerarRefreshToken();
        DateTime refreshExp = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiracao = refreshExp;
        Banco.SaveChanges();

        // Retornando apenas os dados que você quer expor do usuário
        var userResponse = new
        {
            id = user.Id,
            email = user.Email
        };

        return Ok(new
        {
            token = accessToken,
            refreshToken = refreshToken,
            user = userResponse
        });
    }

    [HttpPost("/refresh-token")]
    public ActionResult RenovarToken([FromBody] RefreshTokenRequest body)
    {
        string? refreshToken = body?.RefreshToken;

        if (string.IsNullOrEmpty(refreshToken)) return BadRequest(new { Mensagem = "Refresh token não informado" });

        Usuario? user = Banco.TabelaUsuario.SingleOrDefault(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiracao > DateTime.UtcNow);

        if (user is null) return Unauthorized(new { Mensagem = "Refresh token inválido ou expirado" });

        string novoAccessToken = GerarToken(user);
        string novoRefreshToken = GerarRefreshToken();
        user.RefreshToken = novoRefreshToken;
        user.RefreshTokenExpiracao = DateTime.UtcNow.AddDays(7);

        Banco.SaveChanges();

        return Ok(new
        {
            Token = novoAccessToken,
            RefreshToken = novoRefreshToken
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