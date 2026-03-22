# Prompt: Integração Front-end — CRUD de Empresas (Company)

## Contexto

O back-end do MyCRM expõe **exclusivamente uma API GraphQL** (Hot Chocolate 15+). Não existe REST. Toda a integração deve ser feita via GraphQL sobre o endpoint único:

```
POST /graphql
```

Autenticação: **JWT Bearer Token** no header `Authorization: Bearer <token>`.
O `tenant_id` é extraído automaticamente do token JWT — não envie header de tenant.

---

## Schema GraphQL — Company

### Tipos de saída (`CompanyDto`)

```graphql
type CompanyDto {
  id: UUID!
  tenantId: UUID!
  legalName: String!
  tradeName: String
  registrationNumber: String      # CNPJ (único por tenant)
  stateRegistration: String
  municipalRegistration: String
  email: String                   # único por tenant
  primaryPhone: String
  secondaryPhone: String
  whatsAppNumber: String
  website: String
  contactPersonName: String
  contactPersonEmail: String
  contactPersonPhone: String
  companyType: CompanyType
  industry: String
  profileImageUrl: String
  address: AddressDto
  status: CompanyStatus!
  notes: String
  createdAt: DateTime!
  updatedAt: DateTime
}

type AddressDto {
  street: String!
  number: String
  complement: String
  neighborhood: String!
  city: String!
  state: String!      # 2 letras (ex: "SP")
  zipCode: String!
  country: String!    # ISO 2 letras (ex: "BR")
}

enum CompanyType {
  SUPPLIER            # 1
  PARTNER             # 2
  SCHOOL              # 3
  CORPORATE_CUSTOMER  # 4
  BILLING_ACCOUNT     # 5
  SERVICE_PROVIDER    # 6
  SPONSOR             # 7
  OTHER               # 8
}

enum CompanyStatus {
  ACTIVE      # 1
  INACTIVE    # 2
  BLOCKED     # 3
  SUSPENDED   # 4
}
```

---

## Operações CRUD

### 1. CREATE — Criar empresa

**Mutation:**

```graphql
mutation CreateCompany($input: CreateCompanyInput!) {
  createCompany(input: $input) {
    id
    legalName
    tradeName
    registrationNumber
    email
    primaryPhone
    companyType
    status
    createdAt
  }
}
```

**Variáveis:**

```json
{
  "input": {
    "legalName": "Empresa Exemplo Ltda",          // OBRIGATÓRIO — máx 300 chars
    "tradeName": "Empresa Exemplo",               // opcional — máx 300 chars
    "registrationNumber": "12.345.678/0001-99",   // opcional — CNPJ, máx 30 chars, único por tenant
    "stateRegistration": "123456789",             // opcional — IE, máx 30 chars
    "municipalRegistration": "987654",            // opcional — IM, máx 30 chars
    "email": "contato@empresa.com",               // opcional — válido, máx 254 chars, único por tenant
    "primaryPhone": "(11) 3000-0000",             // opcional — máx 30 chars
    "secondaryPhone": "(11) 3000-0001",           // opcional — máx 30 chars
    "whatsAppNumber": "(11) 99999-0000",          // opcional — máx 30 chars
    "website": "https://empresa.com",             // opcional — máx 500 chars
    "contactPersonName": "João Silva",            // opcional — máx 300 chars
    "contactPersonEmail": "joao@empresa.com",     // opcional — válido, máx 254 chars
    "contactPersonPhone": "(11) 98888-0000",      // opcional — máx 30 chars
    "companyType": "SUPPLIER",                    // opcional — enum CompanyType
    "industry": "Tecnologia",                     // opcional — máx 150 chars
    "profileImageUrl": "https://cdn.../foto.png", // opcional — máx 2000 chars
    "notes": "Observações livres"                 // opcional — máx 2000 chars
  }
}
```

**Erros possíveis:**

| `errorCode` | Mensagem | Causa |
|---|---|---|
| `COMPANY_EMAIL_DUPLICATE` | "A company with this email already exists." | Email já cadastrado no tenant |
| `COMPANY_REGISTRATION_DUPLICATE` | "A company with this registration number already exists." | CNPJ já cadastrado no tenant |
| `VALIDATION_ERROR` | Mensagens do FluentValidation | Campo inválido (ex: email malformado, campo muito longo) |

