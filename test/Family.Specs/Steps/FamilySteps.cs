using AV.Household.Family.Model;
using AV.Household.Family.Specs.API;
using AV.Household.Family.Specs.Drivers;
using Bogus;
using FluentAssertions;

namespace AV.Household.Family.Specs.Steps;

[Binding]
public class FamilySteps
{
    private static readonly Faker Faker = new Faker("ru");

    private readonly FamilyServiceDriver _familyServiceDriver;
    private readonly AuthFakerDriver _authFakerDriver;

    private List<(string Name, bool isAdult)> _givenFamilyMembers = new ();
    private List<FamilyMemberSummaryDTO>? _familyMemberSummaryResult;

    public FamilySteps(FamilyServiceDriver familyServiceDriver, AuthFakerDriver authFakerDriver)
    {
        _familyServiceDriver = familyServiceDriver;
        _authFakerDriver = authFakerDriver;
    }

    [Given(@"Семья из")]
    public async Task GivenFamily(Table membersTable)
    {
        _givenFamilyMembers = membersTable.Rows
            .Select(row => (Name: row["Name"].Trim(), IsAdult: row["Adult"].Trim() == "да"))
            .ToList();
        
        var members = _givenFamilyMembers
            .Select(givenMember => new FamilyMember(
                Guid.NewGuid(),
                1,
                givenMember.Name,
                Faker.Person.Phone,
                Faker.Person.Email,
                givenMember.isAdult
            ));
        await _familyServiceDriver.InitCollectionAsync(members);
    }

    [Given(@"в систему вошел (.*)")]
    public async Task GivenUserLogin(string userName) =>
        await _familyServiceDriver.SetTokenAsync(
            _authFakerDriver.GetBearer(
                userName,
                1,
                _givenFamilyMembers.Single(x=>x.Name == userName).isAdult));

    [When(@"Пользователь получает список членов семьи")]
    public async Task WhenUserGetFamilyMembers() => 
        _familyMemberSummaryResult = await _familyServiceDriver.GetFamilyMembersAsync();

    [When(@"Пользователь добавляет взрослого (.*)")]
    public async Task WhenUserAddAdult(string name) =>
        await _familyServiceDriver.AddMemberAsync(
            new AddFamilyMemberDTO(Faker.Person.Email, true, name, Faker.Person.Phone));

    [When(@"Пользователь добавляет ребенка (.*)")]
    public async Task WhenUserAddChild(string name) =>
        await _familyServiceDriver.AddMemberAsync(
            new AddFamilyMemberDTO(Faker.Person.Email, false, name, Faker.Person.Phone));

    [Then(@"количество элементов в списке членов (.*)")]
    public void ThenFamilyMembersCountIs(int count) =>
        _familyMemberSummaryResult.Should().HaveCount(count);

    [Then(@"в списке членов есть (.*)")]
    public void ThenFamilyMembersContain(string member) =>
        _familyMemberSummaryResult.Should().ContainSingle(familyMember => familyMember.Name == member);
}