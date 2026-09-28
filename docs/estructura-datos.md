# Estructura de Datos y Modelo Relacional (MySQL)

Para cumplir con los requerimientos de persistencia del proyecto Mini KYC, se ha diseñado una estructura relacional optimizada en MySQL.

## 1. Modelo de Datos (Tabla: `ClientsKyc`)
La entidad principal almacena la trazabilidad de cada registro procesado por el sistema:

- `Id` (INT, Primary Key, Auto_Increment) — Identificador único autoincremental del registro.
- `Email` (VARCHAR(150), Not Null) — Correo electrónico proporcionado por el usuario.
- `FullName` (VARCHAR(250), Nullable) — Nombre completo extraído mediante la IA / OCR.
- `DocumentNumber` (VARCHAR(50), Nullable) — Número de documento de identidad extraído.
- `OcrConfidence` (DECIMAL(5,2), Nullable) — Nivel de confianza devuelto por el motor de IA.
- `ImageUrl` (TEXT, Nullable) — URL o ruta de almacenamiento de la imagen adjunta.
- `CreatedAt` (DATETIME, Default = CURRENT_TIMESTAMP) — Marca de tiempo exacta de la creación del registro.

### Script de Creación de la Tabla:
```sql
CREATE TABLE ClientsKyc (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(150) NOT NULL,
    FullName VARCHAR(250) NULL,
    DocumentNumber VARCHAR(50) NULL,
    OcrConfidence DECIMAL(5,2) NULL,
    ImageUrl TEXT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
