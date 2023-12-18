using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

// Train unique => singleton
//=================================================================================================
public class Train : MonoBehaviour
{
    [SerializeField][Range(1, 300)] private int secondsBeforeDeparture = 90; // secondes à attendre avant départ 4min = 240
    [SerializeField] private float speed = 20;
    [SerializeField] private int capacity = 21 * 2 * 3;

    public Vector3 OnBoardTrainPos; // endroit pour embarquer dans le train
    private List<Passenger> passengersOnBoard;
    private bool departed; // parti ou pas ?

    // Evénement "départ du train"
    public Action OnTrainDeparted;

    private TextMeshPro timeTMP;
    private DateTime departureTime;

    public bool Departed => departed;

    private static Train instance;

    public static Train Instance => instance;

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            ComputeBoardPosition();
        }
    }

    private void Start()
    {
        passengersOnBoard = new List<Passenger>();
        timeTMP = GetComponentInChildren<TextMeshPro>();
        departed = false;
        departureTime = GameManager.Instance.CurrentTime.AddSeconds(secondsBeforeDeparture);

        UpdateGUI();
    }

    private void ComputeBoardPosition()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 9, NavMesh.AllAreas))
        {
            OnBoardTrainPos = hit.position;
        }
    }

    void Update()
    {
        if (departed)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
        }
        else if (GameManager.Instance.CurrentTime.CompareTo(departureTime) >= 0)
        {
            departed = true;
            OnTrainDeparted?.Invoke();
        }
    }

    public bool BoardAPassenger(Passenger p)
    {
        if (!departed && passengersOnBoard.Count < capacity)
        {
            passengersOnBoard.Add(p);
            UpdateGUI();
            return true;
        }

        Debug.LogWarning("PassengerOnBoard IMPOSSIBLE: TRAIN PARTI OU PLEIN * " + passengersOnBoard.Count + "/" + capacity);
        return false;
    }

    private void UpdateGUI()
    {
        timeTMP.text = "(" + passengersOnBoard.Count + ") Départ à: " + departureTime.ToString("HH:mm:ss");
    }
}
