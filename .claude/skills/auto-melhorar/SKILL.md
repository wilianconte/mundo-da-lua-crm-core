---
name: auto-melhorar
description: >
  Revisa LEARNINGS.md e as skills do agente de backend, propõe melhorias concretas
  com base em padrões de sucesso/falha acumulados, e executa as aprovadas pelo usuário.
when_to_use: >
  Use quando o usuário pedir para melhorar o agente, otimizar skills, revisar aprendizados,
  ou mencionar: "melhorar o agente", "otimizar", "revisar learnings", "o agente está bom?",
  "o que pode melhorar", "auto-melhorar".
argument-hint: "[skill-especifica | todas]"
model: opus
disable-model-invocation: true
allowed-tools: Read Write Edit Glob
effort: high
---

# Auto-Melhoria — backend-agent (mundo-da-lua-crm-core)

**Escopo:** $ARGUMENTS (se vazio, revisa todas as skills)

---

## Fase 1 — Ler os aprendizados

Leia `LEARNINGS.md` na raiz do repositório e extraia:

- **Padrões de sucesso** — abordagens que funcionaram bem
- **Padrões de falha** — o que gerou retrabalho ou correção
- **Preferências confirmadas** — comportamentos validados pelo usuário
- **Questões em aberto** — que podem indicar lacuna nas skills

Se `LEARNINGS.md` tiver menos de 5 entradas, informe que ainda não há base suficiente e sugira mais sessões de uso antes de otimizar.

---

## Fase 2 — Auditar as skills atuais

Leia os arquivos `SKILL.md` de cada skill em `.claude/skills/`:

| Skill | Arquivo |
|---|---|
| `core` | `.claude/skills/core/SKILL.md` |
| `auto-melhorar` | `.claude/skills/auto-melhorar/SKILL.md` |
| `consultar-regras` | `.claude/skills/consultar-regras/SKILL.md` |

Para cada skill, avalie:

1. **Trigger accuracy** — a `description` + `when_to_use` cobre os casos reais dos learnings?
2. **Processo** — os passos refletem o que o usuário espera?
3. **Formato de saída** — o formato foi aceito sem retrabalho?
4. **Lacunas** — há situações frequentes que a skill não cobre?

Verifique também:
- `CLAUDE.md` — está lean (< 120 linhas)? Reflete o comportamento real?
- `references/` do core — entidades e padrões estão atualizados?
- `settings.json` — hooks estão funcionando conforme esperado?

---

## Fase 3 — Avaliar desempenho de modelos

Leia a seção `## Desempenho de Modelos` do `LEARNINGS.md` e aplique as regras:

| Condição | Ação proposta |
|---|---|
| `⚠️ parcial` 2x seguidas na mesma skill | Propor upgrade do modelo (ex: haiku → sonnet, sonnet → opus) |
| `❌ falhou` 1x na mesma skill | Propor upgrade imediato |
| `✅ efetivo` 5x seguidas com modelo mais capaz | Propor downgrade (reduz custo sem perda de qualidade) |
| Sem registros suficientes (< 3 entradas) | Informar que ainda não há dados — pular esta fase |

Para cada proposta de troca de modelo, usar o formato:

```md
### Modelo: [skill]
**Situação atual:** model: [atual]
**Evidência:** [datas e resultados dos logs]
**Proposta:** model: [novo]
**Motivo:** [upgrade por falha / downgrade por eficiência]
**Confiança:** alta | média | baixa
```

---

## Fase 4 — Pesquisar boas práticas externas

Antes de propor melhorias, pesquise:
- Documentação oficial do Claude Code (skills, hooks, agentes)
- Padrões do rules-manager em `D:\Dev\Mundo da Lua\mundo-da-lua-crm\mundo-da-lua-crm-rules\LEARNINGS.md`
- Qualquer padrão novo que o rules-agent tenha adotado que seja aplicável ao backend

---

## Fase 4 — Gerar propostas de melhoria

Para cada problema identificado, gere uma proposta no formato:

```md
### Skill: [nome]
**Problema:** [descrição clara do que está subótimo]
**Evidência:** [entrada em LEARNINGS.md que sustenta — com data]
**Proposta:**
  - Antes: [trecho atual]
  - Depois: [trecho proposto]
**Impacto esperado:** [o que melhora]
**Confiança:** alta | média | baixa
```

Ordene por impacto estimado (maior primeiro).

---

## Fase 5 — Validar com o usuário

Apresente todas as propostas **antes** de executar qualquer mudança.

Pergunte:
- Quais propostas aprovar
- Se há ajustes antes de aplicar
- Se há melhorias que o usuário percebeu mas não estão na lista

**Não edite nenhuma skill antes da confirmação explícita.**

---

## Fase 6 — Executar melhorias aprovadas

Para cada proposta aprovada:

1. Edite o arquivo correspondente com a melhoria
2. Registre em `LEARNINGS.md`:
   ```
   [YYYY-MM-DD] — auto-melhoria — Observação: [skill] atualizada — [descrição da mudança] — Ação: monitorar nas próximas sessões — Confiança: média
   ```

---

## Fase 7 — Atualizar CLAUDE.md se necessário

Se novas skills foram criadas ou descontinuadas, atualize a tabela de skills em `CLAUDE.md`.

---

## Ciclo recomendado

Execute `/auto-melhorar` após **5 a 10 implementações** — tempo suficiente para acumular padrões reais sem deixar problemas se acumularem.
