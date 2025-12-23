using FluentMigrator;

namespace CardExpirationNotifier.DataStorage.Migrations;

[Migration(1)]
public class CreatePartitionedCardTable : Migration
{
    public override void Up()
    {
        // Create the main partitioned table
        Execute.Sql(@"
            CREATE TABLE payment_cards (
                id BIGSERIAL,
                card_token VARCHAR(64) NOT NULL,
                card_mask VARCHAR(20) NOT NULL,
                expiration_year INT NOT NULL,
                expiration_month INT NOT NULL,
                card_type VARCHAR(50) NOT NULL,
                user_first_name VARCHAR(100) NOT NULL,
                user_last_name VARCHAR(100) NOT NULL,
                notification_sent BOOLEAN NOT NULL DEFAULT false,
                created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                PRIMARY KEY (id, expiration_year, expiration_month)
            ) PARTITION BY RANGE (expiration_year, expiration_month);
        ");

        // Create unique index on card_token with partition columns
        Execute.Sql(@"
            CREATE UNIQUE INDEX idx_payment_cards_card_token ON payment_cards (card_token, expiration_year, expiration_month);
        ");

        // Create index for notification queries
        Execute.Sql(@"
            CREATE INDEX idx_payment_cards_notification ON payment_cards (expiration_year, expiration_month, notification_sent);
        ");

        // Create partitions from 2020-01 to 2035-12
        for (int year = 20; year <= 35; year++)
        {
            for (int month = 1; month <= 12; month++)
            {
                var partitionName = $"payment_cards_y{year:D2}_m{month:D2}";
                var nextMonth = month + 1;
                var nextYear = year;

                if (nextMonth > 12)
                {
                    nextMonth = 1;
                    nextYear++;
                }

                Execute.Sql($@"
                    CREATE TABLE {partitionName} PARTITION OF payment_cards
                    FOR VALUES FROM ({year}, {month}) TO ({nextYear}, {nextMonth});
                ");
            }
        }
    }

    public override void Down()
    {
        // Drop all partitions
        for (int year = 20; year <= 35; year++)
        {
            for (int month = 1; month <= 12; month++)
            {
                var partitionName = $"payment_cards_y{year:D2}_m{month:D2}";
                Execute.Sql($"DROP TABLE IF EXISTS {partitionName};");
            }
        }

        // Drop the main table
        Execute.Sql("DROP TABLE IF EXISTS payment_cards;");
    }
}
