---
name: entregar
description: >
  Workflow completo de entrega de uma implementação backend: git, build, testes, PR e
  registro obrigatório em build/output/. Use ao finalizar qualquer implementação.
when_to_use: >
  Invocar quando a implementação estiver concluída e for hora de entregar: commitar,
  criar PR, registrar em build/output/ e atualizar a skill com novos conhecimentos.
  Exemplos: "vamos entregar", "criar o PR", "finalizar a task", "commitar e abrir PR".
model: sonnet
disable-model-invocation: true
allowed-tools: >
  Bash(dotnet build *) Bash(dotnet build) Bash(dotnet test *) Bash(dotnet ef *)
  Bash(git checkout *) Bash(git pull) Bash(git pull *) Bash(git add *) Bash(git commit *) Bash(git status) Bash(git status *) Bash(git diff) Bash(git diff *)
  Bash(gh pr create *) Bash(gh pr view *) Bash(gh pr view)
  Read Write Edit Glob
effort: medium
---

# Entregar — Workflow de Entrega Backend

Execute **sempre** nesta ordem — não pule etapas:

---

## Passo 1 — Garantir branch correta

```bash
# Nunca commitar em dev ou main
git status   # confirmar branch claude/<descricao>
```

Se estiver em `dev` ou `main`: `git checkout -b claude/<descricao>` antes de continuar.

---

## Passo 2 — Migration (se houve alteração de entidade)

Se criou ou alterou entidade → migration obrigatória antes do build:

```bash
# Módulo CRM
dotnet ef migrations add <Nome> \
  --project "2 - CRM/CRM.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context CRMDbContext --output-dir Migrations

# Módulo Auth
dotnet ef migrations add <Nome> \
  --project "3 - Auth/Auth.Infrastructure" \
  --startup-project "1 - Gateway/MundoDaLua.GraphQL" \
  --context AuthDbContext --output-dir Migrations
```

Consulte `references/migrations.md` para comandos completos e armadilhas conhecidas.

---

## Passo 3 — Build

```bash
dotnet build MyCRM.sln --verbosity minimal
```

Zero erros exigido. Se houver erros, corrija antes de continuar.

---

## Passo 4 — Testes

```bash
dotnet test "4 - Tests/UnitTests/MyCRM.UnitTests.csproj" --no-build --verbosity minimal
```

Zero falhas exigido. Se algum teste existente quebrar, identifique a causa e corrija.

---

## Passo 5 — Registrar em build/output/

Crie `build/output/<YYYY-MM-DD>-<descricao-kebab>.md`:

```markdown
# <Título da implementação>

**Data:** YYYY-MM-DD
**Branch:** claude/<nome>
**Decisão arquitetural:** DEC-XXX (se aplicável)

## O que foi feito
Entidades criadas/alteradas, handlers, migrations, seeds, mutations, testes.

## Arquivos criados ou modificados
- `caminho/arquivo.cs` — descrição breve

## Decisões tomadas
- Contexto que não está óbvio no código; alternativas descartadas

## Impacto em outras partes do sistema
- Dependências / integrações afetadas

## Testes
- N testes criados/atualizados — cenários cobertos
```

> Este arquivo é consumido pelo rules-agent para rastrear o histórico de decisões.

---

## Passo 6 — Atualizar skill core

Registre os novos conhecimentos adquiridos na implementação:

| O que mudou | Onde atualizar |
|---|---|
| Novo módulo ou projeto | `SKILL.md` do core (tabelas de projetos e schemas) |
| Nova entidade | `references/entities.md` |
| Nova armadilha de migration | `references/migrations.md` |
| Novo padrão ou anti-padrão | `references/patterns.md` |
| Erro corrigido | `references/` (seção adequada) + `LEARNINGS.md` |

**Regra de ouro para erros:** identifique a causa raiz → avalie se pode se repetir → formule regra preventiva → adicione no arquivo correto. Cada erro deve ser cometido uma única vez.

---

## Passo 7 — Criar PR

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

> O hook em `settings.json` bloqueia `gh pr create` automaticamente se build ou testes falharem.

**Regras de branch:**
- Feature: `claude/<descricao>` → PR para `dev`
- Correção: `claude/fix-<descricao>` → PR para `dev`
- PR para `main`: **somente se o usuário pedir explicitamente**

Reportar o link do PR ao usuário ao finalizar.
