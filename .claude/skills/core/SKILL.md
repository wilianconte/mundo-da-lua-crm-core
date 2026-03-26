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

## REGRA OBRIGATÓRIA — BRANCH E PULL REQUEST

**Ao iniciar qualquer implementação, crie uma branch `claude/*` antes de escrever qualquer código:**

```bash
git checkout -b claude/<descricao-curta-da-feature>
# Exemplos:
# claude/add-employee-entity
# claude/add-student-course-enrollment
# claude/fix-tenant-filter-bug
```

**Ao finalizar a implementação, crie um Pull Request para `main`:**

```bash
git add <arquivos-relevantes>
git commit -m "feat(...): descrição da implementação"
gh pr create --title "titulo curto" --body "$(cat <<'EOF'
## Summary
- bullet 1
- bullet 2

## Test plan
- [ ] Aplicação sobe sem erros
- [ ] Migration aplicada com sucesso
- [ ] Queries/mutations disponíveis no schema GraphQL

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

Regras:
- Nunca commitar diretamente em `main`
- Nome da branch deve descrever a feature/fix em inglês, no formato `claude/<descricao>`
- O PR deve ser criado antes de encerrar a tarefa — sem precisar que o usuário peça
- O link do PR deve ser reportado ao usuário ao final

---

## REGRA OBRIGATÓRIA — ATUALIZAÇÃO DA SKILL

**Ao finalizar qualquer implementação, execute obrigatoriamente os dois passos abaixo:**

### Passo 1 — Atualizar conhecimentos

Atualize os arquivos desta skill com os novos conhecimentos adquiridos:
- Novos módulos ou projetos → `SKILL.md` (tabela de projetos e schemas)
- Novas entidades e suas convenções → `references/entities.md`
- Novas armadilhas de migration → `references/migrations.md`
- Novos anti-padrões ou padrões estabelecidos → `references/patterns.md`

### Passo 2 — Análise de erros e prevenção futura

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
| `auth` | `AuthDbContext` | `users` | Auth |

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
