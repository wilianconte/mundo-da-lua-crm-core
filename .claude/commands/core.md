# Skill: Arquitetura Core — MyCRM

Você está trabalhando no backend do **MyCRM**. Aplique rigorosamente as regras abaixo em toda decisão de código.

---

## REGRA OBRIGATÓRIA — ATUALIZAÇÃO DA SKILL

**Ao finalizar qualquer implementação, execute obrigatoriamente os dois passos abaixo:**

### Passo 1 — Atualizar conhecimentos

Atualize este arquivo (`core.md`) com os novos conhecimentos adquiridos:
- Novos módulos ou projetos adicionados à solução
- Novas entidades e suas convenções
- Schemas e tabelas criados no banco
- Decisões de design que se mostraram corretas ou incorretas
- Armadilhas encontradas e como foram resolvidas
- Novos padrões ou convenções estabelecidos

### Passo 2 — Análise de erros e prevenção futura

**Sempre que um erro for corrigido durante ou após uma implementação:**

1. Identifique a causa raiz do erro (não apenas o sintoma)
2. Avalie se o erro poderia se repetir em situações similares
3. Formule uma regra preventiva clara e objetiva
4. Adicione a regra nas seções adequadas deste arquivo:
   - Checklists → para verificações pontuais
   - Anti-padrões → para comportamentos a evitar
   - Padrões existentes → para complementar com novas observações
   - Nova seção → se o erro revelar uma categoria ainda não coberta

**O objetivo é que cada erro cometido seja cometido uma única vez.**

**Nunca finalize uma tarefa sem verificar se algo novo precisa ser registrado aqui.**

---

## ESTRUTURA DO PROJETO (estado atual)

**Solução:** `mundo-da-lua-crm-core/MyCRM.sln`

**Convenção de nomes de projeto:** `MyCRM.{Modulo}.{Camada}`

**Projetos ativos:**

| Projeto | Caminho físico do .csproj | Namespace raiz |
|---|---|---|
| `MyCRM.GraphQL` | `1 - Gateway/MundoDaLua.GraphQL/MyCRM.GraphQL.csproj` | `MyCRM.GraphQL` |
| `MyCRM.CRM.Domain` | `2 - CRM/CRM.Domain/MyCRM.CRM.Domain.csproj` | `MyCRM.CRM.Domain` |
| `MyCRM.CRM.Application` | `2 - CRM/CRM.Application/MyCRM.CRM.Application.csproj` | `MyCRM.CRM.Application` |
| `MyCRM.CRM.Infrastructure` | `2 - CRM/CRM.Infrastructure/MyCRM.CRM.Infrastructure.csproj` | `MyCRM.CRM.Infrastructure` |
| `MyCRM.Auth.Domain` | `3 - Auth/Auth.Domain/MyCRM.Auth.Domain.csproj` | `MyCRM.Auth.Domain` |
| `MyCRM.Auth.Application` | `3 - Auth/Auth.Application/MyCRM.Auth.Application.csproj` | `MyCRM.Auth.Application` |
| `MyCRM.Auth.Infrastructure` | `3 - Auth/Auth.Infrastructure/MyCRM.Auth.Infrastructure.csproj` | `MyCRM.Auth.Infrastructure` |
| `MyCRM.Shared.Kernel` | `3 - Shared/Shared.Kernel/MyCRM.Shared.Kernel.csproj` | `MyCRM.Shared.Kernel` |
| `MyCRM.UnitTests` | `4 - Tests/UnitTests/MyCRM.UnitTests.csproj` | `MyCRM.UnitTests` |

**Nota:** Ao adicionar novos módulos, o padrão de diretório é `2 - {Modulo}/` com subpastas `{Modulo}.Domain`, `{Modulo}.Application`, `{Modulo}.Infrastructure`.

---

## DECISÕES NÃO NEGOCIÁVEIS

| Tema | Decisão |
|---|---|
| Runtime | .NET 9 |
| Interface pública | **GraphQL único** — zero REST |
| Servidor GraphQL | Hot Chocolate 15+ |
| Arquitetura | Clean Architecture + DDD + CQRS |
| Mediação | MediatR |
| Validação | FluentValidation |
| Banco | PostgreSQL + EF Core 9 + Npgsql |
| Mapeamento | Mapster |
| Cache | Redis |
| Logs | Serilog |
| Tracing/Métricas | OpenTelemetry |

---

## ESTRUTURA DE MÓDULO

Cada módulo segue exatamente esta estrutura:

