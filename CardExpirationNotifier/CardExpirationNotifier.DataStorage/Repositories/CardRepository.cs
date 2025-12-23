using System.Data;
using System.Reflection;
using CardExpirationNotifier.DataStorage.Infrastructure;
using CardExpirationNotifier.DataStorage.Models;
using Dapper;
using Npgsql;

namespace CardExpirationNotifier.DataStorage.Repositories;

public class CardRepository : ICardRepository
{
    private readonly string _connectionString;
    private static readonly string AddCardSql;
    private static readonly string GetCardsByDateRangeSql;
    private static readonly string GetPendingNotificationsSql;
    private static readonly string GetCardsToNotifySql;
    private static readonly string MarkAsNotifiedSql;

    static CardRepository()
    {
        // Register snake_case column mapper for PaymentCard
        SqlMapper.SetTypeMap(typeof(PaymentCard), new SnakeCaseColumnMapper(typeof(PaymentCard)));

        AddCardSql = LoadSqlResource("AddCard.sql");
        GetCardsByDateRangeSql = LoadSqlResource("GetCardsByDateRange.sql");
        GetPendingNotificationsSql = LoadSqlResource("GetPendingNotifications.sql");
        GetCardsToNotifySql = LoadSqlResource("GetCardsToNotify.sql");
        MarkAsNotifiedSql = LoadSqlResource("MarkAsNotified.sql");
    }

    public CardRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    private static string LoadSqlResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"CardExpirationNotifier.DataStorage.Sql.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"SQL resource '{resourceName}' not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public async Task<long> AddCardAsync(PaymentCard card)
    {
        using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<long>(AddCardSql, card);
    }

    public async Task<IEnumerable<PaymentCard>> GetCardsByDateRangeAsync(int startYear, int startMonth, int endYear, int endMonth, int offset, int limit)
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<PaymentCard>(GetCardsByDateRangeSql, new { StartYear = startYear, StartMonth = startMonth, EndYear = endYear, EndMonth = endMonth, Offset = offset, Limit = limit });
    }

    public async Task<IEnumerable<PaymentCard>> GetPendingNotificationsAsync(int offset, int limit)
    {
        var now = DateTime.UtcNow;
        var currentYear = now.Year % 100; // Get last 2 digits
        var currentMonth = now.Month;

        using var connection = CreateConnection();
        return await connection.QueryAsync<PaymentCard>(GetPendingNotificationsSql, new { Year = currentYear, Month = currentMonth, Offset = offset, Limit = limit });
    }

    public async Task<IEnumerable<PaymentCard>> GetCardsToNotifyAsync(int year, int month, int limit)
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<PaymentCard>(GetCardsToNotifySql, new { Year = year, Month = month, Limit = limit });
    }

    public async Task MarkAsNotifiedAsync(IEnumerable<long> cardIds)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync(MarkAsNotifiedSql, new { CardIds = cardIds.ToArray() });
    }
}
