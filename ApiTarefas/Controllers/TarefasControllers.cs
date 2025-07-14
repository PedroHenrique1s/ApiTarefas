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
    public ActionResult ListarTarefas(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] int? status = null,
        [FromQuery] int? id = null,
        [FromQuery] string? search = null
    )
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized("ID do usuário não encontrado no token.");
        }

        var usuarioId = int.Parse(userIdString);

        var query = Banco.TabelaTarefas
            .Where(t => t.UsuarioId == usuarioId);

        // Aplica filtro por ID se fornecido
        if (id.HasValue)
        {
            query = query.Where(t => t.Id == id.Value);
        }

        // Aplica filtro por status se fornecido
        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        // Aplica filtro de busca geral
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();

            // Tenta converter para número
            bool isNumeric = int.TryParse(search, out int numero);

            query = query.Where(t =>
                (t.Descricao != null && t.Descricao.ToLower().Contains(searchLower)) ||
                (isNumeric && (t.Id == numero || t.Status == numero))
            );
        }

        var totalDeRegistros = query.Count();

        var listaPaginada = query
            .Include(t => t.Usuario)
            .OrderByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new {
                t.Id,
                t.Descricao,
                t.Status,
                Usuario = new {
                    t.Usuario!.Nome,
                    t.Usuario.Email
                }
            })
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