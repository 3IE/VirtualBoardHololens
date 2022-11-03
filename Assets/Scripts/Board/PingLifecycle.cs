using UnityEngine;

public class PingLifecycle : MonoBehaviour
{
    [SerializeField] private float _timeToLive = 1f;
    // Start is called before the first frame update
    void OnEnable() => _timeToLive = Time.time + _timeToLive;

    // Update is called once per frame
    void Update()
    {
        if (_timeToLive < Time.time)
            Destroy(gameObject);
    }
}
