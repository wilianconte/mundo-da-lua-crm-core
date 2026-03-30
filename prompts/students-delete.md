# Exclusao de Estudante - Guia para o Front-end

> **Data:** 2026-03-30
> **Modulo:** Students
> **Autenticacao:** JWT obrigatorio em todas as operacoes

---

## Operacoes disponiveis

| Operacao | Tipo | Descricao |
|---|---|---|
| `deleteStudent` | Mutation | Remove o cadastro do estudante |
| `deleteStudentCourse` | Mutation | Remove a matricula de um curso |
| `deleteStudentGuardian` | Mutation | Remove o vinculo com um responsavel |

---

## Mutations

### `deleteStudent`

Recebe apenas o `id` do estudante. Retorna `true` em caso de sucesso.

```graphql
mutation DeleteStudent($id: UUID!) {
  deleteStudent(id: $id)
}
```

**Exemplo**

```json
{
  "id": "00000000-0000-0000-0000-000000000001"
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `STUDENT_NOT_FOUND` | Estudante nao encontrado |

---

### `deleteStudentCourse`

Recebe apenas o `id` da matricula. Retorna `true` em caso de sucesso.

```graphql
mutation DeleteStudentCourse($id: UUID!) {
  deleteStudentCourse(id: $id)
}
```

**Exemplo**

```json
{
  "id": "00000000-0000-0000-0000-000000000050"
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `STUDENT_COURSE_NOT_FOUND` | Matricula nao encontrada |

---

### `deleteStudentGuardian`

Recebe apenas o `id` do vinculo. Retorna `true` em caso de sucesso.

```graphql
mutation DeleteStudentGuardian($id: UUID!) {
  deleteStudentGuardian(id: $id)
}
```

**Exemplo**

```json
{
  "id": "00000000-0000-0000-0000-000000000060"
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `STUDENT_GUARDIAN_NOT_FOUND` | Responsavel nao encontrado |

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

### Excluir estudante

1. Usuario aciona "Excluir" na listagem ou pagina de detalhe.
2. Front exibe dialogo de confirmacao com nome do estudante.
3. Apos confirmacao, front envia `deleteStudent` com o `id`.
4. Em sucesso: redireciona para a listagem de estudantes e exibe confirmacao.
5. Em erro: exibe feedback por codigo de negocio sem sair da pagina.

### Excluir matricula em curso

1. Usuario aciona "Remover" no curso listado na pagina de detalhe do estudante.
2. Front exibe dialogo de confirmacao com o nome do curso.
3. Apos confirmacao, front envia `deleteStudentCourse` com o `id` da matricula.
4. Em sucesso: atualiza a lista de cursos na tela sem recarregar a pagina inteira.
5. Em erro: exibe feedback por codigo de negocio.

### Excluir responsavel

1. Usuario aciona "Remover" no responsavel listado na pagina de detalhe do estudante.
2. Front exibe dialogo de confirmacao com o nome do responsavel.
3. Apos confirmacao, front envia `deleteStudentGuardian` com o `id` do vinculo.
4. Em sucesso: atualiza a lista de responsaveis na tela sem recarregar a pagina inteira.
5. Em erro: exibe feedback por codigo de negocio.
