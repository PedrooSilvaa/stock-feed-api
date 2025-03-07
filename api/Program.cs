using api.Data;
using api.Interfaces;
using api.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Adiciona os serviços de controllers ao container de serviços da aplicação.
// Isso permite que a aplicação utilize controllers para lidar com requisições HTTP.
builder.Services.AddControllers();
// Adiciona o contexto do banco de dados ApplicationDBContext ao container de serviços.
// Aqui, é configurado para usar o SQL Server e a string de conexão chamada "DefaultConnection" que está no appsettings.json.
builder.Services.AddDbContext<ApplicationDBContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// Registra a interface e a implementação do repositório de estoque no container de injeção de dependência.
// Sempre que a aplicação solicitar um `IStockRepository`, será entregue uma instância de `StockReository`.
// Isso segue o ciclo de vida Scoped (uma instância por requisição).
builder.Services.AddScoped<IStockRepository, StockReository>();
// Registra a interface e a implementação do repositório de comentários no container de injeção de dependência.
// Funciona do mesmo modo que o repositório de estoque.
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
// Adiciona novamente os controllers, mas com uma configuração extra para usar o pacote Newtonsoft.Json.
// Essa configuração é necessária para personalizar a serialização/deserialização JSON, como ignorar loops de referência (evita erros de serialização circular).
builder.Services.AddControllers().AddNewtonsoftJson(options => {
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
 