---

### 2. READ — Listar empresas (paginado)

**Query com paginação por cursor e filtros:**

```graphql
query GetCompanies(
  $first: Int
  $after: String
  $where: CompanyFilterInput
  $order: [CompanySortInput!]
) {
  companies(
    first: $first
    after: $after
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
      legalName
      tradeName
      registrationNumber
      email
      primaryPhone
      companyType
      status
      createdAt
    }
  }
}
```

**Variáveis (exemplo — página 1, 20 itens, filtro por nome):**

```json
{
  "first": 20,
  "after": null,
  "where": {
    "legalName": { "contains": "Exemplo" },
    "status": { "eq": "ACTIVE" }
  },
  "order": [{ "legalName": "ASC" }]
}
```

**Filtros disponíveis:** qualquer campo do `CompanyDto` via `CompanyFilterInput` (gerado automaticamente pelo Hot Chocolate com operadores: `eq`, `neq`, `contains`, `startsWith`, `gt`, `lt`, `in`, etc.).

---

### 3. READ — Buscar empresa por ID

**Query:**

```graphql
query GetCompanyById($id: UUID!) {
  companyById(id: $id) {
    id
    legalName
    tradeName
    registrationNumber
    stateRegistration
    municipalRegistration
    email
    primaryPhone
    secondaryPhone
    whatsAppNumber
    website
    contactPersonName
    contactPersonEmail
    contactPersonPhone
    companyType
    industry
    profileImageUrl
    status
    notes
    createdAt
    updatedAt
    address {
      street
      number
      complement
      neighborhood
      city
      state
      zipCode
      country
    }
  }
}
```

**Variáveis:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Retorna `null`** se a empresa não existir ou pertencer a outro tenant.

---

### 4. UPDATE — Atualizar empresa

**Mutation:**

```graphql
mutation UpdateCompany($id: UUID!, $input: UpdateCompanyInput!) {
  updateCompany(id: $id, input: $input) {
    id
    legalName
    tradeName
    registrationNumber
    email
    primaryPhone
    companyType
    status
    updatedAt
  }
}
```

