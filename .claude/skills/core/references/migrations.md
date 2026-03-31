# Migrations e Seed — MyCRM

## REGRA OBRIGATÓRIA — MIGRATION APÓS ALTERAÇÃO DE ENTIDADE

**Sempre que uma implementação criar, alterar ou remover uma entidade de domínio (propriedades, relacionamentos, índices, configurações EF), os dois passos abaixo são OBRIGATÓRIOS antes de encerrar a tarefa — sem exceção e sem esperar que o usuário peça:**

### Quando esta regra se aplica

| Mudança | Cria migration? |
|---|---|
| Nova entidade | ✅ sim |
| Nova propriedade em entidade existente | ✅ sim |
| Remoção de propriedade | ✅ sim |
| Novo relacionamento (FK) | ✅ sim |
| Novo índice | ✅ sim |
| Alteração em `IEntityTypeConfiguration` | ✅ sim |
| Seed de dados apenas (`DataSeeder.cs`) | ❌ não |
| Handlers, DTOs, GraphQL resolvers | ❌ não |

### Se o build estiver quebrado

Não é possível criar ou aplicar migration com build quebrado. Corrija o build primeiro, depois execute os dois passos.

### Se a migration detectar mudanças inesperadas

Inspecionar o arquivo `.cs` gerado antes de aplicar. Se contiver alterações não relacionadas à implementação atual, investigar a causa (snapshot desatualizado, entidade esquecida em migration anterior) antes de prosseguir.

---

## COMANDOS EF CLI (sempre com --startup-project)

### Módulo CRM (`CRMDbContext` — schema `crm`)

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

### Módulo Auth (`AuthDbContext` — schema `auth`)

```bash
# Criar migration
dotnet ef migrations add <Nome> \
  --project         "3 - Auth/Auth.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context AuthDbContext \
  --output-dir Migrations

# Aplicar
dotnet ef database update \
  --project         "3 - Auth/Auth.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context AuthDbContext

# Reverter
dotnet ef database update <NomeMigrationAnterior> \
  --project         "3 - Auth/Auth.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context AuthDbContext

# Remover última migration (não aplicada)
dotnet ef migrations remove \
  --project         "3 - Auth/Auth.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context AuthDbContext

# Gerar script SQL idempotente (produção)
dotnet ef migrations script \
  --project         "3 - Auth/Auth.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context AuthDbContext \
  --idempotent \
  --output migrations-auth.sql
```

**Nomear a migration de forma descritiva:** `AddCourseEntity`, `AddWorkloadToCourse`, `AddStudentGuardianRelationship`, etc.

---

## ARMADILHAS CONHECIDAS

| Situação | Causa | Solução |
|---|---|---|
| `column "nome_coluna" does not exist` em índice parcial | `HasFilter` usando snake_case sem aspas | Usar `\"NomeColuna\"` (PascalCase com aspas duplas escapadas) |
| `schema "X" does not exist` no `RenameTable` | Snapshot desatualizado aponta para schema antigo | Reverter com `database update 0`, deletar arquivos de migration, recriar do zero |
| Seed não executa | `ResetMigrationsIfSchemaLostAsync` verificando schema incorreto → limpa history → migration falha antes do seed | Corrigir o schema verificado na função para o atual (`crm`) |
| Seed não executa (2) | Migration aplicada via `dotnet ef database update` — seed só roda via startup da aplicação | Subir a aplicação com `dotnet run` |
| `error CS0234: 'EntityFrameworkCore' does not exist` em `CRM.Application` | Query handlers em `Application` usam `using Microsoft.EntityFrameworkCore` mas o projeto não referencia EF Core (só `Infrastructure` deve referenciar) | Remover o `using` de EF Core dos handlers em `Application`, usar abstrações de repositório em vez de `DbContext` diretamente — é um erro de arquitetura pré-existente no projeto |

---

## APLICAÇÃO AUTOMÁTICA NO STARTUP

