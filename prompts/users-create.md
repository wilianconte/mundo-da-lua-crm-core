# Cadastro de Usuario - Guia para o Front-end

> **Data:** 2026-03-29
> **Modulo:** Auth
> **Autenticacao:** JWT obrigatorio

---

## Operacoes disponiveis

| Operacao | Tipo | Descricao |
|---|---|---|
| `createUser` | Mutation | Cria um novo usuario |
| `users` | Query | Listagem paginada de usuarios |
| `userById` | Query | Detalhe de usuario por ID |

---

## Mutations

### `createUser`

```graphql
mutation CreateUser($input: CreateUserInput!) {
  createUser(input: $input) {
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

**Input**

```graphql
input CreateUserInput {
  name: String!
  email: String!
  password: String!
  personId: UUID
}
```

**Exemplo**

```json
{
  "input": {
    "name": "Maria Souza",
    "email": "maria@mundodalua.com",
    "password": "Senha@123",
    "personId": "00000000-0000-0000-0000-000000000001"
  }
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `USER_EMAIL_DUPLICATE` | Ja existe usuario com esse e-mail no tenant |
| `USER_PERSON_ALREADY_LINKED` | `personId` ja vinculado a outro usuario |
| `VALIDATION_ERROR` | Campos invalidos |

---

## Validacoes no front

| Campo | Regra |
|---|---|
| `name` | Obrigatorio, maximo 200 caracteres |
| `email` | Obrigatorio, formato valido, maximo 254 caracteres |
| `password` | Obrigatorio, maximo 128 caracteres, sempre mascarada |
| `personId` | Opcional, selecionar de uma busca de pessoas |

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

1. Usuario preenche Nome, E-mail, Senha e Pessoa (opcional).
2. Front valida os campos obrigatorios.
3. Front envia `createUser`.
4. Em sucesso: redireciona para a listagem de usuarios e exibe confirmacao.
5. Em erro: exibe feedback por codigo de negocio.
