using UnityEngine;

public class PingLifecycle : MonoBehaviour
{
    [SerializeField] private float _timeToLive = 1f;

    // Update is called once per frame
    private void Update()
    {
        if (_timeToLive < Time.time)
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        _timeToLive = Time.time + _timeToLive;
    }
}
