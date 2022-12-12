using AV.Household.Family.Model;
using AV.Household.Family.Specs.API;
using AV.Household.Family.Specs.Drivers;
using AV.Household.WebServer.Testing.Driver;
using FluentAssertions;

namespace AV.Household.Family.Specs.Steps;

[Binding]
public class FamilySteps
{
    private readonly FamilyServiceDriver _familyServiceDriver;
    private readonly AuthFakerDriver _authFakerDriver;

    private Dictionary<string, (string Email, string Phone, bool IsAdult)> _givenFamilyMembers = new ();
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
            .ToDictionary(row => row["Name"].Trim(), row=> (Email: row["Email"].Trim(), Phone:row["Phone"].Trim(),  IsAdult: row["Adult"].Trim() == "да"));
        
        var members = _givenFamilyMembers
            .Select(givenMember => new FamilyMember(
                Guid.NewGuid(),
                1,
                givenMember.Key,
                givenMember.Value.Phone,
                givenMember.Value.Email,
                givenMember.Value.IsAdult
            ));
        await _familyServiceDriver.InitCollectionAsync(members);
    }

    [Given(@"в систему вошел (.*)")]
    public async Task GivenUserLogin(string userName) =>
        await _familyServiceDriver.SetTokenAsync(
            _authFakerDriver.GetBearer(
                _givenFamilyMembers[userName].Email,
                1,
                _givenFamilyMembers[userName].IsAdult));

    [When(@"Пользователь получает список членов семьи")]
    public async Task WhenUserGetFamilyMembers() => 
        _familyMemberSummaryResult = await _familyServiceDriver.GetFamilyMembersAsync();

    [When(@"Пользователь добавляет взрослого (\w+)\(([\w@.]+),\s([\d,\+]+)\)")]
    public async Task WhenUserAddAdult(string name, string email, string phone) =>
        await _familyServiceDriver.AddMemberAsync(
            new AddFamilyMemberDTO(email, true, name, phone));

    [When(@"Пользователь добавляет ребенка (\w+)\(([\w@.]+),\s([\d,\+]+)\)")]
    public async Task WhenUserAddChild(string name, string email, string phone) =>
        await _familyServiceDriver.AddMemberAsync(
            new AddFamilyMemberDTO(email, false, name, phone));

    [Then(@"количество элементов в списке членов (.*)")]
    public void ThenFamilyMembersCountIs(int count) =>
        _familyMemberSummaryResult.Should().HaveCount(count);

    [Then(@"в списке членов есть (.*)")]
    public void ThenFamilyMembersContain(string member) =>
        _familyMemberSummaryResult.Should().ContainSingle(familyMember => familyMember.Name == member);
}