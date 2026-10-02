import React from 'react';

export const Navbar = () => {
    return (
        <nav className="navbar navbar-expand border-bottom px-4 shadow-sm" style={{ height: '60px', background: '#FFB800' }}>
            <div className="container-fluid p-0">
                <div className="d-flex align-items-center">
                    <span className="navbar-brand fw-bold m-0 fs-4" style={{color: 'rgb(72, 176, 255)'}}>FRACTAL</span>
                    <span className="text-muted ms-3 ps-3 border-start small">Onboarding Digital - KYC</span>
                </div>
                <div className="d-flex align-items-center gap-2">
                    <span className="text-secondary small fw-medium">Operador</span>
                    <span className="badge bg-primary px-2 py-1">OP</span>
                </div>
            </div>
        </nav>
    );
};