﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;


public class AlgoritmoMcts
{
    //public GameManager gameManager;
    /*public GameObject playerIA;*/

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }


    public Ficha Mcts(List<Ficha> hand, GameManager gameManager)
    {
        NodoMCTS root = new NodoMCTS
        {
            fieldTokens = gameManager.GetFichasMCTS(),
            leftSideValue = gameManager.leftSideValue,
            rightSideValue = gameManager.rightSideValue
        };

        if (root.hand == null)
        {
            root.hand = new List<Ficha>();
        }

        root.hand.AddRange(hand);

        var list = jugadasLegales(root, root.hand);

        foreach (var play in list) // Añado cada una de las jugadas posibles al árbol, como hijos de la raíz
        {
            NodoMCTS son = new NodoMCTS
            {
                selectedToken = play,
                wins = 0,
                timesVisited = 0,
                parent = root
            };
            if (root.children == null)
            {
                root.children = new List<NodoMCTS>();
            }

            root.children.Add(son);
        }

        Debug.Log(JsonUtility.ToJson(root));
        for (int i = 0; i < 100; i++)
        {
            NodoMCTS current = Selection(root); //este current ya es la jugada posible con mejor UCT seleccionada

            NodoMCTS prometedor = Expansion(current);

            //      Simulación y Retropropagación
            current.wins += Simulacion(prometedor, gameManager);
            current.timesVisited++;
            //current = current.parent;             Preguntar esto a rafael, lo hacen en otros ejemplos de codigo
        }

        var visits = 0;
        Ficha move = new Ficha();
        foreach (var finalMove in root.children)
        {
            if (finalMove.timesVisited > visits)
            {
                visits = finalMove.timesVisited;
                move = finalMove.selectedToken;
            }
        }

        return move; // Se toma como mejor jugada la de mayor robustez, o sea, la más visitada.
        //return Selection(root).selectedToken;
    }


    public NodoMCTS Selection(NodoMCTS nodo)
    {
        NodoMCTS resultado = new NodoMCTS();
        double valorUCT = 0;

        foreach (var item in nodo.children)
        {
            double tasaExito = item.timesVisited > 0 ? item.wins / item.timesVisited : 0;
            double valorAux = tasaExito + Math.Sqrt(Math.Log(item.parent.timesVisited) / item.timesVisited);
            if (valorAux > valorUCT)
            {
                valorUCT = valorAux;
                resultado = item;
            }
        }

        if (resultado == null)
        {
            if (nodo.children.Any())
            {
                return nodo.getRandomChild();
            }

            return nodo;
        }

        return resultado;
    }

    public NodoMCTS Expansion(NodoMCTS nodoMcts)
    {
        Debug.Log(JsonUtility.ToJson(nodoMcts));
        NodoMCTS newNode = new NodoMCTS
        {
            parent = nodoMcts,
            fieldTokens = nodoMcts.fieldTokens
        };
        Debug.Log(nodoMcts);
        //si coloco la ficha a la izquierda y por la izquierda de la ficha
        Debug.Log(nodoMcts.leftSideValue);
        Debug.Log(nodoMcts.selectedToken.leftValue);
        if (nodoMcts.leftSideValue == nodoMcts.selectedToken.leftValue)
        {
            var newList = new List<Ficha>();
            newList.Add(nodoMcts.selectedToken);
            newList.AddRange(newNode.fieldTokens);
            newNode.fieldTokens = newList;
            newNode.leftSideValue = nodoMcts.selectedToken.rightValue;
            newNode.rightSideValue =
                newNode.fieldTokens[newNode.fieldTokens.Count - 1].rightValue;
        }
        //si coloco la ficha a la izquierda y por la derecha de la ficha
        else if (nodoMcts.leftSideValue == nodoMcts.selectedToken.rightValue)
        {
            var newList = new List<Ficha>();
            newList.Add(nodoMcts.selectedToken);
            newList.AddRange(newNode.fieldTokens);
            newNode.fieldTokens = newList;
            newNode.leftSideValue = nodoMcts.selectedToken.leftValue;
            newNode.rightSideValue =
                newNode.fieldTokens[newNode.fieldTokens.Count - 1].rightValue;
            //newNode.rightSideValue = nodoMcts.rightSideValue;
        }
        //si coloco la ficha a la derecha y x la izquierda de la ficha
        else if (nodoMcts.rightSideValue == nodoMcts.selectedToken.leftValue)
        {
            newNode.fieldTokens.Add(nodoMcts.selectedToken);
            newNode.leftSideValue = nodoMcts.fieldTokens[0].leftValue; // o de la siguiente forma
            //newNode.leftSideValue = nodoMcts.leftSideValue;
            newNode.rightSideValue =
                newNode.fieldTokens[newNode.fieldTokens.Count - 1].rightValue;
        }
        //si coloco la ficha a la derecha y por la derecha de la ficha
        else if (nodoMcts.rightSideValue == nodoMcts.selectedToken.rightValue)
        {
            newNode.fieldTokens.Add(nodoMcts.selectedToken);
            newNode.leftSideValue = nodoMcts.fieldTokens[0].leftValue; // o de la siguiente forma
            //newNode.leftSideValue = nodoMcts.leftSideValue;
            newNode.rightSideValue = newNode.fieldTokens[newNode.fieldTokens.Count - 1].leftValue;
        }

        /*newNode.fieldTokens.Add(nodoMcts.selectedToken);
        newNode.leftSideValue = newNode.fieldTokens[0].GetComponent<Ficha>().leftValue;
        newNode.rightSideValue = newNode.fieldTokens[newNode.fieldTokens.Count - 1].GetComponent<Ficha>().rightValue;*/

        nodoMcts.timesVisited++;
        newNode.timesVisited = 1;
        /*List<GameObject> jugadasLegales = new List<GameObject>();
            
        foreach (var ficha in newNode.hand)
        {
            if (ficha.GetComponent<Ficha>().leftValue==nodoMcts.leftSideValue || ficha.GetComponent<Ficha>().leftValue == nodoMcts.rightSideValue)
            {
                jugadasLegales.Add(ficha);
            }
            else if (ficha.GetComponent<Ficha>().rightValue == nodoMcts.leftSideValue || ficha.GetComponent<Ficha>().rightValue == nodoMcts.rightSideValue)
            {
                jugadasLegales.Add(ficha);
            }
        }

        if (jugadasLegales.Any())
        {
            var index = Random.Range(0, jugadasLegales.Count - 1);
            newNode.selectedToken = jugadasLegales[index];
        }
        else
            newNode.selectedToken = null;*/
        return newNode;
    }


    public int Simulacion(NodoMCTS nodoPrometedor, GameManager gameManager)
    {
        Debug.Log(JsonUtility.ToJson(nodoPrometedor));
        int fichasOponente = 20 - (nodoPrometedor.hand.Count + nodoPrometedor.fieldTokens.Count);

        List<Ficha> tokenDisponibles = gameManager.GetFichasDisponibles();

        List<Ficha> tokensIA = nodoPrometedor.hand;

        while (fichasOponente > 0 && tokensIA.Count > 0)
        {
            List<Ficha> legalPlays = jugadasLegales(nodoPrometedor, tokenDisponibles);
            if (legalPlays.Any())
            {
                var index = Random.Range(0, legalPlays.Count - 1);
                nodoPrometedor.selectedToken = legalPlays[index];
                tokenDisponibles.Remove(legalPlays[index]);
                PlaceTokenAux(nodoPrometedor, nodoPrometedor.selectedToken);
                fichasOponente--;
                if (fichasOponente == 0)
                {
                    return -1; //gana humano
                }
            }

            List<Ficha> legalPlaysIA = jugadasLegales(nodoPrometedor, tokensIA);
            if (legalPlaysIA.Any())
            {
                var index = Random.Range(0, legalPlaysIA.Count - 1);
                nodoPrometedor.selectedToken = legalPlaysIA[index];
                tokensIA.Remove(legalPlaysIA[index]);
                PlaceTokenAux(nodoPrometedor, legalPlaysIA[index]);
                if (tokensIA.Count == 0)
                {
                    return 1; // gana la IA
                }
            }

            if (!legalPlays.Any() && !legalPlaysIA.Any())
            {
                if (tokensIA.Count < fichasOponente)
                {
                    return 1;
                }

                if (tokensIA.Count > fichasOponente)
                {
                    return -1;
                }

                return 0;
            }
        }

        return 0; //empate
    }

    public void RetroPropagación()
    {
    }


    public List<Ficha> jugadasLegales(NodoMCTS nodo, List<Ficha> tokenDisponibles)
    {
        List<Ficha> jugadasLegales1 = new List<Ficha>();

        foreach (var ficha in tokenDisponibles)
        {
            if (ficha.leftValue == nodo.leftSideValue ||
                ficha.leftValue == nodo.rightSideValue)
            {
                jugadasLegales1.Add(ficha);
            }

            else if (ficha.rightValue == nodo.leftSideValue ||
                     ficha.rightValue == nodo.rightSideValue)
            {
                jugadasLegales1.Add(ficha);
            }
        }

        return jugadasLegales1;
    }

    public void PlaceTokenAux(NodoMCTS nodo, Ficha token)
    {
        //si coloco la ficha a la izquierda y por la izquierda de la ficha
        if (nodo.leftSideValue == token.leftValue)
        {
            var newList = new List<Ficha>();
            newList.Add(token);
            newList.AddRange(nodo.fieldTokens);
            nodo.fieldTokens = newList;
            nodo.leftSideValue = token.rightValue;
            nodo.rightSideValue = nodo.fieldTokens[nodo.fieldTokens.Count - 1].rightValue;
        }
        //si coloco la ficha a la izquierda y por la derecha de la ficha
        else if (nodo.leftSideValue == token.rightValue)
        {
            var newList = new List<Ficha>();
            newList.Add(token);
            newList.AddRange(nodo.fieldTokens);
            nodo.leftSideValue = token.leftValue;
            nodo.rightSideValue = nodo.fieldTokens[nodo.fieldTokens.Count - 1].rightValue;
            //newNode.rightSideValue = nodoMcts.rightSideValue;
        }
        //si coloco la ficha a la derecha y x la izquierda de la ficha
        else if (nodo.rightSideValue == token.leftValue)
        {
            nodo.fieldTokens.Add(token);
            nodo.leftSideValue = nodo.fieldTokens[0].leftValue; // o de la siguiente forma
            //newNode.leftSideValue = nodoMcts.leftSideValue;
            nodo.rightSideValue = token.rightValue;
        }
        //si coloco la ficha a la derecha y por la derecha de la ficha
        else if (nodo.rightSideValue == token.rightValue)
        {
            nodo.fieldTokens.Add(token);
            nodo.leftSideValue = nodo.fieldTokens[0].leftValue; // o de la siguiente forma
            //newNode.leftSideValue = nodoMcts.leftSideValue;
            nodo.rightSideValue = token.leftValue;
        }
    }
}