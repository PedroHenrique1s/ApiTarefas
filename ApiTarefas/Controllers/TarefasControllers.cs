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
[Authorize]

public class TarefasController(BancoDados Banco) : ControllerBase
{
    [HttpGet]
    public ActionResult ListarTarefas()
    {
        List<object> lista = [..Banco.TabelaTarefas.OrderBy(c => c.Id).ToList().Select(c => new{
            c.Descricao,
            c.Status,
        })];

        if (lista.Count > 0) return Ok(lista);
        return NotFound();
    }

    [HttpGet("{id}")]
    public IActionResult BuscarTarefa(int id)
    {
        Tarefas? tarefa = Banco.TabelaTarefas.SingleOrDefault(c => c.Id == id);
        if (tarefa == null) return NotFound();
        return Ok(tarefa);
    }

    [HttpPost]
    public IActionResult CriarTarefa(Tarefas model)
    {
        Banco.TabelaTarefas.Add(model);
        Banco.SaveChanges();
        return Ok(new { Mensagem = "Categoria cadastrada com sucesso" });
    }

    [HttpPut("{id}")]
    public ActionResult EditarTarefa(int id, Tarefas model)
    {
        Tarefas? tarefa = Banco.TabelaTarefas.SingleOrDefault(c => c.Id == id);
        if (tarefa == null) return NotFound();
        Banco.Entry(tarefa).State =
        Microsoft.EntityFrameworkCore.EntityState.Detached;
        model.Id = id;
        Banco.TabelaTarefas.Update(model);
        return Ok(new { Mensagem = "Categoria atualizada com sucesso" });
    }

    [HttpDelete("{id}")]
    public ActionResult ExcluirTarefa(int id)
    {
        Tarefas? tarefa = Banco.TabelaTarefas.SingleOrDefault(c => c.Id == id);
        if (tarefa == null) return NotFound();
        Banco.Remove(tarefa);
        Banco.SaveChanges();
        return Ok(new{ Mensagem = "Categoria excluída e seus produtos foram excluídos" });
    }
}