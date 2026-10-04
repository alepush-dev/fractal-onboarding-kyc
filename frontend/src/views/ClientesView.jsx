import React, { useEffect, useState } from 'react';
import { kycService } from '../api/kycService';

export const ClientesView = () => {
    const [records, setRecords] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchRecords = async () => {
            try {
                const data = await kycService.getRecords();
                setRecords(data);
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchRecords();
    }, []);

    return (
        <div className="container-fluid p-4">
            <div className="card border-0 shadow-sm p-4">
                <div className="d-flex align-items-center justify-content-between pb-3 border-bottom mb-4">
                    <div>
                        <h2 className="h5 fw-bold m-0 text-dark">Listado de Clientes Registrados</h2>
                        <p className="text-muted small mb-0">Historial de verificaciones KYC procesadas por IA.</p>
                    </div>
                    <span className="badge bg-primary">Total: {records.length}</span>
                </div>

                {error && <div className="alert alert-danger py-2 small">{error}</div>}

                <div className="table-responsive">
                    <table className="table table-hover align-middle">
                        <thead className="table-light text-secondary small">
                            <tr>
                                <th>Correo</th>
                                <th>Nombre Completo</th>
                                <th>N° Documento</th>
                                <th>Confianza OCR</th>
                                <th>Fecha</th>
                            </tr>
                        </thead>
                        <tbody className="small text-muted">
                            {loading ? (
                                <tr>
                                    <td colSpan="5" className="text-center py-4">
                                        <div className="spinner-border spinner-border-sm text-primary me-2" role="status"></div>
                                        Cargando registros...
                                    </td>
                                </tr>
                            ) : records.length === 0 ? (
                                <tr>
                                    <td colSpan="5" className="text-center py-4">
                                        No hay clientes registrados todavía. Realiza un proceso en la pestaña de Inicio.
                                    </td>
                                </tr>
                            ) : (
                                records.map((record, index) => (
                                    <tr key={index}>
                                        <td className="fw-semibold text-dark">{record.Email || record.email}</td>
                                        <td>{record.FullName || record.fullName}</td>
                                        <td><span className="font-monospace">{record.DocumentNumber || record.documentNumber}</span></td>
                                        <td>
                                            <span className="badge bg-success-subtle text-success border border-success-subtle">
                                                {record.Confidence || record.confidence}%
                                            </span>
                                        </td>
                                        <td>{new Date(record.CreatedAt || record.createdAt).toLocaleString()}</td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
};