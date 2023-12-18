using UnityEngine;
using TMPro;
using UnityEngine.AI;
using System;
using System.Collections.Generic;
using System.Text;
using Random = UnityEngine.Random;

// Responsable du temps
//=================================================================================================
public class GameManager : MonoBehaviour, IPassengerInstantiator
{
    [SerializeField] public bool globalForceRealData; // override les autres

    [SerializeField] private TextMeshProUGUI timeTMP;
    [SerializeField] private Passenger prefabPassenger;
    [SerializeField] private Transform stationGround;
    [SerializeField] private TextMeshProUGUI finalText;

    private static DateTime currentTime;
    public DateTime CurrentTime => currentTime;
    private List<Passenger> allPassengers;

    private static GameManager instance;
    public static GameManager Instance => instance;

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else if (instance != this)
            Destroy(this);
    }

    private void Init()
    {
        allPassengers = new List<Passenger>();
        currentTime = new DateTime(2023, 12, 13, 15, 1, 0);
        Train.Instance.OnTrainDeparted += OnTrainDeparted;
        finalText.transform.parent.gameObject.SetActive(false);

        BackEndManager.OnServerGetMachinesSpeed += OnServerGetMachinesSpeed;
        BackEndManager.OnServerGetPassengerSpeed += OnServerGetPassengerSpeed;
        BackEndManager.OnServerGetPassengerBehavior += OnServerGetPassengerBehavior;
    }

    private void OnServerGetPassengerBehavior(bool freeze)
    {
        Debug.Log("GameManager::OnServerGetPassengerFreeze ########## " + freeze);

        Passenger.SetPassengersFreeze(freeze);
        foreach (var passenger in allPassengers)
            passenger.Freeze(freeze);
    }

    private void OnServerGetPassengerSpeed(float factor)
    {
        Debug.Log("GameManager::OnServerGetPassengerBehavior ########## " + factor);

        Passenger.SetPassengersSpeedFactor(factor);
        foreach (var passenger in allPassengers)
            passenger.ChangeSpeedAccordingToFactor();
    }

    private void OnServerGetMachinesSpeed(List<float> speeds)
    {
        Debug.Log("GameManager::OnServerGetMachinesSpeed ########## " + string.Join(" * ", speeds));

        globalForceRealData = false;
        MachinesManager.Instance.SetSpeedsList(speeds);
    }

    private void OnTrainDeparted()
    {
        Train.Instance.OnTrainDeparted -= OnTrainDeparted;

        BackEndManager.OnServerGetMachinesSpeed -= OnServerGetMachinesSpeed;
        BackEndManager.OnServerGetPassengerSpeed -= OnServerGetPassengerSpeed;
        BackEndManager.OnServerGetPassengerBehavior -= OnServerGetPassengerBehavior;

        finalText.transform.parent.gameObject.SetActive(true);
        finalText.text = "TRAIN PARTI !!!";

        int boarded = 0;
        int hasTicket = 0;
        int noTicket = 0;
        StringBuilder sbBoarded = new StringBuilder();
        StringBuilder sbHasTicket = new StringBuilder();
        StringBuilder sbNoTicket = new StringBuilder();

        foreach (Passenger p in allPassengers)
        {
            if (p.BoardedTrain)
            {
                if (sbBoarded.Length > 0)
                    sbBoarded.Append(", ");
                sbBoarded.Append(p.Id);
                boarded++;
            }
            else if (p.HasTicket)
            {
                if (sbHasTicket.Length > 0)
                    sbHasTicket.Append(", ");
                sbHasTicket.Append(p.Id);
                hasTicket++;
            }
            else
            {
                if (sbNoTicket.Length > 0)
                    sbNoTicket.Append(", ");
                sbNoTicket.Append(p.Id);
                noTicket++;
            }
        }

        finalText.text = "LE TRAIN EST PARTI !!!" + "\n\n" +
            boarded + " passagers ont pu avoir leur place à bord du train:\n" + sbBoarded + "\n\n" +
            hasTicket + " passagers ont un ticket mais restent à quai:\n" + sbHasTicket + "\n\n" +
            noTicket + " errent dans la station, sans ticket:\n" + sbNoTicket;
    }

    void Update()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime);

        timeTMP.text = currentTime.ToString("HH:mm:ss"); // 24h format
    }

    // IPassengerInstantiator
    public void Instantiate(float passengerSpeed)
    {
        if (!Train.Instance.Departed)
        {
            Passenger p = GameObject.Instantiate(prefabPassenger);
            //p.transform.position = Vector3.zero;
            p.transform.position = GetRandomWalkablePoint(stationGround.position, 17);
            p.gameObject.SetActive(true);

            allPassengers.Add(p);
        }
    }

    public static Vector3 GetRandomWalkablePoint(Vector3 center, float maxDistance)
    {
        Vector3 randomPos = center + Random.insideUnitSphere * maxDistance;
        randomPos.y = 0; // au sol

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, 4, NavMesh.GetAreaFromName("Walkabke"));

        return hit.position;
    }
}
