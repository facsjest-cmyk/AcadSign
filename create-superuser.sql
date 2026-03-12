-- Créer l'utilisateur superuser
-- Le hash BCrypt pour "hkiko1969**TT" avec workFactor 12
INSERT INTO "AppUsers" ("Id", "Username", "Email", "PasswordHash", "Role", "IsActive", "CreatedAt")
VALUES (
    gen_random_uuid(),
    'superuser',
    'superuser@acadsign.com',
    '$2a$12$KQZ8vXJ5YmVxN3hZQnN6ZO8F7XqJ9K5L3mN2pQ4rS6tU8vW0xY2Za',
    2,
    true,
    NOW()
)
ON CONFLICT ("Username") DO NOTHING;

-- Vérifier que l'utilisateur a été créé
SELECT "Id", "Username", "Email", "Role", "IsActive", "CreatedAt" 
FROM "AppUsers" 
WHERE "Username" = 'superuser';
