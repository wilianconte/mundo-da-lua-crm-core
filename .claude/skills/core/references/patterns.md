# Padrões de Código — MyCRM

## FLUXO DE ESCRITA (obrigatório)

```
GraphQL Mutation → Input → MediatR Command → FluentValidation
  → Handler → Aggregate/Domain Rules → UoW/Transaction
  → Domain Events → Outbox → Payload de retorno
```

**Nunca** coloque regra de negócio no resolver GraphQL.

## FLUXO DE LEITURA (obrigatório)

```
GraphQL Query → Authorization → Query Handler
  → EF Core projection (AsNoTracking) ou Dapper se justificado
  → DTO/Read Model → resultado
```

---

## PADRÃO DE ENTIDADES

```csharp
// Toda entidade não-tenant herda de:
public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public void SoftDelete() { IsDeleted = true; DeletedAt = DateTimeOffset.UtcNow; }
    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}

// Toda entidade tenant-aware herda de:
public abstract class TenantEntity : BaseEntity, IHasTenantId
{
    public Guid TenantId { get; set; }
}
```

Regras:
- **Sem hard delete** — sempre `SoftDelete()`
- **DateTimeOffset** nunca DateTime
- **Guid gerado no construtor** — sem dependência de banco para ID
- Factory methods estáticos para criação (`Person.Create(...)`)
- Setters `private` — mutação apenas por métodos de domínio

---

## PADRÃO DE CONFIGURAÇÃO EF (IEntityTypeConfiguration)

```csharp
public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("people");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired();

        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        // ATENÇÃO: filtros de índice parcial usam aspas duplas (colunas PascalCase do EF)
        builder.HasIndex(x => new { x.TenantId, x.DocumentNumber })
            .IsUnique()
            .HasFilter("\"DocumentNumber\" IS NOT NULL");  // ← aspas duplas obrigatórias

        builder.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL");           // ← aspas duplas obrigatórias
    }
}
```

**Regra crítica:** Filtros de índice parcial (`HasFilter`) **sempre** usam o nome da coluna entre aspas duplas escapadas — `\"NomeColuna\"`. Nunca usar snake_case sem aspas, pois o EF Core usa PascalCase como padrão de nomes de coluna neste projeto.

---

## PADRÃO DE DBCONTEXT

```csharp
public sealed class CRMDbContext : DbContext
{
    private readonly ITenantService _tenant;

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Person>   People    => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("crm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CRMDbContext).Assembly);

        // Query filter obrigatório para cada entidade tenant-aware
        modelBuilder.Entity<Customer>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);
        modelBuilder.Entity<Person>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Injeta TenantId em cada entidade adicionada
        foreach (var entry in ChangeTracker.Entries<Customer>())
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = _tenant.TenantId;

        foreach (var entry in ChangeTracker.Entries<Person>())
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = _tenant.TenantId;

        return base.SaveChangesAsync(cancellationToken);
    }
}
```

Ao adicionar nova entidade ao DbContext: registrar `DbSet`, adicionar `HasQueryFilter` e bloco no `SaveChangesAsync`.

---

## PADRÃO DE DEPENDENCY INJECTION (Infrastructure)

```csharp
public static IServiceCollection AddCustomersInfrastructure(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<CRMDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    // Registrar todos os repositórios do módulo aqui
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    services.AddScoped<IPersonRepository,  PersonRepository>();

    return services;
}
```

Ao criar um novo repositório, **sempre** registrá-lo no `DependencyInjection.cs` do módulo.

---

## PADRÃO DE COMMAND/HANDLER

```csharp
// Command — record imutável
public record CreatePersonCommand(string FullName, string? Email, string? DocumentNumber)
    : IRequest<Result<PersonDto>>;

// Validator — sempre junto ao command
public sealed class CreatePersonValidator : AbstractValidator<CreatePersonCommand>
{
    public CreatePersonValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(254).When(x => x.Email is not null);
    }
}

// Handler
public sealed class CreatePersonHandler : IRequestHandler<CreatePersonCommand, Result<PersonDto>>
{
    public async Task<Result<PersonDto>> Handle(CreatePersonCommand request, CancellationToken ct)
    {
        // 1. validações de negócio (duplicatas via IPersonRepository)
        // 2. criar via factory method
        // 3. persistir
        // 4. retornar Result<Dto>
    }
}
```

