namespace MyCRM.Shared.Kernel;

public static class SystemPermissions
{
    public const string StudentsRead   = "students:read";
    public const string StudentsCreate = "students:create";
    public const string StudentsUpdate = "students:update";
    public const string StudentsDelete = "students:delete";

    public const string StudentGuardiansRead   = "student_guardians:read";
    public const string StudentGuardiansCreate = "student_guardians:create";
    public const string StudentGuardiansUpdate = "student_guardians:update";
    public const string StudentGuardiansDelete = "student_guardians:delete";

    public const string StudentCoursesRead   = "student_courses:read";
    public const string StudentCoursesCreate = "student_courses:create";
    public const string StudentCoursesUpdate = "student_courses:update";
    public const string StudentCoursesDelete = "student_courses:delete";

    public const string CustomersRead   = "customers:read";
    public const string CustomersCreate = "customers:create";
    public const string CustomersUpdate = "customers:update";
    public const string CustomersDelete = "customers:delete";

    public const string EmployeesRead   = "employees:read";
    public const string EmployeesCreate = "employees:create";
    public const string EmployeesUpdate = "employees:update";
    public const string EmployeesDelete = "employees:delete";

    public const string CoursesRead   = "courses:read";
    public const string CoursesCreate = "courses:create";
    public const string CoursesUpdate = "courses:update";
    public const string CoursesDelete = "courses:delete";

    public const string PeopleRead   = "people:read";
    public const string PeopleCreate = "people:create";
    public const string PeopleUpdate = "people:update";
    public const string PeopleDelete = "people:delete";

    public const string CompaniesRead   = "companies:read";
    public const string CompaniesCreate = "companies:create";
    public const string CompaniesUpdate = "companies:update";
    public const string CompaniesDelete = "companies:delete";

    public const string UsersManage   = "users:manage";
    public const string RolesManage   = "roles:manage";
    public const string TenantsManage = "tenants:manage";
    public const string PlansManage   = "plans:manage";

    public static IReadOnlyList<(string Name, string Group)> All =>
    [
        (StudentsRead,   "Alunos"),
        (StudentsCreate, "Alunos"),
        (StudentsUpdate, "Alunos"),
        (StudentsDelete, "Alunos"),
        (StudentGuardiansRead,   "Responsáveis"),
        (StudentGuardiansCreate, "Responsáveis"),
        (StudentGuardiansUpdate, "Responsáveis"),
        (StudentGuardiansDelete, "Responsáveis"),
        (StudentCoursesRead,   "Matrículas"),
        (StudentCoursesCreate, "Matrículas"),
        (StudentCoursesUpdate, "Matrículas"),
        (StudentCoursesDelete, "Matrículas"),
        (CustomersRead,   "Clientes"),
        (CustomersCreate, "Clientes"),
        (CustomersUpdate, "Clientes"),
        (CustomersDelete, "Clientes"),
        (EmployeesRead,   "Funcionários"),
        (EmployeesCreate, "Funcionários"),
        (EmployeesUpdate, "Funcionários"),
        (EmployeesDelete, "Funcionários"),
        (CoursesRead,   "Cursos"),
        (CoursesCreate, "Cursos"),
        (CoursesUpdate, "Cursos"),
        (CoursesDelete, "Cursos"),
        (PeopleRead,   "Pessoas"),
        (PeopleCreate, "Pessoas"),
        (PeopleUpdate, "Pessoas"),
        (PeopleDelete, "Pessoas"),
        (CompaniesRead,   "Empresas"),
        (CompaniesCreate, "Empresas"),
        (CompaniesUpdate, "Empresas"),
        (CompaniesDelete, "Empresas"),
        (UsersManage,   "Administração"),
        (RolesManage,   "Administração"),
        (TenantsManage, "Administração"),
        (PlansManage,   "Administração"),
    ];
}
