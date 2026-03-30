# Cadastro de Estudante - Guia para o Front-end

> **Data:** 2026-03-30
> **Modulo:** Students
> **Autenticacao:** JWT obrigatorio em todas as operacoes

---

## Operacoes disponiveis

| Operacao | Tipo | Descricao |
|---|---|---|
| `createStudent` | Mutation | Cria o cadastro do estudante |
| `createStudentCourse` | Mutation | Vincula um curso ao estudante |
| `createStudentGuardian` | Mutation | Vincula um responsavel ao estudante |
| `students` | Query | Listagem paginada de estudantes |
| `studentById` | Query | Detalhe de estudante por ID |
| `studentCourses` | Query | Listagem paginada de matriculas |
| `studentGuardians` | Query | Listagem paginada de responsaveis |

---

## Fluxo geral de cadastro

O cadastro e feito em ate tres etapas via mutations independentes:

1. `createStudent` — obrigatorio, retorna o `id` do estudante.
2. `createStudentCourse` — opcional, usa o `id` retornado.
3. `createStudentGuardian` — opcional, usa o `id` retornado.

Os passos 2 e 3 podem ser realizados depois, na pagina de detalhe do estudante.

---

## Mutations

### `createStudent`

```graphql
mutation CreateStudent($input: CreateStudentInput!) {
  createStudent(input: $input) {
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
      createdAt
      updatedAt
      createdBy
      updatedBy
    }
  }
}
```

**Input**

```graphql
input CreateStudentInput {
  personId: UUID!
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
  "input": {
    "personId": "00000000-0000-0000-0000-000000000001",
    "registrationNumber": "2024001",
    "schoolName": "Escola Municipal Centro",
    "gradeOrClass": "5º Ano",
    "enrollmentType": "Regular",
    "unitId": "00000000-0000-0000-0000-000000000010",
    "classGroup": "Turma A",
    "startDate": "2024-02-01",
    "notes": "Aluno com necessidades especiais",
    "academicObservation": "Dificuldade em matematica"
  }
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `STUDENT_PERSON_ALREADY_LINKED` | `personId` ja vinculado a outro estudante |
| `STUDENT_REGISTRATION_DUPLICATE` | Numero de matricula ja existe no tenant |
| `VALIDATION_ERROR` | Campos invalidos |

---

### `createStudentCourse`

Executar apos obter o `id` retornado pelo `createStudent`.

```graphql
mutation CreateStudentCourse($input: CreateStudentCourseInput!) {
  createStudentCourse(input: $input) {
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
      createdAt
      updatedAt
      createdBy
      updatedBy
    }
  }
}
```

**Input**

```graphql
input CreateStudentCourseInput {
  studentId: UUID!
  courseId: UUID!
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
  "input": {
    "studentId": "<id retornado pelo createStudent>",
    "courseId": "00000000-0000-0000-0000-000000000020",
    "enrollmentDate": "2024-02-01",
    "startDate": "2024-02-05",
    "endDate": "2024-12-20",
    "classGroup": "Turma A",
    "shift": "Manha",
    "scheduleDescription": "Seg, Qua e Sex das 08h as 10h",
    "unitId": "00000000-0000-0000-0000-000000000010",
    "notes": "Matricula via contrato anual"
  }
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `STUDENT_COURSE_DUPLICATE` | Estudante ja matriculado nesse curso |
| `VALIDATION_ERROR` | Campos invalidos |

---

### `createStudentGuardian`

Executar apos obter o `id` retornado pelo `createStudent`.

```graphql
mutation CreateStudentGuardian($input: CreateStudentGuardianInput!) {
  createStudentGuardian(input: $input) {
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
      createdAt
      updatedAt
      createdBy
      updatedBy
    }
  }
}
```

**Input**

```graphql
input CreateStudentGuardianInput {
  studentId: UUID!
  guardianPersonId: UUID!
  relationshipType: GuardianRelationshipType!
  isPrimaryGuardian: Boolean
  isFinancialResponsible: Boolean
  receivesNotifications: Boolean
  canPickupChild: Boolean
  notes: String
}
```

**Exemplo**

```json
{
  "input": {
    "studentId": "<id retornado pelo createStudent>",
    "guardianPersonId": "00000000-0000-0000-0000-000000000030",
    "relationshipType": "Mother",
    "isPrimaryGuardian": true,
    "isFinancialResponsible": true,
    "receivesNotifications": true,
    "canPickupChild": true,
    "notes": "Contato preferencial"
  }
}
```

**Erros de negocio**

| Codigo | Quando ocorre |
|---|---|
| `STUDENT_GUARDIAN_DUPLICATE` | Responsavel ja vinculado a esse estudante |
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

### `StudentStatus` (retornado nas queries)

| Valor | Label sugerido |
|---|---|
| `Active` | Ativo |
| `Inactive` | Inativo |
| `Graduated` | Formado |
| `Transferred` | Transferido |
| `Suspended` | Suspenso |

### `StudentCourseStatus` (retornado nas queries)

| Valor | Label sugerido |
|---|---|
| `Active` | Ativo |
| `Pending` | Pendente |
| `Completed` | Concluido |
| `Cancelled` | Cancelado |
| `Suspended` | Suspenso |

---

## Validacoes no front

### `createStudent`

| Campo | Regra |
|---|---|
| `personId` | Obrigatorio, selecionar de uma busca de pessoas |
| `registrationNumber` | Opcional, maximo 50 caracteres |
| `schoolName` | Opcional, maximo 200 caracteres |
| `gradeOrClass` | Opcional, maximo 100 caracteres |
| `enrollmentType` | Opcional, maximo 100 caracteres |
| `classGroup` | Opcional, maximo 100 caracteres |
| `startDate` | Opcional, formato de data |

### `createStudentCourse`

| Campo | Regra |
|---|---|
| `courseId` | Obrigatorio, selecionar de uma lista de cursos |
| `enrollmentDate` | Opcional, nao deve ser posterior a `startDate` |
| `endDate` | Opcional, deve ser posterior a `startDate` |

### `createStudentGuardian`

| Campo | Regra |
|---|---|
| `guardianPersonId` | Obrigatorio, selecionar de uma busca de pessoas |
| `relationshipType` | Obrigatorio, usar enum `GuardianRelationshipType` |
| `isPrimaryGuardian` | Recomendado informar; apenas um responsavel pode ser primario por estudante |

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

1. Usuario busca e seleciona uma **Pessoa** existente (campo `personId`).
2. Usuario preenche os campos do estudante (matricula, escola, turma etc.).
3. Front envia `createStudent` e armazena o `id` retornado.
4. (Opcional) Usuario adiciona um ou mais cursos: front envia `createStudentCourse` para cada um.
5. (Opcional) Usuario adiciona um ou mais responsaveis: front envia `createStudentGuardian` para cada um.
6. Em sucesso: redireciona para o detalhe do estudante e exibe confirmacao.
7. Em erro em qualquer etapa: exibe feedback por codigo de negocio. O estudante ja criado **nao e desfeito automaticamente** — o front deve permitir retomar o cadastro a partir do passo que falhou.
