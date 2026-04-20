# LEARNINGS — backend-agent (mundo-da-lua-crm-core)

Aprendizado persistente do agente de backend. Lido ao iniciar tarefas complexas, atualizado ao encerrar cada implementação.

Formato:
> `[YYYY-MM-DD] — [tipo] — Observação: [o que foi notado] — Ação: [o que fazer ou evitar] — Confiança: alta | média | baixa`

---

## O que funcionou

> `2026-04-19 — estrutura — Observação: separar CLAUDE.md (comportamento do agente) de SKILL.md (referência técnica) eliminou duplicação e torna cada arquivo mais fácil de manter — Ação: manter CLAUDE.md com no máximo ~100 linhas; detalhes técnicos sempre no SKILL.md ou references/ — Confiança: alta`

> `2026-04-19 — estrutura — Observação: campos description + when_to_use separados nas skills melhoram precisão de trigger sem estourar o limite de 1.536 chars — Ação: sempre usar os dois campos separados em skills novas — Confiança: alta`

> `2026-04-19 — estrutura — Observação: skills com efeitos colaterais (auto-melhorar, qualquer skill que escreve arquivos ou dispara CLIs) devem ter disable-model-invocation:true para evitar acionamento indesejado — Ação: aplicar disable-model-invocation:true em toda skill que modifica estado — Confiança: alta`

> `2026-04-19 — integração — Observação: o rules repo (mundo-da-lua-crm-rules) é a fonte de verdade funcional; prompts chegam em build/input/ já referenciando RN-XXX e DEC-XXX — Ação: ao processar build/input/, ler os arquivos RN/DEC do rules repo antes de implementar para garantir fidelidade às regras de negócio — Confiança: alta`

> `2026-04-19 — monitor — Observação: verificar TaskList antes de armar um novo Monitor evita duplicação do loop no SessionStart — Ação: sempre chamar TaskList no início da sessão; só criar monitor se não existir task com build/input em andamento — Confiança: alta`

> `2026-04-19 — módulo-financeiro — Observação: módulo financeiro foi implementado dentro do CRMDbContext (schema crm), não em módulo separado — Ação: novos domínios financeiros continuam no schema crm até haver decisão explícita de separação — Confiança: alta`

> `2026-04-19 — módulo-financeiro — Observação: Wallet.Balance é calculado via query (GetCurrentBalanceAsync), não armazenado como campo — Ação: nunca ler Balance direto da entidade; sempre usar o método de cálculo para evitar inconsistências — Confiança: alta`

> `2026-04-19 — módulo-financeiro — Observação: Transfer gera dois Transactions (Expense + Income) em vez de entidade Transfer separada — Ação: não criar entidade Transfer; modelar transferência como par de transactions vinculadas — Confiança: alta`

---

## O que falhou

> `2026-04-19 — build — Observação: migration não pode ser criada com build quebrado — Ação: sempre corrigir erros de compilação antes de executar dotnet ef migrations add — Confiança: alta`

---

## Preferências do usuário

> `2026-04-19 — preferência — Observação: usuário prefere respostas curtas e diretas, sem sumários longos ao final — Ação: encerrar respostas com no máximo 2 linhas de resumo — Confiança: alta`

> `2026-04-19 — preferência — Observação: usuário valoriza busca por referências externas (docs oficiais, comunidade) antes de criar algo do zero — Ação: ao criar nova skill ou padrão, pesquisar antes de inventar estrutura — Confiança: alta`

> `2026-04-19 — preferência — Observação: usuário quer autonomia do agente com confirmação apenas em ações destrutivas ou irreversíveis — Ação: agir autonomamente em implementações; pedir confirmação apenas em push, delete de branch, ou ações que afetam outros — Confiança: alta`

---

## Padrões do projeto

> `2026-04-19 — padrão — Observação: três repositórios com papéis distintos: rules (documentação funcional), core (backend .NET), front (frontend) — Ação: sempre verificar o repositório correto antes de implementar — Confiança: alta`

> `2026-04-19 — padrão — Observação: build/input/ recebe prompts do rules-agent; build/output/ registra implementações concluídas — Ação: nunca apagar arquivos de build/input/ sem processar; nunca pular o build/output/ — Confiança: alta`

> `2026-04-19 — padrão — Observação: entidades de negócio usam permissões granulares (read/create/update/delete); entidades admin usam manage — Ação: seguir essa convenção sem exceção; nunca usar [Authorize] genérico em resolvers de negócio — Confiança: alta`

> `2026-04-19 — padrão — Observação: módulos seguem estrutura {Modulo}.Domain / {Modulo}.Application / {Modulo}.Infrastructure com namespace MyCRM.{Modulo}.{Camada} — Ação: ao criar novo módulo, seguir exatamente essa estrutura; nunca misturar responsabilidades entre camadas — Confiança: alta`

---

## Desempenho de Modelos

Registre aqui o resultado de cada invocação de skill com modelo atribuído.
O `/auto-melhorar` analisa esta seção e propõe trocas de modelo quando detecta padrão de falha.

**Formato:**
> `[YYYY-MM-DD] — modelo — Skill: <nome> | Modelo: <haiku|sonnet|opus> | Resultado: ✅ efetivo | ⚠️ parcial | ❌ falhou | Observação: <o que aconteceu>`

**Regra de escalação:**
- `⚠️ parcial` 2x seguidas na mesma skill → `/auto-melhorar` propõe upgrade de modelo
- `❌ falhou` 1x → `/auto-melhorar` propõe upgrade imediato
- `✅ efetivo` 5x seguidas com modelo mais barato → `/auto-melhorar` pode propor downgrade

_(sem registros ainda — primeiro log após próxima invocação de skill com modelo atribuído)_

---

## Questões em aberto

_(nenhuma no momento)_
