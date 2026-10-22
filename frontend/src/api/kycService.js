// URL del backend: viene de una variable de entorno (Netlify / .env.local)
const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:51923/api';

// Lee la respuesta como JSON sin romperse si el servidor devuelve otra cosa (por ejemplo, una página HTML de error)
async function leerRespuesta(response) {
    const texto = await response.text();
    try {
        return texto ? JSON.parse(texto) : {};
    } catch {
        return {};
    }
}

export const kycService = {
    async processKyc(formData) {
        const response = await fetch(`${API_BASE_URL}/Kyc/process`, {
            method: 'POST',
            body: formData,
        });

        const data = await leerRespuesta(response);

        if (!response.ok) {
            throw new Error(data.message || data.title || 'Error al procesar el KYC en el servidor.');
        }

        return data;
    },

    async getRecords() {
        const response = await fetch(`${API_BASE_URL}/Kyc/records`);
        const data = await leerRespuesta(response);

        if (!response.ok) {
            throw new Error(data.message || 'Error al obtener el historial de clientes.');
        }

        return data;
    }
};
