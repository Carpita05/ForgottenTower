# 🏰 Forgotten Tower — 2D Action-RPG (TFC)

Este repositorio contiene el código fuente, la arquitectura de scripts y los archivos base de la demostración técnica (*Vertical Slice*) desarrollada como **Trabajo de Fin de Ciclo (TFC)**. El proyecto consiste en un videojuego de acción y rol (Action-RPG) en perspectiva *top-down* ambientado en una mazmorra medieval, diseñado bajo principios de ingeniería de software modular, patrones desacoplados y sistemas guiados por eventos.

---

## 🚀 Características Técnicas Destacadas

El núcleo del software destaca por la implementación de sistemas modulares mediante el patrón de diseño **Singleton**, la optimización de recursos y la mitigación procedural de errores comunes de colisión e interfaz en Unity.

### 🎭 1. Sistema de Negociación Moral (Mecánica Core)
Inspirado en mecánicas clásicas de RPGs de culto, al reducir la salud de un enemigo por debajo del 50%, se activa de forma probabilística un evento de negociación narrativa justificado por el *lore* (el Poder de Nicky).
* **Efecto Máquina de Escribir (Typewriter):** Revelación dinámica de caracteres utilizando la propiedad `maxVisibleCharacters` de *TextMeshPro*. Este algoritmo evita el renderizado erróneo y los parpadeos visuales al procesar etiquetas complejas de color en formato HTML (`<color>`).
* **Opción Perdonar (Spare):** El enemigo se desvanece de forma segura mediante un sistema de partículas personalizado (`Particle System` ordenado en la capa tipográfica 2D) y otorga recompensas dinámicas aleatorias indexadas en un vector (70% Poción de Vida / 30% Poción de Fuerza).
* **Opción Eliminar (Kill):** El enemigo entra en un estado de furia (*Enrage*). El script tiñe su *sprite* en tiempo real, recalcula su daño base y le asigna un multiplicador de **puntos triples (x3)** al ser derrotado definitivamente.

### 🎒 2. Gestión de Inventario Avanzada e Interacción Gráfica
Un sistema completo de interfaz de usuario interactiva basado en interfaces de la API de eventos nativa (`EventSystems`).
* **Lógica Drag & Drop Fluida:** Control dinámico a través de `IBeginDragHandler`, `IDragHandler` e `IEndDragHandler` para permitir el arrastre, apilado automático respetando los límites de carga (`maxStackSize`), e intercambio de casillas (*Swapping*) entre la mochila y la barra de acceso rápido (*Hotbar*).
* **Mitigación de Errores Críticos (Bugs Solucionados):**
  * **Control de Escala:** Implementación de seguros `rect.localScale = Vector3.one` para neutralizar el redimensionamiento desproporcionado de *sprites* al instanciarse en el Canvas.
  * **Doble Colisión en un Frame:** Inclusión de un cerrojo lógico (`isPickedUp`) para bloquear el objeto en la milésima de segundo en la que es recogido, anulando la duplicación exponencial de objetos provocada por colisiones múltiples concurrentes.
  * **Sistema Anti-Traspaso de Paredes (Raycast):** Al arrastrar y soltar un objeto fuera de los menús, el sistema traza un rayo invisible mediante `Physics2D.RaycastAll`. Si detecta un obstáculo sólido antes de la distancia máxima de descarte (`maxDropDistance`), acorta la trayectoria instantáneamente, evitando que el botín aparezca fuera de los límites del mapa (*Out of Bounds*).

### 💾 3. Persistencia de Datos y Ciclos de Guardado
Arquitectura híbrida diseñada para garantizar que la progresión del usuario final se guarde de forma segura y eficiente.
* **Progresión del Mundo (JSON):** Serialización profunda del estado exacto del jugador, inventario, cofres abiertos, misiones del script de Quests y jefes derrotados, guardando la información físicamente en formato `saveData.json` dentro de la ruta local protegida del sistema operativo (`Application.persistentDataPath`).
* **Configuraciones Técnicas (PlayerPrefs):** Gestión asíncrona e independiente para almacenar los volúmenes master del mezclador de audio y el estado del tutorial (*Onboarding*).

