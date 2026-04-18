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

    public const string WalletsRead   = "wallets:read";
    public const string WalletsCreate = "wallets:create";
    public const string WalletsUpdate = "wallets:update";
    public const string WalletsDelete = "wallets:delete";

    public const string CategoriesRead   = "categories:read";
    public const string CategoriesCreate = "categories:create";
    public const string CategoriesUpdate = "categories:update";
    public const string CategoriesDelete = "categories:delete";

    public const string PaymentMethodsRead   = "payment_methods:read";
    public const string PaymentMethodsCreate = "payment_methods:create";
    public const string PaymentMethodsUpdate = "payment_methods:update";
    public const string PaymentMethodsDelete = "payment_methods:delete";

    public const string TransactionsRead     = "transactions:read";
    public const string TransactionsCreate   = "transactions:create";
    public const string TransactionsUpdate   = "transactions:update";
    public const string TransactionsDelete   = "transactions:delete";
    public const string TransactionsReconcile = "transactions:reconcile";
    public const string TransactionsTransfer  = "transactions:transfer";

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
        (WalletsRead,   "Financeiro"),
        (WalletsCreate, "Financeiro"),
        (WalletsUpdate, "Financeiro"),
        (WalletsDelete, "Financeiro"),
        (CategoriesRead,   "Financeiro"),
        (CategoriesCreate, "Financeiro"),
        (CategoriesUpdate, "Financeiro"),
        (CategoriesDelete, "Financeiro"),
        (PaymentMethodsRead,   "Financeiro"),
        (PaymentMethodsCreate, "Financeiro"),
        (PaymentMethodsUpdate, "Financeiro"),
        (PaymentMethodsDelete, "Financeiro"),
        (TransactionsRead,      "Financeiro"),
        (TransactionsCreate,    "Financeiro"),
        (TransactionsUpdate,    "Financeiro"),
        (TransactionsDelete,    "Financeiro"),
        (TransactionsReconcile, "Financeiro"),
        (TransactionsTransfer,  "Financeiro"),
        (UsersManage,   "Administração"),
        (RolesManage,   "Administração"),
        (TenantsManage, "Administração"),
        (PlansManage,   "Administração"),
    ];
}
