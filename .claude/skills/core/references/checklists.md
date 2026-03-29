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
- [ ] Autorização explícita? (`[Authorize]` na classe — nunca verificação manual via IHttpContextAccessor)
- [ ] `AsNoTracking()` nas leituras?
- [ ] Soft delete (nunca hard delete)?
- [ ] Repositório registrado no `DependencyInjection.cs`?
- [ ] Entidade adicionada ao DbContext (`DbSet`, `HasQueryFilter`, `SaveChangesAsync`)?
- [ ] **Migration criada e aplicada (se houve alteração de entidade)?** ← OBRIGATÓRIO
- [ ] `ResetMigrationsIfSchemaLostAsync` verificando schema/tabela corretos?
- [ ] **Seed criado ou atualizado para a nova entidade?** ← OBRIGATÓRIO (ver regra abaixo)
- [ ] **Seeds existentes revisados** — a mudança quebra algum seed já implementado?
- [ ] **Skill `core` atualizada com novos conhecimentos?**

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
- [ ] Policies de autorização aplicadas (`{modulo}:{recurso}:read` e `{modulo}:{recurso}:write`)
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
