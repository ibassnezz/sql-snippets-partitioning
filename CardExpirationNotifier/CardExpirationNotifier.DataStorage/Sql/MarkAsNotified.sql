UPDATE payment_cards
SET notification_sent = true
WHERE id = ANY(@CardIds)
