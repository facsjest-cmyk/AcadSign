-- Créer l'utilisateur superuser avec mot de passe haché
-- Note: Le hash BCrypt sera généré et inséré manuellement
INSERT INTO "AppUsers" ("Id", "Username", "Email", "PasswordHash", "Role", "IsActive", "CreatedAt")
VALUES (
    '00000000-0000-0000-0000-000000000001'::uuid,
    'superuser',
    'superuser@acadsign.com',
    '$2a$12$KQZ8vXJ5YmVxN3hZQnN6ZO8F7XqJ9K5L3mN2pQ4rS6tU8vW0xY2Za',
    2,
    true,
    NOW()
)
ON CONFLICT ("Username") DO UPDATE 
SET "PasswordHash" = EXCLUDED."PasswordHash",
    "Role" = EXCLUDED."Role",
    "IsActive" = EXCLUDED."IsActive";
