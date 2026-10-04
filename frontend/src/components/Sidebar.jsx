import React from 'react';

export const Sidebar = ({ currentView, setCurrentView }) => {
    const menuItems = [
        { id: 'inicio', label: 'Inicio', icon: 'bi-house-door' },
        { id: 'clientes', label: 'Clientes', icon: 'bi-people' },
    ];

    return (
        <div className=" border-end d-flex flex-column justify-content-between p-3" style={{ width: '260px', minHeight: 'calc(100vh - 60px)',background: 'rgb(72, 176, 255)' }}>
            <div className="list-group list-group-flush gap-1">
                {menuItems.map((item) => {
                    const isActive = currentView === item.id;
                    return (
                        <button
                            key={item.id}
                            onClick={() => setCurrentView(item.id)}
                            className={`btn text-white d-flex align-items-center gap-2 py-2 px-3 rounded-2 fw-medium small ${isActive
                                    ? 'bg-primary-subtle text-primary border-0'
                                    : 'text-secondary bg-transparent border-0'
                                }`}>
                            <i className={`${item.icon} fs-6`}></i>
                            <span>{item.label}</span>
                        </button>
                    );
                })}
            </div>

            <div className="pt-3 border-top text-white" style={{ fontSize: '11px' }}>
                <span className="fw-bold text-white">FRACTAL</span>
                <p className="mb-0 text-muted">Tecnología que impulsa tu negocio</p>
            </div>
        </div>
    );
};
