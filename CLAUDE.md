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

---

## RBAC (estado atual e regras obrigatórias)

### Estado atual validado

- O backend usa RBAC por policy no GraphQL via `SystemPermissions` + `PermissionAuthorizationHandler`.
- Policies são registradas automaticamente em `Program.cs` com base em `SystemPermissions.All`.
- `PermissionSeeder` sincroniza permissões do sistema no banco.
- `AuthDataSeeder` garante que o role `Administrador` receba todas as permissões ativas.

### Regras para novas features

- Nunca usar apenas `[Authorize]` em resolver de negócio; usar sempre `[Authorize(Policy = SystemPermissions.Xxx)]`.
- Se criar nova funcionalidade protegida:
1. Criar constante em `3 - Shared/Shared.Kernel/SystemPermissions.cs`.
2. Adicionar em `SystemPermissions.All` com grupo.
3. Aplicar a policy nos resolvers de query/mutation.
- Manter `Login` e `RefreshToken` como `[AllowAnonymous]`.
- Normalizar permissões no serviço antes de comparar/cachear:
1. `Trim()`
2. `ToLowerInvariant()`
3. Remover entradas vazias e duplicadas

### Cobertura obrigatória de autorização (regressão)

- Para mutations principais de cada entidade, cobrir:
1. non-admin com permissão correta => sucesso
2. non-admin sem permissão => `AUTH_NOT_AUTHORIZED`
3. sem vazamento entre policies (`cross-contamination guard`)
- Entidades obrigatórias: `students`, `student_guardians`, `student_courses`, `customers`, `employees`, `courses`, `people`, `companies`, `users/roles`.

### Permissões por domínio (convencão)

- CRUD completo com granularidade: `read`, `create`, `update`, `delete`.
- Exemplo: `students:*`, `student_guardians:*`, `student_courses:*`, `customers:*`, etc.
