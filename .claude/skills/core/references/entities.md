# Entidades Existentes — MyCRM

## Person — entidade mestre de identidade (`crm.people`)

`Person` é a entidade central para todos os indivíduos do sistema. Todos os papéis futuros (Guardian, Student, Patient, Employee, Lead, Supplier) referenciarão `Person` por `PersonId` — **nunca duplicar dados pessoais em módulos separados**.

```
Person 1──0..1  Guardian    (FK em Guardian.PersonId)
Person 1──0..1  Student     (FK em Student.PersonId)
Person 1──0..1  Patient     (FK em Patient.PersonId)
Person 1──0..1  Employee    (FK em Employee.PersonId)
Person 1──0..*  Lead        (FK em Lead.PersonId)
Person 1──0..1  Supplier    (FK em Supplier.PersonId)
```

Enums em `MyCRM.CRM.Domain.Entities`: `PersonStatus`, `Gender`, `MaritalStatus`.

---

## Company — entidade mestre de organizações (`crm.companies`)

`Company` é a entidade central para todos os CNPJs, empresas, escolas, fornecedores, parceiros e entidades jurídicas do sistema. Todos os papéis futuros (Supplier, Partner, School, CorporateCustomer, BillingAccount, ServiceProvider) referenciarão `Company` por `CompanyId` — **nunca duplicar dados de organização em módulos separados**.

```
Company 1──0..1  Supplier          (FK em Supplier.CompanyId)
Company 1──0..1  Partner           (FK em Partner.CompanyId)
Company 1──0..1  School            (FK em School.CompanyId)
Company 1──0..1  CorporateCustomer (FK em CorporateCustomer.CompanyId)
Company 1──0..1  BillingAccount    (FK em BillingAccount.CompanyId)
Company 1──0..1  ServiceProvider   (FK em ServiceProvider.CompanyId)
```

Enums em `MyCRM.CRM.Domain.Entities`: `CompanyStatus`, `CompanyType`.

Estratégia de deduplicação:
- `RegistrationNumber` (CNPJ) é único por tenant (partial index, nullable).
- `Email` é único por tenant (partial index, nullable).

Address é owned value object (mesmo padrão de `Customer`).

---

## Student — entidade de papel para alunos (`crm.students`)

`Student` é a extensão de papel de `Person` para o contexto escolar. Não duplica dados pessoais.

```
Person 1──0..1  Student  (FK em Student.PersonId)
Student 1──0..* StudentGuardian (FK em StudentGuardian.StudentId)
```

Campos específicos: `RegistrationNumber`, `SchoolName`, `GradeOrClass`, `EnrollmentType`, `UnitId`, `ClassGroup`, `StartDate`, `Status` (StudentStatus), `Notes`, `AcademicObservation`.

Enums: `StudentStatus` (Active, Inactive, Graduated, Transferred, Suspended).

Restrição de unicidade: `(TenantId, PersonId)` único onde `IsDeleted = false` — uma pessoa não pode ter dois registros de aluno ativos.

---

## StudentGuardian — entidade de relacionamento aluno↔responsável (`crm.student_guardians`)

Entidade de relacionamento entre `Student` e `Person` (responsável). Armazena atributos do vínculo:
`RelationshipType` (GuardianRelationshipType), `IsPrimaryGuardian`, `IsFinancialResponsible`, `ReceivesNotifications`, `CanPickupChild`, `Notes`.

```
StudentGuardian *──1 Student  (FK em StudentGuardian.StudentId, cascade delete)
StudentGuardian *──1 Person   (FK em StudentGuardian.GuardianPersonId, restrict)
```

Enums: `GuardianRelationshipType` (Father, Mother, Grandmother, Grandfather, Uncle, Aunt, LegalGuardian, Other).

Restrição de unicidade: `(TenantId, StudentId, GuardianPersonId)` único onde `IsDeleted = false` — o mesmo responsável não pode ser adicionado duas vezes para o mesmo aluno.

**Design decision:** Não foi criada entidade separada `Guardian` — o responsável é representado diretamente por `Person`, seguindo o padrão de papéis via FK.

---

## Course — entidade mestre de ofertas educacionais (`crm.courses`)

`Course` representa qualquer programa ou oferta educacional estruturada (reforço escolar, inglês, turma, workshop, etc.).
Mantida genérica para suportar múltiplos contextos de negócio sem criar entidades paralelas.

```
Course 1──0..* StudentCourse  (FK em StudentCourse.CourseId)
```

Campos específicos: `Name`, `Code`, `Type` (CourseType), `Description`, `StartDate`, `EndDate`, `ScheduleDescription`, `Capacity`, `Workload`, `UnitId`, `Status` (CourseStatus), `IsActive`, `Notes`.

Enums: `CourseType` (AfterSchool, Language, SchoolClass, Workshop, Other), `CourseStatus` (Draft, Active, Inactive, Completed, Cancelled).

