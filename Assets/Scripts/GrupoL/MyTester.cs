// I M P O R T A C I O N E S //
#region importaciones

using System;
using System.Collections.Generic;//Para almacenar y manipular colecciones de datos
using System.IO;//Para operaciones de entrada y salida de archivos
using System.Linq;
using UnityEngine;
using NavigationDJIA.World;
using QMind.Interfaces;

#endregion

/// <summary>
/// Este código define la clase MyTester que implementa la interfaz IQMind y gestiona la lectura de un archivo CSV (la tablaQ) para almacenar los valores
/// asociados a los estados representados por QState_L. Están programados los métodos para inicializar la instancia, calcular la siguiente acción basada
/// en los valores de la tabla y la posición actual del agente, además de la de lectura del CSV.
/// </summary>

//C L A S E //
#region clase
public class MyTester : IQMind
{
    //V A R I A B L E S //
    #region variables
    QState_L estadoActual;//Estado actual del agente
    Dictionary<QState_L, float[]> TablaQ;//Diccionario que almacena los estados
    WorldInfo world;
    string folder = Application.dataPath + "/Scripts/GrupoL/";//Ruta del directorio donde se encuentra el archivo
    #endregion

    // M É T O D O S //
    #region métodos
    //Calcula y devuelve la siguiente posición del agente basado en su posición actual y la del player
    public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
    {
        //Crea estado actual
        estadoActual = QState_L.createState(currentPosition, otherPosition, world);

        //Obtiene las posibles acciones y sus valores asociados al estado actual desde el diccionario
        float[] possibleActions = TablaQ[estadoActual];

        // Seleccionar acción con mayor valor Q y devuelve la celda correspondiente
        int maxIndex = possibleActions.ToList().IndexOf(possibleActions.Max());

        switch (maxIndex)
        {
            case 0: // norte
                return world[currentPosition.x, currentPosition.y + 1];
            case 1: // este
                return world[currentPosition.x + 1, currentPosition.y];
            case 2: // sur
                return world[currentPosition.x, currentPosition.y - 1];
            default: // oeste
                return world[currentPosition.x - 1, currentPosition.y];
        }
    }

    //Inicializa la instancia de Mytester
    public void Initialize(WorldInfo worldInfo)
    {
        world = worldInfo;
        //Lee el archivo .csv desde el directorio especificado
        TablaQ = readCSV();
    }

    //Lee el archivo .csv y lo convierte en un diccionario
    private Dictionary<QState_L, float[]> readCSV()
    {
        Dictionary<QState_L, float[]> qTable = new Dictionary<QState_L, float[]>();

        using (var reader = new StreamReader(folder + "TablaQ.csv")) //StreamReader lee el archivo línea por línea
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
}
#endregion
