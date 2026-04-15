---
name: core
description: Arquitetura e padrões obrigatórios do backend MyCRM. Use SEMPRE que estiver implementando entidades, handlers CQRS, queries ou mutations GraphQL, migrations EF Core, seeds, repositórios, ou qualquer tarefa de backend no projeto mundo-da-lua-crm-core. Auto-ativa para qualquer tarefa envolvendo C#, EF Core, Hot Chocolate, MediatR, DDD, CQRS ou Clean Architecture neste projeto. Se o usuário pedir para criar uma entidade, handler, mutation, migration ou qualquer artefato de backend, esta skill deve ser consultada primeiro.
---

# Skill: Arquitetura Core — MyCRM

Você está trabalhando no backend do **MyCRM**. Aplique rigorosamente as regras abaixo em toda decisão de código.

**Arquivos de referência detalhados:**
- `references/entities.md` — entidades existentes, campos, enums, restrições, design decisions
- `references/patterns.md` — padrões de código (entidades, EF, DbContext, DI, CQRS, GraphQL, multi-tenancy)
- `references/migrations.md` — comandos EF CLI, armadilhas, seed, migration obrigatória
- `references/checklists.md` — checklists de CRUD e de entrega

---

## FLUXO DE DESENVOLVIMENTO OBRIGATÓRIO

Execute **sempre** nesta ordem — não pule etapas:

```
1. git checkout dev && git pull
2. git checkout -b claude/<descricao>
3. [implementar — seguir checklists.md]
4. Se alterou entidade → criar migration (obrigatório antes do build)
5. Escrever testes unitários (obrigatório)
6. dotnet build MyCRM.sln --verbosity minimal
7. dotnet test "4 - Tests/UnitTests/MyCRM.UnitTests.csproj" --no-build --verbosity minimal
8. Criar build/output/<YYYY-MM-DD>-<descricao>.md descrevendo o que foi feito (obrigatório)
9. Atualizar esta skill com novos conhecimentos
10. gh pr create --base dev ...
```

> O hook em `.claude/settings.json` bloqueia `gh pr create` automaticamente
> se build ou testes falharem.

**Ao criar a PR, incluir no body:**
```bash
gh pr create --base dev --title "titulo curto" --body "$(cat <<'EOF'
## Summary
- bullet 1
- bullet 2

## Validação
- Build: dotnet build ✅
- Testes: dotnet test ✅ (N passando)
- Migration: [criada / não necessária]

## Test plan
- [ ] Aplicação sobe sem erros
- [ ] Migration aplicada com sucesso
- [ ] Queries/mutations disponíveis no schema GraphQL

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

Regras:
- Nunca commitar diretamente em `main` ou `dev`
- Nome da branch: `claude/<descricao>` em inglês (feature) ou `claude/fix-<descricao>` (correção)
- O PR deve ser criado antes de encerrar a tarefa — sem precisar que o usuário peça
- O link do PR deve ser reportado ao usuário ao final

---

## REGRA OBRIGATÓRIA — ATUALIZAÇÃO DA SKILL

**Ao finalizar qualquer implementação, execute obrigatoriamente os três passos abaixo:**

### Passo 1 — Registrar a implementação em `build/output/`

Crie um arquivo Markdown em `build/output/` descrevendo o que foi feito.
O arquivo é consumido pelo **agente guardião de regras** para rastrear o histórico de decisões.
A pasta `build/input/` pode conter prompts/specs de entrada fornecidos pelo usuário.

**Caminho:** `build/output/<YYYY-MM-DD>-<descricao-kebab>.md`

**Estrutura obrigatória:**

```markdown
# <Título da implementação>

**Data:** YYYY-MM-DD
**Branch:** claude/<nome-da-branch>
**Decisão arquitetural:** DEC-XXX (se aplicável)

## O que foi feito

Descrição objetiva das mudanças: entidades criadas/alteradas, handlers,
migrations, seeds, mutations GraphQL, testes.

## Arquivos criados ou modificados

- `caminho/do/arquivo.cs` — descrição breve
- ...

## Decisões tomadas

- Por que X foi feito de determinada forma (contexto que não está no código)
- Alternativas descartadas e o motivo

## Impacto em outras partes do sistema

- O que pode ser afetado por estas mudanças
- Integrações / dependências a observar

## Testes

