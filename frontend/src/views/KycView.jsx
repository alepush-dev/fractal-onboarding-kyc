import React, { useState } from 'react';
import { kycService } from '../api/kycService';
import logoImage from '../assets/image.png';

export const KycView = () => {
    const [email, setEmail] = useState('');
    const [imageFile, setImageFile] = useState(null);
    const [loading, setLoading] = useState(false);
    const [result, setResult] = useState(null);
    const [error, setError] = useState(null);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!email || !imageFile) {
            setError('Por favor completa el correo y selecciona una imagen.');
            return;
        }

        setLoading(true);
        setError(null);
        setResult(null);

        const formData = new FormData();
        formData.append('Email', email);
        formData.append('ImageFile', imageFile);

        try {
            const data = await kycService.processKyc(formData);
            setResult(data);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };
    return (

        <div className="container-fluid p-5">
            <br />
            <div className="row mb-5">
                <div className="col-12 text-center">
                    <img
                        src={logoImage}
                        alt="Banner Central"
                        className="img-fluid"
                        style={{ width: '200px', height: 'auto', objectFit: 'contain' }}
                    />
                </div>

            </div>
            <br />
            <div className="row g-4">
                <div className="col-lg-6">
                    <div className="card border-0 shadow-sm p-4 h-100">
                        <div className="d-flex align-items-center gap-2 pb-3 border-bottom mb-4">
                            <i className="bi bi-file-earmark-text text-primary fs-5"></i>
                            <h2 className="h5 fw-bold m-0 text-dark">Verifica tu identidad</h2>
                        </div>
                        <p className="text-muted small mb-4">
                            Ingresa tu correo electrónico y carga el documento de identidad para procesarlo.
                        </p>

                        <form onSubmit={handleSubmit} className="d-flex flex-column gap-3">
                            <div>
                                <label className="form-label small fw-semibold text-secondary">Correo electrónico</label>
                                <input
                                    type="email"
                                    className="form-control"
                                    placeholder="usuario@ejemplo.com"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    required
                                />
                            </div>
                            <div>
                                <label className="form-label small fw-semibold text-secondary">Documento de identidad (Foto)</label>
                                <input
                                    type="file"
                                    className="form-control"
                                    accept="image/*"
                                    onChange={(e) => setImageFile(e.target.files[0])}
                                    required
                                />
                            </div>

                            <button
                                type="submit"
                                disabled={loading}
                                className="btn  w-100 mt-2 py-2 fw-semibold"
                                style={{ background: 'rgb(72, 176, 255)' }}
                            >
                                {loading ? 'Procesando...' : 'Verificar y Registrar →'}
                            </button>
                        </form>

                        {error && <div className="alert alert-danger mt-3 py-2 small mb-0">{error}</div>}
                    </div>
                </div>


                <div className="col-lg-6">
                    <div className="card border-0 shadow-sm p-4 h-100">
                        <div className="d-flex align-items-center justify-content-between pb-3 border-bottom mb-4">
                            <div className="d-flex align-items-center gap-2">
                                <i className="bi bi-cpu text-primary fs-5"></i>
                                <h2 className="h5 fw-bold m-0 text-dark">Datos extraídos</h2>
                            </div>
                            <span className="badge bg-light text-secondary border">
                                <i className="bi bi-clock me-1"></i> {result ? 'Procesado' : 'Esperando documento'}
                            </span>
                        </div>

                        {!result ? (
                            <div className="h-100 d-flex flex-column align-items-center justify-content-center text-muted p-5 border border-dashed rounded bg-light" style={{ minHeight: '200px' }}>
                                <i className="bi bi-cloud-arrow-up fs-1 opacity-50 mb-2"></i>
                                <p className="small mb-0">Sube un documento para ver la respuesta del OCR...</p>
                            </div>
                        ) : (
                            <div>
                                <div className="alert alert-success py-2 px-3 small d-flex align-items-center gap-2 mb-3">
                                    <i className="bi bi-check-circle-fill"></i>
                                    <span className="fw-semibold">¡Documento procesado con éxito por la IA!</span>
                                </div>

                                <div className="mb-3 text-center bg-light p-2 rounded border">
                                    <span className="text-muted d-block small mb-2 text-start">Documento escaneado:</span>
                                    <div
                                        className="overflow-hidden rounded border bg-white d-flex align-items-center justify-content-center mx-auto"
                                        style={{ width: '100%', height: '160px' }}
                                    >
                                        <img
                                            src={`${(import.meta.env.VITE_API_URL || 'https://localhost:7222/api').replace(/\/api\/?$/, '')}${result.imageUrl}`}
                                            alt="Documento de identidad"
                                            style={{ width: '100%', height: '100%', objectFit: 'contain' }}
                                        />
                                    </div>
                                </div>

                                <div className="mb-3 small">
                                    <span className="text-muted d-block">Correo registrado:</span>
                                    <span className="font-monospace fw-bold text-dark">{result.email}</span>
                                </div>

                                <div className="bg-light p-3 rounded border">
                                    <h6 className="fw-bold text-secondary small mb-2">Campos extraídos por Gemini Flash:</h6>
                                    <ul className="list-unstyled mb-0 small d-flex flex-column gap-1">
                                        <li><strong>Nombre:</strong> {result.fullName}</li>
                                        <li><strong>N° Documento:</strong> {result.documentNumber}</li>
                                        <li><strong>Confianza OCR:</strong> {(result.confidence * 100).toFixed(0)}%</li>
                                    </ul>
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};
