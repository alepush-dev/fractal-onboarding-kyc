# 🔄 Análisis de Secuencia - Onboarding Digital (Mini KYC)

A continuación se muestra el diagrama de secuencia del flujo principal de la aplicación:

```mermaid
sequenceDiagram
    autonumber
    actor User as Usuario (Frontend - Angular)
    participant API as API Backend (.NET)
    participant AI as Servicio IA / OCR (Gemini Flash)
    participant DB as Base de Datos (SQL Server)

    User->>API: Envía FormData (Correo + Imagen JPG/PNG)
    
    activate API
    API->>API: Valida formato de archivo y errores
    
    alt Archivo no soportado o inválido
        API-->>User: Retorna HTTP 400 (Bad Request)
    else Archivo válido
        API->>AI: Envía bytes de imagen + Prompt estructurado
        activate AI
        AI-->>API: Retorna JSON (Nombre, Documento, Confianza OCR)
        deactivate AI
        
        API->>DB: Ejecuta Stored Procedure (sp_InsertOnboardingRecord)
        activate DB
        DB-->>API: Confirma inserción exitosa y ID creado
        deactivate DB
        
        API-->>User: Retorna HTTP 200 con datos procesados
    end
    deactivate API
