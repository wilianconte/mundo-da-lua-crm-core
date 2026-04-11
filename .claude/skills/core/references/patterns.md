# Padrões de Código — MyCRM

## FLUXO DE ESCRITA (obrigatório)

```
GraphQL Mutation → CreateXxxInput → CreateXxxCommand (MediatR)
  → ValidationBehavior (FluentValidation automático)
  → Handler: validações de negócio via repositório
  → Domain factory method (Xxx.Create(...))
  → repository.AddAsync() + repository.SaveChangesAsync()
  → Result<XxxDto>.Success(entity.Adapt<XxxDto>())
  → Mutation converte Result em payload ou lança GraphQLException
```

> Domain Events e Outbox **não estão implementados**. O flow é direto: handler → repositório → SaveChanges → Result.

**Nunca** coloque regra de negócio no resolver GraphQL.

## FLUXO DE LEITURA (obrigatório)

```
GraphQL Query → verificação de autenticação
  → db.Xxx.AsNoTracking() com [UsePaging][UseProjection][UseFiltering][UseSorting]
  → Hot Chocolate projeta automaticamente para o tipo solicitado
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

## PADRÃO DE REPOSITÓRIO

### Interface base (`Shared.Kernel`)

```csharp
// MyCRM.Shared.Kernel.Repositories.IRepository<TEntity>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> Query();
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### Interface especializada (Domain)

Cada repositório de domínio estende `IRepository<T>` com métodos específicos de negócio:

```csharp
// MyCRM.CRM.Domain.Repositories.IPersonRepository
public interface IPersonRepository : IRepository<Person>
{
    Task<bool> DocumentNumberExistsAsync(Guid tenantId, string documentNumber, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(Guid tenantId, string email, Guid? excludeId = null, CancellationToken ct = default);
}

// MyCRM.CRM.Domain.Repositories.IEmployeeRepository
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<bool> PersonAlreadyEmployedAsync(Guid tenantId, Guid personId, CancellationToken ct = default);
    Task<bool> EmployeeCodeExistsAsync(Guid tenantId, string code, CancellationToken ct = default);
}
```

Regras:
- Interface de domínio fica em `{Modulo}.Domain` — não referencia EF Core
- Implementação concreta fica em `{Modulo}.Infrastructure`
- Registro no `DependencyInjection.cs` do módulo Infrastructure
- **Sem Unit of Work explícita** — transação é implícita via `SaveChangesAsync()` do EF Core; operações simples não precisam de transação explícita

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

**Localização:** `Shared.Kernel/Results/Result.cs`

```csharp
// Result<T> — para commands que retornam dados
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public IReadOnlyList<string> Errors { get; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string errorCode, params string[] errors) => new(errorCode, errors);
}

// Result — para commands que não retornam dados (delete, etc.)
public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public IReadOnlyList<string> Errors { get; }

    public static Result Success() => new(true);
    public static Result Failure(string errorCode, params string[] errors) => new(false, errorCode, errors);
}
```

Uso no handler:
```csharp
// Retorno de sucesso
return Result<PersonDto>.Success(person.Adapt<PersonDto>());

// Retorno de falha de negócio
return Result<PersonDto>.Failure("PERSON_EMAIL_DUPLICATE", "A person with this email already exists.");

// Erro de validação vem do ValidationBehavior automaticamente — código: "VALIDATION_ERROR"
```

---

## PIPELINE BEHAVIOR — ValidationBehavior

O `ValidationBehavior<TRequest, TResponse>` está em `Shared.Kernel` e intercepta todos os commands antes do handler. Não há nada a implementar por command — basta criar o Validator e ele será chamado automaticamente.

**Registro (obrigatório em cada módulo Application):**
```csharp
// {Modulo}.Application/DependencyInjection.cs
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));  // ← obrigatório
});
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

Comportamento:
- Se houver validators registrados e algum falhar → retorna `Result.Failure("VALIDATION_ERROR", string[])` sem chamar o handler
- Se não houver validator → passa direto para o handler
- Funciona para qualquer `IRequest<Result<T>>` ou `IRequest<Result>`

---

## DOMAIN EVENTS

> **Status: não implementado.** O write flow menciona Domain Events e Outbox, mas nenhum mecanismo foi implementado ainda no codebase. Não crie `INotification`, `IDomainEvent` ou outbox sem alinhamento explícito com o projeto.

---

## DATALOADER (N+1)

> **Status: não implementado.** O padrão é reconhecido como necessário para evitar N+1 em queries relacionadas no GraphQL, mas nenhum DataLoader existe no codebase. Queries atualmente fazem projeção direta via EF Core. Não crie DataLoaders sem alinhamento explícito.

---

## PADRÃO GRAPHQL — ERRO DE MUTAÇÃO

Toda mutation converte `Result` em payload ou lança `GraphQLException`:

```csharp
var result = await sender.Send(new CreatePersonCommand(...), ct);

return result.IsSuccess
    ? result.Value!
    : throw new GraphQLException(
        result.Errors.Select(e =>
            ErrorBuilder.New()
                .SetMessage(e)
                .SetExtension("code", result.ErrorCode)
                .Build()));
