using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.CRM.Infrastructure.Persistence;

public static class DataSeeder
{
    public static readonly Guid SeedTenantId = new("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(CRMDbContext db, ITenantService tenantService)
    {
        tenantService.SetTenant(SeedTenantId);

        await EnsureAdminPersonAsync(db);
        await SeedPeopleAsync(db);
        await SeedCustomersAsync(db);
        await SeedCompaniesAsync(db);
        await SeedCoursesAsync(db);
        await SeedEmployeesAsync(db);
    }

    // ── Person do Administrador ───────────────────────────────────────────────

    /// <summary>
    /// Garante que a Person do administrador existe no CRM.
    /// Idempotente — seguro de chamar em todo startup.
    /// </summary>
    private static async Task EnsureAdminPersonAsync(CRMDbContext db)
    {
        const string adminEmail = "admin@mundodalua.com";

        var exists = await db.People
            .IgnoreQueryFilters()
            .AnyAsync(p => p.Email == adminEmail && p.TenantId == SeedTenantId);

        if (exists)
            return;

        var adminPerson = Person.Create(
            tenantId:      SeedTenantId,
            fullName:      "Administrador do Sistema",
            email:         adminEmail,
            preferredName: "Admin",
            notes:         "Conta administrativa do sistema — criada automaticamente pelo seed.");

        await db.People.AddAsync(adminPerson);
        await db.SaveChangesAsync();
    }

    // ── People ────────────────────────────────────────────────────────────────

    private static async Task SeedPeopleAsync(CRMDbContext db)
    {
        if (await db.People.AnyAsync())
            return;

        // Covers a realistic mix of future roles:
        //   guardians, students, clinicians, employees, leads.
        // Role-specific entities (Student, Guardian, etc.) will reference
        // these Person records by PersonId once those modules are implemented.
        var people = new[]
        {
            // Guardiã / Responsável
            Person.Create(SeedTenantId,
                fullName: "Maria José Ramos",
                email: "maria.ramos@email.com",
                documentNumber: "012.345.678-00",
                birthDate: new DateOnly(1960, 4, 25),
                preferredName: "Dona Maria",
                primaryPhone: "(11) 92345-6789",
                whatsAppNumber: "(11) 92345-6789",
                gender: Gender.Female,
                maritalStatus: MaritalStatus.Widowed,
                nationality: "Brasileira",
                occupation: "Aposentada",
                notes: "Responsável pelo aluno Lucas Almeida"),

            // Aluno — criança
            Person.Create(SeedTenantId,
                fullName: "Lucas Almeida Pereira",
                email: "lucas.almeida@email.com",
                documentNumber: "789.012.345-00",
                birthDate: new DateOnly(2010, 9, 12),
                preferredName: "Lucas",
                gender: Gender.Male,
                nationality: "Brasileira",
                occupation: "Estudante",
                notes: "Aluno do ensino fundamental"),

            // Aluno — adolescente
            Person.Create(SeedTenantId,
                fullName: "Thiago Nascimento Barbosa",
                email: "thiago.barbosa@email.com",
                documentNumber: "901.234.567-00",
                birthDate: new DateOnly(2008, 2, 7),
                preferredName: "Thiago",
                gender: Gender.Male,
                nationality: "Brasileira",
                occupation: "Estudante",
                notes: "Paciente da clínica e aluno do reforço escolar"),

            // Aluna — adulta
            Person.Create(SeedTenantId,
                fullName: "Fernanda Oliveira Santos",
                email: "fernanda.santos@email.com",
                documentNumber: "654.321.098-00",
                birthDate: new DateOnly(2005, 6, 18),
                preferredName: "Fê",
                primaryPhone: "(11) 96543-2109",
                whatsAppNumber: "(11) 96543-2109",
                gender: Gender.Female,
                nationality: "Brasileira",
                occupation: "Estudante"),

            // Funcionária — professora
            Person.Create(SeedTenantId,
                fullName: "Ana Paula Souza",
                email: "ana.souza@email.com",
                documentNumber: "123.456.789-00",
                birthDate: new DateOnly(1985, 3, 15),
                preferredName: "Ana",
                primaryPhone: "(11) 91234-5678",
                whatsAppNumber: "(11) 91234-5678",
                gender: Gender.Female,
                maritalStatus: MaritalStatus.Single,
                nationality: "Brasileira",
                occupation: "Professora"),

            // Funcionário — psicólogo
            Person.Create(SeedTenantId,
                fullName: "Pedro Henrique Gomes",
                email: "pedro.gomes@email.com",
                documentNumber: "345.678.901-00",
                birthDate: new DateOnly(1988, 12, 3),
                preferredName: "Pedro",
                primaryPhone: "(11) 95678-9012",
                secondaryPhone: "(11) 94321-0987",
                whatsAppNumber: "(11) 95678-9012",
                gender: Gender.Male,
                maritalStatus: MaritalStatus.Married,
                nationality: "Brasileira",
                occupation: "Psicólogo"),

            // Funcionária — fonoaudióloga
            Person.Create(SeedTenantId,
                fullName: "Juliana Carvalho Rocha",
                email: "juliana.rocha@email.com",
                documentNumber: "678.901.234-00",
                birthDate: new DateOnly(1992, 8, 14),
                preferredName: "Juli",
                primaryPhone: "(11) 93456-7890",
                whatsAppNumber: "(11) 93456-7890",
                gender: Gender.Female,
                maritalStatus: MaritalStatus.StableUnion,
                nationality: "Brasileira",
                occupation: "Fonoaudióloga"),

            // Paciente / Médico
            Person.Create(SeedTenantId,
                fullName: "Roberto Silva Costa",
                email: "roberto.costa@email.com",
                documentNumber: "321.654.987-00",
                birthDate: new DateOnly(1970, 1, 30),
                preferredName: "Roberto",
                primaryPhone: "(21) 97654-3210",
                gender: Gender.Male,
                maritalStatus: MaritalStatus.Divorced,
                nationality: "Brasileira",
                occupation: "Médico"),

            // Lead — potencial aluno
            Person.Create(SeedTenantId,
                fullName: "Carlos Eduardo Mendes",
                email: "carlos.mendes@email.com",
                documentNumber: "987.654.321-00",
                birthDate: new DateOnly(1978, 7, 22),
                preferredName: "Carlos",
                primaryPhone: "(31) 99876-5432",
                whatsAppNumber: "(31) 99876-5432",
                gender: Gender.Male,
                maritalStatus: MaritalStatus.Married,
                nationality: "Brasileira",
                occupation: "Engenheiro"),

            // Lead — potencial aluna
            Person.Create(SeedTenantId,
                fullName: "Beatriz Lima Ferreira",
                email: "beatriz.lima@email.com",
                documentNumber: "456.789.123-00",
                birthDate: new DateOnly(1995, 11, 5),
                preferredName: "Bia",
                primaryPhone: "(51) 94567-8901",
                secondaryPhone: "(51) 98765-4321",
                whatsAppNumber: "(51) 94567-8901",
                gender: Gender.Female,
                maritalStatus: MaritalStatus.Single,
                nationality: "Brasileira",
                occupation: "Designer"),
        };

        await db.People.AddRangeAsync(people);
        await db.SaveChangesAsync();
    }

    // ── Customers ─────────────────────────────────────────────────────────────

    private static async Task SeedCustomersAsync(CRMDbContext db)
    {
        if (await db.Customers.AnyAsync())
            return;

        var customers = new[]
        {
            Customer.Create(SeedTenantId, "Ana Souza", "ana.souza.cliente@email.com", CustomerType.Individual, "(11) 91234-5678", "123.456.789-00"),
            Customer.Create(SeedTenantId, "Tech Solutions Ltda", "contato@techsolutions.com.br", CustomerType.Company, "(21) 98765-4321", "12.345.678/0001-99"),
            Customer.Create(SeedTenantId, "Carlos Mendes", "carlos.mendes.cliente@email.com", CustomerType.Individual, "(31) 99876-5432", "987.654.321-00"),
            Customer.Create(SeedTenantId, "Farmácia Saúde", "farmacia@saude.com", CustomerType.Company, "(41) 93456-7890", "98.765.432/0001-11"),
            Customer.Create(SeedTenantId, "Beatriz Lima", "beatriz.lima.cliente@email.com", CustomerType.Individual, "(51) 94567-8901", "456.789.123-00"),
        };

        await db.Customers.AddRangeAsync(customers);
        await db.SaveChangesAsync();
    }

    // ── Companies ─────────────────────────────────────────────────────────────

    private static async Task SeedCompaniesAsync(CRMDbContext db)
    {
        if (await db.Companies.AnyAsync())
            return;

        // Covers a realistic mix of future roles:
        //   schools, suppliers, partners, corporate customers, service providers.
        // Role-specific entities (School, Supplier, etc.) will reference
        // these Company records by CompanyId once those modules are implemented.
        var companies = new[]
        {
            // Escola — tenant principal (Mundo da Lua)
            Company.Create(SeedTenantId,
                legalName: "Mundo da Lua Educação Ltda",
                tradeName: "Escola Mundo da Lua",
                registrationNumber: "12.345.678/0001-90",
                stateRegistration: "123.456.789.000",
                municipalRegistration: "98765432",
                email: "contato@mundodalua.com.br",
                primaryPhone: "(11) 3456-7890",
                whatsAppNumber: "(11) 94567-8901",
                website: "https://www.mundodalua.com.br",
                contactPersonName: "Diretora Carla Mendes",
                contactPersonEmail: "carla.mendes@mundodalua.com.br",
                contactPersonPhone: "(11) 94567-8901",
                companyType: CompanyType.School,
                industry: "Educação",
                notes: "Escola de educação infantil e ensino fundamental — tenant principal"),

            // Fornecedor — material didático
            Company.Create(SeedTenantId,
                legalName: "Editora Saber Vivo S.A.",
                tradeName: "Saber Vivo",
                registrationNumber: "23.456.789/0001-01",
                email: "vendas@sabervivo.com.br",
                primaryPhone: "(11) 2345-6789",
                whatsAppNumber: "(11) 92345-6789",
                website: "https://www.sabervivo.com.br",
                contactPersonName: "Marcos Pinheiro",
                contactPersonEmail: "marcos.pinheiro@sabervivo.com.br",
                contactPersonPhone: "(11) 92345-6789",
                companyType: CompanyType.Supplier,
                industry: "Editorial / Material Didático",
                notes: "Fornecedor de livros e materiais pedagógicos"),

            // Fornecedor — alimentação
            Company.Create(SeedTenantId,
                legalName: "Nutrição Escolar Distribuidora Ltda",
                tradeName: "NutriEscola",
                registrationNumber: "34.567.890/0001-12",
                email: "pedidos@nutriescola.com.br",
                primaryPhone: "(11) 3234-5678",
                companyType: CompanyType.Supplier,
                industry: "Alimentação",
                notes: "Fornecedor de merenda e lanches para a cantina"),

            // Parceiro — clínica de saúde mental
            Company.Create(SeedTenantId,
                legalName: "Clínica Mente Sã Psicologia Ltda",
                tradeName: "Mente Sã",
                registrationNumber: "45.678.901/0001-23",
                email: "contato@mentesa.com.br",
                primaryPhone: "(11) 3345-6789",
                whatsAppNumber: "(11) 93456-7890",
                website: "https://www.mentesa.com.br",
                contactPersonName: "Dra. Renata Oliveira",
                contactPersonEmail: "renata.oliveira@mentesa.com.br",
                contactPersonPhone: "(11) 93456-7890",
                companyType: CompanyType.Partner,
                industry: "Saúde Mental",
                notes: "Parceira clínica para encaminhamento de alunos — psicologia e fonoaudiologia"),

            // Parceiro — academia de esportes
            Company.Create(SeedTenantId,
                legalName: "Sport Kids Academia Infantil Ltda",
                tradeName: "Sport Kids",
                registrationNumber: "56.789.012/0001-34",
                email: "info@sportkids.com.br",
                primaryPhone: "(11) 4456-7890",
                whatsAppNumber: "(11) 94567-1234",
                companyType: CompanyType.Partner,
                industry: "Esporte e Lazer",
                notes: "Parceira para atividades extracurriculares esportivas"),

            // Cliente corporativo — empresa conveniada
            Company.Create(SeedTenantId,
                legalName: "Tech Solutions Desenvolvimento de Software Ltda",
                tradeName: "Tech Solutions",
                registrationNumber: "67.890.123/0001-45",
                email: "rh@techsolutions.com.br",
                primaryPhone: "(11) 5567-8901",
                website: "https://www.techsolutions.com.br",
                contactPersonName: "Amanda Carvalho",
                contactPersonEmail: "amanda.carvalho@techsolutions.com.br",
                contactPersonPhone: "(11) 95678-2345",
                companyType: CompanyType.CorporateCustomer,
                industry: "Tecnologia da Informação",
                notes: "Empresa com funcionários que têm filhos matriculados — convênio corporativo"),

            // Prestador de serviços — manutenção
            Company.Create(SeedTenantId,
                legalName: "Conserva Fácil Serviços Gerais ME",
                tradeName: "Conserva Fácil",
                registrationNumber: "78.901.234/0001-56",
                email: "contato@conservafacil.com.br",
                primaryPhone: "(11) 6678-9012",
                whatsAppNumber: "(11) 96789-3456",
                contactPersonName: "João Batista",
                contactPersonEmail: "joao@conservafacil.com.br",
                companyType: CompanyType.ServiceProvider,
                industry: "Manutenção e Conservação",
                notes: "Prestador de serviços de limpeza e manutenção predial"),

            // Prestador de serviços — TI
            Company.Create(SeedTenantId,
                legalName: "Infra Digital Soluções em TI Ltda",
                tradeName: "Infra Digital",
                registrationNumber: "89.012.345/0001-67",
                email: "suporte@infradigital.com.br",
                primaryPhone: "(11) 7789-0123",
                whatsAppNumber: "(11) 97890-4567",
                website: "https://www.infradigital.com.br",
                contactPersonName: "Ricardo Fonseca",
                contactPersonEmail: "ricardo.fonseca@infradigital.com.br",
                contactPersonPhone: "(11) 97890-4567",
                companyType: CompanyType.ServiceProvider,
                industry: "Tecnologia da Informação",
                notes: "Gerencia a infraestrutura de TI, câmeras, rede e sistemas da escola"),

            // Patrocinador — fundação cultural
            Company.Create(SeedTenantId,
                legalName: "Fundação Cultural Aprender Juntos",
                tradeName: "Fundação Aprender",
                registrationNumber: "90.123.456/0001-78",
                email: "contato@aprenderjuntos.org.br",
                primaryPhone: "(11) 8890-1234",
                website: "https://www.aprenderjuntos.org.br",
                contactPersonName: "Dra. Sônia Bastos",
                contactPersonEmail: "sonia.bastos@aprenderjuntos.org.br",
                companyType: CompanyType.Sponsor,
                industry: "Terceiro Setor / Educação",
                notes: "Patrocina bolsas de estudo e projetos culturais na escola"),
        };

        await db.Companies.AddRangeAsync(companies);
        await db.SaveChangesAsync();
    }

    // ── Courses ───────────────────────────────────────────────────────────────

    private static async Task SeedCoursesAsync(CRMDbContext db)
    {
        if (await db.Courses.AnyAsync())
            return;

        // Covers a realistic mix of course types offered by Mundo da Lua:
        //   reforço escolar, idiomas, turmas regulares, workshops e oficinas.
        var courses = new[]
        {
            // Reforço Escolar — Fundamental I (1º ao 5º ano)
            CreateActive(Course.Create(SeedTenantId,
                name: "Reforço Escolar — Fundamental I",
                type: CourseType.AfterSchool,
                code: "REF-FI-2025",
                description: "Aulas de reforço em Português e Matemática para alunos do 1º ao 5º ano do ensino fundamental.",
                startDate: new DateOnly(2025, 2, 10),
                endDate: new DateOnly(2025, 12, 12),
                scheduleDescription: "Segundas e quartas-feiras, 13h–15h",
                capacity: 15,
                workload: 160,
                notes: "Foco em leitura, escrita e raciocínio lógico")),

            // Reforço Escolar — Fundamental II (6º ao 9º ano)
            CreateActive(Course.Create(SeedTenantId,
                name: "Reforço Escolar — Fundamental II",
                type: CourseType.AfterSchool,
                code: "REF-FII-2025",
                description: "Reforço em Matemática, Ciências e Português para alunos do 6º ao 9º ano.",
                startDate: new DateOnly(2025, 2, 10),
                endDate: new DateOnly(2025, 12, 12),
                scheduleDescription: "Terças e quintas-feiras, 14h–16h",
                capacity: 12,
                workload: 160,
                notes: "Preparação para provas bimestrais e SAEB")),

            // Inglês — Nível A1
            CreateActive(Course.Create(SeedTenantId,
                name: "Inglês — Nível A1",
                type: CourseType.Language,
                code: "ING-A1-2025-1",
                description: "Curso de inglês para iniciantes. Foco em vocabulário básico, saudações e frases do cotidiano.",
                startDate: new DateOnly(2025, 3, 3),
                endDate: new DateOnly(2025, 8, 29),
                scheduleDescription: "Segundas, quartas e sextas, 10h–11h",
                capacity: 10,
                workload: 80,
                notes: "Material didático incluído na mensalidade")),

            // Inglês — Nível A2
            CreateActive(Course.Create(SeedTenantId,
                name: "Inglês — Nível A2",
                type: CourseType.Language,
                code: "ING-A2-2025-1",
                description: "Continuação do nível A1. Gramática básica, presente e passado simples, diálogos do dia a dia.",
                startDate: new DateOnly(2025, 3, 3),
                endDate: new DateOnly(2025, 8, 29),
                scheduleDescription: "Segundas, quartas e sextas, 11h–12h",
                capacity: 10,
                workload: 80,
                notes: "Pré-requisito: aprovação no nível A1 ou teste de nivelamento")),

            // Espanhol Básico — em elaboração
            Course.Create(SeedTenantId,
                name: "Espanhol Básico",
                type: CourseType.Language,
                code: "ESP-B1-2025",
                description: "Curso introdutório de espanhol para crianças e adolescentes.",
                startDate: new DateOnly(2025, 9, 1),
                endDate: new DateOnly(2026, 2, 27),
                scheduleDescription: "Terças e quintas-feiras, 10h–11h",
                capacity: 10,
                workload: 60,
                notes: "Aguardando confirmação do professor responsável — em rascunho"),

            // Turma Regular — 3º Ano A (2025)
            CreateActive(Course.Create(SeedTenantId,
                name: "Turma 3º Ano A — 2025",
                type: CourseType.SchoolClass,
                code: "T3A-2025",
                description: "Turma regular do 3º ano do ensino fundamental — período matutino.",
                startDate: new DateOnly(2025, 2, 3),
                endDate: new DateOnly(2025, 12, 19),
                scheduleDescription: "Segunda a sexta, 7h–12h",
                capacity: 25,
                workload: 800,
                notes: "Professora titular: Ana Paula Souza")),

            // Turma Regular — 6º Ano A (2025)
            CreateActive(Course.Create(SeedTenantId,
                name: "Turma 6º Ano A — 2025",
                type: CourseType.SchoolClass,
                code: "T6A-2025",
                description: "Turma regular do 6º ano do ensino fundamental — período vespertino.",
                startDate: new DateOnly(2025, 2, 3),
                endDate: new DateOnly(2025, 12, 19),
                scheduleDescription: "Segunda a sexta, 13h–18h",
                capacity: 28,
                workload: 800)),

            // Workshop de Teatro — já concluído
            CreateCompleted(Course.Create(SeedTenantId,
                name: "Workshop de Teatro Infantil",
                type: CourseType.Workshop,
                code: "WKS-TEA-2024",
                description: "Oficina de teatro para crianças de 7 a 12 anos, com apresentação final aberta aos pais.",
                startDate: new DateOnly(2024, 8, 5),
                endDate: new DateOnly(2024, 11, 29),
                scheduleDescription: "Sábados, 9h–11h",
                capacity: 20,
                workload: 40,
                notes: "Apresentação de encerramento realizada em 29/11/2024")),

            // Workshop de Robótica — ativo
            CreateActive(Course.Create(SeedTenantId,
                name: "Workshop de Robótica e Programação",
                type: CourseType.Workshop,
                code: "WKS-ROB-2025-1",
                description: "Introdução à robótica e lógica de programação usando kits educacionais. Para alunos de 10 a 15 anos.",
                startDate: new DateOnly(2025, 4, 7),
                endDate: new DateOnly(2025, 7, 11),
                scheduleDescription: "Sextas-feiras, 14h–16h",
                capacity: 12,
                workload: 30,
                notes: "Parceria com empresa de tecnologia educacional")),

            // Oficina de Leitura — ativo
            CreateActive(Course.Create(SeedTenantId,
                name: "Oficina de Leitura e Escrita Criativa",
                type: CourseType.Other,
                code: "OFI-LEC-2025",
                description: "Programa extracurricular de incentivo à leitura e desenvolvimento da escrita criativa para alunos de 8 a 14 anos.",
                startDate: new DateOnly(2025, 3, 10),
                endDate: new DateOnly(2025, 11, 28),
                scheduleDescription: "Quintas-feiras, 14h30–16h",
                capacity: 16,
                workload: 64,
                notes: "Acervo de livros disponível na biblioteca da escola")),
        };

        await db.Courses.AddRangeAsync(courses);
        await db.SaveChangesAsync();
    }

    // ── Employees ─────────────────────────────────────────────────────────────

    private static async Task SeedEmployeesAsync(CRMDbContext db)
    {
        if (await db.Employees.AnyAsync())
            return;

        // Look up the people already seeded that represent employees of Mundo da Lua.
        // We resolve by email because Person IDs are dynamically generated.
        var ana     = await db.People.FirstOrDefaultAsync(p => p.Email == "ana.souza@email.com");
        var pedro   = await db.People.FirstOrDefaultAsync(p => p.Email == "pedro.gomes@email.com");
        var juliana = await db.People.FirstOrDefaultAsync(p => p.Email == "juliana.rocha@email.com");

        if (ana is null || pedro is null || juliana is null)
            return; // People seed has not run — skip silently

        var employees = new[]
        {
            // Professora titular — Ana Paula Souza
            Employee.Create(SeedTenantId,
                personId:     ana.Id,
                employeeCode: "MDL-001",
                hireDate:     new DateOnly(2020, 2, 3),
                position:     "Professora do Ensino Fundamental I",
                department:   "Pedagogia",
                contractType: "CLT",
                workSchedule: "Segunda a sexta, 7h–12h",
                workloadHours: 30m,
                payrollNumber: "FLH-001",
                costCenter:   "CC-PEDAGOGY",
                notes:        "Professora titular da turma 3º Ano A"),

            // Psicólogo — Pedro Henrique Gomes
            Employee.Create(SeedTenantId,
                personId:     pedro.Id,
                employeeCode: "MDL-002",
                hireDate:     new DateOnly(2022, 3, 14),
                position:     "Psicólogo Escolar",
                department:   "Clínica / Apoio",
                contractType: "PJ",
                workSchedule: "Terças e quintas-feiras, 8h–17h",
                workloadHours: 16m,
                payrollNumber: "FLH-002",
                costCenter:   "CC-CLINIC",
                notes:        "Atende alunos encaminhados pela coordenação pedagógica"),

            // Fonoaudióloga — Juliana Carvalho Rocha
            Employee.Create(SeedTenantId,
                personId:     juliana.Id,
                employeeCode: "MDL-003",
                hireDate:     new DateOnly(2023, 8, 1),
                position:     "Fonoaudióloga",
                department:   "Clínica / Apoio",
                contractType: "PJ",
                workSchedule: "Segundas e quartas-feiras, 9h–16h",
                workloadHours: 14m,
                payrollNumber: "FLH-003",
                costCenter:   "CC-CLINIC",
                notes:        "Especialista em linguagem infantil e dislexia"),
        };

        await db.Employees.AddRangeAsync(employees);
        await db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Course CreateActive(Course course)
    {
        course.Publish();
        return course;
    }

    private static Course CreateCompleted(Course course)
    {
        course.Publish();
        course.Complete();
        return course;
    }
}
