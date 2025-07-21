// I M P O R T A C I O N E S //
#region importaciones

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;

#endregion

/// <summary>
/// Este código implementa el algoritmo Q-learning para entrenar un agente en un entorno. Utiliza una tabla Q (diccionario) para almacenar y actualizar sus 
/// valores Q, determinando las acciones del agente basándose en estrategias para equilibrar entre explorar nuevas acciones o explotar las acciones que han
/// resultado ser efectivas. El entrenamiento involucra la iteración del agente con el entorno y la actualización de los valores se maneja con el uso de 
/// recompensas y penalizaciones y transiciones entre estados.
/// </summary>

//C L A S E //
#region clase
public class MyTrainer : IQMindTrainer
{
    //V A R I A B L E S //
    #region variables
    float alpha, epsilon, gamma;//Parámetros de entrenamiento
    private QState_L estadoActual;//Estado actual
    private string accionActual;//Accion actual
    private bool nuevoEP; //Determina si empieza un nuevo episodio
    private WorldInfo world;
    private INavigationAlgorithm nav;
    string folder = Application.dataPath + "/Scripts/GrupoL/";//Ubicación del archivo
    private float prizeSum, prizeNumber = 0; //Para calcular average reward
    #endregion

    enum posiblesAcciones
    {
        norte,
        este,
        sur,
        oeste,
        none
    }

    Dictionary<QState_L, float[]> QTable; //Diccionario

    //Propiedades para seguir el estado del entrenamiento
    public int CurrentEpisode { get; private set; }
    public int CurrentStep { get; private set; }
    public CellInfo AgentPosition { get; private set; }
    public CellInfo OtherPosition { get; private set; }
    public float Return { get; private set; } //Recompensa
    public float ReturnAveraged { get; private set; } //Recompensa media
    public event EventHandler OnEpisodeStarted;
    public event EventHandler OnEpisodeFinished;