```
2 - {Modulo}/
├── {Modulo}.Domain          ← entidades, enums, value objects, interfaces de repositório
├── {Modulo}.Application     ← commands, queries, handlers, validators, DTOs
└── {Modulo}.Infrastructure  ← DbContext, repositórios, migrations, seed, configurações EF
```

Regras de dependência (setas = "depende de"):
```
GraphQL → Application → Domain
Infrastructure → Application → Domain
Shared.Kernel ← todos
```

---

## SCHEMAS POSTGRESQL (estado atual)

| Schema | DbContext | Tabelas existentes | Módulo |
|---|---|---|---|
| `crm` | `CRMDbContext` | `customers`, `people`, `companies`, `students`, `student_guardians`, `courses`, `student_courses` | CRM |
| `auth` | `AuthDbContext` | `users` | Auth |

Schemas planejados (ainda não implementados):

| Schema | Módulo |
|---|---|
| `escola` | Alunos, turmas, matrículas, frequência |
| `clinica` | Pacientes, agenda, prontuários |
| `financeiro` | Contratos, cobranças, pagamentos |
| `rh` | Funcionários, contratos de trabalho |

Cada módulo tem seu próprio `DbContext` e suas próprias migrations.

---

## ENTIDADES EXISTENTES

### Person — entidade mestre de identidade (`crm.people`)

`Person` é a entidade central para todos os indivíduos do sistema. Todos os papéis futuros (Guardian, Student, Patient, Employee, Lead, Supplier) referenciarão `Person` por `PersonId` — **nunca duplicar dados pessoais em módulos separados**.

```
Person 1──0..1  Guardian    (FK em Guardian.PersonId)
Person 1──0..1  Student     (FK em Student.PersonId)
Person 1──0..1  Patient     (FK em Patient.PersonId)
Person 1──0..1  Employee    (FK em Employee.PersonId)
Person 1──0..*  Lead        (FK em Lead.PersonId)
Person 1──0..1  Supplier    (FK em Supplier.PersonId)
```

Enums em `MyCRM.CRM.Domain.Entities`: `PersonStatus`, `Gender`, `MaritalStatus`.

### Company — entidade mestre de organizações (`crm.companies`)

`Company` é a entidade central para todos os CNPJs, empresas, escolas, fornecedores, parceiros e entidades jurídicas do sistema. Todos os papéis futuros (Supplier, Partner, School, CorporateCustomer, BillingAccount, ServiceProvider) referenciarão `Company` por `CompanyId` — **nunca duplicar dados de organização em módulos separados**.

```
Company 1──0..1  Supplier          (FK em Supplier.CompanyId)
Company 1──0..1  Partner           (FK em Partner.CompanyId)
Company 1──0..1  School            (FK em School.CompanyId)
Company 1──0..1  CorporateCustomer (FK em CorporateCustomer.CompanyId)
Company 1──0..1  BillingAccount    (FK em BillingAccount.CompanyId)
Company 1──0..1  ServiceProvider   (FK em ServiceProvider.CompanyId)
```

Enums em `MyCRM.CRM.Domain.Entities`: `CompanyStatus`, `CompanyType`.

Estratégia de deduplicação:
- `RegistrationNumber` (CNPJ) é único por tenant (partial index, nullable).
- `Email` é único por tenant (partial index, nullable).

Address é owned value object (mesmo padrão de `Customer`).

### Student — entidade de papel para alunos (`crm.students`)

`Student` é a extensão de papel de `Person` para o contexto escolar. Não duplica dados pessoais.

```
Person 1──0..1  Student  (FK em Student.PersonId)
Student 1──0..* StudentGuardian (FK em StudentGuardian.StudentId)
```

Campos específicos: `RegistrationNumber`, `SchoolName`, `GradeOrClass`, `EnrollmentType`, `UnitId`, `ClassGroup`, `StartDate`, `Status` (StudentStatus), `Notes`, `AcademicObservation`.

Enums: `StudentStatus` (Active, Inactive, Graduated, Transferred, Suspended).

Restrição de unicidade: `(TenantId, PersonId)` único onde `IsDeleted = false` — uma pessoa não pode ter dois registros de aluno ativos.

### StudentGuardian — entidade de relacionamento aluno↔responsável (`crm.student_guardians`)

Entidade de relacionamento entre `Student` e `Person` (responsável). Armazena atributos do vínculo:
`RelationshipType` (GuardianRelationshipType), `IsPrimaryGuardian`, `IsFinancialResponsible`, `ReceivesNotifications`, `CanPickupChild`, `Notes`.

