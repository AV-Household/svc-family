using AV.Household.Family.Model;
using AV.Household.Family.Repository;
using AV.Household.WebServer.Extensions.Auth;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AV.Household.Family.Controllers;

/// <summary>
/// Контроллер для получения данных о членах семьи 
/// </summary>
[ApiController]
[Route("family")]
[Authorize]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class FamilyController : ControllerBase
{
    private readonly IFamilyMemberRepository _familyMemberRepository;

    /// <summary>
    /// Конструктор контроллера
    /// </summary>
    /// <param name="familyMemberRepository">Объект для хранения данных о членах семьи</param>
    public FamilyController(IFamilyMemberRepository? familyMemberRepository)
    {
        ArgumentNullException.ThrowIfNull(familyMemberRepository, nameof(familyMemberRepository));
        _familyMemberRepository = familyMemberRepository;
    }

    /// <summary>
    /// Возвращает список членов семьи выбранного домохозяйства
    /// </summary>
    /// <response code="200">Список членов семьи</response>
    /// <response code="403">Нет прав просматривать информацию о домохозяйстве</response>
    /// <response code="404">Домохозяйство не найдено</response>
    [HttpGet]
    public async Task<ActionResult<IList<FamilyMemberSummaryDTO>>> Get()
    {
        var household = User.GetHousehold() ?? int.MinValue;

        var result = await _familyMemberRepository.GetAllForHouseholdAsync(household);
        return result.Any()
            ? Ok(result.AsQueryable().ProjectToType<FamilyMemberSummaryDTO>())
            : NotFound();
    }

    /// <summary>
    /// Возвращает информацию о члене семьи
    /// </summary>
    /// <param name="id">Идентификатор члена семьи</param>
    /// <response code="200">Информация о члене семьи</response>
    /// <response code="403">Нет прав просматривать информацию о члене семьи</response>
    /// <response code="404">Член семьи не найден</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IList<FamilyMemberDTO>>> Get(Guid id)
    {
        var household = User.GetHousehold() ?? int.MinValue;
        var member = await _familyMemberRepository.GetAsync(id);

        if (member is null || member.HouseholdId != household)
            return NotFound();

        return Ok(member.Adapt<FamilyMemberDTO>());
    }

    /// <summary>
    /// Добавляет нового члена семьи
    /// </summary>
    /// <param name="member">Информация о добавляемом члене семьи</param>
    /// <response code="201">Член семьи добавлен</response>
    /// <response code="403">Нет прав добавлять члена семьи</response>
    /// <response code="404">Домохозяйство не найдено</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FamilyMemberDTO))]
    public async Task<ActionResult> Post(AddFamilyMemberDTO member)
    {
        var household = User.GetHousehold() ?? int.MinValue;

        if (!ChekHouseholdExist(household))
            return NotFound();

        if (!User.IsInRole("Adult"))
            return Forbid();

        var addedMember = await _familyMemberRepository.AddAsync(
            member.BuildAdapter()
                .AddParameters(nameof(FamilyMember.HouseholdId), household)
                .AdaptToType<FamilyMember>());

        return CreatedAtAction(nameof(Get), new {household = household, id = addedMember.Id},
            addedMember.Adapt<FamilyMemberDTO>());
    }

    /// <summary>
    /// TODO: Реализовать метод
    /// </summary>
    /// <param name="household"></param>
    /// <returns></returns>
    private bool ChekHouseholdExist(int household) => true;

    /// <summary>
    /// Краткая информация о члене семьи
    /// </summary>
    /// <param name="Id">Идентификатор</param>
    /// <param name="Name">Имя</param>
    /// <param name="IsAdult">Является ли взрослым</param>
    public record FamilyMemberSummaryDTO(Guid Id, string Name, bool IsAdult);

    /// <summary>
    /// Подробная информация о члене семьи
    /// </summary>
    /// <param name="Id">Идентификатор</param>
    /// <param name="Name">Имя</param>
    /// <param name="IsAdult">Является ли взрослым</param>
    /// <param name="EMail">Почта</param>
    /// <param name="Phone">Телефон</param>
    public record FamilyMemberDTO(Guid Id, string Name, bool IsAdult, string EMail, string Phone);

    /// <summary>
    /// Информация о добавляемом члене семьи
    /// </summary>
    /// <param name="Name">Имя</param>
    /// <param name="IsAdult">Является ли взрослым</param>
    /// <param name="EMail">Почта</param>
    /// <param name="Phone">Телефон</param>
    public record AddFamilyMemberDTO(string Name, bool IsAdult, string EMail, string Phone);
}