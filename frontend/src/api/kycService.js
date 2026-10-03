const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7222/api';

export const kycService = {
    async processKyc(formData) {
        const url = `${API_BASE_URL}/Kyc/process`;
        const response = await fetch(url, {
            method: 'POST',
            body: formData,
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || data.Error || 'Error al procesar el KYC en el servidor.');
        }

        return data;
    },

    async getRecords() {
        const response = await fetch(`${API_BASE_URL}/Kyc/records`);
        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || 'Error al obtener el historial de clientes.');
        }

        return data;
    }
};