```
StudentGuardian *──1 Student  (FK em StudentGuardian.StudentId, cascade delete)
StudentGuardian *──1 Person   (FK em StudentGuardian.GuardianPersonId, restrict)
```

Enums: `GuardianRelationshipType` (Father, Mother, Grandmother, Grandfather, Uncle, Aunt, LegalGuardian, Other).

Restrição de unicidade: `(TenantId, StudentId, GuardianPersonId)` único onde `IsDeleted = false` — o mesmo responsável não pode ser adicionado duas vezes para o mesmo aluno.

**Design decision:** Não foi criada entidade separada `Guardian` — o responsável é representado diretamente por `Person`, seguindo o padrão de papéis via FK.

### Course — entidade mestre de ofertas educacionais (`crm.courses`)

`Course` representa qualquer programa ou oferta educacional estruturada (reforço escolar, inglês, turma, workshop, etc.).
Mantida genérica para suportar múltiplos contextos de negócio sem criar entidades paralelas.

```
Course 1──0..* StudentCourse  (FK em StudentCourse.CourseId)
```

Campos específicos: `Name`, `Code`, `Type` (CourseType), `Description`, `StartDate`, `EndDate`, `ScheduleDescription`, `Capacity`, `Workload`, `UnitId`, `Status` (CourseStatus), `IsActive`, `Notes`.

Enums: `CourseType` (AfterSchool, Language, SchoolClass, Workshop, Other), `CourseStatus` (Draft, Active, Inactive, Completed, Cancelled).

Restrição de unicidade: `(TenantId, Code)` único onde `Code IS NOT NULL`.

**Design decision:** `Course` é a fonte da verdade dos dados de programa. Dados específicos de matrícula (datas, turma, turno) ficam em `StudentCourse`. `IsActive` é um flag de conveniência gerenciado pelos métodos de domínio (`Activate`, `Deactivate`, `Complete`, `Cancel`).

### StudentCourse — entidade de relacionamento aluno↔curso (`crm.student_courses`)

Entidade de associação entre `Student` e `Course`. Armazena todos os atributos específicos da matrícula.

```
StudentCourse *──1 Student  (FK em StudentCourse.StudentId, restrict)
StudentCourse *──1 Course   (FK em StudentCourse.CourseId, restrict)
```

Campos específicos: `EnrollmentDate`, `StartDate`, `EndDate`, `CancellationDate`, `CompletionDate`, `Status` (StudentCourseStatus), `ClassGroup`, `Shift`, `ScheduleDescription`, `UnitId`, `Notes`.

Enums: `StudentCourseStatus` (Active, Pending, Completed, Cancelled, Suspended).

Restrição: re-matrícula histórica é permitida (mesmo aluno no mesmo curso em períodos diferentes). A lógica de negócio impede matrículas ativas/pendentes simultâneas para o mesmo par `(StudentId, CourseId)` dentro do mesmo tenant. O índice `(TenantId, StudentId, CourseId)` com filtro `IsDeleted = false` apoia esse controle.

**Design decision:** Ambas as FKs usam `DeleteBehavior.Restrict` — nem a exclusão de Student nem de Course deve apagar automaticamente o histórico de matrículas.

### Customer (`crm.customers`)

Entidade legada de CRM genérico. Futuramente será refatorada para referenciar `Person`.

---

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

## MIGRATIONS

### Comandos EF CLI (sempre com --startup-project)

```bash
# Criar migration
dotnet ef migrations add <Nome> \
  --project         "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext \
  --output-dir Migrations

# Aplicar
dotnet ef database update \
  --project         "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext

# Reverter para migration anterior
dotnet ef database update <NomeMigrationAnterior> \
  --project         "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext

# Remover última migration (não aplicada)
dotnet ef migrations remove \
  --project         "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext

# Gerar script SQL idempotente (produção)
dotnet ef migrations script \
  --project         "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext \
  --idempotent \
  --output migrations-crm.sql
```

### Armadilhas conhecidas

| Situação | Causa | Solução |
|---|---|---|
| `column "nome_coluna" does not exist` em índice parcial | `HasFilter` usando snake_case sem aspas | Usar `\"NomeColuna\"` (PascalCase com aspas duplas escapadas) |
| `schema "X" does not exist` no `RenameTable` | Snapshot desatualizado aponta para schema antigo | Reverter com `database update 0`, deletar arquivos de migration, recriar do zero |
| Seed não executa | `ResetMigrationsIfSchemaLostAsync` verificando schema incorreto → limpa history → migration falha antes do seed | Corrigir o schema verificado na função para o atual (`crm`) |
| Seed não executa (2) | Migration aplicada via `dotnet ef database update` — seed só roda via startup da aplicação | Subir a aplicação com `dotnet run` |

