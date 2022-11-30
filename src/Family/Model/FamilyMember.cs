namespace AV.Household.Family.Model;

/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="HouseholdId"></param>
/// <param name="Name"></param>
/// <param name="Phone"></param>
/// <param name="EMail"></param>
/// <param name="IsAdult"></param>
public record FamilyMember(
    Guid Id, 
    int HouseholdId, 
    string Name, 
    string Phone, 
    string EMail,
    bool IsAdult);
