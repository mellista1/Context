# 🍽️ Context — Inteligencia contextual para negocios gastronómicos

> Proyecto desarrollado en la **Hackatón organizada por Y-HAT** 🏆

---

## ¿Qué es Context?

**Context** es una plataforma de gestión inteligente orientada a negocios gastronómicos que va más allá del registro de ventas: entiende el *mundo que rodea al negocio* y lo convierte en información accionable.

La premisa es simple: lo que pasa afuera de tu restaurante importa tanto como lo que pasa adentro. Un evento en la plaza, una feria barrial, un feriado que nadie anotó... Context los detecta, los clasifica y te avisa qué esperar — basándose en lo que *ya pasó antes*.

---

## El problema que resolvemos

Los dueños de negocios gastronómicos suelen lidiar con contextos que impactan directamente en sus ventas: eventos locales, tendencias, clima, feriados. Sin embargo, pocas herramientas los ayudan a **anticipar** esos impactos o a aprender de experiencias pasadas.

Context cierra esa brecha.

---

## ¿Cómo funciona?

### 1. Ingesta de fuentes de información
El sistema consume fuentes de noticias y eventos del entorno del negocio (feeds, APIs, datos locales).

### 2. Clasificación inteligente con IA
Un agente clasifica automáticamente cada noticia o evento en dos categorías:

| Tipo | Descripción | Ejemplo |
|------|-------------|---------|
| **No agendable** | Información de contexto general que no genera una acción directa | Tendencias gastronómicas, modas, pronóstico del tiempo |
| **Agendable** | Evento concreto que ocurrirá en una fecha y puede impactar el negocio | Recital en el parque cercano, feria artesanal en la plaza, partido del club del barrio |

### 3. Registro y aprendizaje
Los datos de cada jornada (ventas, productos más pedidos, contexto activo) se almacenan para construir una **memoria histórica del negocio**.

### 4. Recomendaciones y estadísticas
Cuando un tipo de evento se repite, Context lo reconoce y te informa:

- ¿Cuál fue el producto más vendido la última vez que hubo una feria en la plaza?
- ¿Cuánto incrementaron las ventas durante el evento anterior?
- Alertas anticipadas cuando se detecta un evento similar al historial

---

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Frontend | TypeScript + HTML + CSS |
| Backend | C# (.NET) |
| Infraestructura | Docker Compose |
| IA | Agente clasificador con Anthropic API |

---

## Estructura del repositorio

```
Context/
├── backend/          # API y lógica de negocio (.NET)
├── frontend/         # Interfaz de usuario (TypeScript)
├── docker-compose.yml
└── setup.md          # Instrucciones de instalación
```

---

## Diferencial frente a otras soluciones

La mayoría de los sistemas de gestión gastronómica registran lo que *ya ocurrió*. **Context registra el porqué** — y lo usa para prepararte para lo que viene.

No se trata solo de analítica. Se trata de **memoria contextual aplicada al negocio**.

---

## Equipo

Proyecto desarrollado en el marco de la **Hackatón Y-HAT** por un equipo de desarrolladores apasionados por la IA aplicada y los negocios reales.

---
<img src="docs/imagenes/Screenshot from 2026-06-11 22-36-14.png" alt="Dashboard principal" width="700"/>
<img src="docs/imagenes/Screenshot from 2026-06-11 22-36-23.png" alt="Dashboard principal" width="700"/>
<img src="docs/imagenes/Screenshot from 2026-06-11 22-38-04.png" alt="Dashboard principal" width="700"/>
<img src="docs/imagenes/Screenshot from 2026-06-11 22-58-32.png" alt="Dashboard principal" width="700"/>
<img src="docs/imagenes/Screenshot from 2026-06-11 22-59-17.png" alt="Dashboard principal" width="700"/>
<img src="docs/imagenes/Screenshot from 2026-06-11 23-25-40.png" alt="Dashboard principal" width="700"/>
<img src="docs/imagenes/Screenshot from 2026-06-11 23-25-43.png" alt="Dashboard principal" width="700"/>
