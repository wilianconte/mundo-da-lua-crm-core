# Ajustes Frontend — Área Minha Assinatura (DEC-008)

**Data:** 2026-04-15
**Branch:** claude/subscription-plans
**Input:** `build/input/2026-04-15-dec008-ajustes-frontend.md`
**Decisão arquitetural:** DEC-008

---

## Resumo executivo

Dois ajustes para destravar a implementação da área "Minha Assinatura" no frontend:
correção de bug de validação em `UpgradeTenantPlanHandler` e adição da query `getMyTenant`.
Suite de testes: 296 → **297 testes, 0 falhas**.

---

## O que foi feito

### PASSO 1 — Bug fix: `UpgradeTenantPlanHandler` não bloqueava `PendingCancellation`

**Arquivo:** `3 - Auth/Auth.Application/Commands/Plans/UpgradeTenantPlan/UpgradeTenantPlanHandler.cs`

Adicionada validação (RN-029.10) logo após a validação de `UPGRADE_TO_FREE_NOT_ALLOWED`:

```csharp
// 3b. Valida que o plano não está em PendingCancellation (RN-029.10)
if (activePlan.Status == TenantPlanStatus.PendingCancellation)
    return Result.Failure("UPGRADE_BLOCKED_PENDING_CANCELLATION",
        "Não é possível fazer upgrade com cancelamento pendente. Reverta o cancelamento primeiro.");
```

### PASSO 2 — Query `getMyTenant` adicionada

**Arquivo:** `1 - Gateway/MundoDaLua.GraphQL/GraphQL/Plans/TenantPlanQueries.cs`

Nova query exposta no schema GraphQL — retorna o tenant autenticado sem exigir `tenants:manage`:

```graphql
query { myTenant { id name status } }
```

Permite ao frontend detectar `Tenant.Status = Suspended` e redirecionar para tela de pagamento (RN-030.9).

---

## Arquivos criados ou modificados

| Arquivo | Tipo |
|---|---|
| `3 - Auth/Auth.Application/Commands/Plans/UpgradeTenantPlan/UpgradeTenantPlanHandler.cs` | Modificado — validação `UPGRADE_BLOCKED_PENDING_CANCELLATION` adicionada |
| `1 - Gateway/MundoDaLua.GraphQL/GraphQL/Plans/TenantPlanQueries.cs` | Modificado — query `GetMyTenant` adicionada |
| `4 - Tests/UnitTests/Plans/UpgradeTenantPlanHandlerTests.cs` | Modificado — 1 teste adicionado |

---

## Decisões tomadas

**Posição da validação `PendingCancellation`:** inserida após `UPGRADE_TO_FREE_NOT_ALLOWED` e antes de `PLAN_SAME_AS_CURRENT`. O plano Free é rejeitado antes de qualquer verificação de status porque é uma regra de negócio mais fundamental (não depende de estado do TenantPlan).

**`getMyTenant` sem `tenants:manage`:** a query retorna apenas o tenant do próprio usuário autenticado (filtro por `TenantId` do serviço), não expõe lista de tenants. A permissão `tenants:manage` é necessária apenas para operações administrativas sobre outros tenants.

**Sem migration:** nenhuma entidade foi alterada.

---

## Impacto em outras partes do sistema

- Tenants com `Status = PendingCancellation` que tentarem upgrade receberão o erro `UPGRADE_BLOCKED_PENDING_CANCELLATION` — comportamento anterior era incorreto (upgrade era permitido).
- `getMyTenant` fica imediatamente disponível no schema GraphQL após deploy.

---

## Testes

1 teste adicionado em `UpgradeTenantPlanHandlerTests.cs`:

- `Handle_PlanPendingCancellation_ReturnsUpgradeBlockedPendingCancellation`
  — garante que upgrade é bloqueado quando `Status = PendingCancellation` (RN-029.10)

Suite: **297 testes, 0 falhas**.
