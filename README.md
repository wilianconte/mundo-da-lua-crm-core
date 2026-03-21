# MyCRM — Mundo da Lua

Backend do sistema de gestão da **Mundo da Lua** — plataforma que centraliza serviços educacionais, atendimento clínico, financeiro e RH em uma única API GraphQL.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 9 |
| API | GraphQL — Hot Chocolate 15 |
| Arquitetura | Clean Architecture + DDD + CQRS |
| Banco de dados | PostgreSQL |
| ORM | Entity Framework Core 9 + Npgsql |
| Mediação | MediatR |
| Validação | FluentValidation |
| Mapeamento | Mapster |
| Autenticação | JWT Bearer |

---

## Estrutura do projeto

```
mundo-da-lua-crm-core/
├── 1 - Gateway/
│   └── MundoDaLua.GraphQL/          ← entrada da aplicação (Program.cs)
├── 2 - CRM/
│   ├── CRM.Domain/                  ← entidades, enums, interfaces de repositório
│   ├── CRM.Application/             ← commands, queries, handlers, validators, DTOs
│   └── CRM.Infrastructure/          ← DbContext, repositórios, migrations, seed
├── 3 - Auth/
│   ├── Auth.Domain/
│   ├── Auth.Application/
│   └── Auth.Infrastructure/
├── 3 - Shared/
│   └── Shared.Kernel/               ← BaseEntity, TenantEntity, Result<T>, IRepository
├── 4 - Tests/
│   └── UnitTests/
├── Dockerfile
└── MyCRM.sln
```

---

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

---

## Configuração

### Variáveis de ambiente (produção)

Nunca commite credenciais no `appsettings.json`. Em produção, injete via variáveis de ambiente:

```bash
ConnectionStrings__DefaultConnection="Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
Jwt__Key="chave-secreta-minimo-256-bits"
Jwt__Issuer="mundo-da-lua-crm"
Jwt__Audience="mundo-da-lua-crm-clients"
```

### Desenvolvimento local (User Secrets)

```bash
cd "1 - Gateway/MundoDaLua.GraphQL"

dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Database=mycrm_dev;Username=postgres;Password=postgres"

dotnet user-secrets set "Jwt:Key" "dev-secret-key-minimo-256-bits-aqui!!"
```

### appsettings.json (valores padrão não-sensíveis)

```json
{
  "Jwt": {
    "Issuer": "mundo-da-lua-crm",
    "Audience": "mundo-da-lua-crm-clients",
    "ExpiresInMinutes": 60,
    "RefreshExpiresInDays": 7
  }
}
```

---

## Migrations

### Aplicação automática na inicialização

As migrations são aplicadas automaticamente quando a aplicação sobe, via `MigrateAllDbContextsAsync()` em `Program.cs`. Isso vale tanto em desenvolvimento quanto em produção (Docker).

```
dotnet run --project "1 - Gateway/MundoDaLua.GraphQL"
# → aplica migrations pendentes
# → executa seed de dados
```

---

### Criar nova migration (manual)

Use sempre `--project` (onde está o DbContext) e `--startup-project` (entrada da aplicação).

**CRMDbContext** — schema `crm`:

```bash
dotnet ef migrations add <NomeDaMigration> \
  --project     "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext \
  --output-dir Migrations
```

**AuthDbContext** — schema `auth`:

```bash
dotnet ef migrations add <NomeDaMigration> \
  --project     "3 - Auth/Auth.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context AuthDbContext \
  --output-dir Migrations
```

---

### Aplicar migrations manualmente

```bash
# CRM
dotnet ef database update \
  --project     "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext

# Auth
dotnet ef database update \
  --project     "3 - Auth/Auth.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context AuthDbContext
```

---

### Reverter uma migration

```bash
# Voltar para a migration anterior (substitua <NomeDaMigrationAnterior>)
dotnet ef database update <NomeDaMigrationAnterior> \
  --project     "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext

# Remover a última migration ainda não aplicada ao banco
dotnet ef migrations remove \
  --project     "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext
```

---

### Listar migrations e status

```bash
dotnet ef migrations list \
  --project     "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext
```

---

### Gerar script SQL (para aplicar em produção sem acesso direto ao banco)

```bash
dotnet ef migrations script \
  --project     "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext \
  --idempotent \
  --output migrations-crm.sql

dotnet ef migrations script \
  --project     "3 - Auth/Auth.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context AuthDbContext \
  --idempotent \
  --output migrations-auth.sql
```

> `--idempotent` gera um script seguro para ser aplicado múltiplas vezes (verifica `__EFMigrationsHistory` antes de cada migration).

---

## Executar localmente

```bash
# Restaurar dependências
dotnet restore

# Rodar (migrations + seed aplicados automaticamente)
dotnet run --project "1 - Gateway/MundoDaLua.GraphQL"
```

A API estará disponível em:
- GraphQL Playground: `http://localhost:5000/graphql`

---

## Docker

### Build

```bash
docker build -t mycrm-api .
```

### Run

```bash
docker run -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Host=...;Database=...;Username=...;Password=...;SSL Mode=Require" \
  -e "Jwt__Key=chave-secreta-minimo-256-bits" \
  -e "Jwt__Issuer=mundo-da-lua-crm" \
  -e "Jwt__Audience=mundo-da-lua-crm-clients" \
  mycrm-api
```

A API estará disponível em `http://localhost:8080/graphql`.

> Migrations e seed são aplicados automaticamente ao iniciar o container.

---

## Seed de dados

O seed é executado automaticamente na inicialização (apenas se o banco estiver vazio).

| Módulo | Dados |
|---|---|
| **People** | 10 pessoas cobrindo todos os papéis futuros (alunos, responsáveis, funcionários, leads) |
| **Customers** | 5 clientes (pessoas físicas e jurídicas) |
| **Auth** | 1 usuário administrador — `admin@mundodalua.com` / `Admin@123` |

> Troque a senha do administrador imediatamente após o primeiro deploy em produção.

---

## Schemas do banco

| Schema PostgreSQL | Módulo | DbContext |
|---|---|---|
| `crm` | Clientes, Pessoas | `CRMDbContext` |
| `auth` | Usuários, autenticação | `AuthDbContext` |

---

## Módulos planejados

| Schema | Módulo |
|---|---|
| `escola` | Alunos, turmas, matrículas, frequência |
| `clinica` | Pacientes, agenda, prontuários |
| `financeiro` | Contratos, cobranças, pagamentos |
| `rh` | Funcionários, contratos de trabalho |