### Aplicação automática no startup

O `MigrateAllDbContextsAsync` em `MigrationExtensions.cs` aplica migrations e roda o seed automaticamente. O `ResetMigrationsIfSchemaLostAsync` deve verificar **sempre** o schema e tabela corretos do momento atual:

```csharp
// Manter sincronizado com o schema real
await ResetMigrationsIfSchemaLostAsync(customersDb, "crm", "customers");
```

---

## PADRÃO DE SEED

```csharp
public static class DataSeeder
{
    public static readonly Guid SeedTenantId = new("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(CRMDbContext db, ITenantService tenantService)
    {
        tenantService.SetTenant(SeedTenantId);
        await SeedPeopleAsync(db);
        await SeedCustomersAsync(db);
    }

    // Cada entidade tem seu próprio método privado com guard independente
    private static async Task SeedPeopleAsync(CRMDbContext db)
    {
        if (await db.People.AnyAsync()) return;
        // ...
        await db.SaveChangesAsync();
    }
}
```

Regras:
- Cada entidade tem guard próprio (`if (await db.Xxx.AnyAsync()) return`)
- `tenantService.SetTenant(SeedTenantId)` antes de qualquer operação
- Seed de People cobre todos os papéis futuros (guardians, students, employees, leads)

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

### Padrão de Queries GraphQL

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

### Padrão de Mutations GraphQL

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

### Checklist CRUD obrigatório

- [ ] `Get{Entidade}ByIdQuery` + Handler
- [ ] `GetAll{Entidades}Query` + Handler (`AsNoTracking`, retorna `IQueryable` ou lista)
- [ ] `Create{Entidade}Command` + Handler + Validator
- [ ] `Update{Entidade}Command` + Handler + Validator
- [ ] `Delete{Entidade}Command` + Handler (soft delete)
- [ ] `{Entidade}Dto` + mapping Mapster
- [ ] `{Entidade}Queries` no GraphQL (lista paginada + por ID)
- [ ] `{Entidade}Mutations` no GraphQL (create, update, delete)
- [ ] Inputs e payloads explícitos (`Create{Entidade}Input`, `Update{Entidade}Input`, `{Entidade}Payload`)
- [ ] Policies de autorização aplicadas (`{modulo}:{recurso}:read` e `{modulo}:{recurso}:write`)
- [ ] **`{Entidade}Queries` e `{Entidade}Mutations` registrados no `Program.cs` via `.AddTypeExtension<>()`**

---

## PADRÃO GRAPHQL

```csharp
[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class CrmMutations
{
    [Authorize(Policy = "crm:person:write")]
    public async Task<PersonPayload> CreatePersonAsync(
        CreatePersonInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreatePersonCommand(input), ct);
        return result.IsSuccess
            ? new PersonPayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }
}
```

Regras:
- Inputs e payloads explícitos — nunca expor entidade EF como tipo público
- `GetBy*` usa `FirstOrDefaultAsync` — nunca `FindAsync` (ignora query filters)
- DataLoader para N+1

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

## CHECKLIST ANTES DE ENTREGAR

**Antes de iniciar:**
- [ ] Qual módulo/domínio?
- [ ] A entidade pertence a um módulo existente ou precisa de módulo novo?
- [ ] Qual caso de uso?
- [ ] Qual policy de autorização?
- [ ] Há evento de domínio ou efeito assíncrono?
- [ ] A operação exige transação?

**Antes de finalizar:**
- [ ] Regra de negócio está no domínio/aplicação, não no resolver?
- [ ] Isolamento de tenant garantido?
- [ ] Validação com FluentValidation?
- [ ] Autorização explícita?
- [ ] `AsNoTracking()` nas leituras?
- [ ] Soft delete (nunca hard delete)?
- [ ] Repositório registrado no `DependencyInjection.cs`?
- [ ] Entidade adicionada ao DbContext (`DbSet`, `HasQueryFilter`, `SaveChangesAsync`)?
- [ ] Migration gerada e testada?
- [ ] `ResetMigrationsIfSchemaLostAsync` verificando schema/tabela corretos?
- [ ] **Skill `core.md` atualizada com novos conhecimentos?**

---

## FLUXO OBRIGATÓRIO PARA TESTES DE CONSULTA GRAPHQL

**Toda vez que for testar uma query ou mutation GraphQL, seguir este fluxo sem exceção:**

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
