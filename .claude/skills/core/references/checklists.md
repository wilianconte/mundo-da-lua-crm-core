# Checklists — MyCRM

## CHECKLIST ANTES DE INICIAR

- [ ] **Branch criada?** (`git checkout -b claude/<descricao>`)
- [ ] Qual módulo/domínio?
- [ ] A entidade pertence a um módulo existente ou precisa de módulo novo?
- [ ] Qual caso de uso?
- [ ] Qual policy de autorização?
- [ ] Há evento de domínio ou efeito assíncrono?
- [ ] A operação exige transação?

---

## CHECKLIST ANTES DE FINALIZAR

- [ ] Regra de negócio está no domínio/aplicação, não no resolver?
- [ ] Isolamento de tenant garantido?
- [ ] Validação com FluentValidation?
- [ ] Autorizacao explicita por policy? (`[Authorize(Policy = SystemPermissions.Xxx)]`; evitar `[Authorize]` generico em operacoes de negocio)
- [ ] `AsNoTracking()` nas leituras?
- [ ] Soft delete (nunca hard delete)?
- [ ] Repositório registrado no `DependencyInjection.cs`?
- [ ] Entidade adicionada ao DbContext (`DbSet`, `HasQueryFilter`, `SaveChangesAsync`)?
- [ ] **Migration criada e aplicada (se houve alteração de entidade)?** ← OBRIGATÓRIO
- [ ] `ResetMigrationsIfSchemaLostAsync` verificando schema/tabela corretos?
- [ ] **Seed criado ou atualizado para a nova entidade?** ← OBRIGATÓRIO (ver regra abaixo)
- [ ] **Seeds existentes revisados** — a mudança quebra algum seed já implementado?
- [ ] **Skill `core` atualizada com novos conhecimentos?**
- [ ] Permissões normalizadas no backend (`Trim + ToLowerInvariant + remove vazios/duplicados`)?
- [ ] Regressão RBAC non-admin com e sem permissão nas mutations principais coberta?
- [ ] **Testes escritos e passando para todos os novos handlers e resolvers?** ← OBRIGATÓRIO (ver regra abaixo)

---

## REGRA OBRIGATÓRIA — TESTES PARA TODA NOVA FUNCIONALIDADE

**Toda implementação deve ter testes antes de encerrar a tarefa — sem precisar que o usuário peça.**

### O que testar obrigatoriamente

| Camada | O que criar | Localização |
|---|---|---|
| Handler (command) | Casos: success, not found, duplicate, validações de negócio | `4 - Tests/UnitTests/{Dominio}/` |
| GraphQL Queries | Schema (campo existe), auth (sem token nega), paginação, filtros, soft-delete | `4 - Tests/UnitTests/GraphQL/{Entidade}QueryTests.cs` |
| GraphQL Mutations (RBAC) | Com permissão ✅, sem permissão ❌, cross-contamination guard | `4 - Tests/UnitTests/GraphQL/AllEntitiesRbacRegressionTests.cs` |

### Padrões de teste existentes a seguir

- **Handler tests**: ver `DeleteRoleHandlerTests.cs`, `UpdateRoleHandlerTests.cs`, `UpdateTenantHandlerTests.cs`
- **Query tests**: ver `UserQueryTests.cs`, `TenantQueryTests.cs`
- **RBAC mutations**: ver `AllEntitiesRbacRegressionTests.cs` — adicionar o novo `{Entidade}Mutations` ao `BuildExecutorAsync` e criar os casos `With/WithoutPermission`

### Ao finalizar qualquer implementação

1. Rodar `dotnet test` — zero falhas obrigatório antes do commit
2. Se algum teste existente quebrar, identificar a causa e corrigir (não ignorar)
3. Se um campo novo obrigatório foi adicionado a um input existente, atualizar TODAS as mutation strings nos testes que usam aquele input

---

## CHECKLIST RBAC POR ENTIDADE (MUTATIONS)

- [ ] `students`: create/update/delete com autorização validada
- [ ] `student_guardians`: create/update/delete com autorização validada
- [ ] `student_courses`: create/update/delete com autorização validada
- [ ] `customers`: create/update/delete com autorização validada
- [ ] `employees`: create/update/delete com autorização validada
- [ ] `courses`: create/update/delete com autorização validada
- [ ] `people`: create/update/delete com autorização validada
- [ ] `companies`: create/update/delete com autorização validada
- [ ] `users`: create/update/delete com `users:manage`
- [ ] `roles`: create/update/delete com `roles:manage`
- [ ] `tenants`: update/delete com `tenants:manage`
- [ ] Guardas de cross-contamination entre entidades/permissões críticas validados

---

## REGRA OBRIGATÓRIA — SEED PARA TODA NOVA ENTIDADE

