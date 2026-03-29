# Tela de Listagem de Usuários — Guia para o Front-end

## Contexto

O módulo de usuários expõe via GraphQL os dados de acesso/identidade da conta — não há vínculo direto com `Person` (CRM) retornado pela query de usuários. Caso precise cruzar dados de `Person`, consulte a documentação de pessoas separadamente.

| Conceito | Módulo | O que representa |
|---|---|---|
| `User` | `auth` | Conta de acesso — credenciais, login, status |
| `Person` | `crm` | Identidade humana — nome completo, CPF, contato |

> `User` **não expõe** `personId` no schema GraphQL. O join entre usuário e pessoa, quando necessário, deve ocorrer via `Person.userId` (no módulo CRM) ou outro identificador de negócio definido pelo back-end.

---

## Endpoints disponíveis

| Operação | Endpoint GraphQL | Autenticação |
|---|---|---|
| Login | `mutation { login(...) }` | ❌ Anônimo |
| Listar usuários (paginado) | `query { getUsers(...) }` | ✅ JWT obrigatório |
| Buscar usuário por ID | `query { getUserById(...) }` | ✅ JWT obrigatório |

---

## Como obter o token JWT (autenticação obrigatória)

Todas as queries exigem autenticação. O token vem do campo `token` retornado pela mutation de login:

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

Enviar em **todas** as requisições seguintes:
```
Authorization: Bearer <token>
```

O `tenantId` está embutido no JWT — o servidor filtra os dados por tenant automaticamente, sem necessidade de header extra.

---

## Campos disponíveis em `User`

Estes são os únicos campos expostos pelo schema. Campos sensíveis são ignorados pelo servidor e nunca aparecem na resposta.

```graphql
{
  id          # Guid — identificador único do usuário
  name        # string — nome de exibição da conta
  email       # string — e-mail de login
  isActive    # boolean — se o usuário está ativo
  createdAt   # DateTimeOffset — data/hora de criação (UTC)
  updatedAt   # DateTimeOffset? — data/hora da última alteração (UTC), null se nunca alterado
  createdBy   # Guid? — id do usuário que criou o registro
  updatedBy   # Guid? — id do usuário que fez a última alteração
}
```

> Campos **omitidos pelo servidor** (não existem no schema): `passwordHash`, `tenantId`, `isDeleted`, `deletedAt`.

---

## Query de listagem de usuários

A query usa **paginação cursor-based** (padrão Hot Chocolate). Suporta filtragem e ordenação declarativas.

### Assinatura

```graphql
query GetUsers(
  $first: Int
  $after: String
  $last: Int
  $before: String
  $where: UserFilterInput
  $order: [UserSortInput!]
) {
  getUsers(
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
      createdAt
      updatedAt
      createdBy
      updatedBy
    }
  }
}
```

### Variáveis — primeira página (20 itens, ordem alfabética)

```json
{
  "first": 20,
  "order": [{ "name": "ASC" }]
}
```

### Variáveis — próxima página (cursor-based)

```json
{
  "first": 20,
  "after": "<endCursor da página anterior>",
  "order": [{ "name": "ASC" }]
}
```

---

## Query de busca por ID

Retorna um único usuário ou `null` se não encontrado.

```graphql
query GetUserById($id: UUID!) {
  getUserById(id: $id) {
    id
    name
    email
    isActive
    createdAt
    updatedAt
    createdBy
    updatedBy
  }
}
```

```json
{
  "id": "00000000-0000-0000-0000-000000000001"
}
```

---

## Filtros úteis (`UserFilterInput`)

### Apenas usuários ativos

```graphql
getUsers(where: { isActive: { eq: true } })
```

### Buscar por e-mail (correspondência parcial)

```graphql
getUsers(where: { email: { contains: "mundodalua" } })
```

### Buscar por nome (parcial, corresponde ao `contains`)

```graphql
getUsers(where: { name: { contains: "admin" } })
```

### Combinar filtros (AND implícito)

```graphql
getUsers(where: {
  and: [
    { isActive: { eq: true } },
    { name: { contains: "admin" } }
  ]
})
```

---

## Opções de ordenação (`UserSortInput`)

```graphql
# Alfabético crescente
getUsers(order: [{ name: ASC }])

# Mais recentes primeiro
getUsers(order: [{ createdAt: DESC }])

# Ativos primeiro, depois alfabético
getUsers(order: [{ isActive: DESC }, { name: ASC }])
```

---

## Modelo de dados sugerido para a tela

```ts
interface UserRow {
  id: string                  // Guid
  name: string
  email: string
  isActive: boolean
  createdAt: string           // ISO 8601 com offset UTC
  updatedAt: string | null
  createdBy: string | null    // Guid do usuário auditor
  updatedBy: string | null
}

interface UserPageResult {
  totalCount: number
  pageInfo: {
    hasNextPage: boolean
    hasPreviousPage: boolean
    startCursor: string
    endCursor: string
  }
  nodes: UserRow[]
}
```

---

## Notas de implementação

- **Autenticação**: todas as queries (`getUsers`, `getUserById`) possuem o atributo `[Authorize]` — requisições sem JWT válido retornam erro `401`.
- **Multi-tenancy**: o servidor utiliza o `tenantId` embutido no JWT para filtrar os usuários automaticamente. O front não precisa (nem deve) enviar o tenant em headers ou variáveis de query.
- **Soft delete**: registros com `isDeleted = true` são filtrados automaticamente pelo EF Core e **nunca aparecem** nas respostas — não é necessário filtrar no front.
- **Paginação**: prefira sempre enviar `first` (forward paging). Evite `last`/`before` a não ser que a UX exija navegação reversa explícita.
- **Projeção**: a query suporta `[UseProjection]` — solicite apenas os campos que a tela realmente precisa para evitar over-fetching.
