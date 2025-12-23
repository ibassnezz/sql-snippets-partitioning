using CardExpirationNotifier.BusinessLogic.Services;
using CardExpirationNotifier.DataStorage.Repositories;
using FluentMigrator.Runner;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException("PostgreSQL connection string is not configured");
var kafkaBootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    ?? throw new InvalidOperationException("Kafka BootstrapServers is not configured");
var kafkaTopic = builder.Configuration["Kafka:Topic"]
    ?? throw new InvalidOperationException("Kafka Topic is not configured");
var batchSize = builder.Configuration.GetValue<int>("NotificationService:BatchSize", 10);
var intervalSeconds = builder.Configuration.GetValue<int>("NotificationService:IntervalSeconds", 60);

// Register FluentMigrator
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(CardExpirationNotifier.DataStorage.Migrations.CreatePartitionedCardTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

// Register repositories
builder.Services.AddSingleton<ICardRepository>(sp => new CardRepository(connectionString));

// Register Kafka producer
builder.Services.AddSingleton<IKafkaProducerService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaProducerService>>();
    return new KafkaProducerService(kafkaBootstrapServers, kafkaTopic, logger);
});

// Register services
builder.Services.AddSingleton<ICardService>(sp =>
{
    var repository = sp.GetRequiredService<ICardRepository>();
    var kafkaProducer = sp.GetRequiredService<IKafkaProducerService>();
    var logger = sp.GetRequiredService<ILogger<CardService>>();
    return new CardService(repository, kafkaProducer, logger, batchSize);
});

// Register background service
builder.Services.AddHostedService(sp =>
{
    var cardService = sp.GetRequiredService<ICardService>();
    var logger = sp.GetRequiredService<ILogger<NotificationBackgroundService>>();
    return new NotificationBackgroundService(cardService, logger, intervalSeconds);
});

var app = builder.Build();

// Run migrations
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Card Expiration Notifier API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();