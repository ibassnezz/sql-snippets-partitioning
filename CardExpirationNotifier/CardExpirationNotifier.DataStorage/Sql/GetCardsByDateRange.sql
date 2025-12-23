SELECT id, card_token, card_mask, expiration_year, expiration_month, card_type, user_first_name, user_last_name, notification_sent, created_at
FROM payment_cards
WHERE (expiration_year > @StartYear OR (expiration_year = @StartYear AND expiration_month >= @StartMonth))
  AND (expiration_year < @EndYear OR (expiration_year = @EndYear AND expiration_month <= @EndMonth))
ORDER BY expiration_year, expiration_month, id
OFFSET @Offset LIMIT @Limit
