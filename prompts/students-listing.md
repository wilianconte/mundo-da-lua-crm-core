# Listagem de Estudantes - Guia para o Front-end

> **Data:** 2026-03-30
> **Modulo:** Students
> **Autenticacao:** JWT obrigatorio

---

## Operacoes disponiveis

| Operacao | Tipo | Descricao |
|---|---|---|
| `students` | Query | Listagem paginada de estudantes com filtros e ordenacao |
| `studentById` | Query | Detalhe de um estudante por ID |
| `studentCourses` | Query | Listagem paginada de matriculas em cursos |
| `studentCourseById` | Query | Detalhe de uma matricula por ID |
| `studentGuardians` | Query | Listagem paginada de responsaveis |
| `studentGuardianById` | Query | Detalhe de um responsavel por ID |

---

## Queries

### `students`

```graphql
query GetStudents(
  $first: Int
  $after: String
  $last: Int
  $before: String
  $where: StudentFilterInput
  $order: [StudentSortInput!]
) {
  students(
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
      personId
      registrationNumber
      schoolName
      gradeOrClass
      enrollmentType
      unitId
      classGroup
      startDate
      status
      notes
      academicObservation
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
  "order": [{ "createdAt": "DESC" }]
}
```

**Exemplo — proxima pagina**

```json
{
  "first": 20,
  "after": "<endCursor da pagina anterior>",
  "order": [{ "createdAt": "DESC" }]
}
```

**Exemplo — apenas ativos de uma unidade**

```json
{
  "first": 20,
  "where": {
    "and": [
      { "status": { "eq": "ACTIVE" } },
      { "unitId": { "eq": "00000000-0000-0000-0000-000000000010" } }
    ]
  }
}
```

---

### `studentById`

Retorna um unico estudante ou `null` se nao encontrado.

```graphql
query GetStudentById($id: UUID!) {
  studentById(id: $id) {
    id
    personId
    registrationNumber
    schoolName
    gradeOrClass
    enrollmentType
    unitId
    classGroup
    startDate
    status
    notes
    academicObservation
    createdAt
    updatedAt
    createdBy
    updatedBy
  }
}
```

---

### `studentCourses`

Util para listar as matriculas de um estudante especifico.

```graphql
query GetStudentCourses(
  $first: Int
  $after: String
  $where: StudentCourseFilterInput
  $order: [StudentCourseSortInput!]
) {
  studentCourses(
    first: $first
    after: $after
    where: $where
    order: $order
  ) {
    totalCount
    pageInfo {
      hasNextPage
      endCursor
    }
    nodes {
      id
      studentId
      courseId
      enrollmentDate
      startDate
      endDate
      classGroup
      shift
      scheduleDescription
      unitId
      notes
      status
      createdAt
      updatedAt
    }
  }
}
```

**Exemplo — cursos de um estudante especifico**

```json
{
  "first": 50,
  "where": { "studentId": { "eq": "00000000-0000-0000-0000-000000000001" } },
  "order": [{ "startDate": "DESC" }]
}
```

> Maximo permitido e 50. Se o estudante tiver mais de 50 matriculas, usar paginacao com `after: endCursor`.

---

### `studentGuardians`

Util para listar os responsaveis de um estudante especifico.

```graphql
query GetStudentGuardians(
  $first: Int
  $after: String
  $where: StudentGuardianFilterInput
  $order: [StudentGuardianSortInput!]
) {
  studentGuardians(
    first: $first
    after: $after
    where: $where
    order: $order
  ) {
    totalCount
    pageInfo {
      hasNextPage
      endCursor
    }
    nodes {
      id
      studentId
      guardianPersonId
      relationshipType
      isPrimaryGuardian
      isFinancialResponsible
      receivesNotifications
      canPickupChild
      notes
      createdAt
      updatedAt
    }
  }
}
```

**Exemplo — responsaveis de um estudante**

```json
{
  "first": 50,
  "where": { "studentId": { "eq": "00000000-0000-0000-0000-000000000001" } },
  "order": [{ "isPrimaryGuardian": "DESC" }]
}
```

---

## Campos disponiveis — `Student`

