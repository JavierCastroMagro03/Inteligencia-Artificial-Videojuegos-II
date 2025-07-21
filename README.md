# 🧠 Inteligencia-Artificial-Videojuegos

**Repositorio de la asignatura de Inteligencia Artificial del Grado en Diseño y Desarrollo de Videojuegos.**  
Incluye los apuntes de clase y la práctica del segundo bloque realizados durante el curso, también se incluye su respectiva memoria explicativa.

---

## Resumen del temario

La asignatura se ha dividido en dos bloques de temario. A continuación, se desglosa el contenido del segundo bloque:

### 🔹 Bloque II – Aprendizaje automático y técnicas avanzadas

1. **Aprendizaje automático**  
   Introducción al aprendizaje supervisado, no supervisado y por refuerzo.

2. **Redes neuronales**  
   Desde el perceptrón hasta redes multicapa (MLP), backpropagation y entrenamiento de modelos.

3. **Árboles de decisión**  
   Algoritmos ID3 e ID4, entropía, ganancia de información y aplicaciones prácticas.

4. **Predicción con n-gramas**  
   Modelado de secuencias para predecir acciones del jugador o del entorno.

5. **Aprendizaje por refuerzo**  
   Fundamentos del Q-learning y su aplicación en videojuegos.

---

## Práctica Bloque II

Este repositorio contiene la segunda práctica desarrollada durante la asignatura.

Para una mejor comprensión de la implementación, se recomienda consultar la **memoria explicativa** disponible en PDF.

---

## 📋 Enunciado original de la Práctica 2

**Práctica 2: Machine Learning** se centró en implementar un agente inteligente que aprende a través de técnicas de aprendizaje por refuerzo, específicamente **Q-Learning**, dentro de un entorno virtual desarrollado con Unity™.

El objetivo principal era entrenar al agente (`Agent`) para **escapar de un oponente** (`Player`) que se desplaza hacia él usando A*. El entrenamiento se realiza en una escena específica (`TrainPlayGround`aunque creé escenarios extra en los que evaluar) mediante una implementación personalizada de la interfaz `IQMindTrainer`. El comportamiento aprendido se evalúa en una segunda escena (`TestPlayGround` aunque personalmente desarrollé escenarios diferentes en los que entrenar) con condiciones aleatorias.

### Objetivos específicos:

- **Entrenamiento del agente**  
  Programar desde cero un sistema de entrenamiento basado en Q-Learning que:
  - Genere una tabla Q en formato `.csv`.
  - Se pueda regenerar y validar automáticamente.
  - Enseñe al agente a evitar ser atrapado.

- **Evaluación del comportamiento aprendido**  
  Medir el número promedio de pasos que el agente puede moverse sin ser atrapado por el oponente, en 10 ejecuciones de prueba. La nota final depende de este promedio:

  | Promedio de pasos | Calificación |
  |--------------------|--------------|
  | 250–1000           | 6            |
  | 1000–5000          | 8            |
  | > 5000             | 10           |

La nota obtenida fue de `9/10`

---