---

## CRUD PADRÃO OBRIGATÓRIO POR ENTIDADE

**Toda vez que uma nova entidade for criada, os seguintes artefatos devem ser gerados automaticamente, sem precisar ser solicitados:**

### Application Layer (`{Modulo}.Application`)

| Artefato | Caminho |
|---|---|
| `Get{Entidade}ByIdQuery` + Handler | `Queries/Get{Entidade}ById/` |
| `GetAll{Entidades}Query` + Handler | `Queries/GetAll{Entidades}/` |
| `Create{Entidade}Command` + Handler + Validator | `Commands/{Entidades}/Create{Entidade}/` |
| `Update{Entidade}Command` + Handler + Validator | `Commands/{Entidades}/Update{Entidade}/` |
| `Delete{Entidade}Command` + Handler | `Commands/{Entidades}/Delete{Entidade}/` |
| `{Entidade}Dto` | `DTOs/` |
| Mapping config Mapster | `Mappings/` |

**Convenção de subpastas em `Commands/`:**
- Organizar por entidade usando o **plural** da entidade como subpasta: `Commands/{Entidades}/{Operacao}{Entidade}/`
- Usar plural para evitar colisão de namespace com a classe de domínio de mesmo nome
- Exemplos: `Commands/People/CreatePerson/`, `Commands/Customers/DeleteCustomer/`
- Namespace resultante: `MyCRM.{Modulo}.Application.Commands.{Entidades}.{Operacao}{Entidade}`

### GraphQL Layer (`MyCRM.GraphQL`)

| Artefato | Caminho |
|---|---|
| `{Entidade}Queries` — `Get{Entidades}` (lista paginada) + `Get{Entidade}ById` | `GraphQL/{Modulo}/` |
| `{Entidade}Mutations` — `Create`, `Update`, `Delete` | `GraphQL/{Modulo}/` |
| `{Entidade}Input` (Create e Update separados) | `GraphQL/{Modulo}/` |
| `{Entidade}Payload` | `GraphQL/{Modulo}/` |

---

## PADRÃO GRAPHQL

### Queries

```csharp
[QueryType]
public sealed class {Entidade}Queries
{
    [Authorize(Policy = "{modulo}:{recurso}:read")]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<{Entidade}> Get{Entidades}([Service] {Modulo}DbContext db) =>
        db.{Entidades}.AsNoTracking();

    [Authorize(Policy = "{modulo}:{recurso}:read")]
    public async Task<{Entidade}?> Get{Entidade}ByIdAsync(
        Guid id,
        [Service] {Modulo}DbContext db,
        CancellationToken ct) =>
        await db.{Entidades}.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
}
```

### Mutations

```csharp
[MutationType]
public sealed class {Entidade}Mutations
{
    [Authorize(Policy = "{modulo}:{recurso}:write")]
    public async Task<{Entidade}Payload> Create{Entidade}Async(
        Create{Entidade}Input input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new Create{Entidade}Command(input), ct);
        return result.IsSuccess
            ? new {Entidade}Payload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    [Authorize(Policy = "{modulo}:{recurso}:write")]
    public async Task<{Entidade}Payload> Update{Entidade}Async(
        Guid id, Update{Entidade}Input input,
        [Service] ISender sender,
        CancellationToken ct) { /* idem */ }

    [Authorize(Policy = "{modulo}:{recurso}:write")]
    public async Task<bool> Delete{Entidade}Async(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct) { /* idem */ }
}
```

Regras:
- Inputs e payloads explícitos — nunca expor entidade EF como tipo público
- `GetBy*` usa `FirstOrDefaultAsync` — nunca `FindAsync` (ignora query filters)
- DataLoader para N+1
- **`{Entidade}Queries` e `{Entidade}Mutations` devem ser registrados no `Program.cs` via `.AddTypeExtension<>()`** — sem isso o campo não aparece no schema GraphQL

