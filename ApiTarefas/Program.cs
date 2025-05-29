using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiTarefas.Contexto;
using ApiTarefas.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = false,
        ValidateAudience = true,
        ValidAudience = "ApiEstoque",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["ChaveSecreta"]!))
    };
    options.Events = new()
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            await EnviarAcessoNaoAutenticado(context);
        },
        OnAuthenticationFailed = async context =>
        {
            context.NoResult();
            await EnviarAcessoNaoAutenticado(context);
        },
        OnTokenValidated = async context =>
        {
            var jti = context.Principal!.FindFirstValue(JwtRegisteredClaimNames.Jti);
            var blacklist = context.HttpContext.RequestServices.GetRequiredService<BlackList>();
            if (jti != null && await blacklist.ChecarTokenRevogado(jti))
            {
                context.NoResult();
                await EnviarAcessoRevogado(context);
            }
        }
    };
});

builder.Services.AddSingleton<BlackList>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<BancoDados>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);
builder.Services.AddCors(
    options => options.AddDefaultPolicy(
        policy => policy.SetIsOriginAllowed(origin => origin == "http://localhost:4200" || origin is null).AllowAnyMethod().AllowAnyHeader()
    )
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task EnviarAcessoNaoAutenticado(BaseContext<JwtBearerOptions> context)
{
    context.Response.StatusCode = 401;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new
    {
        Mensagem = "Acesso não autenticado"
    });
}

static async Task EnviarAcessoRevogado(BaseContext<JwtBearerOptions> context)
{
    context.Response.StatusCode = 401;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new
    {
        Mensagem = "Acesso revogado - faça login novamente"
    });
}