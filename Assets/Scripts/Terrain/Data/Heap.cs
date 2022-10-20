using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heap {

    private int NoE;
    private List<heapStructure> listOfElements = new List<heapStructure>();

    private heapStructure tempElement;

    public Heap() {
        tempElement.value = 0;
        tempElement.coordinates = Vector2Int.zero;

        listOfElements.Add(tempElement);
        NoE = 1;
    }

    public void Add(int val, Vector2Int coord) {
        heapStructure element;
        
        element.value = val;
        element.coordinates = coord;

        listOfElements.Add(element);

        int currentPos = NoE;
        NoE++;

        while (currentPos > 1) {
            if (listOfElements[currentPos].value < listOfElements[currentPos / 2].value) {
                swapElement(currentPos, currentPos / 2);

                currentPos /= 2;
            } else break;
        }
    }

    public void RemoveTop() {
        if (Empty()) {
            Debug.Log("ERROR");
            return;
        }

        swapElement(1, NoE - 1);

        listOfElements.RemoveAt(NoE - 1);
        NoE--;

        int currentPos = 1, nextPos;
        heapStructure element = listOfElements[1];

        while(2 * currentPos < NoE) {

            tempElement = listOfElements[2 * currentPos];
            nextPos = 2 * currentPos;

            if (nextPos + 1 < NoE) {
                if (listOfElements[nextPos].value > listOfElements[nextPos + 1].value) {
                    tempElement = listOfElements[nextPos + 1];
                    nextPos++;
                }
            }

            if (element.value > tempElement.value) {
                swapElement(currentPos, nextPos);
                currentPos = nextPos;
            } else break;
        }
    }

    public bool Empty() {
        return (NoE == 1);
    }

    public heapStructure Top() {
        if (Empty()) {
            Debug.Log("ERROR");
            return tempElement;
        }

        return listOfElements[1];
    }

    private void swapElement(int x, int y) {
        tempElement = listOfElements[x];
        listOfElements[x] = listOfElements[y];
        listOfElements[y] = tempElement;
    }

}

[System.Serializable]

public struct heapStructure {
    public int value;
    public Vector2Int coordinates;
}
