import React, { useState } from 'react';
import { Navbar } from './components/Navbar';
import { Sidebar } from './components/Sidebar';
import { KycView } from './views/KycView';
import { ClientesView } from './views/ClientesView';

function App() {
  const [currentView, setCurrentView] = useState('inicio');

  const renderView = () => {
    switch (currentView) {
      case 'inicio':
      case 'procesos':
        return <KycView />;
      case 'clientes':
        return <ClientesView />;
      case 'reportes':
      case 'configuracion':
        return (
          <div className="container py-5 text-center text-muted">
            <h3 className="h5 fw-bold mb-1">Sección en construcción</h3>
            <p className="small">Esta vista estará disponible próximamente.</p>
          </div>
        );
      default:
        return <KycView />;
    }
  };

  return (
    <div className="min-vh-100 bg-light d-flex flex-column">
      <Navbar />
      <div className="d-flex flex-grow-1 overflow-hidden" style={{ minHeight: 'calc(100vh - 60px)' }}>
        <Sidebar currentView={currentView} setCurrentView={setCurrentView} />
        <main className="flex-grow-1 overflow-y-auto bg-light">
          {renderView()}
        </main>
      </div>
    </div>
  );
}

export default App;
