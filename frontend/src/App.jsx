import { useState } from 'react';
import { Navbar } from './components/Navbar';
import { Sidebar } from './components/Sidebar';
import { KycView } from './views/KycView';
import { ClientesView } from './views/ClientesView';

function App() {
  const [currentView, setCurrentView] = useState('inicio');

  const renderView = () => {
    switch (currentView) {
      case 'clientes':
        return <ClientesView />;
      case 'inicio':
      default:
        return <KycView />;
    }
  };

  return (
    <div className="min-vh-100 bg-light d-flex flex-column">
      <Navbar />
      {/* En celular: menú arriba y contenido abajo. Desde md: menú a la izquierda */}
      <div className="d-flex flex-column flex-md-row flex-grow-1" style={{ minHeight: 'calc(100vh - 60px)' }}>
        <Sidebar currentView={currentView} setCurrentView={setCurrentView} />
        <main className="flex-grow-1 overflow-y-auto bg-light">
          {renderView()}
        </main>
      </div>
    </div>
  );
}

export default App;
