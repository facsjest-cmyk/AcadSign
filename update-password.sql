UPDATE "AppUsers" 
SET "PasswordHash" = '$2a$12$sHh0AKyEwCW3clJPcUk.B.o.KFoCZwdUTQLaNa1tJYbCP0x06ntmW' 
WHERE "Username" = 'superuser';

SELECT "Username", "Email", "Role", "IsActive" 
FROM "AppUsers" 
WHERE "Username" = 'superuser';
