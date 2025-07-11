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

[ApiController]
[Route("[controller]")]
[Authorize]

public class TarefasController(BancoDados Banco) : ControllerBase
{
    [HttpGet]
    public ActionResult ListarTarefas([FromQuery] int page = 1, [FromQuery] int pageSize = 15)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized("ID do usuário não encontrado no token.");
        }

        var usuarioId = int.Parse(userIdString);

        var query = Banco.TabelaTarefas
            .Where(t => t.UsuarioId == usuarioId)
            .Include(t => t.Usuario)
            .OrderByDescending(t => t.Id) 
            .Select(t => new {
                t.Id,
                t.Descricao,
                t.Status,
                Usuario = new {
                    t.Usuario!.Nome,
                    t.Usuario.Email
                }
            });

        var totalDeRegistros = query.Count();

        var listaPaginada = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        bool hasNext = totalDeRegistros > (page * pageSize);

        var respostaFormatada = new {
            items = listaPaginada,
            hasNext = hasNext
        };

        return Ok(respostaFormatada);
    }

    [HttpGet("{id}")]
    public IActionResult BuscarTarefa(int id)
    {
        var tarefa = Banco.TabelaTarefas
            .Include(t => t.Usuario)
            .Where(t => t.Id == id)
            .Select(t => new {
                t.Descricao,
                t.Status,
                Usuario = new {
                    t.Usuario!.Nome,
                    t.Usuario.Email
                }
            })
            .FirstOrDefault();

        if (tarefa == null) return NotFound();
        return Ok(tarefa);
    }

    [HttpPost]
    public IActionResult CriarTarefa(Tarefas model)
    {
        Banco.TabelaTarefas.Add(model);
        Banco.SaveChanges();
        return Ok(new { Mensagem = "Tarefa cadastrada com sucesso" });
    }

    [HttpPut("{id}")]
    public ActionResult EditarTarefa(int id, Tarefas model)
    {
        Tarefas? tarefa = Banco.TabelaTarefas.SingleOrDefault(c => c.Id == id);
        if (tarefa == null) return NotFound();

        // Atualiza apenas os campos necessários
        tarefa.Descricao = model.Descricao;
        tarefa.Status = model.Status;
        tarefa.UsuarioId = model.UsuarioId;

        Banco.SaveChanges();

        return Ok(new { Mensagem = "Tarefa atualizada com sucesso" });
    }

    [HttpDelete("{id}")]
    public ActionResult ExcluirTarefa(int id)
    {
        Tarefas? tarefa = Banco.TabelaTarefas.SingleOrDefault(c => c.Id == id);
        if (tarefa == null) return NotFound();
        Banco.Remove(tarefa);
        Banco.SaveChanges();
        return Ok(new{ Mensagem = "Tarefa excluída com sucesso" });
    }
}