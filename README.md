Proyecto TFC: 2D Action-RPG - Forgotten Tower
Este repositorio contiene el código fuente, la arquitectura de scripts y los archivos ejecutables de la demostración técnica (Vertical Slice) desarrollada como Trabajo de Fin de Ciclo (TFC). El proyecto es un videojuego de acción y rol (Action-RPG) en perspectiva top-down ambientado en una mazmorra, diseñado bajo una arquitectura de software modular, escalable y profesional en Unity.

🚀 Características Técnicas Destacadas
El núcleo del proyecto destaca por la implementación de sistemas desacoplados mediante el patrón de diseño Singleton y la integración avanzada de componentes nativos de Unity.

🎭 1. Sistema de Negociación Moral (Mecánica Core)
Inspirado en RPGs de culto, al reducir la salud de un enemigo normal por debajo del 50%, existe una probabilidad dinámica de pausar el combate para iniciar un evento narrativo y moral.

Efecto Máquina de Escribir: Revelación de caracteres optimizada utilizando la propiedad maxVisibleCharacters de TextMeshPro, evitando el renderizado erróneo de etiquetas de color (<color>) en tiempo real.

Opción Perdonar (Spare): El enemigo se desvanece mediante un sistema de partículas personalizado y otorga recompensas aleatorias indexadas (70% Poción de Vida / 30% Poción de Fuerza).

Opción Eliminar (Kill): El enemigo entra en estado de furia (Enrage), tiñendo su sprite mediante código y otorgando un multiplicador de puntos x3 al ser derrotado.

🎒 2. Gestión Avanzada de Inventario (Drag & Drop)
Un sistema completo de interfaz de usuario (UI) interactiva basado en físicas de la interfaz (EventSystems).

Lógica de Casillas: Permite arrastrar objetos libremente, apilar consumibles idénticos respetando los límites de carga (maxStackSize), e intercambiar posiciones entre la mochila y la barra de acceso rápido (Hotbar).

División de Montones: Implementación de interacciones secundarias (Clic derecho) para dividir conjuntos de objetos de forma matemática.

Sistema de Descarte (Raycast): Al arrastrar un objeto fuera de los límites de la interfaz, el sistema calcula una trayectoria hacia el cursor, comprueba colisiones con paredes mediante Physics2D.RaycastAll y genera los modelos físicos en el mundo 2D con un efecto de rebote procedural.

💾 3. Persistencia de Datos y Serialización
Arquitectura dual para el guardado de la partida, garantizando la seguridad de los archivos del usuario.

Progreso Físico (JSON): Serialización profunda del estado del mundo (inventario exacto, misiones activas, cofres abiertos y enemigos derrotados) escribiendo un archivo saveData.json en la ruta segura del sistema (Application.persistentDataPath).

Preferencias Técnicas (PlayerPrefs): Guardado asíncrono de los volúmenes de audio y del estado de finalización del tutorial (Onboarding guiado por eventos).

🎵 4. Motor de Música Dinámica y Mezcla de Audio
Control total de la inmersión sonora mediante un controlador global conectado al mezclador nativo.

Transiciones de Combate: Detección de rangos de persecución mediante distancias euclidianas para realizar crossfades matemáticos entre pistas de exploración y tensión.

Eventos de Jefes Finales: Al cruzar el umbral del jefe, el lienzo de interfaz (BossHealthCanvas) sincroniza su salud al 100% y dispara la banda sonora épica.

🎬 5. Cinemáticas e Iluminación URP 2D
Apartado visual impulsado por el Universal Render Pipeline y el control de flujo entre escenas.

Iluminación Dinámica: Uso de Global Light 2D para la atmósfera base, complementado con luces focales dinámicas interactuando con Shadow Caster 2D para proyectar sombras en tiempo real.

Cinemática Interrumpible: Reproducción de video en alta definición (.mov) exclusiva para "Nueva Partida", con capacidad de interrupción mediante el nuevo Input System para realizar una carga asíncrona del nivel.

🛠️ Tecnologías y Herramientas Utilizadas
Motor Gráfico: Unity (Universal Render Pipeline 2D).

Lenguaje de Programación: C# (.NET Standard).

Arquitectura de UI: Unity UI (Canvas, CanvasGroup, EventTrigger) y TextMeshPro.

Control de Entradas: Unity New Input System (Gestión moderna de periféricos).

Persistencia: C# System.IO (Manejo de archivos) y JSONUtility.

Control de Versiones: Git y GitHub.

📁 Estructura del Repositorio
El proyecto mantiene una estructura limpia de archivos temporales mediante un filtro estricto de .gitignore, focalizándose en el código fuente.

/Assets/Scripts/: Contiene la lógica íntegra del juego (Controladores, Managers, ScriptableObjects, etc.).

/Build/ (si aplica): Archivo ejecutable final .exe para su testeo en entornos Windows.

/Documentación/: Manuales de usuario y memoria técnica del proyecto.

Desarrollado como Trabajo de Fin de Ciclo (TFC) - Desarrollo de Aplicaciones Multiplataforma.
