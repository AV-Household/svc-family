using AV.Household.Family.Model;
using AV.Household.WebServer.Extensions.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AV.Household.Family.Repository;

/// <inheritdoc cref="IFamilyMemberRepository"/>
public sealed class MongoFamilyMemberRepository : IFamilyMemberRepository
{
    private readonly IMongoCollection<FamilyMember> _familyMemberCollection;

    /// <summary>
    /// Конструктор репозитария на основе Mongo DB
    /// </summary>
    /// <param name="databaseOptions">Опции подключения к БД</param>
    public MongoFamilyMemberRepository(IOptions<FamilyDatabase> databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions?.Value);
        var connectionString = databaseOptions.Value.ConnectionString;
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseOptions.Value.Database);
        _familyMemberCollection = database.GetCollection<FamilyMember>(nameof(FamilyMember));
    }

    /// <inheritdoc/>
    public async Task<FamilyMember?> GetAsync(Guid id) =>
        await (await _familyMemberCollection.FindAsync(x => x.Id == id)).SingleOrDefaultAsync();

    /// <inheritdoc/>
    public async Task<IList<FamilyMember>> GetAllForHouseholdAsync(int householdId) =>
        await (await _familyMemberCollection.FindAsync(x => x.HouseholdId == householdId)).ToListAsync();

    /// <inheritdoc/>
    public async Task<FamilyMember> AddAsync(FamilyMember familyMember)
    {
        await _familyMemberCollection.InsertOneAsync(familyMember);
        return familyMember;
    }

    /// <summary>
    /// Опции для работы с БД хранящей информацию о членах семьи
    /// </summary>
    public sealed class FamilyDatabase : BaseDatabase
    {
        /// <summary>
        /// Имя коллекции хранящей информацию о членах семьи.
        /// По умолчанию FamilyMember.
        /// </summary>
        public string FamilyMembersCollection { get; set; } = nameof(FamilyMember);
    }
}