### 🎵 4. Motor de Audio Dinámico e Inmersión Sonora
Controlador de audio inteligente (`LevelMusicManager`) conectado directamente con el mezclador de Unity (*Audio Mixer*).
* **Crossfades Matemáticos:** El juego calcula en tiempo real la distancia euclidiana entre el jugador y las coordenadas de los enemigos activos. Si el jugador entra en zona de peligro, el motor realiza una transición paramétrica atenuando la música de exploración y activando la de combate de forma orgánica.
* **Eventos de Jefes Finales:** Al cruzar el umbral delimitado por una habitación de jefe, el sistema automatiza la activación del lienzo visual (`BossHealthCanvas`), sincroniza la salud y conmuta la banda sonora hacia una pista orquestal exclusiva.

### 🎬 5. Onboarding Guiado por Eventos y Cinemáticas
* **Tutorial Contextual:** El flujo de aprendizaje se ha rediseñado para pasar de una lista lineal a un sistema guiado por eventos independientes distribuidos por el mapa (`TutorialTrigger`). Utiliza el *New Input System* para verificar de forma empírica que el usuario ha asimilado mecánicas específicas (Moverse, Atacar, Interactuar) antes de ocultar los paneles de interfaz.
* **Flujo de Escena Intermedia:** Implementación de un decodificador de video nativo (`Video Player`) para reproducir cinemáticas en alta definición (`.mov`). El script evalúa si el usuario seleccionó "Nueva Partida" (purgando los JSON previos) o "Continuar", permitiendo la interrupción segura por teclado (*Escape*) y activando una carga de escena asíncrona.

---

## 🎮 Controles del Sistema

| Acción | Entrada (Teclado) | Descripción |
| :--- | :---: | :--- |
| **Movimiento** | Flechas de Dirección | Desplazamiento del personaje en el eje 2D. |
| **Atacar** | `Z` | Ejecuta el ataque básico con el arma equipada. |
| **Interactuar** | `C` | Hablar con NPCs / Abrir cofres del entorno. |
| **Abrir Inventario**| `TAB` | Muestra/Oculta la mochila principal. |
| **Barra Rápida** | `1` al `0` | Consumir el objeto asignado al hueco de la Hotbar. |
| **Dividir Montón** | `Clic Derecho` | Divide a la mitad un *stack* en el inventario. |
| **Saltar Cinemática**| `ESC` | Cancela el video introductorio y carga la partida. |

---

## 🛠️ Tecnologías y Subsistemas Utilizados

* **Motor de Desarrollo:** Unity 2022.3 LTS
* **Canal de Renderizado:** Universal Render Pipeline (URP 2D Config)
* **Lenguaje:** C# (.NET Standard)
* **APIs & Componentes:**
  * **New Input System:** Gestión moderna, reactiva y mapeable de periféricos de entrada.
  * **TextMeshPro:** Renderizado de fuentes mediante campos de distancia firmados (SDF).
  * **Unity UI (UGUI):** Arquitectura interactiva de menús (`CanvasGroup`, `RaycastAll`).
  * **Físicas Unity 2D:** Manejo dinámico de fuerzas, `Physics2D.Raycast` y `PolygonCollider2D` como confinar de cámaras cinemáticas (*Cinemachine*).

---

## 📁 Estructura del Repositorio

El proyecto mantiene una distribución limpia siguiendo los estándares de desarrollo profesionales y eliminando archivos temporales o de caché mediante un filtrado estricto en `.gitignore`.

```text
├── Assets/
│   ├── Prefabs/          # Moldes maestros de objetos del mundo e interfaz de usuario
│   ├── Sprites/          # Archivos de arte y hojas de sprites pixel art (UI / Entorno)
│   └── Scripts/          # Arquitectura de código modular en C#
│       ├── Core/         # Managers globales (SaveController, TutorialManager, etc.)
│       ├── Inventory/    # Lógica del inventario (InventoryController, ItemDragHandler, Slot)
│       └── Items/        # Scripts hijos y heredados (Item, HealthPotionItem, StrengthPotionItem)
├── Build/                # Ejecutable optimizado .exe listo para pruebas en Windows
└── Documentación/        # Memoria técnica del TFC y manual de usuario
