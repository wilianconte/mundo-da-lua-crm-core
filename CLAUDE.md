# MyCRM — Diretrizes do Projeto

> Este arquivo é carregado automaticamente no início de cada sessão do Claude Code.
> Não remova nem renomeie este arquivo.

---

## SKILL OBRIGATÓRIA

Para qualquer tarefa de backend (entidades, handlers, migrations, GraphQL, repositórios, seeds, testes), carregue a skill antes de escrever qualquer código:

```
/core
```

A skill carrega as regras de arquitetura, padrões de código, entidades existentes, checklists de CRUD e convenções de migration do projeto.

---

## FLUXO DE BRANCHES E PULL REQUESTS

| Situação | Regra |
|---|---|
| Corrigir uma issue | `git checkout dev && git pull` → criar branch `claude/fix-<descricao>` → implementar → PR para `dev` |
| Iniciar qualquer tarefa | `git checkout dev && git pull` → criar branch `claude/<descricao>` |
| Abrir PR | **sempre para `dev`** (`--base dev`) |
| PR para `main` | **somente se o usuário pedir explicitamente** |
| Commit direto em `dev` ou `main` | **proibido** |

```bash
# Iniciar
git checkout dev && git pull
git checkout -b claude/<descricao-curta>

# Finalizar
gh pr create --base dev --title "..." --body "..."
```

O link do PR deve ser reportado ao usuário ao final de cada tarefa.

---

## STACK E DECISÕES NÃO NEGOCIÁVEIS

| Tema | Decisão |
|---|---|
| Runtime | .NET 9 |
| Interface pública | GraphQL único — zero REST |
| Servidor GraphQL | Hot Chocolate 15+ |
| Arquitetura | Clean Architecture + DDD + CQRS |
| Mediação | MediatR |
| Validação | FluentValidation |
| Banco | PostgreSQL + EF Core 9 + Npgsql |
| Mapeamento | Mapster |

Consulte `.claude/skills/core/SKILL.md` e seus `references/` para regras detalhadas.
