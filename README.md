# Proyecto TFC: 2D Action-RPG - Forgotten Tower

Este repositorio contiene el código fuente, la arquitectura de scripts y los archivos ejecutables de la demostración técnica (*Vertical Slice*) desarrollada como **Trabajo de Fin de Ciclo (TFC)**. El proyecto es un videojuego de acción y rol (Action-RPG) en perspectiva top-down ambientado en una mazmorra/torre, diseñado bajo una arquitectura de software modular, escalable y profesional en Unity.

---

## 🚀 Características Técnicas Destacadas

El núcleo del proyecto destaca por la implementación de sistemas desacoplados mediante el patrón de diseño **Singleton** e integración avanzada de componentes nativos de Unity:

### 🎭 1. Sistema de Negociación Moral (Mecánica Core)
Inspirado en RPGs de culto, al reducir la salud de un enemigo normal por debajo del 50%, existe una probabilidad dinámica de pausar el combate para iniciar un evento narrativo y moral justificado por el *lore* del juego (Poder de Nicky).
* **Efecto Máquina de Escribir Optimizado:** Revelación de caracteres sutil utilizando la propiedad `maxVisibleCharacters` de *TextMeshPro*, evitando el renderizado erróneo de etiquetas HTML de color (`<color>`) en tiempo real.
* **Opción Perdonar (Spare):** El enemigo se desvanece mediante un sistema de partículas verdes personalizado (`Particle System` optimizado en orden de capa 2D) y otorga recompensas aleatorias indexadas (70% Poción de Vida / 30% Poción de Fuerza).
* **Opción Eliminar (Kill):** El enemigo entra en estado de furia (*Enrage*), tiñendo su sprite mediante código y otorgando un multiplicador de **puntos triples (x3)** al ser derrotado de forma convencional.

### 🎵 2. Motor de Música Dinámica y Mezcla de Audio
Control total de la inmersión sonora mediante el script `LevelMusicManager` conectado al mezclador global.
* **Transiciones de Combate:** El juego detecta mediante distancias euclidianas si el jugador entra o sale del rango de persecución de los enemigos ordinarios, haciendo un *crossfade* automático entre la música de exploración y la de combate.
* **Música de Jefes:** Al cruzar el umbral de un jefe, el lienzo de interfaz (`BossHealthCanvas`) se activa sincronizando instantáneamente su salud al 100% y disparando un hilo musical épico de fondo.
* **Persistencia de Audio:** Sistema de *Sliders* interactivos en el menú de opciones que se conectan dinámicamente con el mánager inmortal (`DontDestroyOnLoad`), guardando las preferencias del usuario en el registro local mediante `PlayerPrefs`.

### 🎬 3. Sistema Cinematográfico Intermedio
Flujo de juego profesional mediante una escena de transición (`CinematicController`) intercalada entre el Menú Principal y el nivel jugable.
* **Lógica de Selección:** La cinemática guiada por un archivo de video en alta definición (`.mov`) se reproduce **únicamente** si el jugador selecciona "Nueva Partida" (borrando de forma segura los archivos de guardado físicos `.json` previos). Si se selecciona "Continuar", el sistema salta directamente a la acción.
* **Interrupción Dinámica:** Implementación del nuevo *Input System* para capturar la tecla *Escape*, permitiendo el salto instantáneo del clip y la carga asíncrona del nivel.

### 💡 4. Iluminación Avanzada 2D (URP)
El apartado visual utiliza el potencial del **Universal Render Pipeline** configurado específicamente mediante un asset dedicado (`URP_2D_Config`).
* Configuración de iluminación ambiental oscura a través de un `Global Light 2D` para cimentar la atmósfera.
* Uso de luces focales dinámicas agrupadas para antorchas y efectos mágicos, interactuando directamente con componentes `Shadow Caster 2D` para la proyección de sombras en tiempo real.

---

## 🛠️ Tecnologías y Herramientas Utilizadas

* **Motor Gráfico:** Unity (Configurado con Universal Render Pipeline 2D)
* **Lenguaje de Programación:** C# (.NET Standard)
* **Subsistemas de Unity:**
    * *Input System* (Gestión moderna de periféricos de entrada)
    * *TextMeshPro* (Renderizado de tipografías vectoriales y etiquetas dinámicas)
    * *Video Player* (Decodificación de cinemáticas integradas en el pipeline de renderizado)
* **Control de Versiones:** Git & GitHub

---

## 📁 Estructura del Repositorio

El repositorio se ha organizado siguiendo las directrices oficiales de entrega de proyectos técnicos, manteniendo el código limpio de archivos temporales gracias a un filtrado estricto de `.gitignore`:
