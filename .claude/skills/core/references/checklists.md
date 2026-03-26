# Checklists — MyCRM

## CHECKLIST ANTES DE INICIAR

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
- [ ] Autorização explícita?
- [ ] `AsNoTracking()` nas leituras?
- [ ] Soft delete (nunca hard delete)?
- [ ] Repositório registrado no `DependencyInjection.cs`?
- [ ] Entidade adicionada ao DbContext (`DbSet`, `HasQueryFilter`, `SaveChangesAsync`)?
- [ ] **Migration criada e aplicada (se houve alteração de entidade)?** ← OBRIGATÓRIO
- [ ] `ResetMigrationsIfSchemaLostAsync` verificando schema/tabela corretos?
- [ ] **Skill `core` atualizada com novos conhecimentos?**

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
| `Employee` | ✅ | ✅ | ✅ | ⏳ seed pendente |
