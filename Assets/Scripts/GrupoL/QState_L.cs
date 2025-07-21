// I M P O R T A C I O N E S //
#region importaciones

using System;
using NavigationDJIA.World;

#endregion

/// <summary>
/// Este código define la clase QState_L que proporciona una representación de estado para el agente en el entorno. La clase encapsula información sobre la 
/// posibilidad de realizar movimientos en las diferentes direcciones, la dirección relativa entre agente y player. También se hace el cálculo de la 
/// distancia manhattan y operaciones de comparación y representación de cadenas.
/// </summary>

// C L A S E //
#region clase
public class QState_L
{
    // V A R I A B L E S //
    #region variables
    bool norte, este, sur, oeste; //Posibles direcciones a tomar. True = se puede mover False = no se puede mover
    int direccion; //Dirección en la que se encuentra el agente: N = 0, NE = 1, E = 2, SE = 3, S = 4, SO = 5, O = 6, NO = 7
    int distancia; //Distancia entre el agente y el player.
    #endregion

    //Constructor
    public QState_L(bool n, bool e, bool s, bool w, int dis, int dir)
    {
        norte = n;
        este = e;
        sur = s;
        oeste = w;
        direccion = dir;
        distancia = dis;
    }

    // M É T O D O S //
    #region métodos

    //Creación de nuevo estado basado en la posición del agente, la posición del player y la información del mundo
    public static QState_L createState(CellInfo agent, CellInfo other, WorldInfo world)
    {
        //Cálculo de las posiciones adyacentes al agente (norte, este, sur, oeste) y verificación de si son transitables (Walkable)
        CellInfo norte, este, sur, oeste;
        norte = world[agent.x, agent.y + 1];
        este = world[agent.x + 1, agent.y];
        sur = world[agent.x, agent.y - 1];
        oeste = world[agent.x - 1, agent.y];

        //Distancia manhattan y dirección entre agente y player
        int distance = calcularDistancia(Dstmanhattan(agent, other));
        return new QState_L(norte.Walkable, este.Walkable, sur.Walkable, oeste.Walkable, distance, calcularDireccion(agent, other));
    }

    //Calcula la distancia manhattan entre el agente y el player
    public static float Dstmanhattan(CellInfo agent, CellInfo other)
    {
        float m = MathF.Abs(agent.x - other.x) + MathF.Abs(agent.y - other.y);
        return m;
    }

    //Métodos de acceso a la distancia y la dirección
    public int getDistance()
    {
        return distancia;
    }

    public int getDirection()
    {
        return direccion;
    }

    //Compara si dos estados son iguales
    public bool Equals(QState_L other)
    {
        if (other.norte == norte &&
            other.este == este &&
            other.sur == sur &&
            other.oeste == oeste &&
            other.distancia == distancia) return true;

        return false;
    }

    //Para permitir la comparación con cualquier objeto
    public override bool Equals(object obj)
    {
        return Equals(obj as QState_L);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + norte.GetHashCode();
        hash = hash * 23 + este.GetHashCode();
        hash = hash * 23 + sur.GetHashCode();
        hash = hash * 23 + oeste.GetHashCode();
        hash = hash * 23 + distancia.GetHashCode();
        hash = hash * 23 + direccion.GetHashCode();
        return hash;
    }

    //Representación del estado
    public override string ToString()
    {
        return $"{norte};{este};{sur};{oeste};{distancia};{direccion}";
    }

    //Calcula la dirección del player con respecto al agente
    public static int calcularDireccion(CellInfo agent, CellInfo other)
    {
        // Diferencia en coordenadas
        int dx = other.x - agent.x;
        int dy = other.y - agent.y;

        // Mapeo de direcciones basado en las diferencias de coordenadas
        if (dy > 0)
        {
            //NorEste
            if (dx > 0)
                return 1; 
            //NorOeste
            else if (dx < 0)
                return 7;
            //Norte
            else
                return 0;
        }
        else if (dy < 0)
        {
            //SurEste
            if (dx > 0)
                return 3;
            //SurOeste
            else if (dx < 0)
                return 5;
            //Sur
            else
                return 4;
        }
        else
        {
            //Este
            if (dx > 0)
                return 2;
            //Oeste
            else if (dx < 0)
                return 6;
            else
                return 6; //Para el caso que no está definido en la lógica, se ha tomado la decisión de que se mueva al Oeste
        }
    }

    //Discretización de la distancia manhattan en 4 niveles (0-3)
    public static int calcularDistancia(float manhattan)
    {
        //Muy cerca
        if (manhattan <= 1 && manhattan >= 0)
        {
            return 0;
        }
        //Cerca
        else if (manhattan <= 2 && manhattan > 1)
        {
            return 1;
        }
        //Distancia media
        else if (manhattan <= 3 && manhattan > 2)
        {
            return 2;
        }
        //Lejos
        else
        {
            return 3;
        }
    }
    #endregion
}
#endregion
