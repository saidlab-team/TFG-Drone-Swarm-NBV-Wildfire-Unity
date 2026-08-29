using UnityEngine;

public class NextBestViewScript : MonoBehaviour
{





    
       
}
// CELDAS QUE ESTA VIENDO 1 DRON
/*
    Vector3 camPoint = camera.WorldToViewportPoint(worldPos); está dentro del frustrumn si esta entre (0,0) (inferior izq) y (1,1) (superiror dcha)

    celdas de 20 en 20 en X Z
    centros en 10+ inicio celda (si celda es (0,20) (20,0) centro esta en (10,10)) -> Basicamente en los impares (10,10) (30,30) . . .

    Inicializo una grid local para cada dron -> con los indices obtengo los centros de las celdas -> usando WorldToViewportPoint veo cuales está viendo la camara y los almaceno
    Mientras 
        Comparo lo que esta viendo con el estado de la celda correspondiente via GRID de FIRESPREAD (si es null --> normal)
        SI NO ENCUENTRA NADA
            Genera 4 posiciones aleatorias (centros de celda) a una distancia x del dron
            Con la informacion de la grid anterior mira los 8 vecinos de cada una de las posiciones
            Se mueve hacia la mas prometedora, si todas son igual de malas, la elige de forma aleatoria (??)
        SI ENCUENTRA ALGO (FUEGO o CONSUMIDO o PRECALENTADO) (una vez ha encontrado algo, siempre va por esta rama)
            Rota sobre SÍ mismo (0 90 180 270) para obtener la mejor orientacion
            Se mueve hacia la mejor posicion


*/
