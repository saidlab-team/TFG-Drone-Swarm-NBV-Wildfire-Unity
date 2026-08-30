# Visualización e Interacción Inmersiva con Datos de UAV mediante Tecnologías XR

Simulación de monitorización de incendios forestales dinámicos mediante una flota de drones (UAVs) coordinada con un algoritmo Next-Best-View (NBV), desarrollada en Unity con integración de terreno geoespacial real (Cesium) y compatibilidad con Realidad Virtual (OpenXR).

Trabajo Fin de Grado — Grado en Ingeniería Informática, Universidad Carlos III de Madrid (2025-2026).

<p align="center">
  <img src="docs/images/overview.gif" alt="Vista general del sistema en funcionamiento" width="700"/>
</p>

## Descripción

El sistema simula, sobre un terreno geoespacial fiel a la realidad, la propagación de un incendio forestal mediante un autómata celular 3D, mientras una flota de UAVs coordinada mediante un algoritmo Next-Best-View explora el entorno, detecta el frente de fuego y maximiza la ganancia de información captada, minimizando la redundancia entre los agentes.

## Demo

| <img src="docs/P1.gif" width="240"/> | <img src="docs/images/P2.gif" width="240"/> |

## Tecnologías

- Unity 2022.3 LTS (C#)
- Cesium for Unity (terreno geoespacial)
- OpenXR + XR Interaction Toolkit (compatibilidad VR)
- Unity Input System

## Estructura del repositorio

Localización de los scripts desarrollados:
```
Assets/CustomScripts/
├── Drone/         Controlador de movimiento, cámara y físicas del UAV
├── Environment/   Terreno y configuración de Cesium
├── Events/        Sistema de eventos asíncronos
├── FireSpread/    Autómata celular de propagación del incendio
├── Loader/        Carga inicial y colocación de vegetación
├── Metrics/       Captura y exportación de métricas (.csv)
├── NVB/           Algoritmo Next-Best-View
├── UI/            Interfaz de usuario y HUD
└── Vegetation/    Colocación y ajuste de altura de la vegetación
```

## Requisitos previos

- Unity Hub con Unity 2022.3 LTS o superior
- Git y Git LFS
- Cuenta gratuita en Cesium ion (token de acceso a los 3D Tiles)

## Instalación

```bash
git lfs install
git clone https://github.com/Fran2712/TFG-Drone-Swarm-NBV-Wildfire.git
```

1. Abre el proyecto con Unity Hub.
2. Instala Cesium for Unity desde el Package Manager si no se resuelve automáticamente (`Add package from git URL`: `https://github.com/CesiumGS/cesium-unity.git`).
3. Introduce tu token de Cesium ion en `Cesium > Cesium ion Assets`.
4. Abre la escena principal y pulsa Play.

## Compatibilidad VR

Soporte para visores compatibles con OpenXR, validado mediante el XR Device Simulator de Unity.

## Autor y tutor

- Autor: Francisco Javier Ruiz Joya
- Tutor: João Valente
- Universidad Carlos III de Madrid

## Licencia

Esta obra está sujeta a la licencia Creative Commons Reconocimiento – No Comercial – Sin Obra Derivada 4.0 (CC BY-NC-ND 4.0).
