# [Nome da Tela] - Guia para o Front-end

> **Data:** YYYY-MM-DD
> **Modulo:** [nome do modulo]
> **Autenticacao:** JWT obrigatorio em todas as operacoes

---

## Operacoes disponiveis

| Operacao | Tipo | Descricao |
|---|---|---|
| `nomeMutation` | Mutation | Descricao curta |
| `nomeQuery` | Query | Descricao curta |

---

## Mutations

### `nomeMutation`

```graphql
mutation NomeMutation($input: NomeInput!) {
  nomeMutation(input: $input) {
    # campos retornados
  }
}
```

**Input**

```graphql
input NomeInput {
  campo: Tipo!
  campoOpcional: Tipo
}
```

**Exemplo**

```json
{
  "input": {
    "campo": "valor"
  }
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `CODIGO_ERRO` | Descricao |

---

## Enums

### `NomeEnum`

| Valor | Label sugerido |
|---|---|
| `Valor` | Texto exibido |

---

## Validacoes no front

| Campo | Regra |
|---|---|
| `campo` | Descricao da regra |

---

## Tratamento de erro

```json
{
  "errors": [
    {
      "message": "mensagem tecnica",
      "extensions": { "code": "CODIGO_ERRO" }
    }
  ]
}
```

Mapear `extensions.code` para mensagens amigaveis. Exibir `message` apenas em logs internos.

---

## Fluxo de UX

1. Passo 1.
2. Passo 2.
3. Em sucesso: acao pos-sucesso.
4. Em erro: feedback por codigo de negocio.
