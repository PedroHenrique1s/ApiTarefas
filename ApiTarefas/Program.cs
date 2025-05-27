using ApiTarefas.Contexto;
using ApiTarefas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().ConfigureApiBehaviorOptions(
    options =>
    {
        options.SuppressMapClientErrors = true;
        options.InvalidModelStateResponseFactory = context =>
        {
            return new BadRequestObjectResult(new
            {
                Mensagem = "Preencha todos os campos corretamente",
                Erros = context.ModelState.Where(x => x.Value!.Errors.Count > 0).ToDictionary(
                  e => e.Key,
                  e => e.Value!.Errors[0].ErrorMessage
                )
            });
        };
    }
);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<BancoDados>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);
builder.Services.AddCors(
    options => options.AddDefaultPolicy(
        policy => policy.SetIsOriginAllowed(origin => origin == "http://localhost:4200" || origin is null).AllowAnyMethod().AllowAnyHeader()
    )
);
var app = builder.Build();

Gerador.GerarChave();
DotNetEnv.Env.Load();
builder.Configuration.AddEnvironmentVariables();
app.UseCors();
app.UseRouting();

app.UseStatusCodePages(
    async context => 
    {
        var response = context.HttpContext.Response;
        if(response.StatusCode == 404 && !response.HasStarted)
        {
            response.ContentType = "application/json";
            await response.WriteAsJsonAsync(new{Mensagem = "Nenhum registro encontrado"});
        }
    }
);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