O `MigrateAllDbContextsAsync` em `MigrationExtensions.cs` aplica migrations e roda o seed automaticamente. O `ResetMigrationsIfSchemaLostAsync` deve verificar **sempre** o schema e tabela corretos do momento atual:

```csharp
// Manter sincronizado com o schema real
await ResetMigrationsIfSchemaLostAsync(customersDb, "crm", "customers");
```

Ordem recomendada no startup para RBAC:

1. `PermissionSeeder.SeedAsync(authDb)` para sincronizar permissões do sistema.
2. `AuthDataSeeder.SeedAsync(authDb, ...)` para garantir role admin + usuário admin.

Isso evita ambiente em que o role `Administrador` exista sem permissões vinculadas.

---

## REGRA OBRIGATÓRIA — SEED PARA TODA NOVA ENTIDADE

**Toda nova entidade deve ter `Seed{Entidades}Async` implementado e registrado em `SeedAsync` antes de encerrar a tarefa.**

Além disso, **revisar os seeds existentes** sempre que:
- Uma nova FK obrigatória for adicionada a entidade que já tem seed → atualizar os registros
- Um enum usado no seed for alterado → verificar se os valores ainda são válidos
- Um campo usado no seed for removido → remover do seed para não quebrar compilação

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
        await SeedCompaniesAsync(db);
        await SeedCoursesAsync(db);
    }

    // Cada entidade tem seu próprio método privado com guard independente
    private static async Task SeedPeopleAsync(CRMDbContext db)
    {
        if (await db.People.AnyAsync()) return;
        // ...
        await db.SaveChangesAsync();
    }

    // Helpers para aplicar estado de domínio no seed sem duplicar lógica
    private static Course CreateActive(Course course) { course.Publish(); return course; }
    private static Course CreateCompleted(Course course) { course.Publish(); course.Complete(); return course; }
}
```

Regras:
- Cada entidade tem guard próprio (`if (await db.Xxx.AnyAsync()) return`)
- `tenantService.SetTenant(SeedTenantId)` antes de qualquer operação
- Seed de People cobre todos os papéis futuros (guardians, students, employees, leads)
- Para aplicar estado de domínio no seed (Active, Completed, etc.), usar métodos helper privados que chamam os domain methods — não definir estado diretamente via propriedade

### Estado do seed (ordem de execução em `SeedAsync`)

| Ordem | Método | Entidade | Status |
|---|---|---|---|
| 1 | `SeedPeopleAsync` | `Person` | ✅ implementado |
| 2 | `SeedCustomersAsync` | `Customer` | ✅ implementado |
| 3 | `SeedCompaniesAsync` | `Company` | ✅ implementado |
| 4 | `SeedCoursesAsync` | `Course` | ✅ implementado |
| 5 | `SeedEmployeesAsync` | `Employee` | ✅ implementado |

### Dados de seed de Courses

10 cursos cobrindo todos os `CourseType`:

| Código | Nome | Tipo | Status |
|---|---|---|---|
| `REF-FI-2025` | Reforço Escolar — Fundamental I | AfterSchool | Active |
| `REF-FII-2025` | Reforço Escolar — Fundamental II | AfterSchool | Active |
| `ING-A1-2025-1` | Inglês — Nível A1 | Language | Active |
| `ING-A2-2025-1` | Inglês — Nível A2 | Language | Active |
| `ESP-B1-2025` | Espanhol Básico | Language | Draft |
| `T3A-2025` | Turma 3º Ano A — 2025 | SchoolClass | Active |
| `T6A-2025` | Turma 6º Ano A — 2025 | SchoolClass | Active |
| `WKS-TEA-2024` | Workshop de Teatro Infantil | Workshop | Completed |
| `WKS-ROB-2025-1` | Workshop de Robótica e Programação | Workshop | Active |
| `OFI-LEC-2025` | Oficina de Leitura e Escrita Criativa | Other | Active |
