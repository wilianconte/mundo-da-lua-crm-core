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

## FLUXO DE DESENVOLVIMENTO OBRIGATÓRIO

Execute **sempre** nesta ordem, sem pular etapas:

```
1. git checkout dev && git pull          ← partir sempre de dev atualizado
2. git checkout -b claude/<descricao>    ← nunca commitar em dev ou main
3. [implementar]                         ← seguir skill core
4. Se alterou entidade → criar migration  ← obrigatório antes do build
5. Escrever testes unitários             ← obrigatório antes do commit
6. dotnet build MyCRM.sln               ← zero erros exigido
7. dotnet test "4 - Tests/UnitTests/MyCRM.UnitTests.csproj"  ← zero falhas exigido
8. Criar build/output/<YYYY-MM-DD>-<descricao>.md            ← obrigatório
9. Atualizar skill core com novos conhecimentos
10. gh pr create --base dev             ← só se build ✅ e testes ✅
```

> **O hook no `.claude/settings.json` bloqueia automaticamente `gh pr create`
> se o build ou os testes falharem.** Não é necessário rodar manualmente antes.

### Regras de branch e PR

| Situação | Regra |
|---|---|
| Corrigir uma issue | `git checkout dev && git pull` → `claude/fix-<descricao>` → PR para `dev` |
| Qualquer nova feature | `git checkout dev && git pull` → `claude/<descricao>` → PR para `dev` |
| Abrir PR | **sempre para `dev`** (`--base dev`) |
| PR para `main` | **somente se o usuário pedir explicitamente** |
| Commit direto em `dev` ou `main` | **proibido** |

O link do PR deve ser reportado ao usuário ao final de cada tarefa.

---

## REGISTRO DE IMPLEMENTAÇÕES (obrigatório)

Ao finalizar qualquer implementação, crie um arquivo Markdown em `build/output/`:

**Caminho:** `build/output/<YYYY-MM-DD>-<descricao-kebab>.md`

**Estrutura mínima:**

```markdown
# <Título da implementação>

**Data:** YYYY-MM-DD
**Branch:** claude/<nome>
**Decisão arquitetural:** DEC-XXX (se aplicável)

## O que foi feito
Descrição das mudanças: entidades, handlers, migrations, seeds, mutations, testes.

## Arquivos criados ou modificados
- `caminho/arquivo.cs` — descrição breve

## Decisões tomadas
- Contexto e alternativas descartadas (o que não está óbvio no código)

## Impacto em outras partes do sistema
- Dependências / integrações afetadas

## Testes
- N testes criados/atualizados — cenários cobertos
```

> Este arquivo é consumido pelo **agente guardião de regras** para rastrear
> o histórico de decisões e garantir consistência arquitetural entre sessões.
> A pasta `build/input/` pode conter prompts/specs de entrada fornecidos pelo usuário.

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
- Manter `Login`, `RefreshToken` e `RegisterTenant` como `[AllowAnonymous]` — são as únicas mutations públicas.
- Normalizar permissões no serviço antes de comparar/cachear:
1. `Trim()`
2. `ToLowerInvariant()`
3. Remover entradas vazias e duplicadas

### Cobertura obrigatória de autorização (regressão)

- Para mutations principais de cada entidade, cobrir:
1. non-admin com permissão correta => sucesso
2. non-admin sem permissão => `AUTH_NOT_AUTHORIZED`
3. sem vazamento entre policies (`cross-contamination guard`)
- Entidades obrigatórias: `students`, `student_guardians`, `student_courses`, `customers`, `employees`, `courses`, `people`, `companies`, `users/roles`, `tenants`.

### Permissões por domínio (convencão)

- Entidades de negócio: CRUD granular — `read`, `create`, `update`, `delete`.
  - Exemplo: `students:read`, `students:create`, `students:update`, `students:delete`
- Entidades administrativas da plataforma: permissão única `manage`.
  - Exemplo: `users:manage`, `roles:manage`, `tenants:manage`
