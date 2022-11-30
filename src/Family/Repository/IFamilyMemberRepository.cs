using AV.Household.Family.Model;

namespace AV.Household.Family.Repository;

/// <summary>
/// Репозитарий для сохранения и получения информаии о членах семьи
/// </summary>
public interface IFamilyMemberRepository
{
    /// <summary>
    /// Получает информацию о члене семьи по его идентификатору
    /// </summary>
    /// <param name="id">идентификатор</param>
    /// <returns>Информация о члене семьи</returns>
    public Task<FamilyMember?> GetAsync(Guid id);
    
    /// <summary>
    /// Получить информацию обо всех членах семьи для данного домохозяйства
    /// </summary>
    /// <param name="householdId">Идентификатор домохозяйства</param>
    /// <returns></returns>
    public Task<IList<FamilyMember>> GetAllForHouseholdAsync(int householdId);
    
    /// <summary>
    /// Добавляет члена семьи 
    /// </summary>
    /// <param name="familyMember">Информация о члене семьи</param>
    /// <returns>Обновленная после вставки в БД информация о члене семьи</returns>
    public Task<FamilyMember> AddAsync(FamilyMember familyMember);
}