---

## MULTI-TENANCY

```csharp
// Query filter obrigatório em todo DbContext tenant-aware
modelBuilder.Entity<Person>()
    .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _tenant.TenantId);

// SaveChangesAsync injeta TenantId automaticamente
// TenantId vem do JWT claim tenant_id via ITenantService (scoped)
```

Regras:
- **Nunca** ler `HttpContext` diretamente fora do middleware de tenant
- **Nunca** passar `tenantId` manualmente como parâmetro por toda a cadeia
- Toda entidade de negócio implementa `IHasTenantId`
- Bypass de tenant = exceção explícita e auditável (só `SystemAdmin`)

---

## RESULT PATTERN

```csharp
Result<T>.Success(value)
Result<T>.Failure("CODIGO_ERRO", "mensagem")
Result.Success()
Result.Failure("CODIGO_ERRO", "mensagem")

// Erro de validação vem do ValidationBehavior automaticamente — código: "VALIDATION_ERROR"
```

---

## AUTORIZAÇÃO

Modelo de permissão:
```
{modulo}:{recurso}:{operacao}
```
Exemplos: `crm:person:read`, `crm:person:write`, `escola:aluno:read`, `clinica:agenda:write`

---

## FLUXO OBRIGATÓRIO PARA TESTES DE CONSULTA GRAPHQL

### Passo 1 — Obter o token JWT

```bash
TOKEN=$(curl -s -X POST http://localhost:5095/graphql \
  -H "Content-Type: application/json" \
  -d '{"query":"mutation { login(input: { tenantId: \"00000000-0000-0000-0000-000000000001\", email: \"admin@mundodalua.com\", password: \"Admin@123\" }) { token } }"}' \
  | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
```

Credenciais de seed (ambiente de dev):
- **TenantId:** `00000000-0000-0000-0000-000000000001`
- **Email:** `admin@mundodalua.com`
- **Senha:** `Admin@123`

### Passo 2 — Executar a consulta com o token

```bash
curl -s -X POST http://localhost:5095/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"query":"{ suaQueryAqui }"}'
```

### Observações críticas

- O tenant **não** vem de header HTTP — vem do claim `tenant_id` dentro do JWT
- A porta padrão de dev é **5095** (definida em `launchSettings.json`)
- O campo do token no `LoginDto` é **`token`**, não `accessToken`
- Sempre verificar se a aplicação está rodando antes de testar (`dotnet run --project "1 - Gateway/MundoDaLua.GraphQL/MyCRM.GraphQL.csproj"`)

---

## ANTI-PADRÕES PROIBIDOS

- criar endpoint REST como atalho
- expor entidade EF como tipo GraphQL público sem critério
- colocar regra de negócio no resolver
- obter `tenantId` do `HttpContext` fora do middleware
- usar `FindAsync` em queries (ignora query filters)
- ignorar `AsNoTracking()` em leituras
- chamar integração externa crítica dentro da transação principal
- aplicar migrations automaticamente em produção sem controle
- duplicar lógica entre módulos por conveniência
- criar repositório sem registrá-lo no DI
- usar snake_case sem aspas duplas em `HasFilter` de índice parcial
- duplicar dados de identidade pessoal fora de `Person`
- criar classe `[QueryType]` ou `[MutationType]` sem registrá-la no `Program.cs` via `.AddTypeExtension<>()` — o campo simplesmente não aparece no schema GraphQL e o erro retornado é `"The field X does not exist on the type Query/Mutation"`, não um erro de compilação
- alterar entidade, relacionamento ou configuração EF sem criar e aplicar migration — o banco fica dessincronizado com o modelo e o próximo `database update` detecta mudanças pendentes impedindo o deploy
- usar `using Microsoft.EntityFrameworkCore` em projetos `Application` — só `Infrastructure` deve referenciar EF Core diretamente; handlers devem usar abstrações de repositório
