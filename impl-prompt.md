Implemente a estrutura de assinatura e planos do sistema Mundo da Lua CRM no projeto backend (.NET / C#). Esta é a implementação da decisão arquitetural DEC-008 (planos dinâmicos).

## Contexto geral

O sistema é um CRM multi-tenant. Cada tenant deve ter exatamente um TenantPlan com Status = Active a qualquer momento. O plano controla os limites de uso (alunos, funcionários, cursos) e features booleanas (relatórios, acesso API). A especificação completa está em `d:\Dev\Mundo da Lua\mundo-da-lua-crm\mundo-da-lua-crm-rules\docs\`.

## Estrutura do projeto

- `3 - Auth/Auth.Domain/` — entidades, enums, interfaces de repositório
- `3 - Auth/Auth.Infrastructure/` — EF Core, repositórios, migrations
- `3 - Auth/Auth.Application/` — handlers CQRS (MediatR)
- `1 - Gateway/MundoDaLua.GraphQL/` — resolvers GraphQL (Hot Chocolate)

Siga os padrões já existentes no projeto: BaseEntity (Id, IsDeleted, CreatedAt, UpdatedAt), TenantEntity (adiciona TenantId), repositórios com interface em Domain e implementação em Infrastructure, handlers MediatR em Application.

---

## PASSO 1 — Novos enums em Auth.Domain/Enums/

Crie os arquivos:

**FeatureType.cs**
```csharp
public enum FeatureType { Numeric, Boolean }
```

**TenantPlanStatus.cs**
```csharp
public enum TenantPlanStatus { Active, Paused, PendingCancellation, Expired, Cancelled, Upgraded }
```

**BillingStatus.cs**
```csharp
public enum BillingStatus { Pending, Paid, Overdue, Cancelled, Refunded }
```

Atualize o enum existente **TenantStatus.cs**: remova o valor `Trial`, mantendo apenas Active, Suspended, Cancelled.

Remova (ou marque como obsoleto e não use mais) o enum antigo **TenantPlan.cs** se existir.

---

## PASSO 2 — Novas entidades em Auth.Domain/Entities/

### Plan.cs (herda BaseEntity — NÃO TenantEntity, é global)
Campos: Name (string, required), DisplayName (string, required), Price (decimal, required), IsActive (bool), SortOrder (int)

### Feature.cs (herda BaseEntity — global)
Campos: Key (string, required), Description (string, required), Type (FeatureType)

### PlanFeature.cs (herda BaseEntity — global)
Campos: PlanId (Guid), Plan (nav), FeatureId (Guid), Feature (nav), Value (int?)

### TenantPlan.cs (herda BaseEntity — NÃO TenantEntity, tem TenantId explícito)
Campos:
- TenantId (Guid)
- PlanId (Guid), Plan (nav)
- StartDate (DateOnly)
- EndDate (DateOnly?) — null = permanente (Free)
- IsTrial (bool)
- FallbackPlanId (Guid?) — FK para Plan
- FallbackPlan (nav Plan?)
- CancelledAt (DateOnly?)
- PausedAt (DateOnly?) — data em que o plano foi pausado para trial de outro plano
- Status (TenantPlanStatus)

### Billing.cs (herda BaseEntity — NÃO TenantEntity, tem TenantId explícito)
Campos:
- TenantId (Guid)
- TenantPlanId (Guid), TenantPlan (nav)
- Amount (decimal)
- DueDate (DateOnly)
- PaidAt (DateTime?)
- ReferenceMonth (string) — formato YYYY-MM
- Status (BillingStatus)
- InvoiceUrl (string?)

### Atualize Tenant.cs
Remova o campo Plan (enum TenantPlan antigo) se existir. O plano vigente é obtido via TenantPlan com Status = Active.

---

## PASSO 3 — Interfaces de repositório em Auth.Domain/Repositories/

Crie interfaces seguindo o padrão dos repositórios existentes:

**IPlanRepository**: GetByIdAsync, GetByNameAsync (ex: "free"), GetAllActiveAsync, GetFreePlanAsync

**ITenantPlanRepository**:
- GetActiveByTenantIdAsync(Guid tenantId) → TenantPlan?
- GetPausedByTenantIdAsync(Guid tenantId) → TenantPlan?
- HasUsedTrialForPlanAsync(Guid tenantId, Guid planId) → bool
- GetByIdAsync(Guid id) → TenantPlan?
- AddAsync(TenantPlan tenantPlan)
- UpdateAsync(TenantPlan tenantPlan)

**IBillingRepository**:
- GetPendingByTenantPlanIdAsync(Guid tenantPlanId) → Billing?
- GetByIdAsync(Guid id) → Billing?
- AddAsync(Billing billing)
- UpdateAsync(Billing billing)

---

## PASSO 4 — EF Core em Auth.Infrastructure/

### Configurações (IEntityTypeConfiguration<T>) para cada nova entidade

**PlanConfiguration**: tabela "Plans", índice único em Name

**FeatureConfiguration**: tabela "Features", índice único em Key

**PlanFeatureConfiguration**: tabela "PlanFeatures", índice único composto (PlanId, FeatureId)

**TenantPlanConfiguration**: tabela "TenantPlans"
- Índice único parcial: UNIQUE (TenantId) WHERE Status = "Active"
- Índice único parcial: UNIQUE (TenantId) WHERE Status = "Paused"
- Índice único parcial: UNIQUE (TenantId, PlanId) WHERE IsTrial = 1
- FK para Plan (PlanId), FK para Plan (FallbackPlanId) com DeleteBehavior.Restrict

**BillingConfiguration**: tabela "Billings"
- Índice único parcial: UNIQUE (TenantId, ReferenceMonth) WHERE Status = "Pending"

Para índices parciais no EF Core use HasIndex().HasFilter(). Verifique qual banco de dados o projeto usa e aplique a sintaxe correta do HasFilter.

### Implementações dos repositórios em Auth.Infrastructure/Repositories/

Implemente PlanRepository, TenantPlanRepository e BillingRepository seguindo o padrão dos repositórios existentes (AuthDbContext).

Registre os novos repositórios no DI container onde os demais já estão registrados.

---

## PASSO 5 — Migration com seed

Crie uma migration EF Core com:
1. Todas as novas tabelas (Plans, Features, PlanFeatures, TenantPlans, Billings)
2. Remoção da coluna Plan do Tenant (se existir)
3. Seed dos dados iniciais inseridos diretamente na migration (use GUIDs fixos gerados com Guid.NewGuid() uma vez):

**Planos:**
- free: DisplayName="Gratuito", Price=0, IsActive=true, SortOrder=1
- basic: DisplayName="Básico", Price=49, IsActive=true, SortOrder=2
- pro: DisplayName="Pro", Price=99, IsActive=true, SortOrder=3

**Features:**
- max_students: Type=Numeric, Description="Número máximo de alunos ativos"
- max_employees: Type=Numeric, Description="Número máximo de funcionários ativos"
- max_courses: Type=Numeric, Description="Número máximo de cursos ativos"
- has_reports: Type=Boolean, Description="Acesso a relatórios avançados"
- has_api_access: Type=Boolean, Description="Acesso à API pública"

**PlanFeatures (limites por plano):**

| Feature       | Free | Basic | Pro  |
|---------------|------|-------|------|
| max_students  | 30   | 200   | null |
| max_employees | 2    | 10    | null |
| max_courses   | 3    | 20    | null |
| has_reports   | 0    | 1     | 1    |
| has_api_access| 0    | 0     | 1    |

---

## PASSO 6 — Atualizar RegisterTenantHandler

Após criar o Tenant, crie automaticamente um TenantPlan com:
- TenantId = novo tenant Id
- PlanId = Id do plano "free" (buscar via IPlanRepository.GetFreePlanAsync)
- StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
- EndDate = StartDate.AddDays(30)
- IsTrial = true
- FallbackPlanId = Id do plano "free"
- Status = TenantPlanStatus.Active

---

## PASSO 7 — Serviço de verificação de limites

Crie `Auth.Application/Services/PlanLimitService.cs` com interface `IPlanLimitService`:

```csharp
Task CheckNumericLimitAsync(Guid tenantId, string featureKey, Func<Task<int>> getCurrentCountAsync);
Task CheckBooleanFeatureAsync(Guid tenantId, string featureKey);
```

Lógica:
- Busca TenantPlan Active do tenant
- Busca PlanFeature para o planId + featureKey via consulta ao banco
- Se não encontrar PlanFeature: retorna sem erro (ilimitado/habilitado — RN-028.6)
- Para CheckNumericLimitAsync: chama getCurrentCountAsync(), rejeita se count >= Value (e Value != null)
- Para CheckBooleanFeatureAsync: rejeita se Value = 0

Use DomainException ou o padrão de erro existente no projeto.

Mensagens de erro:
- max_students: "Limite de alunos do plano atingido. Faça upgrade para continuar."
- max_employees: "Limite de funcionários do plano atingido. Faça upgrade para continuar."
- max_courses: "Limite de cursos do plano atingido. Faça upgrade para continuar."
- Boolean desabilitada: "Esta funcionalidade não está disponível no seu plano atual."

Atualize **CreateStudentHandler**, **CreateEmployeeHandler** e **CreateCourseHandler** para injetar e chamar o IPlanLimitService antes de criar. O count deve considerar apenas registros ativos (IsDeleted = false) do tenant.

---

## PASSO 8 — Handlers de assinatura em Auth.Application/Commands/Plans/

Crie pasta `Auth.Application/Commands/Plans/` e implemente os handlers abaixo. Cada handler terá sua própria subpasta com Command e Handler, seguindo o padrão existente.

### UpgradeTenantPlanHandler
Command: TenantId, NewPlanId

Lógica (RN-029.6):
1. Busca TenantPlan Active do tenant
2. Busca o novo plano — erro se não existir ou não ativo
3. Valida que o novo plano é diferente do atual
4. Se TenantPlan atual IsTrial = false: cancela Billing Pending do TenantPlan atual (se existir)
5. TenantPlan atual: Status = Upgraded, EndDate = hoje
6. Se existir TenantPlan Paused para o tenant: Status = Cancelled
7. Cria novo TenantPlan: PlanId=newPlanId, IsTrial=false, StartDate=hoje, EndDate=hoje+1mês, FallbackPlanId=freePlanId, Status=Active
8. Gera Billing:
   - Origem trial: Amount = Plan.Price (valor cheio)
   - Origem pago: Amount = Plan.Price × (diasRestantes / diasTotais), onde diasRestantes = TenantPlanAntigo.EndDate - hoje, diasTotais = TenantPlanAntigo.EndDate - TenantPlanAntigo.StartDate
   - DueDate = hoje + 10 dias, ReferenceMonth = hoje.ToString("yyyy-MM"), Status = Pending
9. Tudo em uma transação

### CancelTenantPlanHandler
Command: TenantId, DowngradeToPlanId

Lógica (RN-029.7):
1. Busca TenantPlan Active
2. Valida que IsTrial = false (trial termina via TerminateTrial)
3. Valida Status = Active
4. TenantPlan: FallbackPlanId = DowngradeToPlanId, CancelledAt = hoje, Status = PendingCancellation
5. Cancela todos Billing Pending do TenantPlan

### RevertCancellationHandler
Command: TenantId

Lógica (RN-029.11):
1. Busca TenantPlan Active
2. Valida Status = PendingCancellation
3. Valida EndDate > hoje
4. TenantPlan: Status = Active, CancelledAt = null, FallbackPlanId = freePlanId

### StartTrialHandler
Command: TenantId, TrialPlanId

Lógica (RN-029.3 e RN-029.14):
1. Busca TenantPlan Active
2. Valida plano do trial existe e está ativo
3. Verifica se já usou trial deste plano — rejeita com erro se sim
4. Valida Status != PendingCancellation
5. Se TenantPlan atual é plano pago (IsTrial=false, Price>0):
   - TenantPlan atual: Status = Paused, PausedAt = hoje
   - FallbackPlanId do novo trial = PlanId do plano pausado
6. Se TenantPlan atual é Free (Price=0):
   - TenantPlan atual: Status = Expired
   - FallbackPlanId do novo trial = freePlanId
7. Cria novo TenantPlan: PlanId=TrialPlanId, IsTrial=true, StartDate=hoje, EndDate=hoje+30d, FallbackPlanId=conforme acima, Status=Active

### TerminateTrialHandler
Command: TenantId, DowngradeToPlanId (nullable — obrigatório apenas quando não há plano pausado)

Lógica (RN-029.9):
1. Busca TenantPlan Active
2. Valida IsTrial = true
3. TenantPlan atual: Status = Expired, EndDate = hoje
4. Verifica TenantPlan Paused para o tenant:
   - SE EXISTE: retoma o plano pausado
     - remainingDays = (int)(PausedTenantPlan.EndDate.Value - PausedTenantPlan.PausedAt.Value).TotalDays
     - PausedTenantPlan.EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(remainingDays)
     - PausedTenantPlan.PausedAt = null
     - PausedTenantPlan.Status = Active
   - SE NÃO EXISTE: DowngradeToPlanId obrigatório
     - Se Free: novo TenantPlan com EndDate=null, FallbackPlanId=null, sem Billing
     - Se pago: novo TenantPlan com EndDate=hoje+1mês, FallbackPlanId=freePlanId, Billing (valor cheio, DueDate=hoje+10d, ReferenceMonth)

### MarkBillingAsPaidHandler
Command: BillingId, TenantId

Lógica (RN-030.9):
1. Busca Billing por Id — erro se não encontrar ou TenantId não bater
2. Valida Status = Pending ou Overdue
3. Billing: Status = Paid, PaidAt = DateTime.UtcNow
4. Se Tenant.Status = Suspended: Tenant.Status = Active

---

## PASSO 9 — GraphQL Mutations

Adicione mutations em um novo resolver `TenantPlanMutation.cs` em `1 - Gateway/MundoDaLua.GraphQL/GraphQL/Plans/`:

- upgradeTenantPlan(input: UpgradeTenantPlanInput!)
- cancelTenantPlan(input: CancelTenantPlanInput!)
- revertCancellation
- startTrial(input: StartTrialInput!)
- terminateTrial(input: TerminateTrialInput!)
- markBillingAsPaid(input: MarkBillingAsPaidInput!)

TenantId deve ser extraído do contexto de autenticação (como já é feito nos outros handlers). Siga o padrão de inputs, erros e retornos do projeto existente.

---

## RESTRIÇÕES IMPORTANTES

- NÃO implemente LAC-012 nem LAC-013 (rotinas agendadas — tarefa separada)
- NÃO implemente queries GraphQL de leitura — foco nos comandos
- Mantenha todos os testes existentes funcionando
- Se encontrar código conflitante com o enum TenantPlan antigo, atualize para o novo modelo
- Use transações onde múltiplas entidades são alteradas atomicamente
- Datas puras: DateOnly. Timestamps: DateTime UTC
- Ao finalizar, confirme os arquivos criados/alterados e se a migration compilou com sucesso