**Variáveis:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "input": {
    "legalName": "Empresa Atualizada Ltda",       // OBRIGATÓRIO
    "tradeName": "Empresa Atualizada",
    "registrationNumber": "12.345.678/0001-99",   // único por tenant (exceto o próprio)
    "stateRegistration": "123456789",
    "municipalRegistration": "987654",
    "email": "novo@empresa.com",                  // único por tenant (exceto o próprio)
    "primaryPhone": "(11) 3001-0000",
    "secondaryPhone": null,
    "whatsAppNumber": "(11) 99999-1111",
    "website": "https://novosite.com",
    "contactPersonName": "Maria Souza",
    "contactPersonEmail": "maria@empresa.com",
    "contactPersonPhone": "(11) 97777-0000",
    "companyType": "PARTNER",
    "industry": "Consultoria",
    "profileImageUrl": null,
    "notes": "Parceiro estratégico"
  }
}
```

**Erros possíveis:**

| `errorCode` | Causa |
|---|---|
| `COMPANY_NOT_FOUND` | ID não encontrado ou pertence a outro tenant |
| `COMPANY_EMAIL_DUPLICATE` | Email já usado por outra empresa do tenant |
| `COMPANY_REGISTRATION_DUPLICATE` | CNPJ já usado por outra empresa do tenant |
| `VALIDATION_ERROR` | Campo inválido |

---

### 5. SET ADDRESS — Definir endereço da empresa

O endereço é uma operação separada do update principal. Use esta mutation para criar ou substituir o endereço.

**Mutation:**

```graphql
mutation SetCompanyAddress($input: SetCompanyAddressInput!) {
  setCompanyAddress(input: $input) {
    id
    address {
      street
      number
      complement
      neighborhood
      city
      state
      zipCode
      country
    }
    updatedAt
  }
}
```

**Variáveis:**

```json
{
  "input": {
    "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "street": "Rua das Flores",          // OBRIGATÓRIO — máx 300 chars
    "number": "123",                     // opcional — máx 20 chars
    "complement": "Sala 45",             // opcional — máx 100 chars
    "neighborhood": "Centro",            // OBRIGATÓRIO — máx 150 chars
    "city": "São Paulo",                 // OBRIGATÓRIO — máx 150 chars
    "state": "SP",                       // OBRIGATÓRIO — exatamente 2 letras
    "zipCode": "01310-100",              // OBRIGATÓRIO — máx 10 chars
    "country": "BR"                      // OBRIGATÓRIO — exatamente 2 letras ISO (default "BR")
  }
}
```

---

### 6. DELETE — Excluir empresa (soft delete)

**Mutation:**

```graphql
mutation DeleteCompany($id: UUID!) {
  deleteCompany(id: $id)
}
```

**Variáveis:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Retorno:** `true` se excluída com sucesso.
**Comportamento:** soft delete — a empresa não aparece mais em nenhuma query, mas não é apagada fisicamente do banco.

**Erros possíveis:**

| `errorCode` | Causa |
|---|---|
| `COMPANY_NOT_FOUND` | ID não encontrado |

---

## Tratamento de Erros GraphQL

Todos os erros retornam no campo `errors` da resposta GraphQL com a seguinte estrutura:

```json
{
  "errors": [
    {
      "message": "A company with this email already exists.",
      "extensions": {
        "code": "COMPANY_EMAIL_DUPLICATE"
      }
    }
  ],
  "data": {
    "createCompany": null
  }
}
```

**Padrão de tratamento recomendado:**

```typescript
// Pseudocódigo — adapte ao framework (Apollo, URQL, etc.)
const { data, errors } = await client.mutate({ mutation: CREATE_COMPANY, variables });

if (errors?.length) {
  const code = errors[0].extensions?.code;
  switch (code) {
    case 'COMPANY_EMAIL_DUPLICATE':
      showFieldError('email', 'Este e-mail já está cadastrado.');
      break;
    case 'COMPANY_REGISTRATION_DUPLICATE':
      showFieldError('registrationNumber', 'Este CNPJ já está cadastrado.');
      break;
    case 'AUTH_NOT_AUTHORIZED':
      redirectToLogin();
      break;
    case 'VALIDATION_ERROR':
      showValidationErrors(errors[0].message);
      break;
    default:
      showGenericError();
  }
  return;
}

// sucesso
const company = data.createCompany;
```

---

## Regras de Negócio Importantes para o Front-end

1. **`legalName` é o único campo obrigatório** em Create e Update.
2. **CNPJ e e-mail são únicos por tenant** — mostre erro inline no campo quando o back retornar `COMPANY_EMAIL_DUPLICATE` ou `COMPANY_REGISTRATION_DUPLICATE`.
3. **O endereço é uma operação separada** — use `setCompanyAddress` após criar a empresa se quiser cadastrar o endereço no mesmo fluxo.
4. **`status` não é alterado via `updateCompany`** — mudanças de status (ativar, desativar, bloquear) devem ser operações separadas se implementadas no futuro.
5. **Paginação por cursor** — use `pageInfo.endCursor` como `after` para carregar a próxima página.
6. **Soft delete** — após deletar, a empresa desaparece das listagens, mas exiba confirmação antes de executar (ação irreversível pelo front-end).
7. **`companyType` aceita `null`** — campo opcional; se enviado, deve ser um dos valores do enum `CompanyType`.
8. **Campos de e-mail são normalizados para minúsculo** no back-end — não se preocupe com case no filtro de duplicidade.

---

## Exemplo de Fluxo Completo: Cadastro de Empresa com Endereço

```
1. Usuário preenche formulário
2. POST /graphql → mutation createCompany(input: {...})
   → Sucesso: recebe { id, legalName, ... }
   → Erro: trata por errorCode
3. Se tem endereço:
   POST /graphql → mutation setCompanyAddress(input: { companyId: <id>, ... })
4. Redireciona para detalhe da empresa (query companyById)
```
