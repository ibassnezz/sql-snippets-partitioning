# Card notifier with DB-partitioning

## Fully vibe-coding approach

I decided to come up with some kind of vibe coding experiment. 
AI agent have to write a service form the beginning without any intervene from developer

## Plot

Service should notify users if the card expires.

## Conditions

1. No code is written by human
2. The solution should work correctly
3. The selected AI agent: [Claude AI](https://claude.com/product/claude-code)
4. IDE [Rider](https://www.jetbrains.com/rider/) 
5. Rider marketplace plugin: Calude Code
6. Problem-solving and debugging: just error messages

## Prompt

```
Given an empty web project. It is necessary to rewrite it with the logic described below.

-----
Business logic

Service should notify users only one time by Kafka topic "card_expired_notification" if the card has its last working month.
Notification should be given partially, 10 cards per request.
Don’t forget to make SELECT FOR UPDATE while sending to avoid race conditions.

-----
All this job is performed by a standard BackgroundJob.

Common description
Service stores payment cards like Visa, MasterCard, etc. Attributes are:
* CardToken (performed as SHA-256 of first 6 digits, last 4 digits, card type),
* CardMask — first 6 + last 4 digits of PAN
* Expiration Date (two-digit year and two-digit month, as on the card)
* CardType
* User first and last name

-----
API

Cards are added through API (HTTP / JSON / REST):

- AddCard: CardMask, Expiration Date, CardType, User first and last name.
Validate expiration date, do not add cards that exceed partition boundaries.
In this case return 400 with message "Hey, man, we don't support such cards"

- ShowCards: request parameters DateRange, Offset, and Limit

- PendingNotification: Offset and Limit, show all cards that need to be notified.
Show in response card requisites and whether the card has been sent or not

Add Swagger to APIs

-----
Storage

Solution should have migration code (made with Goose migration).
The migration must have a partitioned database, partitioned by month and year.
Create partitions from January 2020 till December 2035.

To select data use Dapper and its mapping.
Place SQL request code into separate .sql files.
Storage is performed in PostgreSQL 15.

-----
Testing

Write unit tests in a separate project using xUnit.
All business logic C# classes should be covered.

-----
Projects structure

- CardExpirationNotifier.DataStorage — keep in a separate project
- CardExpirationNotifier.UnitTests — in a separate project, place it in a new tests folder
- Business logic in a separate project called CardExpirationNotifier.BusinessLogic
```

## Debugging
**error_1** wrong migrations
> The table is partitioned by (expiration_year, expiration_month) (line 24), but you're trying to create a unique index on just card_token (line 29). PostgreSQL requires that any unique constraint on a partitioned table must include all partitioning columns.

**fix_1**
> The unique index needs to include the partitioning columns.

**fixed_itself_1**
> YES


**error_2** problem with swagger made the project was not able to run
> System.Reflection.ReflectionTypeLoadException: Unable to load one or more of the requested  types. Could not load type 'Microsoft.OpenApi.Any.IOpenApiAny' from assembly 'Microsoft.OpenApi, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3f5743946376f042'.  ould not load type 'Microsoft.OpenApi.Models.OpenApiDiscriminator' from assembly 'Microsoft.OpenApi...

**fix_2**
> Removed dll and updated project

**fixed_itself_2**
> YES

**error_3** GET request gave me nullable data http://localhost:5293/api/Cards?startYear=20&startMonth=12&endYear=27&endMonth=12&offset=0&limit=50
> [
{
"id": 1,
"cardMask": "",
"expirationYear": 0,
"expirationMonth": 0,
"cardType": "",
"userFirstName": "",
"userLastName": "",
"notificationSent": false,
"createdAt": "0001-01-01T00:00:00"
}
]


**fix_3**
> Set attributes to the dapper collumns

**fixed_itself_3**
> YES


**error_4** The message that has been sent to kafka is also nullbale
> {
"cardMask": "",
"expirationYear": 0,
"expirationMonth": 0,
"cardType": "",
"userFirstName": "",
"userLastName": ""
}

**fix_4**
> Set attributes to the dapper collumns

**fixed_itself_4**
> NO, project has not been run correctly

**error_5** The message that has been sent to kafka is also nullbale
> {
"cardMask": "",
"expirationYear": 0,
"expirationMonth": 0,
"cardType": "",
"userFirstName": "",
"userLastName": ""
}

**fix_5**
> Set attributes to the dapper columns

**fixed_itself_5**
> NO, project has not been run correctly

**error_6** Default constructor is not defined
> A parameterless default constructor or one matching signature (System.Int64 id, System.String card_token, System.String card_mask, System.Int32 expiration_year, System.Int32 expiration_month, System.String card_type, System.String user_first_name, System.String user_last_name, System.Boolean notification_sent, System.DateTime created_at) is required for CardExpirationNotifier.DataStorage.Models.PaymentCard materialization

**fix_6**
> Fixed constructor

**fixed_itself_6**
> YES



**error_6** Incoclusive tests run
> Last runner error: Test runner agent exited unexpectedly Process /usr/local/share/dotnet/x64/dotnet:30111 exited with code '150'. You must install or update .NET to run this application.
**fix_6**
> Fixed constructor

**fixed_itself_6**
> YES
