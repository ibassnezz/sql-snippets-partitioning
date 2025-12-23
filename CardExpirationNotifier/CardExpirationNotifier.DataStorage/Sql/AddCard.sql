INSERT INTO payment_cards (card_token, card_mask, expiration_year, expiration_month, card_type, user_first_name, user_last_name, notification_sent, created_at)
VALUES (@CardToken, @CardMask, @ExpirationYear, @ExpirationMonth, @CardType, @UserFirstName, @UserLastName, @NotificationSent, @CreatedAt)
RETURNING id
