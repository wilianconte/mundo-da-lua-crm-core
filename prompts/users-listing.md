# Tela de Listagem de Usuários — Guia para o Front-end

## Contexto

O sistema possui dois conceitos distintos que se complementam:

| Conceito | Schema | O que representa |
|---|---|---|
| `User` | `auth` | Conta de acesso — credenciais, login, permissões |
| `Person` | `crm` | Identidade humana — nome completo, CPF, contato, dados pessoais |

**Relação:** cada `User` pode estar vinculado a exatamente uma `Person` através do campo `personId` (nullable). A restrição de unicidade garante que uma `Person` nunca tenha dois usuários no mesmo tenant.

```
Person 1 ──── 0..1 User
              └── User.personId → Person.id
```

---

## Estado atual do backend

| Operação | Disponível? | Endpoint GraphQL |
|---|---|---|
| Login | ✅ sim | `mutation { login(...) }` |
| Listar usuários | ✅ sim | `query { getUsers }` |
| Buscar usuário por ID | ✅ sim | `query { getUserById }` |
| Listar pessoas | ✅ sim | `query { getPeople }` |
| Buscar pessoa por ID | ✅ sim | `query { getPersonById }` |

> **Nota:** `personId` estará disponível no campo `User` após o merge do PR que vincula User ↔ Person. Enquanto isso, os demais campos já estão disponíveis.

---

## Como obter o token JWT (autenticação obrigatória)

Todas as queries exigem autenticação. O token vem do campo `token` da mutation de login:

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

Enviar em todas as requisições seguintes:
```
Authorization: Bearer <token>
```

O `tenantId` está embutido no JWT — não é necessário enviá-lo em headers separados.

---

## Query de listagem de usuários (quando implementada)

A query seguirá o padrão de cursor-based pagination do Hot Chocolate:

```graphql
query GetUsers(
  $first: Int
  $after: String
  $where: UserFilterInput
  $order: [UserSortInput!]
) {
  getUsers(first: $first, after: $after, where: $where, order: $order) {
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
    }
  }
}
```

Exemplo de variáveis para primeira página:
```json
{
  "first": 20,
  "order": [{ "name": "ASC" }]
}
```

---

## Como acessar dados de Pessoa a partir de um Usuário

Como `User` (módulo auth) e `Person` (módulo crm) são DbContexts independentes, **não existe join automático no GraphQL** entre as duas entidades. O front-end deve fazer duas requisições:

### Passo 1 — Buscar usuários

```graphql
query GetUsers {
  getUsers(first: 50) {
    nodes {
      id
      name
      email
      isActive
      personId   # ← chave para buscar dados de pessoa
    }
  }
}
```

### Passo 2 — Para cada usuário com `personId`, buscar a pessoa

```graphql
query GetPersonById($id: UUID!) {
  getPersonById(id: $id) {
    id
    fullName
    documentNumber
    birthDate
    gender
    email
    phone
    mobilePhone
    status
  }
}
```

### Estratégia recomendada no front-end

Para evitar N+1 requisições, busque todos os `personId` únicos da listagem e faça uma única query com filtro:

```graphql
query GetPeopleByIds($ids: [UUID!]!) {
  getPeople(where: { id: { in: $ids } }) {
    nodes {
      id
      fullName
      documentNumber
      email
      mobilePhone
    }
  }
}
```

Depois cruze os dados localmente por `user.personId === person.id`.

---

## Campos disponíveis em Person

```graphql
{
  id
  fullName          # nome completo
  documentNumber    # CPF
  birthDate
  gender            # MALE | FEMALE | OTHER | PREFER_NOT_TO_SAY
  maritalStatus
  email
  phone
  mobilePhone
  whatsApp
  status            # ACTIVE | INACTIVE | BLOCKED
  notes
  createdAt
  updatedAt
}
```

---

## Filtros e ordenação úteis para a listagem

### Filtrar usuários ativos

```graphql
query {
  getUsers(where: { isActive: { eq: true } }) {
    nodes { id name email personId }
  }
}
```

### Buscar por e-mail (parcial)

```graphql
query {
  getUsers(where: { email: { contains: "mundodalua" } }) {
    nodes { id name email }
  }
}
```

### Ordenar por nome

```graphql
query {
  getUsers(order: [{ name: ASC }]) {
    nodes { id name email }
  }
}
```

---

## Modelo de dados final para a tela (sugestão)

Após cruzar User + Person, cada linha da listagem terá:

```ts
interface UserRow {
  // dados do User (auth)
  id: string
  email: string
  isActive: boolean
  createdAt: string

  // dados da Person (crm) — null se usuário não vinculado a pessoa
  person: {
    id: string
    fullName: string
    documentNumber: string | null
    mobilePhone: string | null
  } | null
}
```
