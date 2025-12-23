SELECT id, card_token, card_mask, expiration_year, expiration_month, card_type, user_first_name, user_last_name, notification_sent, created_at
FROM payment_cards
WHERE expiration_year = @Year AND expiration_month = @Month AND notification_sent = false
ORDER BY id
FOR UPDATE SKIP LOCKED
LIMIT @Limit