**Toda nova entidade deve ter dados de seed antes de encerrar a tarefa — sem precisar que o usuário peça.**

### O que fazer ao criar uma nova entidade

1. **Criar `Seed{Entidades}Async`** no `DataSeeder` do módulo correspondente
2. **Registrar a chamada em `SeedAsync`** na ordem correta (respeitando dependências de FK)
3. **Atualizar a tabela "Estado do seed"** em `references/migrations.md`
4. **Revisar seeds existentes** — se a nova entidade é referenciada por seeds já implementados (ex: nova FK obrigatória), atualizar os registros existentes para incluir o novo campo

### Quando revisar seeds existentes

| Mudança | Revisar seeds? |
|---|---|
| Nova entidade sem FK para entidade com seed | ❌ não impacta |
| Nova FK obrigatória em entidade com seed | ✅ atualizar registros do seed |
| Nova FK nullable em entidade com seed | ⚠️ avaliar se dados de seed devem preencher o vínculo |
| Alteração de enum usado no seed | ✅ verificar se os valores ainda existem |
| Remoção de campo usado no seed | ✅ remover do seed para não quebrar compilação |

### Exemplo — seed com dependência de FK

```csharp
// Ordem correta: People antes de Students (FK Student.PersonId → Person.Id)
public static async Task SeedAsync(CRMDbContext db, ITenantService tenantService)
{
    tenantService.SetTenant(SeedTenantId);
    await SeedPeopleAsync(db);     // 1º — sem dependências
    await SeedStudentsAsync(db);   // 2º — depende de People
}
```

---

## CHECKLIST CRUD OBRIGATÓRIO POR ENTIDADE

Toda nova entidade deve ter todos os artefatos abaixo antes de ser considerada completa:

### Application Layer

- [ ] `Get{Entidade}ByIdQuery` + Handler
- [ ] `GetAll{Entidades}Query` + Handler (`AsNoTracking`, retorna `IQueryable` ou lista)
- [ ] `Create{Entidade}Command` + Handler + Validator
- [ ] `Update{Entidade}Command` + Handler + Validator
- [ ] `Delete{Entidade}Command` + Handler (soft delete)
- [ ] `{Entidade}Dto` + mapping Mapster

### GraphQL Layer

- [ ] `{Entidade}Queries` no GraphQL (lista paginada + por ID)
- [ ] `{Entidade}Mutations` no GraphQL (create, update, delete)
- [ ] Inputs e payloads explícitos (`Create{Entidade}Input`, `Update{Entidade}Input`, `{Entidade}Payload`)
- [ ] Policies de autorizacao aplicadas (`read/create/update/delete` quando houver CRUD)
- [ ] **`{Entidade}Queries` e `{Entidade}Mutations` registrados no `Program.cs` via `.AddTypeExtension<>()`**

### Infrastructure

- [ ] `IEntityTypeConfiguration<{Entidade}>` criada
- [ ] `DbSet<{Entidade}>` no DbContext
- [ ] `HasQueryFilter` no DbContext
- [ ] Bloco de injeção de `TenantId` no `SaveChangesAsync`
- [ ] Migration criada e aplicada
- [ ] **`Seed{Entidades}Async` criado no `DataSeeder` e registrado em `SeedAsync`**
- [ ] **Seeds existentes revisados** para garantir que mudanças não quebrem dados de dev

---

## STATUS DE IMPLEMENTAÇÃO POR ENTIDADE

| Entidade | CRUD App | GraphQL | Migration | Seed |
|---|---|---|---|---|
| `Customer` | ✅ | ✅ | ✅ | ✅ |
| `Person` | ✅ | ✅ | ✅ | ✅ |
| `Company` | ✅ | ✅ | ✅ | ✅ |
| `Course` | ✅ | ✅ | ✅ | ✅ |
| `Student` | ✅ | ✅ | ✅ | ✅ |
| `StudentGuardian` | ✅ | ✅ | ✅ | ✅ |
| `StudentCourse` | ✅ | ✅ | ✅ | ✅ |
| `Employee` | ✅ | ✅ | ✅ | ✅ |
| `Tenant` | ✅ (Register/Update/Delete) | ✅ | ✅ | ✅ (via AuthDataSeeder) |
| `Wallet` | ✅ | ✅ | ✅ | ✅ |
| `Transaction` | ✅ | ✅ | ✅ | ✅ |
| `TransactionType` | ✅ | ✅ | ✅ | ✅ |
| `Category` | ✅ | ✅ | ✅ | ✅ |
| `PaymentMethod` | ✅ | ✅ | ✅ | ✅ |
| `Reconciliation` | ✅ | ✅ | ✅ | ✅ |

