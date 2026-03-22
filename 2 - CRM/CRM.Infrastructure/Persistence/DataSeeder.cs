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

        await SeedPeopleAsync(db);
        await SeedCustomersAsync(db);
        await SeedCompaniesAsync(db);
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
}
