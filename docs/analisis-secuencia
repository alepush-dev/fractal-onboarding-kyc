#  Diagrama de Secuencia - Onboarding Digital Mini KYC

## Analisis de Secuencia

```mermaid
sequenceDiagram
    autonumber
    
    actor U as Usuario / Cliente
    participant FE as Frontend (React UI)
    participant BE as Backend API (.NET REST)
    participant AI as Servicio IA / OCR (Gemini Flash)
    participant DB as Base de Datos (MySQL)

    Note over U,DB: Fase 1: Interacción de Carga y Validación Inicial
    U->>FE: Ingresa correo y adjunta imagen (JPG/PNG)
    FE->>FE: Valida tamaño y tipo de archivo en cliente
    FE->>BE: HTTP POST multipart/form-data (Email + Image File)

    Note over BE: Fase 2: Control de Errores y Seguridad en Backend
    activate BE
    BE->>BE: Intercepta y valida integridad del archivo
    
    alt Archivo Inválido o Formato No Soportado
        BE-->>FE: HTTP 400 Bad Request (Error descriptivo)
        FE-->>U: Muestra alerta de error al usuario
    else Archivo Válido
        
        Note over BE,AI: Fase 3: Procesamiento Inteligente de Datos (OCR)
        BE->>AI: Envía bytes de imagen + Prompt estructurado (API Key segura)
        activate AI
        AI-->>BE: Retorna JSON estructurado (Nombre, Documento, Confianza %)
        deactivate AI
        
        Note over BE,DB: Fase 4: Persistencia Transaccional por Stored Procedure
        BE->>DB: EXEC sp_InsertOnboardingRecord (Email, FullName, DocumentNumber, OcrConfidence, ImageUrl)
        activate DB
        DB-->>BE: Retorna ID generado + Confirmación con Timestamp
        deactivate DB
        
        BE-->>FE: HTTP 200 OK (Datos de Onboarding procesados exitosamente)
        FE-->>U: Renderiza pantalla de éxito con resultado extraído
    end
    deactivate BE
