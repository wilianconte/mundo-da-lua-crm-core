# Fix: Seed TenantPlan Free para Tenants Existentes (RN-029.1)

**Data:** 2026-04-15
**Branch:** claude/subscription-plans
**Input:** `build/input/2026-04-15-fix-tenants-sem-plano.md`
**Decisão arquitetural:** DEC-008

---

## Resumo executivo

Migration de dados criada e aplicada para vincular tenants existentes (criados antes do DEC-008)
ao plano Free permanente, corrigindo violação de RN-029.1.
Verificação pós-aplicação confirmou 0 tenants sem plano ativo.

---

## O que foi feito

### Migration `SeedFreePlanForExistingTenants` criada

**Arquivo:** `3 - Auth/Auth.Infrastructure/Migrations/20260415041131_SeedFreePlanForExistingTenants.cs`

- `Up`: insere TenantPlan Free permanente (`IsTrial = false`, `EndDate = null`) para cada tenant
  ativo que não possui TenantPlan com `Status = Active`
- `Down`: remove TenantPlans Free permanentes inseridos por esta migration (idempotente)

---

## Arquivos criados

| Arquivo | Tipo |
|---|---|
| `3 - Auth/Auth.Infrastructure/Migrations/20260415041131_SeedFreePlanForExistingTenants.cs` | Criado |
| `3 - Auth/Auth.Infrastructure/Migrations/20260415041131_SeedFreePlanForExistingTenants.Designer.cs` | Criado (gerado pelo EF) |

---

## Decisões tomadas

**Migration via `migrationBuilder.Sql()`:** a operação é puramente de dados (INSERT condicional),
sem alteração de schema. `migrationBuilder.Sql()` é o padrão correto para seeds em migrations EF Core.

**Filtro idempotente no `Up`:** o `NOT EXISTS` garante que re-executar a migration não duplica registros,
tornando-a segura em qualquer ambiente.

**Sem testes unitários:** migration de dados sem lógica de domínio nova — cobertura é a verificação
direta no banco (`SELECT` retornou 0 linhas).

---

## Verificação realizada

```sql
-- Resultado após aplicar: 0 linhas
SELECT t."Id", t."Name"
FROM auth.tenants t
WHERE t."IsDeleted" = false
  AND NOT EXISTS (
      SELECT 1 FROM auth.tenant_plans tp
      WHERE tp."TenantId" = t."Id" AND tp."Status" = 0
  );
```

**Resultado: 0 linhas — todos os tenants têm TenantPlan ativo.**

---

## Testes

Nenhum teste unitário adicionado — migration de dados pura.
Suite permanece em **297 testes, 0 falhas**.
