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
}
