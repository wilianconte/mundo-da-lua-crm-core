# Tela de Listagem de Usuarios - Guia para o Front-end

## Contexto

O modulo de usuarios expoe via GraphQL os dados de acesso/identidade da conta. Nao ha vinculo direto com `Person` (CRM) retornado pela query de usuarios.

| Conceito | Modulo | O que representa |
|---|---|---|
| `User` | `auth` | Conta de acesso - credenciais, login, status |
| `Person` | `crm` | Identidade humana - nome completo, CPF, contato |

Cada `User` pode estar vinculado a uma `Person` (modulo CRM) atraves do campo `personId` (nullable).

---

## Endpoints disponiveis

| Operacao | Endpoint GraphQL | Autenticacao |
|---|---|---|
| Login | `mutation { login(...) }` | Nao exige JWT |
| Listar usuarios (paginado) | `query { users(...) }` | JWT obrigatorio |
| Buscar usuario por ID | `query { userById(...) }` | JWT obrigatorio |

---

## Nome correto dos campos no GraphQL (Hot Chocolate)

O Hot Chocolate remove o prefixo `Get` dos metodos C#.

- Use `users` para listagem
- Use `userById` para busca por ID
- Nao use `getUsers`, `getUserById`, `getUser` ou `user` (campos inexistentes no schema)

Exemplo valido - listagem:

```graphql
query {
  users(first: 10) {
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

Exemplo valido - busca por ID:

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

---

## Como obter o token JWT (autenticacao obrigatoria)

Todas as queries exigem autenticacao. O token vem da mutation `login`:

```graphql
mutation Login {
  login(input: {
    tenantId: "00000000-0000-0000-0000-000000000001"
    email: "admin@mundodalua.com"
    password: "Admin@123"
  }) {
    token
    expiresAt
    userId
    name
    email
  }
}
```

Enviar em todas as requisicoes:

```text
Authorization: Bearer <token>
```

---

## Campos disponiveis em `User`

```graphql
{
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
```

Campos omitidos pelo servidor: `passwordHash`, `tenantId`, `isDeleted`, `deletedAt`.

---

## Variaveis uteis para listagem

Primeira pagina (20 itens, ordem alfabetica):

```json
{
  "first": 20,
  "order": [{ "name": "ASC" }]
}
```

Proxima pagina (cursor-based):

```json
{
  "first": 20,
  "after": "<endCursor da pagina anterior>",
  "order": [{ "name": "ASC" }]
}
```

---

## Filtros uteis (`UserFilterInput`)

```graphql
users(where: { isActive: { eq: true } })
users(where: { email: { contains: "mundodalua" } })
users(where: { personId: { neq: null } })
users(where: { personId: { eq: null } })
users(where: { name: { contains: "admin" } })
users(where: {
  and: [
    { isActive: { eq: true } },
    { name: { contains: "admin" } }
  ]
})
```

---

## Ordenacao (`UserSortInput`)

```graphql
users(order: [{ name: ASC }])
users(order: [{ createdAt: DESC }])
users(order: [{ isActive: DESC }, { name: ASC }])
```

---

## Notas de implementacao

- Autenticacao: `users` e `userById` exigem JWT (`[Authorize]`).
- Multi-tenancy: o servidor usa o `tenantId` do JWT para filtrar dados.
- Soft delete: registros com `isDeleted = true` nao aparecem na resposta.
- Paginacao: preferir `first` (forward paging).
- Projecao: solicitar apenas os campos necessarios.

---

## Validacao de testes - consulta de users

Execucao em **2026-03-29**:

```bash
dotnet test "4 - Tests/UnitTests/MyCRM.UnitTests.csproj" --filter "FullyQualifiedName~UserQuery" --nologo
```

Resultado:

- Passed: 19
- Failed: 0
- Skipped: 0

Cenarios validados:

- Schema expoe `users` e `userById`
- `getUsers`, `getUserById`, `getUser` e `user` retornam erro de campo inexistente
- Campos de `User` permitidos e bloqueio de campos sensiveis
- Autorizacao obrigatoria
- Listagem com base vazia
- Listagem com usuarios seedados
- Filtro por `isActive`
- Filtro por `email contains`
- Paginacao com `first` + `pageInfo.hasNextPage`
- Busca por ID existente
- Busca por ID inexistente retorna `null`
- Soft delete nao retorna na listagem

