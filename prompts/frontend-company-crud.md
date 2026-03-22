# Prompt — Integração Frontend: CRUD de Company

## Contexto

Você está desenvolvendo a interface de cadastro de **empresas (Company)** para o MyCRM.
O backend expõe uma API **GraphQL única** — não há endpoints REST.
O servidor GraphQL roda em `http://localhost:5095/graphql` (dev).

Toda requisição autenticada exige o header:
```
Authorization: Bearer <jwt>
```

O JWT é obtido via mutation de login (veja seção de autenticação abaixo).

---

## Autenticação

Antes de qualquer operação, obtenha o token:

```graphql
mutation Login {
  login(input: {
    tenantId: "00000000-0000-0000-0000-000000000001"
    email: "admin@mundodalua.com.br"
    password: "Admin@123"
  }) {
    token
  }
}
```

O campo retornado é `token` (não `accessToken`). Armazene-o e envie em todas as requisições subsequentes no header `Authorization: Bearer <token>`.

---

## Modelo de dados — Company

Campos retornados pela API (`CompanyDto`):

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | `UUID` | sim | Identificador único |
| `tenantId` | `UUID` | sim | Tenant ao qual pertence |
| `legalName` | `String` | sim | Razão Social |
| `tradeName` | `String?` | não | Nome Fantasia |
| `registrationNumber` | `String?` | não | CNPJ (único por tenant) |
| `stateRegistration` | `String?` | não | Inscrição Estadual |
| `municipalRegistration` | `String?` | não | Inscrição Municipal |
| `email` | `String?` | não | E-mail corporativo (único por tenant) |
| `primaryPhone` | `String?` | não | Telefone principal |
| `secondaryPhone` | `String?` | não | Telefone secundário |
| `whatsAppNumber` | `String?` | não | WhatsApp |
| `website` | `String?` | não | Site |
| `contactPersonName` | `String?` | não | Nome do contato humano |
| `contactPersonEmail` | `String?` | não | E-mail do contato |
| `contactPersonPhone` | `String?` | não | Telefone do contato |
| `companyType` | `CompanyType?` | não | Tipo da empresa (enum) |
| `industry` | `String?` | não | Setor / Segmento |
| `profileImageUrl` | `String?` | não | URL da logo/imagem |
| `address` | `AddressDto?` | não | Endereço (objeto aninhado) |
| `status` | `CompanyStatus` | sim | Status atual (enum) |
| `notes` | `String?` | não | Observações internas |
| `createdAt` | `DateTime` | sim | Data de criação |
| `updatedAt` | `DateTime?` | não | Última atualização |

### Enum — CompanyType

| Valor GraphQL | Int | Descrição |
|---|---|---|
| `SUPPLIER` | 1 | Fornecedor |
| `PARTNER` | 2 | Parceiro |
| `SCHOOL` | 3 | Escola |
| `CORPORATE_CUSTOMER` | 4 | Cliente corporativo |
| `BILLING_ACCOUNT` | 5 | Conta de cobrança |
| `SERVICE_PROVIDER` | 6 | Prestador de serviços |
| `SPONSOR` | 7 | Patrocinador |
| `OTHER` | 8 | Outro |

### Enum — CompanyStatus

| Valor GraphQL | Int | Descrição |
|---|---|---|
| `ACTIVE` | 1 | Ativo |
| `INACTIVE` | 2 | Inativo |
| `BLOCKED` | 3 | Bloqueado |
| `SUSPENDED` | 4 | Suspenso |

### Objeto — AddressDto (endereço aninhado)

| Campo | Tipo | Obrigatório |
|---|---|---|
| `street` | `String` | sim |
| `number` | `String?` | não |
| `complement` | `String?` | não |
| `neighborhood` | `String` | sim |
| `city` | `String` | sim |
| `state` | `String` | sim (2 letras, ex: `SP`) |
| `zipCode` | `String` | sim |
| `country` | `String` | sim (2 letras ISO, ex: `BR`) |

---

## Operações disponíveis

### 1. Listar empresas (com paginação, filtro e ordenação)

A query `getCompanies` suporta paginação por cursor (Relay), filtros e ordenação nativos do Hot Chocolate.

