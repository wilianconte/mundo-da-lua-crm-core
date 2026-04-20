---
name: consultar-regras
description: >
  Localiza e lê regras de negócio (RN-XXX), decisões arquiteturais (DEC-XXX), casos de uso
  (UC-XXX) e cenários de teste (CT-XXX) no repositório mundo-da-lua-crm-rules. Use antes de
  implementar qualquer funcionalidade referenciada por esses identificadores.
when_to_use: >
  Use sempre que um prompt de build/input/ referenciar RN-XXX, DEC-XXX, UC-XXX ou CT-XXX,
  ou quando o usuário pedir para verificar uma regra de negócio, decisão arquitetural ou
  caso de uso antes de implementar. Exemplos: "qual é a RN-036?", "como funciona o UC-001
  do financeiro?", "verifique a DEC-009 antes de implementar".
argument-hint: "RN-XXX | DEC-XXX | UC-XXX | CT-XXX [, ...]"
model: haiku
allowed-tools: Read Glob Grep
effort: low
---

# Consultar Regras de Negócio — mundo-da-lua-crm-rules

**Identificadores solicitados:** $ARGUMENTS

**Repositório de regras:** `D:\Dev\Mundo da Lua\mundo-da-lua-crm\mundo-da-lua-crm-rules\docs\`

---

## Passo 1 — Localizar os arquivos

Para cada identificador em $ARGUMENTS, buscar no repositório de regras:

```
RN-XXX  → docs/02-regras-de-negocio/**/*RN-XXX*.md
DEC-XXX → docs/05-decisoes/**/*DEC-XXX*.md
UC-XXX  → docs/06-casos-de-uso/**/*UC-XXX*.md
CT-XXX  → docs/07-cenarios-de-teste-funcional/**/*CT-XXX*.md
```

Se não encontrar, verificar também em:
- `docs/04-contratos-funcionais/` — para contratos de entidades
- `docs/03-fluxos-funcionais/` — para fluxos funcionais

---

## Passo 2 — Ler e resumir

Para cada arquivo encontrado, extrair:

- **Status** (Validada / Em revisão / Obsoleta)
- **Resumo** — o que a regra/decisão define
- **Condições** — pré-condições e restrições relevantes para implementação
- **Comportamento esperado** — o que o sistema deve fazer
- **Exceções** — casos de borda documentados
- **Impactos** — entidades e handlers afetados

---

## Passo 3 — Verificar lacunas relacionadas

Verificar em `docs/09-pendencias-e-lacunas/` se há LAC-XXX relacionado aos identificadores consultados.

Se houver LAC aberto que **bloqueia implementação**, reportar antes de prosseguir:
```
⚠️ LAC-XXX em aberto: [descrição] — implementação pode estar incompleta
```

---

## Passo 4 — Saída estruturada

Apresentar as regras consultadas no formato:

```
## RN-XXX — [Título]
**Status:** Validada
**Módulo:** [módulo]
**Resumo:** [1-2 linhas]
**Para implementação:**
- [ponto 1 relevante para o código]
- [ponto 2 relevante para o código]
**Exceções a tratar:** [lista ou "nenhuma"]
**LAC relacionado:** [LAC-XXX ou "nenhum"]
```

---

## Estrutura do repositório de regras (referência)

```
docs/
├── 02-regras-de-negocio/     ← RN-XXX por módulo
├── 03-fluxos-funcionais/     ← fluxos funcionais
├── 04-contratos-funcionais/  ← contratos de entidades
├── 05-decisoes/              ← DEC-XXX por módulo
├── 06-casos-de-uso/          ← UC-XXX por módulo
├── 07-cenarios-de-teste-funcional/ ← CT-XXX por módulo
└── 09-pendencias-e-lacunas/  ← LAC-XXX (gaps)
```

Módulos disponíveis: `agendamentos`, `alunos`, `auth`, `companies`, `cross-cutting`,
`cursos`, `customers`, `financeiro`, `funcionarios`, `guardioes`, `matriculas`,
`pessoas`, `tenant`
