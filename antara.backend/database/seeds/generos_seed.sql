INSERT INTO "Genero" ("Nombre") VALUES
('Rock'),
('Pop'),
('Hip Hop'),
('Jazz'),
('Electronica'),
('Metal'),
('Clasica'),
('R&B'),
('Reggae'),
('Salsa')
ON CONFLICT ("Nombre") DO NOTHING;