    //Inicializa el entrenamiento
    public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
    {
        nuevoEP = true;
        world = worldInfo;
        nav = navigationAlgorithm;
        nav.Initialize(world);

        //Inicialización de los parámetros de aprendizaje
        alpha = qMindTrainerParams.alpha;
        epsilon = qMindTrainerParams.epsilon;
        gamma = qMindTrainerParams.gamma;

        CurrentEpisode = 1;
        CurrentStep = 1;

        //Inicialización de la tabla Q
        QTable = new Dictionary<QState_L, float[]>();

        //Si ya existe un archivo con el nombre "TablaQ" lo lee y actualiza sobre él. Si aún no existe una tabla con ese nombre, crea una nueva
        if (File.Exists(folder + "TablaQ.csv"))
        {
            //Lee la tabla
            QTable = readCSV();
        }
        else
        {
            //Se crean todos los estados posibles
            bool[,] array =
            {
                    {false, false, false, false},
                    {false, false, false, true},
                    {false, false, true, false},
                    {false, false, true, true},
                    {false, true, false, false},
                    {false, true, false, true},
                    {false, true, true, false},
                    {false, true, true, true},
                    {true, false, false, false},
                    {true, false, false, true},
                    {true, false, true, false},
                    {true, false, true, true},
                    {true, true, false, false},
                    {true, true, false, true},
                    {true, true, true, false},
                    {true, true, true, true},
                };

            //Un total de 4*8*16 = 512 estados
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < 16; k++)
                    {
                        // Determina el valor inicial para cada acción
                        float[] qValues = new float[4];
                        for (int a = 0; a < 4; a++)
                        {
                            qValues[a] = array[k, a] ? 0.0f : -100.0f;//0 si puede -100 si no
                        }

                        // Create QState_L instance and add to QTable
                        QState_L state = new QState_L(array[k, 0], array[k, 1], array[k, 2], array[k, 3], i, j);
                        QTable.Add(state, qValues);
                    }
                }
            }

            writeCSV();
        }
    }

    #region lectura y escritura
    //Elimina el archivo CSV existente y escribe la TablaQ actualizada en un nuevo archivo
    private void writeCSV()
    {
        File.Delete(folder + "TablaQ.csv");
        string csvWrite = string.Join(Environment.NewLine, QTable.Select(d => $"{d.Key};{string.Join(";", d.Value)}"));
        File.WriteAllText(folder + "TablaQ.csv", csvWrite);
    }

    //Lee el archivo CSV y carga los datos en un diccionario
    private Dictionary<QState_L, float[]> readCSV()
    {
        Dictionary<QState_L, float[]> qTable = new Dictionary<QState_L, float[]>();

        using (var reader = new StreamReader(folder + "TablaQ.csv"))//StreamReader lee el archivo línea por línea
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(';');//Separador

                //Convierte las cadenas extraidas en los tipos correspondientes
                bool value1 = Convert.ToBoolean(values[0]);
                bool value2 = Convert.ToBoolean(values[1]);
                bool value3 = Convert.ToBoolean(values[2]);
                bool value4 = Convert.ToBoolean(values[3]);
                int value5 = int.Parse(values[4]);
                int value6 = int.Parse(values[5]);
                float[] qValues = {
                float.Parse(values[6]),
                float.Parse(values[7]),
                float.Parse(values[8]),
                float.Parse(values[9])
            };

                //Crea un objeto con los valores extraídos
                QState_L key = new QState_L(value1, value2, value3, value4, value5, value6);

                //Agrega los pares clave, valor al diccionario
                qTable.Add(key, qValues);
            }
        }
        return qTable;
    }
    #endregion

    //Ejecuta un paso del entrenamiento
    public void DarPaso(bool train)
    {
        if (nuevoEP) //Si se está empezando un nuevo episodio
        {
            estadoActual = estadoRandom(); //Se genera un estado aleatorio
            OnEpisodeStarted?.Invoke(this, null);//Notificar que se inició un episodio
            nuevoEP = false;
        }
        else
        {
            if (estadoFinal())//Si es un estado final
            {
                CurrentEpisode++;//Incrementa el número de pisodio
                CurrentStep = 0;//Reinicia el contador de pasos
                nuevoEP = true; //Empezamos nuevo episodio
                OnEpisodeFinished?.Invoke(this, null);//Notificamos
            }
            else
            {
                RealizarPasoEpisodio();
                CurrentStep++;
            }
        }
    }

    //Pone al agnete y al player en una posición aleatoria pero que sea válida (walkable)
    public QState_L estadoRandom() //Devuelve un estado aleatorio
    {
        do 
        {
            AgentPosition = world.RandomCell();
            OtherPosition = world.RandomCell();
        }
        while (!AgentPosition.Walkable || !OtherPosition.Walkable || AgentPosition == OtherPosition);//Condición de posición válida

        return QState_L.createState(AgentPosition, OtherPosition, world);
    }

   //Realiza un paso dentro de un episodio de ntrenamiento. Elige una acción y actualiza la posición del agente en el mundo.
    public void RealizarPasoEpisodio() 
    {
        int actual;
        accionActual = elegirAccion(estadoActual, epsilon);
        switch (accionActual)
        {
            case "norte":
                AgentPosition = world[AgentPosition.x, AgentPosition.y + 1];
                actual = 0;
                break;   
            case "este":
                AgentPosition = world[AgentPosition.x + 1, AgentPosition.y];
                actual = 1;
                break;
            case "sur":
                AgentPosition = world[AgentPosition.x, AgentPosition.y - 1];
                actual = 2;
                break;
            default:
                AgentPosition = world[AgentPosition.x - 1, AgentPosition.y];
                actual = 3;
                break;
        }

        //Llamada a GetPAth para obtener el camino del enemigo hacia la nueva posición del agente. Si hay un camino (no es nulo y de longitud mayor a 0)
        //actualiza la posición del enemigo
        CellInfo[] enemyPath = nav.GetPath(OtherPosition, AgentPosition, 3);

        if (enemyPath != null && enemyPath.Length > 0) //Comprobación
        {
            OtherPosition = enemyPath[0];
        }

        //Actualización de los valores de la tabla
        float[] toUpdate = QTable[estadoActual];
        if (enemyPath != null && enemyPath.Length > 0)
        {
            toUpdate[actual] = ActualizarValor(estadoActual, accionActual, enemyPath[0]);
            QTable[estadoActual] = toUpdate;
        }

        //Escribe
        writeCSV();

        //Actualiza el estado actual a un nuevo estado
        estadoActual = QState_L.createState(AgentPosition, OtherPosition, world);
    }

    #region recompensas
    //Calcula y actualiza el valor para un estado en una acción específica
    public float ActualizarValor(QState_L state, string action, CellInfo nextEnemyPos)
    {
        CellInfo nextAgentPos;//Posición del agente basado en la acción elegida
        switch (action)
        {
            case "norte":
                nextAgentPos = world[AgentPosition.x, AgentPosition.y + 1];
                break;
            case "este":
                nextAgentPos = world[AgentPosition.x + 1, AgentPosition.y];
                break;
            case "sur":
                nextAgentPos = world[AgentPosition.x, AgentPosition.y - 1];
                break;
            default:
                nextAgentPos = world[AgentPosition.x - 1, AgentPosition.y];
                break;
        }
        //Crea nuevo estado
        QState_L nextState = QState_L.createState(nextAgentPos, nextEnemyPos, world);

        //Se inicializa la recompensa acumulada
        int accumulatedReward = 0;

        // R E C O M P E N S A S //
        //Si le pilla
        if (estadoFinal())
        {
            accumulatedReward -= 40;
        }

        //Si se aleja
        if (state.getDistance() < nextState.getDistance())
        {
            accumulatedReward += 20;
        }

        //Dirección opuesta a player
        if (!mismaDireccion(OtherPosition.x - AgentPosition.x, nextAgentPos.x - AgentPosition.x) ||
            !mismaDireccion(OtherPosition.y - AgentPosition.y, nextAgentPos.y - AgentPosition.y))
        {
            accumulatedReward += 90;
        }

        //Si mantiene la distancia
        if (state.getDistance() == nextState.getDistance())
        {
            accumulatedReward += 10;
        }

        //Si se mueve hacia player
        if (mismaDireccion(OtherPosition.x - AgentPosition.x, nextAgentPos.x - AgentPosition.x) ||
            mismaDireccion(OtherPosition.y - AgentPosition.y, nextAgentPos.y - AgentPosition.y))
        {
            accumulatedReward += 0;
        }

        //Si se acerca el player
        if (state.getDistance() > nextState.getDistance())
        {
            accumulatedReward -= 10;
        }

        //Actualización de variables
        Return = accumulatedReward;
        prizeSum += accumulatedReward;
        prizeNumber++;
        ReturnAveraged = (prizeSum / prizeNumber);

        //Obtiene los valores para el estado actual y la acción específica.
        float[] current = QTable[state];
        int index;
        switch (action)
        {
            case "norte":
                index = 0;
                break;
            case "este":
                index = 1;
                break;
            case "sur":
                index = 2;
                break;
            default:
                index = 3;
                break;
        }

        float[] next = QTable[nextState];
        int maxIndex = next.ToList().IndexOf(next.Max());//Busca el índice máximo

        //Actualiza el valor con la fórmula
        return (1 - alpha) * current[index] + alpha * (accumulatedReward + gamma * next[maxIndex]);
    }
    #endregion

    //Comprobar si tienen el mismo signo. Determina si el personaje se mieve en la dirección del enemigo o no.
    bool mismaDireccion(int first, int second) 
    {
        if (first > 0 && second < 0)
            return false;
        if (first < 0 && second > 0)
            return false;
        return true;
    } 

    //El agente "muere" al avanzar a una posición no válida o si le pilla el player
    public bool estadoFinal()
    {
        if (!AgentPosition.Walkable || AgentPosition == OtherPosition)
        {
            return true;
        }
        return false;
    }
    
    //Elige una acción basada en el estado actual y el valor de exploración e (epsilon)
    public string elegirAccion(QState_L state, float e)
    {
        List<string> acciones = new List<string>(); //Lista de acciones válidas

        //Si lo valores son -100 esa acción no es válida
        float[] qActions = QTable[state]; 
        if (qActions[0] != -100)
        {
            acciones.Add(posiblesAcciones.norte.ToString());
        }
        if (qActions[1] != -100)
        {
            acciones.Add(posiblesAcciones.este.ToString());
        }
        if (qActions[2] != -100)
        {
            acciones.Add(posiblesAcciones.sur.ToString());
        }
        if (qActions[3] != -100)
        {
            acciones.Add(posiblesAcciones.oeste.ToString());
        }
        if (acciones.Count == 0)
        {
            acciones.Add(posiblesAcciones.none.ToString());
        }

        //Generar un número aleatorio entre 0 y 99
        System.Random random = new System.Random();
        int rand = random.Next(100);

        if (rand <= e * 100) //Si es menor o igual a epsilon * 100 elige una acción aleatoria de la lista de acciones válidas
        {
            if (acciones.Count > 1)
            {
                int indiceAleatorio = random.Next(acciones.Count);
                return acciones[indiceAleatorio];
            }
            else
            {
                return acciones[0];
            }
        }

        //Selecciona acción con mayor valor 
        int maxIndex = qActions.ToList().IndexOf(qActions.Max());
        string accionSeleccionada;
        switch (maxIndex)
        {
            case 0:
                accionSeleccionada = posiblesAcciones.norte.ToString();
                break;
            case 1:
                accionSeleccionada = posiblesAcciones.este.ToString();
                break;
            case 2:
                accionSeleccionada = posiblesAcciones.sur.ToString();
                break;
            case 3:
                accionSeleccionada = posiblesAcciones.oeste.ToString();
                break;
            default:
                accionSeleccionada = posiblesAcciones.none.ToString();
                break;
        }
        return accionSeleccionada;
    }
}
#endregion