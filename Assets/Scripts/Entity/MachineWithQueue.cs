using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

// Machine qui peut vendre un ticket et gérer une file d'attente avec certaine capacité
//=================================================================================================
public class MachineWithQueue : MonoBehaviour
{
    private VendingMachine machine;

    // Texte GUI (temps de fabrication d'un ticket + passagers dans la file)
    [SerializeField] private TextMeshPro TMPro;

    // Image circulaire pour l'attente pendant la fabrication d'un ticket
    private Image radialCircle;
    private float elapsedMakingTime; // pour radialCircle

    [SerializeField] public Queue<Passenger> passengersInLine;

    private WaitForSeconds waitForSeconds;
    [SerializeField][Range(0, 99)] public float makingTicketTime;

    private bool makingTicket;
    private float speed;

    // Max des gens dans la queue, pour ne pas générer de positions abérantes  
    private const int MAX_LINE_CAPACITY = 5;

    public Vector3 firstPosInLine;
    private Vector3[] posInLine = new Vector3[MAX_LINE_CAPACITY + 1];

    public bool CanGetIntoLine => passengersInLine.Count < MAX_LINE_CAPACITY && !Train.Instance.Departed;
    public Vector3 NextPosInLine => posInLine[passengersInLine.Count + 1];
    public bool IsFirstInLine(Passenger p) => passengersInLine.Contains(p) && passengersInLine.Peek() == p;

    private void Awake()
    {
        passengersInLine = new Queue<Passenger>();
        machine = GetComponent<VendingMachine>();
        speed = machine.Speed;
        makingTicket = false;

        if (makingTicketTime < 0.1f) // permet de tester d'autres valeurs
            ComputeRealMakingTicketTime();

        firstPosInLine = transform.position + transform.forward * 0.8f;
        for (int i = 0; i < posInLine.Length; i++)
        {
            posInLine[i] = firstPosInLine + i * transform.forward * 1.1f; // 1.1f
            posInLine[i].y = 0;
        }

        radialCircle = GetComponentInChildren<Image>();
        radialCircle.transform.parent.gameObject.SetActive(false);
        radialCircle.transform.LookAt(2 * radialCircle.transform.position - Camera.main.transform.position);

        TMPro = GetComponentInChildren<TextMeshPro>();
        TMPro.text = passengersInLine.Count.ToString();
        TMPro.transform.LookAt(2 * TMPro.transform.position - Camera.main.transform.position);
    }

    private void Start()
    {
        Train.Instance.OnTrainDeparted += OnCancelEverything;

        UpdateGUI();
    }

    // Renvoie la position où se mettre pour faire la queue devant la machine
    public bool AddToLine(Passenger passenger, out Vector3 pos)
    {
        if (!CanGetIntoLine)
        {
            Debug.LogWarning("TicketDelivery # TROP DE GENS DANS LA QUEUE !");
            pos = Vector3.zero;
            return false;
        }

        if (passengersInLine.Contains(passenger))
        {
            Debug.LogError("DEJA DANS LA QUEUE !!!!!!!!!!");
            pos = Vector3.zero;
            return false;
        }

        passengersInLine.Enqueue(passenger);

        pos = posInLine[passengersInLine.Count - 1];
        UpdateGUI();

        return true;
    }

    internal bool RemoveFromLine(Passenger passenger)
    {
        if (!passengersInLine.Contains(passenger))
        {
            Debug.LogError("PAS DANS CETTE FILE !!!!!!!!!!!");
            return false;
        }

        Queue<Passenger> tempQ = new Queue<Passenger>();
        foreach (Passenger p in passengersInLine)
            if (p != passenger)
                tempQ.Enqueue(p);

        passengersInLine = tempQ;

        return true;
    }


    // Dire à tout le monde de se décaler
    private void UpdatePassengersQueuePosition()
    {
        int i = 0;
        foreach (Passenger p in passengersInLine)
        {
            p.NewPositionInLine(posInLine[i++]);
        }
    }

    // "Fabrique" un ticket et le donne au 1er acheteur dans la queue
    private IEnumerator DeliverTickerToFirstPassenger()
    {
        Debug.Log(name + " #################### DeliverTickerToFirstPassenger " + makingTicketTime);
        yield return waitForSeconds;

        Passenger p = null;
        passengersInLine.TryDequeue(out p);
        if (!Train.Instance.Departed && p != null)
        {
            p.GetTrainTicket();
            makingTicket = false;
        }
        radialCircle.transform.parent.gameObject.SetActive(false);

        UpdatePassengersQueuePosition();
        UpdateGUI();
    }

    void Update()
    {
        if (makingTicket)
        {
            elapsedMakingTime += Time.deltaTime;
            UpdateGUI();
        }
    }

    private void UpdateGUI()
    {
        if (makingTicket)
        {
            radialCircle.fillAmount = elapsedMakingTime / makingTicketTime;
            TMPro.color = Color.black;
            TMPro.text = (makingTicketTime - elapsedMakingTime).ToString("0.0") + "s" + "\n" + passengersInLine.Count;
        }
        else
        {
            TMPro.color = Color.grey;
            TMPro.text = makingTicketTime.ToString("0.0") + "s" + "\n" + passengersInLine.Count;
        }
    }

    // Un acheteur demande d'acheter un ticket
    public void AskToBuyTicket(Passenger passenger)
    {
        if (!makingTicket && passenger == passengersInLine.Peek() && !Train.Instance.Departed)
        {
            makingTicket = true;
            radialCircle.transform.parent.gameObject.SetActive(true);
            elapsedMakingTime = 0;

            StartCoroutine(DeliverTickerToFirstPassenger());
        }
    }

    public void ComputeRealMakingTicketTime()
    {
        makingTicketTime = speed * 60f;
        waitForSeconds = new WaitForSeconds(makingTicketTime);
        UpdateGUI();

        Debug.Log("ComputeRealMakingTicketTime SPEED=" + speed + " MTTime=" + makingTicketTime);
    }

    private void OnCancelEverything()
    {
        Train.Instance.OnTrainDeparted -= OnCancelEverything;
        StopAllCoroutines();
        makingTicket = false;
        passengersInLine.Clear();

        radialCircle.transform.parent.gameObject.SetActive(false);
        TMPro.text = "no ticket !";
    }

    internal void SetSpeed(float s)
    {
        speed = s; // cannot set to vendingmachine !!!

        ComputeRealMakingTicketTime();
    }
}