```

A resposta de erro terá o campo `extensions.code` com o `ErrorCode` do Result (ex: `"PERSON_EMAIL_DUPLICATE"`, `"VALIDATION_ERROR"`).

---

## PADRÃO GRAPHQL — OBJECTTYPE (campos internos ocultos)

Por padrão, Hot Chocolate expõe todos os campos públicos de uma entidade. Para ocultar campos internos (`TenantId`, `IsDeleted`, `DeletedAt`) use `ObjectType<T>` explícito:

```csharp
public sealed class CustomerObjectType : ObjectType<Customer>
{
    protected override void Configure(IObjectTypeDescriptor<Customer> descriptor)
    {
        descriptor.Name("Customer");
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Name);
        // ... campos expostos
        descriptor.Ignore(x => x.TenantId);    // ← nunca expor ao cliente
        descriptor.Ignore(x => x.IsDeleted);
        descriptor.Ignore(x => x.DeletedAt);
    }
}
```

Registrar em Program.cs com `.AddType<CustomerObjectType>()` (não `.AddTypeExtension<>()`).

Alternativa para entidades simples: usar `[GraphQLIgnore]` no campo da entidade.

---

## MULTI-TENANCY — ITenantService

**Interface:** `Shared.Kernel/MultiTenancy/ITenantService.cs`
```csharp
public interface ITenantService
{
    Guid TenantId { get; }
    void SetTenant(Guid tenantId);
}
```

**Implementação em produção:** `HttpTenantService` (Gateway) — resolução dual:
1. Requests HTTP autenticadas: lê claim `tenant_id` do JWT
2. Fallback para seed/background (sem HTTP context): usa `SetTenant(guid)` chamado explicitamente

**TenantMiddleware** (`Middleware/TenantMiddleware.cs`) extrai o claim do JWT e chama `tenantService.SetTenant()` antes de cada request.

**DI Registration:**
```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, HttpTenantService>();
```

Regras:
- **Nunca** leia `HttpContext` diretamente fora do middleware de tenant
- Seed usa `tenantService.SetTenant(SeedTenantId)` antes de qualquer operação
- Handlers recebem `ITenantService` via construtor (injeção padrão)

---

## AUTORIZACAO

Modelo de permissao:
```
{recurso}:{operacao}
```
Exemplos atuais: `students:read`, `students:create`, `student_guardians:update`, `users:manage`, `roles:manage`.

Padrao obrigatorio nos resolvers:
- usar policy explicita com `[Authorize(Policy = SystemPermissions.Xxx)]` para operacoes de negocio;
- evitar `[Authorize]` generico em recursos sensiveis.
- em classes `[QueryType]` e `[MutationType]`, aplicar `[Authorize]` nos metodos/campos e nao no nivel da classe para evitar propagacao indevida para todo o tipo raiz.

Exemplo:
```csharp
[QueryType]
public sealed class StudentQueries
{
    [Authorize(Policy = SystemPermissions.StudentsRead)]
    public IQueryable<Student> GetStudents(...) { ... }
}

[MutationType]
public sealed class StudentMutations
{
    [Authorize(Policy = SystemPermissions.StudentsCreate)]
    public Task<StudentPayload> CreateStudentAsync(...) { ... }
}
```

Regras:
- `Login`, `RefreshToken` e `RegisterTenant` permanecem `[AllowAnonymous]` — únicas mutations públicas.
- Policies sao registradas automaticamente a partir de `SystemPermissions.All` em `Program.cs`.
- Toda nova permissao precisa ser adicionada em `SystemPermissions` e em `SystemPermissions.All`.
- Nao fazer verificacao manual por `IHttpContextAccessor` dentro de resolver para autorizacao.
- Normalizar permissoes ao carregar/cachear e ao validar:
1. `Trim()`
2. `ToLowerInvariant()`
3. Remover valores vazios e duplicados
- Para suites de regressao RBAC em mutations principais, garantir cenarios non-admin:
1. com permissao correta (sucesso)
2. sem permissao (AUTH_NOT_AUTHORIZED)
3. sem vazamento entre policies de entidades diferentes

---

## FLUXO OBRIGATORIO PARA TESTES DE CONSULTA GRAPHQL

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
- usar `IHttpContextAccessor` para verificar autenticacao dentro de resolver — use `[Authorize]` no metodo do resolver/campo; a verificacao manual e propensa a ser esquecida em novos resolvers
- usar `[Authorize(Policy = ...)]` no nivel da classe em `QueryType`/`MutationType` para proteger campo especifico — aplicar no metodo do campo para evitar contaminar o tipo raiz inteiro
- usar `AllowIntrospection(bool)` no HC 15 — está obsoleto, usar `DisableIntrospection(!isDev)`
- usar `IQueryResult` para tipar o resultado de `executor.ExecuteAsync()` em testes HC 15 — o tipo correto é obtido via `.ExpectOperationResult()` que retorna `IOperationResult`
- chamar `.AddAuthorization()` no chain do `AddGraphQLServer()` — o método correto no `IRequestExecutorBuilder` é `.AddAuthorizationCore()` (extensão de `HotChocolateAuthorizeRequestExecutorBuilder`); `.AddAuthorization()` existe apenas em `IServiceCollection` (ASP.NET Core), não no builder HC

---

## UPDATE 2026-03-29 - AUTH MUTATIONS

Quando novas mutations forem adicionadas em `AuthMutations`:

- manter `Login`, `RefreshToken` e `RegisterTenant` com `[AllowAnonymous]`;
- aplicar `[Authorize]` nas mutations sensiveis (ex.: `CreateUserAsync`);
- manter tratamento padrao de erro com `GraphQLException` + `extensions.code`.


