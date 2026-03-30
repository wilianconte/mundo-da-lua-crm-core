# Edicao de Estudante - Guia para o Front-end

> **Data:** 2026-03-30
> **Modulo:** Students
> **Autenticacao:** JWT obrigatorio em todas as operacoes

---

## Operacoes disponiveis

| Operacao | Tipo | Descricao |
|---|---|---|
| `updateStudent` | Mutation | Atualiza dados do estudante |
| `updateStudentCourse` | Mutation | Atualiza dados de uma matricula em curso |
| `updateStudentGuardian` | Mutation | Atualiza dados de um responsavel |
| `studentById` | Query | Carrega os dados atuais para pre-preencher o formulario |
| `studentCourseById` | Query | Carrega dados de uma matricula para edicao |
| `studentGuardianById` | Query | Carrega dados de um responsavel para edicao |

---

## Mutations

### `updateStudent`

O `id` do estudante e passado diretamente como argumento, nao dentro do input.
Todos os campos do input sao opcionais — enviar apenas os que foram alterados.

```graphql
mutation UpdateStudent($id: UUID!, $input: UpdateStudentInput!) {
  updateStudent(id: $id, input: $input) {
    student {
      id
      personId
      registrationNumber
      schoolName
      gradeOrClass
      enrollmentType
      unitId
      classGroup
      startDate
      notes
      academicObservation
      status
      updatedAt
      updatedBy
    }
  }
}
```

**Input**

```graphql
input UpdateStudentInput {
  registrationNumber: String
  schoolName: String
  gradeOrClass: String
  enrollmentType: String
  unitId: UUID
  classGroup: String
  startDate: Date
  notes: String
  academicObservation: String
}
```

**Exemplo**

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "input": {
    "schoolName": "Colegio Novo Nome",
    "gradeOrClass": "6º Ano",
    "academicObservation": "Progresso satisfatorio"
  }
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `STUDENT_NOT_FOUND` | Estudante nao encontrado |
| `STUDENT_REGISTRATION_DUPLICATE` | Numero de matricula ja existe no tenant |
| `VALIDATION_ERROR` | Campos invalidos |

---

### `updateStudentCourse`

O `id` da matricula e passado diretamente como argumento.
Todos os campos do input sao opcionais.

```graphql
mutation UpdateStudentCourse($id: UUID!, $input: UpdateStudentCourseInput!) {
  updateStudentCourse(id: $id, input: $input) {
    studentCourse {
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
      updatedAt
      updatedBy
    }
  }
}
```

**Input**

```graphql
input UpdateStudentCourseInput {
  enrollmentDate: Date
  startDate: Date
  endDate: Date
  classGroup: String
  shift: String
  scheduleDescription: String
  unitId: UUID
  notes: String
}
```

**Exemplo**

```json
{
  "id": "00000000-0000-0000-0000-000000000050",
  "input": {
    "endDate": "2025-12-20",
    "shift": "Tarde",
    "notes": "Turma transferida para o periodo da tarde"
  }
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `STUDENT_COURSE_NOT_FOUND` | Matricula nao encontrada |
| `VALIDATION_ERROR` | Campos invalidos |

---

### `updateStudentGuardian`

O `id` do vinculo e passado diretamente como argumento.
Todos os campos sao **obrigatorios** — o backend substitui os valores anteriores integralmente.

```graphql
mutation UpdateStudentGuardian($id: UUID!, $input: UpdateStudentGuardianInput!) {
  updateStudentGuardian(id: $id, input: $input) {
    studentGuardian {
      id
      studentId
      guardianPersonId
      relationshipType
      isPrimaryGuardian
      isFinancialResponsible
      receivesNotifications
      canPickupChild
      notes
      updatedAt
      updatedBy
    }
  }
}
```

**Input**

```graphql
input UpdateStudentGuardianInput {
  relationshipType: GuardianRelationshipType!
  isPrimaryGuardian: Boolean!
  isFinancialResponsible: Boolean!
  receivesNotifications: Boolean!
  canPickupChild: Boolean!
  notes: String
}
```

> **Atencao:** `relationshipType`, `isPrimaryGuardian`, `isFinancialResponsible`, `receivesNotifications` e `canPickupChild` sao obrigatorios.
> Pre-preencher o formulario com os valores atuais antes de exibir para o usuario.

**Exemplo**

```json
{
  "id": "00000000-0000-0000-0000-000000000060",
  "input": {
    "relationshipType": "Father",
    "isPrimaryGuardian": true,
    "isFinancialResponsible": false,
    "receivesNotifications": true,
    "canPickupChild": true,
    "notes": "Novo contato verificado"
  }
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `STUDENT_GUARDIAN_NOT_FOUND` | Responsavel nao encontrado |
| `VALIDATION_ERROR` | Campos invalidos |

---

## Enums

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

## Validacoes no front

### `updateStudent`

| Campo | Regra |
|---|---|
| `registrationNumber` | Opcional, maximo 50 caracteres |
| `schoolName` | Opcional, maximo 200 caracteres |
| `gradeOrClass` | Opcional, maximo 100 caracteres |
| `enrollmentType` | Opcional, maximo 100 caracteres |
| `classGroup` | Opcional, maximo 100 caracteres |
| `startDate` | Opcional, formato de data |

### `updateStudentCourse`

| Campo | Regra |
|---|---|
| `enrollmentDate` | Opcional, nao deve ser posterior a `startDate` |
| `endDate` | Opcional, deve ser posterior a `startDate` |

### `updateStudentGuardian`

| Campo | Regra |
|---|---|
| `relationshipType` | Obrigatorio, usar enum `GuardianRelationshipType` |
| `isPrimaryGuardian` | Obrigatorio; apenas um responsavel pode ser primario por estudante |
| `isFinancialResponsible` | Obrigatorio |
| `receivesNotifications` | Obrigatorio |
| `canPickupChild` | Obrigatorio |

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

1. Front carrega os dados atuais via `studentById`, `studentCourseById` ou `studentGuardianById`.
2. Formulario e pre-preenchido com os valores retornados.
3. Usuario edita os campos desejados.
4. Front envia a mutation correspondente com o `id` e os campos alterados.
5. Em sucesso: exibe confirmacao e atualiza a interface com os dados retornados pela mutation.
6. Em erro: exibe feedback por codigo de negocio sem fechar o formulario.
