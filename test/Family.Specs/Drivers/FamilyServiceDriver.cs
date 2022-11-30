using AV.Household.Commons.Tasks;
using AV.Household.Family.Model;
using AV.Household.Family.Repository;
using AV.Household.Family.Specs.API;
using AV.Household.WebServer.Testing.Driver;
using DotNet.Testcontainers.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TechTalk.SpecFlow.Infrastructure;

namespace AV.Household.Family.Specs.Drivers;

public class FamilyServiceDriver : MicroserviceDriver<FamilyClient,  Program>
{
    private const string DbName = "Household_Family";
    private const string DbUsername = "test";
    private const string DbPassword = "test";

    private readonly ISpecFlowOutputHelper _specFlowOutputHelper;

    public FamilyServiceDriver(ISpecFlowOutputHelper specFlowOutputHelper)
    {
        _specFlowOutputHelper = specFlowOutputHelper;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(conf =>
            conf.AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    {"JWT__Issuer", "Household"},
                    {"JWT__SecurityKey", "Pa$$w0rd_Pa$$w0rd_Pa$$w0rd"}
                }));


        var connectionString = Database.GetAwaiter().GetResult().ConnectionString;

        builder.ConfigureServices(services =>
            services.AddSingleton(
                Options.Create(new MongoFamilyMemberRepository.FamilyDatabase
                {
                    ConnectionString = connectionString,
                    Database = DbName,
                    FamilyMembersCollection = nameof(FamilyMember)
                })));
        
        base.ConfigureWebHost(builder);
    }

    protected override MongoDbTestcontainerConfiguration? DatabaseConfiguration => new MongoDbTestcontainerConfiguration
    {
        Database = DbName,
        Username = DbUsername,
        Password = DbPassword
    };
    
    public async Task InitCollectionAsync(IEnumerable<FamilyMember> members)
    {
        var connectionString = (await Database).ConnectionString;
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(DbName);
        var familyMemberCollection = database.GetCollection<FamilyMember>(nameof(FamilyMember));
        await familyMemberCollection.DeleteManyAsync(_ => true);
        await familyMemberCollection.InsertManyAsync(members);

        _specFlowOutputHelper.WriteLine("Data was inserted to the database {0}", (await Database).Name);
    }

    public async Task AddMemberAsync(AddFamilyMemberDTO addFamilyMemberDto) =>
        await (await Client).FamilyPostAsync(addFamilyMemberDto).GetResultOrException();

    public async Task<List<FamilyMemberSummaryDTO>> GetFamilyMembersAsync() =>
        (await (await Client).FamilyGetAsync()).ToList();
    
}