| Campo | Tipo | Descricao |
|---|---|---|
| `id` | UUID | Identificador do estudante |
| `personId` | UUID | ID da pessoa vinculada |
| `registrationNumber` | String | Numero de matricula |
| `schoolName` | String | Nome da escola |
| `gradeOrClass` | String | Serie ou turma |
| `enrollmentType` | String | Tipo de matricula |
| `unitId` | UUID | Unidade/filial |
| `classGroup` | String | Grupo de turma |
| `startDate` | Date | Data de inicio |
| `status` | StudentStatus | Status atual |
| `notes` | String | Observacoes gerais |
| `academicObservation` | String | Observacoes academicas |

---

## Enums

### `StudentStatus`

| Valor | Label sugerido |
|---|---|
| `Active` | Ativo |
| `Inactive` | Inativo |
| `Graduated` | Formado |
| `Transferred` | Transferido |
| `Suspended` | Suspenso |

### `StudentCourseStatus`

| Valor | Label sugerido |
|---|---|
| `Active` | Ativo |
| `Pending` | Pendente |
| `Completed` | Concluido |
| `Cancelled` | Cancelado |
| `Suspended` | Suspenso |

### `GuardianRelationshipType`

| Valor | Label sugerido |
|---|---|
| `Father` | Pai |
| `Mother` | Mae |
| `Grandmother` | Avo |
| `Grandfather` | Avo |
| `Uncle` | Tio |
| `Aunt` | Tia |
| `LegalGuardian` | Responsavel legal |
| `Other` | Outro |

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

## Filtros uteis

**Estudantes por status:**
```json
{ "where": { "status": { "eq": "Active" } } }
```

**Varios status ao mesmo tempo (`in`):**
```json
{ "where": { "status": { "in": ["Active", "Suspended"] } } }
```

**Varios estudantes por `personId` em batch:**
```json
{ "where": { "personId": { "in": ["<id1>", "<id2>", "<id3>"] } } }
```

**Estudantes de uma unidade:**
```json
{ "where": { "unitId": { "eq": "<id-da-unidade>" } } }
```

**Estudantes de uma turma:**
```json
{
  "where": {
    "and": [
      { "classGroup": { "eq": "Turma A" } },
      { "status": { "eq": "Active" } }
    ]
  }
}
```

**Multiplos status com `or`:**
```json
{
  "where": {
    "or": [
      { "status": { "eq": "Active" } },
      { "status": { "eq": "Suspended" } }
    ]
  }
}
```

**Responsavel principal de um estudante (via `studentGuardians`):**
```json
{
  "where": {
    "and": [
      { "studentId": { "eq": "<id-do-estudante>" } },
      { "isPrimaryGuardian": { "eq": true } }
    ]
  }
}
```

---

## Responsaveis embutidos na listagem (sem consultas extras)

A navigation property `guardians` esta disponivel diretamente dentro de `students`. O front pode incluir dados dos responsaveis na mesma query, sem chamadas adicionais:

```graphql
students(first: 20) {
  nodes {
    id
    schoolName
    status
    guardians {
      guardianPersonId
      isPrimaryGuardian
      relationshipType
      canPickupChild
    }
  }
}
```

> **Limitacao:** `guardianPersonId` e um UUID. Nome e telefone do responsavel ficam na entidade `Person` — requerem uma query separada de `persons` filtrada por `id: { in: [...] }` com os IDs retornados.

---

## Notas importantes

- O servidor filtra automaticamente por `tenantId` do JWT — nao enviar `tenantId` nas queries.
- Os nomes corretos sao `students`, `studentById`, `studentCourses`, `studentGuardians` etc. Prefixo `get` nao existe no schema.
- Para carregar cursos e responsaveis na pagina de detalhe do estudante, usar `studentCourses` e `studentGuardians` filtrados por `studentId`, ou incluir `guardians { }` diretamente no `studentById`.
- Solicitar apenas os campos necessarios para a tela.
- `createdBy` e `updatedBy` sao do tipo `UUID` (nao String) — exibir nome requer busca adicional por `userById`.

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

1. Front carrega a primeira pagina de `students` com `first: 20` ao montar a tela.
2. Exibe `totalCount` como contador de resultados.
3. Botao "Proxima" ativo quando `pageInfo.hasNextPage = true`; envia `after: endCursor`.
4. Ao alterar filtro ou ordenacao, resetar cursor (remover `after`/`before`) e buscar a primeira pagina novamente.
5. Ao abrir detalhe do estudante, chamar `studentById` para dados completos, `studentCourses` filtrado por `studentId` para cursos e `studentGuardians` filtrado por `studentId` para responsaveis.
6. `studentCourseById` e `studentGuardianById` sao usados para pre-preencher formularios de edicao.
