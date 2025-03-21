using api.Data;
using api.Interfaces;
using api.Models;
using api.Repository;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Adiciona os serviços de controllers ao container de serviços da aplicação.
// Isso permite que a aplicação utilize controllers para lidar com requisições HTTP.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Adiciona o contexto do banco de dados ApplicationDBContext ao container de serviços.
// Aqui, é configurado para usar o SQL Server e a string de conexão chamada "DefaultConnection" que está no appsettings.json.
builder.Services.AddDbContext<ApplicationDBContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// Registra a interface e a implementação do repositório de estoque no container de injeção de dependência.
// Sempre que a aplicação solicitar um `IStockRepository`, será entregue uma instância de `StockReository`.
// Isso segue o ciclo de vida Scoped (uma instância por requisição).
//O mesmo acontece com os demais abaixo
builder.Services.AddScoped<IStockRepository, StockReository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
// Adiciona novamente os controllers, mas com uma configuração extra para usar o pacote Newtonsoft.Json.
// Essa configuração é necessária para personalizar a serialização/deserialização JSON, como ignorar loops de referência (evita erros de serialização circular).
builder.Services.AddControllers().AddNewtonsoftJson(options => {
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

// Configuração do Identity para gerenciar usuários e autenticação
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    // Definições de senha
    options.Password.RequireDigit = true; // Requer pelo menos um número
    options.Password.RequireLowercase = true; // Requer pelo menos uma letra minúscula
    options.Password.RequireUppercase = true; // Requer pelo menos uma letra maiúscula
    options.Password.RequireNonAlphanumeric = true; // Requer pelo menos um caractere especial (!, @, #, etc.)
    options.Password.RequiredLength = 12; // Exige que a senha tenha no mínimo 12 caracteres
})
// Define que os dados de usuário e funções serão armazenados no banco via Entity Framework Core
.AddEntityFrameworkStores<ApplicationDBContext>();

// Configuração da autenticação usando JWT
builder.Services.AddAuthentication(options => {
    // Define o esquema padrão de autenticação como JWT Bearer
    options.DefaultAuthenticateScheme = 
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Configuração do JWT Bearer
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true, // Verifica se o emissor do token é válido
        ValidIssuer = builder.Configuration["JWT:Issuer"], // Define o emissor esperado do token

        ValidateAudience = true, // Verifica se o público do token é válido
        ValidAudience = builder.Configuration["JWT:Audience"], // Define o público esperado do token

        ValidateIssuerSigningKey = true, // Garante que o token foi assinado corretamente
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]) // Obtém a chave secreta para validar o token
        )
    };
});


var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
 