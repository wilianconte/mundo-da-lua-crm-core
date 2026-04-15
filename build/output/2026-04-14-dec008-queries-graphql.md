# Queries GraphQL — Planos e Billing (DEC-008)

**Data:** 2026-04-14
**Branch:** claude/subscription-plans
**Input:** `build/input/2026-04-14-dec008-queries-graphql.md`
**Decisão arquitetural:** DEC-008

---

## Resumo executivo

Três queries GraphQL de leitura foram implementadas para expor planos e billing ao
frontend. Nenhuma migration foi necessária — todos os DbSets e configurações EF já
estavam presentes. A suite de testes permanece em 296 (nenhum teste unitário é
necessário para queries IQueryable simples).

---

## O que foi feito

### `TenantPlanQueries.cs` criado

Arquivo: `1 - Gateway/MundoDaLua.GraphQL/GraphQL/Plans/TenantPlanQueries.cs`

Três queries expostas no schema GraphQL:

| Query GraphQL | Autenticação | Retorno |
|---|---|---|
| `getMyActivePlan` | `[Authorize]` | `TenantPlan` (com Plan → PlanFeatures → Feature) |
| `getPlans` | `[Authorize]` | `[Plan]` (com PlanFeatures → Feature), ordenado por SortOrder |
| `getMyBillings` | `[Authorize(Policy = "plans:manage")]` | paginado, filtrável, ordenável |

### `AuthGraphQLExtensions.cs` atualizado

`TenantPlanQueries` registrada com `AddTypeExtension` no bloco Plans/Subscriptions,
antes de `TenantPlanMutations`.

---

## Arquivos criados ou modificados

| Arquivo | Tipo |
|---|---|
| `1 - Gateway/MundoDaLua.GraphQL/GraphQL/Plans/TenantPlanQueries.cs` | Criado |
| `1 - Gateway/MundoDaLua.GraphQL/Extensions/AuthGraphQLExtensions.cs` | Modificado — `AddTypeExtension<TenantPlanQueries>()` adicionado |

---

## Decisões tomadas

**`AsNoTracking()` em todas as queries:** Queries de leitura não precisam de
change tracking — reduz overhead de memória e melhora performance.

**`getMyActivePlan` sem policy explícita (`[Authorize]` simples):** Todo tenant
autenticado precisa ver seu próprio plano para o funcionamento básico da UI.
`getMyBillings` requer `plans:manage` pois é dado financeiro sensível, acessado
tipicamente apenas pelo administrador do tenant.

**`getPlans` sem filtro de TenantId:** Planos são globais da plataforma — todos os
tenants enxergam o mesmo catálogo. O filtro é apenas `IsActive = true`.

**Includes explícitos no IQueryable:** Mesmo com `[UseProjection]` o Hot Chocolate
pode não projetar corretamente sem os `Include`/`ThenInclude` quando o schema pede
campos aninhados. Os includes garantem que o EF Core gere os JOINs corretos.

---

## Verificações realizadas antes de implementar

- `DbSet<Plan>`, `DbSet<PlanFeature>`, `DbSet<Feature>`, `DbSet<TenantPlan>`,
  `DbSet<Billing>` — todos já presentes em `AuthDbContext` ✅
- `Plan.PlanFeatures` (`ICollection<PlanFeature>`) — já existia ✅
- `PlanFeature.Feature` — já existia, configurado em `PlanFeatureConfiguration` ✅
- `SystemPermissions.PlansManage = "plans:manage"` — já existia ✅
- Nenhuma migration necessária ✅

---

## Impacto em outras partes do sistema

- As três queries ficam imediatamente disponíveis no schema GraphQL após deploy.
- `getMyBillings` respeita `plans:manage` — apenas admins do tenant verão cobranças.
- `TenantPlan` e `Billing` não têm query filter global por TenantId no DbContext
  (filtro é explícito via `Where(x => x.TenantId == tenantService.TenantId)`) —
  se isso mudar, as queries precisarão ser revisadas.

---

## Testes

Nenhum teste unitário adicionado — queries IQueryable simples são cobertas por
testes de integração (Hot Chocolate + EF Core). Suite permanece em **296 testes,
0 falhas**.
