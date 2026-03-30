# Listagem de Usuarios - Guia para o Front-end

> **Data:** 2026-03-30
> **Modulo:** Auth
> **Autenticacao:** JWT obrigatorio

---

## Operacoes disponiveis

| Operacao | Tipo | Descricao |
|---|---|---|
| `users` | Query | Listagem paginada com filtros e ordenacao |
| `userById` | Query | Detalhe de um usuario por ID |

---

## Queries

### `users`

Paginacao baseada em cursor (Relay-style). Suporta filtros e ordenacao.

```graphql
query GetUsers(
  $first: Int
  $after: String
  $last: Int
  $before: String
  $where: UserFilterInput
  $order: [UserSortInput!]
) {
  users(
    first: $first
    after: $after
    last: $last
    before: $before
    where: $where
    order: $order
  ) {
    totalCount
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
    nodes {
      id
      name
      email
      isActive
      personId
      createdAt
      updatedAt
      createdBy
      updatedBy
    }
  }
}
```

**Exemplo — primeira pagina**

```json
{
  "first": 20,
  "order": [{ "name": "ASC" }]
}
```

**Exemplo — proxima pagina**

```json
{
  "first": 20,
  "after": "<endCursor da pagina anterior>",
  "order": [{ "name": "ASC" }]
}
```

**Exemplo — com filtro**

```json
{
  "first": 20,
  "where": {
    "and": [
      { "isActive": { "eq": true } },
      { "name": { "contains": "maria" } }
    ]
  },
  "order": [{ "name": "ASC" }]
}
```

---

### `userById`

Retorna um unico usuario ou `null` se nao encontrado.

```graphql
query GetUserById($id: UUID!) {
  userById(id: $id) {
    id
    name
    email
    isActive
    personId
    createdAt
    updatedAt
    createdBy
    updatedBy
  }
}
```

**Exemplo**

```json
{
  "id": "00000000-0000-0000-0000-000000000001"
}
```

---

## Campos disponiveis

| Campo | Tipo | Descricao |
|---|---|---|
| `id` | UUID | Identificador do usuario |
| `name` | String | Nome completo |
| `email` | String | E-mail de acesso |
| `isActive` | Boolean | Se o usuario esta ativo |
| `personId` | UUID | ID da pessoa vinculada (nullable) |
| `createdAt` | DateTime | Data de criacao |
| `updatedAt` | DateTime | Data da ultima atualizacao |
| `createdBy` | String | Quem criou |
| `updatedBy` | String | Quem atualizou por ultimo |

> `passwordHash`, `tenantId`, `isDeleted` e `deletedAt` nunca sao retornados — nao solicitar.

---

## Paginacao

A API usa paginacao por cursor (Relay Connection). **Nao usar offset/page/skip.**

> **Limite maximo:** `first` e `last` aceitam no maximo **50** itens por pagina. Valores acima retornam erro `HC0051`.

| Campo | Uso |
|---|---|
| `first` + `after` | Avancar paginas |
| `last` + `before` | Retroceder paginas |
| `totalCount` | Total de registros para exibir contador |
| `pageInfo.hasNextPage` | Habilitar/desabilitar botao "Proxima" |
| `pageInfo.hasPreviousPage` | Habilitar/desabilitar botao "Anterior" |
| `pageInfo.endCursor` | Cursor para proxima pagina |
| `pageInfo.startCursor` | Cursor para pagina anterior |

---

## Filtros (`UserFilterInput`)

| Filtro | Exemplo |
|---|---|
| Apenas ativos | `{ "isActive": { "eq": true } }` |
| Por nome | `{ "name": { "contains": "joao" } }` |
| Por e-mail | `{ "email": { "contains": "mundodalua" } }` |
| Com pessoa vinculada | `{ "personId": { "neq": null } }` |
| Sem pessoa vinculada | `{ "personId": { "eq": null } }` |

Combinacao com `and`:

```json
{
  "where": {
    "and": [
      { "isActive": { "eq": true } },
      { "name": { "contains": "admin" } }
    ]
  }
}
```

---

## Ordenacao (`UserSortInput`)

Campos ordenaveis: `name`, `email`, `createdAt`, `updatedAt`, `isActive`.

```json
{ "order": [{ "createdAt": "DESC" }] }
{ "order": [{ "isActive": "DESC" }, { "name": "ASC" }] }
```

---

## Notas importantes

- O servidor filtra automaticamente por `tenantId` do JWT — nao enviar `tenantId` nas queries.
- Registros excluidos (`isDeleted = true`) nunca aparecem na listagem.
- Os nomes corretos das queries sao `users` e `userById`. Nomes como `getUsers`, `getUserById` ou `user` nao existem no schema e retornam erro.
- Solicitar apenas os campos necessarios para a tela (projecao GraphQL).

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

1. Front carrega a primeira pagina com `first: 20` ao montar a tela.
2. Exibe `totalCount` como contador de resultados.
3. Botao "Proxima" ativo quando `pageInfo.hasNextPage = true`; envia `after: endCursor`.
4. Botao "Anterior" ativo quando `pageInfo.hasPreviousPage = true`; envia `before: startCursor` com `last`.
5. Ao alterar filtro ou ordenacao, resetar cursor (remover `after`/`before`) e buscar a primeira pagina novamente.
6. `userById` e usado para abrir detalhe ou pre-preencher formulario de edicao.
