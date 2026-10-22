const menuItems = [
    { id: 'inicio', label: 'Inicio', icon: 'bi-house-door' },
    { id: 'clientes', label: 'Clientes', icon: 'bi-people' },
];

const COLOR_MENU = 'rgb(72, 176, 255)';

export const Sidebar = ({ currentView, setCurrentView }) => {
    const claseBoton = (id) =>
        `btn d-flex align-items-center gap-2 py-2 px-3 rounded-2 fw-medium small border-0 ${currentView === id ? 'bg-white text-primary' : 'text-white bg-transparent'
        }`;

    const botones = menuItems.map((item) => (
        <button key={item.id} onClick={() => setCurrentView(item.id)} className={claseBoton(item.id)}>
            <i className={`${item.icon} fs-6`}></i>
            <span>{item.label}</span>
        </button>
    ));

    return (
        <>
            {/* Escritorio y tablet (md o más): menú lateral */}
            <aside
                className="d-none d-md-flex flex-column justify-content-between p-3 border-end flex-shrink-0"
                style={{ width: '260px', background: COLOR_MENU }}
            >
                <div className="d-flex flex-column gap-1">{botones}</div>

                <div className="pt-3 border-top text-white" style={{ fontSize: '11px' }}>
                    <span className="fw-bold">FRACTAL</span>
                    <p className="mb-0 text-white-50">Tecnología que impulsa tu negocio</p>
                </div>
            </aside>

            {/* Celular: el mismo menú en una barra horizontal */}
            <nav className="d-flex d-md-none gap-2 px-3 py-2 border-bottom" style={{ background: COLOR_MENU }}>
                {botones}
            </nav>
        </>
    );
};