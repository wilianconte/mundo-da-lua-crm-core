# Skill: Arquitetura Core — MyCRM

Você está trabalhando no backend do **MyCRM**. Aplique rigorosamente as regras abaixo em toda decisão de código.

---

## ESTRUTURA DO PROJETO (estado atual)

**Solução:** `mundo-da-lua-core/MyCRM.sln`

**Convenção de nomes de projeto:** `MyCRM.{Modulo}.{Camada}` (ex: `MyCRM.Customers.Domain`)

**Diretórios físicos** (os diretórios mantêm nomes antigos por restrição do C# LS, mas os `.csproj` e namespaces já usam `MyCRM`):

| Projeto (nome) | Caminho físico do .csproj |
|---|---|
| `MyCRM.GraphQL` | `1 - Gateway/MundoDaLua.GraphQL/MyCRM.GraphQL.csproj` |
| `MyCRM.Customers.Domain` | `2 - Modules/Customers/Customers.Domain/MyCRM.Customers.Domain.csproj` |
| `MyCRM.Customers.Application` | `2 - Modules/Customers/Customers.Application/MyCRM.Customers.Application.csproj` |
| `MyCRM.Customers.Infrastructure` | `2 - Modules/Customers/Customers.Infrastructure/MyCRM.Customers.Infrastructure.csproj` |
| `MyCRM.Shared.Kernel` | `3 - Shared/Shared.Kernel/MyCRM.Shared.Kernel.csproj` |
| `MyCRM.UnitTests` | `4 - Tests/UnitTests/MyCRM.UnitTests.csproj` |

**Namespaces corretos:**
- `MyCRM.GraphQL.*`
- `MyCRM.Customers.Domain.*`
- `MyCRM.Customers.Application.*`
- `MyCRM.Customers.Infrastructure.*`
- `MyCRM.Shared.Kernel.*`
- `MyCRM.UnitTests.*`

**Nota:** Ao adicionar novos módulos, o padrão é `MyCRM.{Modulo}.{Domain|Application|Infrastructure}`.

---

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
2 - Modules/{Modulo}/
├── {Modulo}.Domain          ← entidades, value objects, agregados, eventos, interfaces de repositório
├── {Modulo}.Application     ← commands, queries, handlers, validators, DTOs
├── {Modulo}.Infrastructure  ← DbContext, repositórios, migrations, serviços externos
└── {Modulo}.GraphQL         ← queries, mutations, inputs, payloads, tipos HC
```

Regras de dependência (setas = "depende de"):
```
GraphQL → Application → Domain
Infrastructure → Application → Domain
Shared.Kernel ← todos
```

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
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }
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
- Factory methods estáticos para criação (`Customer.Create(...)`)
- Setters `private` — mutação apenas por métodos de domínio

---

## PADRÃO DE COMMAND/HANDLER

```csharp
// Command — record imutável
public record CriarAlunoCommand(string Nome, string Email, Guid TurmaId)
    : IRequest<Result<AlunoDto>>;

// Validator — sempre junto ao command
public sealed class CriarAlunoValidator : AbstractValidator<CriarAlunoCommand>
{
    public CriarAlunoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
    }
}

// Handler
public sealed class CriarAlunoHandler : IRequestHandler<CriarAlunoCommand, Result<AlunoDto>>
{
    public async Task<Result<AlunoDto>> Handle(CriarAlunoCommand request, CancellationToken ct)
    {
        // 1. validações de negócio (duplicatas, regras)
        // 2. criar via factory method do agregado
        // 3. persistir
        // 4. retornar Result<Dto>
    }
}
```

---

## PADRÃO GRAPHQL

```csharp
// Mutation — delegar para MediatR, sem lógica inline
[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class EscolaMutations
{
    [Authorize(Policy = "escola:aluno:write")]
    public async Task<AlunoPayload> CriarAlunoAsync(
        CriarAlunoInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CriarAlunoCommand(input), ct);
        return result.IsSuccess
            ? new AlunoPayload(result.Value!)
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
- Usar `[UsePaging]`, `[UseProjection]`, `[UseFiltering]`, `[UseSorting]` apenas onde fizer sentido
- `GetBy*` usa `FirstOrDefaultAsync` — nunca `FindAsync` (ignora query filters)
- DataLoader para N+1

---

## MULTI-TENANCY

```csharp
// Query filter obrigatório em todo DbContext tenant-aware
modelBuilder.Entity<Aluno>()
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
// Handlers sempre retornam Result<T> ou Result
Result<T>.Success(value)
Result<T>.Failure("CODIGO_ERRO", "mensagem")
Result.Success()
Result.Failure("CODIGO_ERRO", "mensagem")

// Erro de validação vem do ValidationBehavior automaticamente
// código: "VALIDATION_ERROR"
```

---

## MULTI-TENANCY — SCHEMAS POR MÓDULO

| Schema PostgreSQL | Módulo |
|---|---|
| `auth` | usuários, roles, claims, tenants, refresh tokens |
| `escola` | alunos, turmas, matrículas, frequência, notas |
| `clinica` | pacientes, agenda, prontuários, profissionais |
| `financeiro` | contratos, cobranças, parcelas, pagamentos |

Cada módulo tem seu próprio `DbContext` e suas próprias migrations.

---

## AUTORIZAÇÃO

Modelo de permissão:
```
{modulo}:{recurso}:{operacao}
```
Exemplos: `escola:aluno:read`, `clinica:agenda:write`, `financeiro:cobranca:approve`

- Toda mutation sensível: `[Authorize(Policy = "...")]`
- Toda query sensível: `[Authorize(Policy = "...")]`
- Autorização no backend — nunca dependente de visibilidade na UI

---

## CHECKLIST ANTES DE ENTREGAR

**Antes de iniciar:**
- [ ] Qual módulo/domínio?
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
- [ ] Testes dos cenários críticos?

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