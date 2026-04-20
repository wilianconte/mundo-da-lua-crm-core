---
name: core
description: Arquitetura e padrões obrigatórios do backend MyCRM. Use SEMPRE que estiver implementando entidades, handlers CQRS, queries ou mutations GraphQL, migrations EF Core, seeds, repositórios, ou qualquer tarefa de backend no projeto mundo-da-lua-crm-core. Auto-ativa para qualquer tarefa envolvendo C#, EF Core, Hot Chocolate, MediatR, DDD, CQRS ou Clean Architecture neste projeto.
when_to_use: >
  Carregar antes de escrever qualquer código backend: entidades, handlers, mutations, queries,
  migrations, seeds, repositórios, configurações EF, DI, ou qualquer artefato C# do projeto.
---

# Arquitetura Core — MyCRM

Você está trabalhando no backend do **MyCRM**. Aplique rigorosamente as regras abaixo.

**Referências detalhadas:**
- `references/entities.md` — entidades existentes, campos, enums, restrições, design decisions
- `references/patterns.md` — padrões de código (entidades, EF, DbContext, DI, CQRS, GraphQL, multi-tenancy, anti-padrões)
- `references/migrations.md` — comandos EF CLI, armadilhas conhecidas, seed, migration obrigatória
- `references/checklists.md` — checklists de CRUD e status de implementação por entidade

**Regras de negócio (fonte de verdade funcional):**
`D:\Dev\Mundo da Lua\mundo-da-lua-crm\mundo-da-lua-crm-rules\docs\`
Use `/consultar-regras RN-XXX` antes de implementar qualquer funcionalidade referenciada.

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

## ESTRUTURA DO PROJETO

**Solução:** `MyCRM.sln` — convenção: `MyCRM.{Modulo}.{Camada}`

| Projeto | Caminho do .csproj | Namespace raiz |
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

**Estrutura de módulo:**
```
2 - {Modulo}/
├── {Modulo}.Domain          ← entidades, enums, value objects, interfaces de repositório
├── {Modulo}.Application     ← commands, queries, handlers, validators, DTOs
└── {Modulo}.Infrastructure  ← DbContext, repositórios, migrations, seed, configurações EF
```

**Regras de dependência:**
```
GraphQL → Application → Domain
Infrastructure → Application → Domain
Shared.Kernel ← todos
```

**Registro no Program.cs** ao criar nova entidade:
```csharp
builder.Services
    .AddGraphQLServer()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.{Modulo}.{Entidade}Queries>()
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.{Modulo}.{Entidade}Mutations>()
    .AddType<MyCRM.GraphQL.GraphQL.{Modulo}.{Entidade}ObjectType>() // só se precisar ocultar campos

builder.Services.Add{Modulo}Application();
builder.Services.Add{Modulo}Infrastructure(builder.Configuration);
```

---

## SCHEMAS POSTGRESQL

| Schema | DbContext | Tabelas | Módulo |
|---|---|---|---|
| `crm` | `CRMDbContext` | `customers`, `people`, `companies`, `students`, `student_guardians`, `courses`, `student_courses`, `employees`, `wallets`, `transactions`, `transaction_types`, `categories`, `payment_methods`, `reconciliations` | CRM |
| `auth` | `AuthDbContext` | `users`, `roles`, `user_roles`, `permissions`, `role_permissions`, `refresh_tokens`, `tenants` | Auth |

Schemas planejados: `escola`, `clinica`, `rh` — cada um com DbContext próprio e migrations separadas.

---

## ENTIDADES EXISTENTES

Leia `references/entities.md` para documentação completa.

| Entidade | Tabela | Papel |
|---|---|---|
| `Person` | `crm.people` | Identidade mestre de todos os indivíduos |
| `Company` | `crm.companies` | Identidade mestre de todas as organizações |
| `Student` | `crm.students` | Papel de `Person` no contexto escolar |
| `StudentGuardian` | `crm.student_guardians` | Vínculo aluno↔responsável |
| `Course` | `crm.courses` | Oferta educacional estruturada |
| `StudentCourse` | `crm.student_courses` | Matrícula aluno↔curso |
| `Employee` | `crm.employees` | Papel de `Person` no contexto de RH |
| `Customer` | `crm.customers` | Entidade legada de CRM genérico |
| `Wallet` | `crm.wallets` | Carteira/conta financeira do tenant |
| `Transaction` | `crm.transactions` | Lançamento financeiro (receita/despesa/transferência) |
| `TransactionType` | `crm.transaction_types` | Tipo de transação configurável |
| `Category` | `crm.categories` | Categoria de classificação de transações |
| `PaymentMethod` | `crm.payment_methods` | Forma de pagamento configurável |
| `Reconciliation` | `crm.reconciliations` | Conciliação de transação com extrato |
| `Tenant` | `auth.tenants` | Conta cliente isolada (raiz do multi-tenant) |

---

## RBAC (obrigatório para toda nova entidade)

- Nunca `[Authorize]` genérico em resolver de negócio — sempre `[Authorize(Policy = SystemPermissions.Xxx)]`
- Ao criar recurso protegido:
  1. Constante em `SystemPermissions`
  2. Registrar em `SystemPermissions.All` com grupo
  3. Aplicar policy em Query e Mutation
  4. Validar seed de permissões
- Convenção: entidades de negócio → `read/create/update/delete`; entidades admin → `manage`
- O seed garante que o role `Administrador` tenha todas as permissões ativas