```graphql
query GetCompanies(
  $first: Int
  $after: String
  $where: CompanyFilterInput
  $order: [CompanySortInput!]
) {
  companies(first: $first, after: $after, where: $where, order: $order) {
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
      industry
      status
      createdAt
    }
  }
}
```

**Variáveis — exemplo de uso:**
```json
{
  "first": 20,
  "after": null,
  "where": {
    "status": { "eq": "ACTIVE" },
    "companyType": { "eq": "SUPPLIER" }
  },
  "order": [{ "legalName": "ASC" }]
}
```

Para avançar de página, passe `"after": "<endCursor da página anterior>"`.

---

### 2. Buscar empresa por ID

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
{ "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

Retorna `null` se não encontrado (ou se pertencer a outro tenant).

---

### 3. Criar empresa

Apenas `legalName` é obrigatório. Todos os demais campos são opcionais.

```graphql
mutation CreateCompany($input: CreateCompanyInput!) {
  createCompany(input: $input) {
    id
    legalName
    tradeName
    registrationNumber
    email
    companyType
    status
    createdAt
  }
}
```

**Variáveis — exemplo mínimo:**
```json
{
  "input": {
    "legalName": "Editora ABC Ltda"
  }
}
```

**Variáveis — exemplo completo:**
```json
{
  "input": {
    "legalName": "Editora ABC Ltda",
    "tradeName": "Editora ABC",
    "registrationNumber": "12.345.678/0001-99",
    "stateRegistration": "123.456.789",
    "municipalRegistration": "98765",
    "email": "contato@editoraabc.com.br",
    "primaryPhone": "(11) 3456-7890",
    "secondaryPhone": null,
    "whatsAppNumber": "(11) 94567-8901",
    "website": "https://www.editoraabc.com.br",
    "contactPersonName": "João Silva",
    "contactPersonEmail": "joao.silva@editoraabc.com.br",
    "contactPersonPhone": "(11) 91234-5678",
    "companyType": "SUPPLIER",
    "industry": "Editorial",
    "profileImageUrl": null,
    "notes": "Fornecedor de livros didáticos"
  }
}
```

**Erros possíveis:**

| Código | Mensagem | Causa |
|---|---|---|
| `VALIDATION_ERROR` | Campo inválido | `legalName` vazio, e-mail malformado, campo excede tamanho máximo |
| `COMPANY_EMAIL_ALREADY_EXISTS` | E-mail já cadastrado | Outro registro usa o mesmo e-mail no tenant |
| `COMPANY_REGISTRATION_NUMBER_ALREADY_EXISTS` | CNPJ já cadastrado | Outro registro usa o mesmo CNPJ no tenant |

---

### 4. Atualizar empresa

Aceita os mesmos campos do `CreateCompanyInput`. `legalName` continua obrigatório. O `id` é passado como argumento separado.

```graphql
mutation UpdateCompany($id: UUID!, $input: UpdateCompanyInput!) {
  updateCompany(id: $id, input: $input) {
    id
    legalName
    tradeName
    registrationNumber
    email
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
    "legalName": "Editora ABC S.A.",
    "tradeName": "Editora ABC",
    "registrationNumber": "12.345.678/0001-99",
    "stateRegistration": null,
    "municipalRegistration": null,
    "email": "contato@editoraabc.com.br",
    "primaryPhone": "(11) 3456-7890",
    "secondaryPhone": null,
    "whatsAppNumber": "(11) 94567-8901",
    "website": "https://www.editoraabc.com.br",
    "contactPersonName": "João Silva",
    "contactPersonEmail": "joao.silva@editoraabc.com.br",
    "contactPersonPhone": "(11) 91234-5678",
    "companyType": "SUPPLIER",
    "industry": "Editorial",
    "profileImageUrl": null,
    "notes": "Fornecedor de livros didáticos — contrato renovado"
  }
}
```

> **Atenção:** A mutation de update é uma substituição completa de todos os campos. Envie sempre todos os valores atuais, não apenas os que foram alterados.

---

### 5. Definir / atualizar endereço

O endereço é gerenciado por uma mutation separada. Pode ser chamada tanto para criar quanto para substituir o endereço existente.

```graphql
mutation SetCompanyAddress($input: SetCompanyAddressInput!) {
  setCompanyAddress(input: $input) {
    id
    legalName
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
  "input": {
    "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "street": "Rua das Flores",
    "number": "123",
    "complement": "Sala 4",
    "neighborhood": "Centro",
    "city": "São Paulo",
    "state": "SP",
    "zipCode": "01310-100",
    "country": "BR"
  }
}
```

**Regras de validação do endereço:**

| Campo | Regra |
|---|---|
| `street` | Obrigatório, máx. 300 caracteres |
| `number` | Opcional, máx. 20 caracteres |
| `complement` | Opcional, máx. 100 caracteres |
| `neighborhood` | Obrigatório, máx. 150 caracteres |
| `city` | Obrigatório, máx. 150 caracteres |
| `state` | Obrigatório, exatamente 2 letras (ex: `SP`, `RJ`) |
| `zipCode` | Obrigatório, máx. 10 caracteres |
| `country` | Obrigatório, exatamente 2 letras ISO (padrão: `BR`) |

---

### 6. Excluir empresa (soft delete)

A empresa não é removida do banco — apenas marcada como excluída. Após a exclusão, ela desaparece de todas as queries normais (query filter global).

```graphql
mutation DeleteCompany($id: UUID!) {
  deleteCompany(id: $id)
}
```

**Variáveis:**
```json
{ "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

Retorna `true` em caso de sucesso. Lança erro se a empresa não existir ou não pertencer ao tenant.

---

## Tratamento de erros

Todos os erros seguem o padrão de extensions do GraphQL:

```json
{
  "errors": [
    {
      "message": "Email já cadastrado para outra empresa neste tenant.",
      "extensions": {
        "code": "COMPANY_EMAIL_ALREADY_EXISTS"
      }
    }
  ]
}
```

Verifique sempre `errors` antes de acessar `data`. Erros de validação chegam com código `VALIDATION_ERROR` e podem conter múltiplos itens em `errors[]`.

---

## Regras de negócio importantes para o frontend

1. **`registrationNumber` (CNPJ) e `email` são únicos por tenant.** Se o usuário tentar cadastrar um CNPJ ou e-mail já existente, o backend retorna erro — exiba feedback claro no formulário.

2. **`legalName` é o único campo obrigatório** no cadastro. O formulário de criação pode ser simples (apenas razão social) com edição dos demais campos depois.

3. **Endereço é gerenciado separadamente** — use `setCompanyAddress` após criar a empresa, ou em uma seção/aba dedicada do formulário de edição.

4. **Update é substituição total** — ao editar, envie todos os campos do formulário, mesmo os não alterados. Não use PATCH parcial.

5. **Exclusão é irreversível pela UI** — o soft delete não tem desfazer. Exiba confirmação antes de chamar `deleteCompany`.

6. **Status é somente leitura na UI** — não há mutation para alterar status diretamente pelo CRUD. O status é gerenciado internamente pelo backend (`Active`, `Inactive`, `Blocked`, `Suspended`). Apenas exiba o valor.

7. **Paginação por cursor** — a listagem usa cursor-based pagination (Relay). Guarde o `endCursor` da página atual para carregar a próxima; não há acesso por número de página.

---

## Exemplo de fluxo completo (criar empresa com endereço)

```
1. POST /graphql  → mutation Login          → obtém token JWT
2. POST /graphql  → mutation CreateCompany  → cria empresa, obtém id
3. POST /graphql  → mutation SetCompanyAddress(companyId: <id>) → define endereço
4. POST /graphql  → query GetCompanyById(<id>) → exibe dados completos
```

---

## Limites de tamanho dos campos (para validação client-side)

| Campo | Máx. caracteres |
|---|---|
| `legalName` | 300 |
| `tradeName` | 300 |
| `registrationNumber` | 30 |
| `stateRegistration` | 30 |
| `municipalRegistration` | 30 |
| `email` | 254 |
| `primaryPhone` | 30 |
| `secondaryPhone` | 30 |
| `whatsAppNumber` | 30 |
| `website` | 500 |
| `contactPersonName` | 300 |
| `contactPersonEmail` | 254 |
| `contactPersonPhone` | 30 |
| `industry` | 150 |
| `profileImageUrl` | 2000 |
| `notes` | 2000 |