- N testes criados/atualizados
- Cenários cobertos (resumo)
```

### Passo 2 — Atualizar conhecimentos da skill

Atualize os arquivos desta skill com os novos conhecimentos adquiridos:
- Novos módulos ou projetos → `SKILL.md` (tabela de projetos e schemas)
- Novas entidades e suas convenções → `references/entities.md`
- Novas armadilhas de migration → `references/migrations.md`
- Novos anti-padrões ou padrões estabelecidos → `references/patterns.md`

### Passo 3 — Análise de erros e prevenção futura

**Sempre que um erro for corrigido:**

1. Identifique a causa raiz
2. Avalie se pode se repetir em situações similares
3. Formule uma regra preventiva clara
4. Adicione na seção adequada do arquivo de referência correto

**O objetivo é que cada erro seja cometido uma única vez.**

---

## ESTRUTURA DO PROJETO (estado atual)

**Solução:** `mundo-da-lua-crm-core/MyCRM.sln`

**Convenção de nomes de projeto:** `MyCRM.{Modulo}.{Camada}`

| Projeto | Caminho físico do .csproj | Namespace raiz |
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

**Nota:** Ao adicionar novos módulos, o padrão de diretório é `2 - {Modulo}/` com subpastas `{Modulo}.Domain`, `{Modulo}.Application`, `{Modulo}.Infrastructure`.

---

## REGISTRO NO PROGRAM.CS (ao adicionar nova entidade)

Ao criar `{Entidade}Queries` e `{Entidade}Mutations`, registrar em `Program.cs`:

```csharp
builder.Services
    .AddGraphQLServer()
    // ... (existentes)
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.{Modulo}.{Entidade}Queries>()   // ← adicionar
    .AddTypeExtension<MyCRM.GraphQL.GraphQL.{Modulo}.{Entidade}Mutations>() // ← adicionar
    // Opcional: apenas se precisar ocultar campos internos da entidade
    .AddType<MyCRM.GraphQL.GraphQL.{Modulo}.{Entidade}ObjectType>()
```

Também registrar os módulos Application e Infrastructure:
```csharp
builder.Services.Add{Modulo}Application();
builder.Services.Add{Modulo}Infrastructure(builder.Configuration);
```

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

## ESTRUTURA DE MÓDULO

```
2 - {Modulo}/
├── {Modulo}.Domain          ← entidades, enums, value objects, interfaces de repositório
├── {Modulo}.Application     ← commands, queries, handlers, validators, DTOs
└── {Modulo}.Infrastructure  ← DbContext, repositórios, migrations, seed, configurações EF
```

Regras de dependência (setas = "depende de"):
```
GraphQL → Application → Domain
Infrastructure → Application → Domain
Shared.Kernel ← todos
```

---

## SCHEMAS POSTGRESQL (estado atual)

| Schema | DbContext | Tabelas existentes | Módulo |
|---|---|---|---|
| `crm` | `CRMDbContext` | `customers`, `people`, `companies`, `students`, `student_guardians`, `courses`, `student_courses`, `employees` | CRM |
| `auth` | `AuthDbContext` | `users`, `roles`, `user_roles`, `permissions`, `role_permissions`, `refresh_tokens`, `tenants` | Auth |

Schemas planejados (ainda não implementados):

| Schema | Módulo |
|---|---|
| `escola` | Alunos, turmas, matrículas, frequência |
| `clinica` | Pacientes, agenda, prontuários |
| `financeiro` | Contratos, cobranças, pagamentos |
| `rh` | Funcionários, contratos de trabalho |

Cada módulo tem seu próprio `DbContext` e suas próprias migrations.

---

## ENTIDADES EXISTENTES (resumo)

Leia `references/entities.md` para documentação completa de cada entidade.

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
| `Tenant` | `auth.tenants` | Conta cliente isolada da plataforma (raiz do sistema multi-tenant) |


---

## RBAC (obrigatorio para novas entregas)

- Todo resolver de negocio deve usar policy explicita baseada em `SystemPermissions`.
- Evitar `[Authorize]` generico em recursos sensiveis; preferir `[Authorize(Policy = ...)]`.
- Ao criar novo recurso protegido:
1. adicionar constante em `SystemPermissions`;
2. registrar em `SystemPermissions.All` com grupo;
3. aplicar policy em Query/Mutation;
4. validar seed de permissoes.
- O seed garante que o role `Administrador` tenha todas as permissoes ativas.
