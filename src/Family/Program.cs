using AV.Household.Family.Controllers;
using AV.Household.Family.Model;
using AV.Household.Family.Repository;
using AV.Household.WebServer.Extensions.Server;
using Mapster;

namespace AV.Household.Family;

/// <summary>
/// Точка входа в приложение
/// </summary>
public class Program
{
    /// <summary>
    /// Процедура запускающая веб-сервис
    /// </summary>
    /// <param name="args">аргументы командной строки</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Настройка мапинга
        TypeAdapterConfig<FamilyController.AddFamilyMemberDTO, FamilyMember>
            .NewConfig()
            .Map(dest => dest.HouseholdId,
                _ => MapContext.Current!.Parameters[nameof(FamilyMember.HouseholdId)]);

        // Настройка БД
        builder.Services.AddOptions<MongoFamilyMemberRepository.FamilyDatabase>()
            .Bind(builder.Configuration.GetSection("Database"));
        builder.Services.AddScoped<IFamilyMemberRepository, MongoFamilyMemberRepository>();

        var app = builder.BuildAPIMicroservice();
        app.Run();
    }
}