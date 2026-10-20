# Diagrama de Arquitectura - Onboarding KYC

```mermaid
flowchart LR
    U(["👤 Usuario"])

    subgraph NETLIFY["☁️ Netlify"]
        FE["<b>Frontend</b><br/>React 19 + Vite<br/>Bootstrap<br/>KycView · ClientesView<br/>api/kycService.js"]
    end

    subgraph RAILWAY["☁️ Railway"]
        subgraph API["<b>Backend</b> - ASP.NET Core Web API (.NET 10)"]
            direction TB
            C["<b>Controllers</b><br/>KycController<br/>POST /api/Kyc/process<br/>GET /api/Kyc/records"]
            S["<b>Service</b><br/>IKycService / KycService<br/>Validación · Llamada a IA<br/>Reintentos 429/503 · Parseo JSON"]
            M["<b>Models</b><br/>KycRequest<br/>KycResponseModel"]
            UP[("wwwroot/uploads<br/>Imágenes")]
            C -->|"Inyección de dependencias"| S
            C -.-> M
            S -.-> M
            S -->|"Guarda archivo"| UP
        end
        DB[("<b>MySQL</b><br/>Tabla ClientsKyc<br/>sp_InsertOnboardingRecord")]
    end

    AI["<b>Google Gemini Flash</b><br/>OCR multimodal<br/>REST API"]

    U -->|"Correo + foto del DNI"| FE
    FE -->|"HTTPS · multipart/form-data"| C
    C -->|"JSON (200 / 400 / 500)"| FE
    S -->|"HTTPS · imagen Base64 + prompt<br/>API Key en variable de entorno"| AI
    AI -->|"JSON: FullName, DocumentNumber, Confidence"| S
    C -->|"CALL sp_InsertOnboardingRecord<br/>MySqlConnector"| DB
    S -->|"SELECT historial"| DB
```

## Componentes

| Capa | Tecnología | Responsabilidad |
|---|---|---|
| Frontend | React + Vite (Netlify) | Captura correo e imagen, muestra el resultado y el historial |
| Controller | ASP.NET Core | Recibe la petición HTTP, valida datos obligatorios y devuelve la respuesta |
| Service | C# | Lógica de negocio: guarda la imagen, llama a Gemini, interpreta su respuesta |
| Models | C# | Definen la forma de los datos que entran y salen de la API |
| IA / OCR | Gemini Flash | Extrae nombre, número de documento y nivel de confianza |
| Base de datos | MySQL (Railway) | Persistencia mediante Stored Procedure |
