# MyCRM — Diretrizes do Agente

> Carregado automaticamente no início de cada sessão. Não remova nem renomeie.
> Regras detalhadas de arquitetura, padrões e checklists → `/core` skill.

---

## PASTA BUILD/

| Pasta | Papel |
|---|---|
| `build/input/` | Specs e prompts para implementação — processados manualmente com `/loop` quando solicitado |
| `build/output/` | Registro obrigatório de cada implementação concluída |

---

## SKILLS DO AGENTE

| Skill | Tipo | Quando usar |
|---|---|---|
| `/core` | referência, auto | Antes de escrever qualquer código backend — arquitetura, entidades, padrões, RBAC |
| `/entregar` | procedural, manual | Ao finalizar — git, build, testes, PR, build/output, atualização da skill |
| `/consultar-regras RN-XXX` | leitura, manual | Antes de implementar — lê RN/DEC/UC do repositório de regras |
| `/auto-melhorar` | melhoria, manual | A cada 5-10 implementações — revisa LEARNINGS e melhora as skills |

**Aprendizado persistente:** `LEARNINGS.md` na raiz — lido em tarefas complexas, atualizado ao finalizar implementações.

**Avaliar modelos:** após cada uso de `/consultar-regras`, `/entregar` ou `/auto-melhorar`, registrar em `LEARNINGS.md § Desempenho de Modelos`:
- ✅ efetivo — output aceito sem correção
- ⚠️ parcial — output precisou de ajuste ou retry
- ❌ falhou — output incorreto, tarefa abortada

**Repositório de regras:** `D:\Dev\Mundo da Lua\mundo-da-lua-crm\mundo-da-lua-crm-rules\docs\`

---

## FLUXO DE ENTREGA (resumo)

```
1. git checkout dev && git pull
2. git checkout -b claude/<descricao>
3. /core → /consultar-regras → implementar
4. /entregar  ← build, testes, PR, build/output, atualizar skill
```

> Detalhes completos do fluxo de entrega em `/entregar`.

### Regras de branch e PR

| Situação | Regra |
|---|---|
| Nova feature | `claude/<descricao>` → PR para `dev` |
| Correção | `claude/fix-<descricao>` → PR para `dev` |
| PR para `main` | **somente se o usuário pedir explicitamente** |
| Commit direto em `dev` ou `main` | **proibido** |

---

## STACK (não negociável)

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

