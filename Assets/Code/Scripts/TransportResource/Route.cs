using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{
    [SerializeField]
    private GameObject startS;
    [SerializeField]
    private GameObject destS;
    [SerializeField]
    private TransportationStart m_start;
    public TransportationStart StartingStation { get { return m_start; } }
    [SerializeField]
    private TransportationDestination m_destination;
    public TransportationDestination Destination { get { return m_destination; } }
    [SerializeField]
    private bool m_onRepeat;
    public bool OnRepeat { get { return m_onRepeat; } set {  m_onRepeat = value; } }
    [SerializeField]
    private string m_type;
    public string Type { get { return m_type; } }
    [SerializeField]
    private GameObject m_carrier;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetCarrier(GameObject carrier)
    {
        m_carrier = carrier;
    }
    public void StartCarrier()
    {
        Instantiate(m_carrier);
        //TODO make carrier
    }

    public void SetStation()
    {
        m_start = startS.GetComponent<TransportationStart>();
        m_destination = destS.GetComponent<TransportationDestination>();

    }
}