Restrição de unicidade: `(TenantId, Code)` único onde `Code IS NOT NULL`.

**Design decision:** `Course` é a fonte da verdade dos dados de programa. Dados específicos de matrícula (datas, turma, turno) ficam em `StudentCourse`. `IsActive` é um flag de conveniência gerenciado pelos métodos de domínio (`Activate`, `Deactivate`, `Complete`, `Cancel`).

---

## StudentCourse — entidade de relacionamento aluno↔curso (`crm.student_courses`)

Entidade de associação entre `Student` e `Course`. Armazena todos os atributos específicos da matrícula.

```
StudentCourse *──1 Student  (FK em StudentCourse.StudentId, restrict)
StudentCourse *──1 Course   (FK em StudentCourse.CourseId, restrict)
```

Campos específicos: `EnrollmentDate`, `StartDate`, `EndDate`, `CancellationDate`, `CompletionDate`, `Status` (StudentCourseStatus), `ClassGroup`, `Shift`, `ScheduleDescription`, `UnitId`, `Notes`.

Enums: `StudentCourseStatus` (Active, Pending, Completed, Cancelled, Suspended).

Restrição: re-matrícula histórica é permitida (mesmo aluno no mesmo curso em períodos diferentes). A lógica de negócio impede matrículas ativas/pendentes simultâneas para o mesmo par `(StudentId, CourseId)` dentro do mesmo tenant. O índice `(TenantId, StudentId, CourseId)` com filtro `IsDeleted = false` apoia esse controle.

**Design decision:** Ambas as FKs usam `DeleteBehavior.Restrict` — nem a exclusão de Student nem de Course deve apagar automaticamente o histórico de matrículas.

---

## Employee — entidade de papel para funcionários (`crm.employees`)

`Employee` é a extensão de papel de `Person` para o contexto de RH/emprego. Não duplica dados pessoais.

```
Person 1──0..1  Employee  (FK em Employee.PersonId)
Employee 0..1──0..* Employee  (auto-referência em Employee.ManagerEmployeeId)
```

Campos específicos: `EmployeeCode`, `HireDate`, `TerminationDate`, `Position`, `Department`, `ContractType`, `WorkSchedule`, `WorkloadHours`, `PayrollNumber`, `ManagerEmployeeId`, `UnitId`, `CostCenter`, `Status` (EmployeeStatus), `IsActive`, `Notes`.

Enums: `EmployeeStatus` (Active, Inactive, OnLeave, Suspended, Terminated).

Restrições de unicidade:
- `(TenantId, PersonId)` único onde `IsDeleted = false` — uma pessoa não pode ter dois registros de funcionário ativos.
- `(TenantId, EmployeeCode)` único onde `EmployeeCode IS NOT NULL`.

**Design decision:** `ManagerEmployeeId` é auto-referência para `Employee` (FK Restrict, nullable). `IsActive` é flag de conveniência sincronizado pelos métodos de domínio (`Activate`, `Deactivate`, `PutOnLeave`, `Suspend`, `Terminate`). A FK para `Person` usa `DeleteBehavior.Restrict`.

---

## Role — entidade de papel/permissão (`auth.roles`)

`Role` representa papéis de acesso no sistema, com controle granular de permissões. Permite ativar/desativar roles sem excluí-los.

```
Role 1──0..* UserRole  (FK em UserRole.RoleId)
```

Campos específicos: `Name`, `Description`, `Permissions` (string[]), `IsActive`.

Restrição de unicidade: `(TenantId, Name)` único onde `IsDeleted = false` — nomes de roles são únicos por tenant.

**Design decision:** 
- `Permissions` é um array de strings PostgreSQL para flexibilidade. Cada permissão segue o padrão `resource.action` (ex: `users.read`, `customers.write`, `roles.delete`).
- `IsActive` permite desativar roles temporariamente sem perder histórico de atribuições.
- Métodos de domínio: `Create`, `Update`, `UpdatePermissions`, `Activate`, `Deactivate`.

---

## UserRole — entidade de relacionamento usuário↔role (`auth.user_roles`)

Entidade de associação entre `User` e `Role`. Permite atribuição múltipla de roles por usuário (many-to-many).

```
UserRole *──1 User  (FK em UserRole.UserId, restrict)
UserRole *──1 Role  (FK em UserRole.RoleId, restrict)
```

Restrição de unicidade: `(TenantId, UserId, RoleId)` único onde `IsDeleted = false` — mesmo role não pode ser atribuído duas vezes ao mesmo usuário.

**Design decision:**
- Ambas as FKs usam `DeleteBehavior.Restrict` — nem exclusão de User nem de Role deve apagar automaticamente o histórico de atribuições.
- Soft delete preserva auditoria de quem teve qual role e quando.
- Não há campos adicionais de contexto (data de expiração, escopo) — manter simples até surgirem requisitos concretos.

---

## Customer (`crm.customers`)

Entidade legada de CRM genérico. Futuramente será refatorada para referenciar `Person`.
