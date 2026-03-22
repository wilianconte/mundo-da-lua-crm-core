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

        // Covers a realistic mix of company types for Mundo da Lua:
        //   suppliers, partners, schools, corporate customers,
        //   service providers, sponsors.
        // Role-specific entities (Supplier, Partner, School, etc.) will reference
        // these Company records by CompanyId once those modules are implemented.
        var companies = new[]
        {
            // Supplier — livros e material didático
            Company.Create(SeedTenantId,
                legalName: "Editora Saber Materiais Didáticos Ltda",
                tradeName: "Editora Saber",
                registrationNumber: "12.345.678/0001-01",
                email: "contato@editorasaber.com.br",
                primaryPhone: "(11) 3456-7890",
                whatsAppNumber: "(11) 93456-7890",
                website: "https://www.editorasaber.com.br",
                contactPersonName: "Marcos Andrade",
                contactPersonEmail: "marcos.andrade@editorasaber.com.br",
                contactPersonPhone: "(11) 98765-4321",
                companyType: CompanyType.Supplier,
                industry: "Editorial / Material Didático",
                notes: "Fornecedor principal de livros e materiais pedagógicos"),

            // Partner — instituto pedagógico parceiro
            Company.Create(SeedTenantId,
                legalName: "Instituto Crescer Educação e Desenvolvimento Social",
                tradeName: "Instituto Crescer",
                registrationNumber: "23.456.789/0001-02",
                email: "parceria@institutocrescer.org.br",
                primaryPhone: "(11) 2345-6789",
                website: "https://www.institutocrescer.org.br",
                contactPersonName: "Dra. Sônia Meireles",
                contactPersonEmail: "sonia.meireles@institutocrescer.org.br",
                contactPersonPhone: "(11) 97654-3210",
                companyType: CompanyType.Partner,
                industry: "Educação / Desenvolvimento Social",
                notes: "Parceiro pedagógico para programas de contraturno e inclusão"),

            // School — escola parceira para encaminhamento de alunos
            Company.Create(SeedTenantId,
                legalName: "Escola Estadual Professor João Bosco",
                tradeName: "EE João Bosco",
                registrationNumber: "34.567.890/0001-03",
                stateRegistration: "ISENTO",
                email: "secretaria@eejbosc.edu.br",
                primaryPhone: "(11) 4567-8901",
                contactPersonName: "Claudia Ferraz",
                contactPersonEmail: "diretoria@eejbosc.edu.br",
                contactPersonPhone: "(11) 96543-2109",
                companyType: CompanyType.School,
                industry: "Educação Básica",
                notes: "Escola parceira para encaminhamento de alunos ao contraturno"),

            // CorporateCustomer — empresa que adquire vagas para filhos de funcionários
            Company.Create(SeedTenantId,
                legalName: "TechEdu Sistemas e Consultoria Ltda",
                tradeName: "TechEdu",
                registrationNumber: "45.678.901/0001-04",
                email: "rh@techedu.com.br",
                primaryPhone: "(11) 5678-9012",
                secondaryPhone: "(11) 5678-9013",
                website: "https://www.techedu.com.br",
                contactPersonName: "Renata Borges",
                contactPersonEmail: "renata.borges@techedu.com.br",
                contactPersonPhone: "(11) 95432-1098",
                companyType: CompanyType.CorporateCustomer,
                industry: "Tecnologia da Informação",
                notes: "Cliente corporativo — adquire pacotes de vagas para filhos de colaboradores"),

            // ServiceProvider — empresa de limpeza e conservação
            Company.Create(SeedTenantId,
                legalName: "Limpeza Total Serviços Gerais ME",
                tradeName: "Limpeza Total",
                registrationNumber: "56.789.012/0001-05",
                email: "contato@limpezatotal.com.br",
                primaryPhone: "(11) 6789-0123",
                whatsAppNumber: "(11) 94321-0987",
                contactPersonName: "José Carlos Pinto",
                contactPersonEmail: "jose.pinto@limpezatotal.com.br",
                contactPersonPhone: "(11) 94321-0987",
                companyType: CompanyType.ServiceProvider,
                industry: "Serviços de Limpeza e Conservação",
                notes: "Prestador de serviços de limpeza e conservação das instalações"),

            // Sponsor — patrocinador de eventos e projetos sociais
            Company.Create(SeedTenantId,
                legalName: "Banco Comunitário Solidário S.A.",
                tradeName: "BancoCom",
                registrationNumber: "67.890.123/0001-06",
                email: "marketing@bancocom.com.br",
                primaryPhone: "(11) 7890-1234",
                website: "https://www.bancocom.com.br",
                contactPersonName: "Flávia Mendonça",
                contactPersonEmail: "flavia.mendonca@bancocom.com.br",
                contactPersonPhone: "(11) 93210-9876",
                companyType: CompanyType.Sponsor,
                industry: "Serviços Financeiros",
                notes: "Patrocinador de eventos anuais e projetos sociais"),

            // ServiceProvider — desenvolvimento de sistemas
            Company.Create(SeedTenantId,
                legalName: "SoftHouse Desenvolvimento de Sistemas Ltda",
                tradeName: "SoftHouse",
                registrationNumber: "78.901.234/0001-07",
                email: "suporte@softhouse.dev",
                primaryPhone: "(11) 8901-2345",
                whatsAppNumber: "(11) 92109-8765",
                website: "https://www.softhouse.dev",
                contactPersonName: "Diego Queiroz",
                contactPersonEmail: "diego.queiroz@softhouse.dev",
                contactPersonPhone: "(11) 92109-8765",
                companyType: CompanyType.ServiceProvider,
                industry: "Tecnologia — Desenvolvimento de Software",
                notes: "Fornecedor de sistemas de gestão e suporte técnico"),

            // Supplier — gráfica para material de eventos e campanhas
            Company.Create(SeedTenantId,
                legalName: "Gráfica Rápida Impressões ME",
                tradeName: "Gráfica Rápida",
                registrationNumber: "89.012.345/0001-08",
                email: "orcamento@graficarapida.com.br",
                primaryPhone: "(11) 9012-3456",
                whatsAppNumber: "(11) 91098-7654",
                contactPersonName: "Sandra Matos",
                contactPersonEmail: "sandra@graficarapida.com.br",
                contactPersonPhone: "(11) 91098-7654",
                companyType: CompanyType.Supplier,
                industry: "Gráfica e Comunicação Visual",
                notes: "Fornecedor de material gráfico para eventos e campanhas"),
        };

        await db.Companies.AddRangeAsync(companies);
        await db.SaveChangesAsync();
    }
}
