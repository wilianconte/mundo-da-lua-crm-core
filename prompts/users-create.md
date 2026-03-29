# Cadastro de Usuario - Guia para o Front-end

## Objetivo

Orientar a implementacao da tela de cadastro de usuario com o contrato GraphQL ja disponivel no backend.

---

## Status atual do backend (2026-03-29)

Mutation de cadastro ja publicada em Auth:

- `createUser` (autenticada)
- `login` (anonima)

Queries relacionadas:

- `users` (listagem paginada)
- `userById` (detalhe por ID)

Arquivos de referencia:

- `1 - Gateway/MundoDaLua.GraphQL/GraphQL/Auth/AuthMutations.cs`
- `1 - Gateway/MundoDaLua.GraphQL/GraphQL/Auth/Inputs/CreateUserInput.cs`

---

## Mutation de cadastro

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

## Input

```graphql
input CreateUserInput {
  name: String!
  email: String!
  password: String!
  personId: UUID
}
```

## Exemplo de variaveis

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

---

## Autenticacao

`createUser` exige JWT valido:

```text
Authorization: Bearer <token>
```

---

## Regras de negocio retornadas pelo backend

- `USER_EMAIL_DUPLICATE`: ja existe usuario com esse email no tenant.
- `USER_PERSON_ALREADY_LINKED`: `personId` ja vinculado a outro usuario no tenant.
- `VALIDATION_ERROR`: erro de validacao (campos invalidos).

---

## Validacoes recomendadas no front

- `name`: obrigatorio, maximo 200.
- `email`: obrigatorio, formato valido, maximo 254.
- `password`: obrigatoria, maximo 128, sempre mascarada.
- `personId`: opcional.

---

## Tratamento de erro no front

Padrao GraphQL atual:

- mensagem em `errors[].message`
- codigo em `errors[].extensions.code`

Recomendacao:

- mapear `extensions.code` para mensagens amigaveis
- manter mensagem tecnica somente em logs internos

---

## Fluxo sugerido de UX

1. Usuario preenche Nome, E-mail, Senha e Pessoa (opcional).
2. Front valida campos obrigatorios.
3. Front envia mutation `createUser`.
4. Em sucesso, redireciona para listagem `users` e mostra confirmacao.
5. Em erro, mostra feedback por codigo de negocio.
