using System.Collections;
using UnityEngine;

// EchoStaff MAX skill — a decoy that emits sound, drawing CaveMaw away from the player.
public class EchoClone : MonoBehaviour
{
    private SoundSource _soundSource;

    public void Init(float lifetime)
    {
        _soundSource = new SoundSource(transform.position, lifetime, isPlayer: false);
        SoundDetector.Instance?.RegisterSource(_soundSource);
        StartCoroutine(LifetimeRoutine(lifetime));
    }

    private IEnumerator LifetimeRoutine(float lifetime)
    {
        // Pulse sound every second to keep CaveMaw interested
        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            SoundDetector.Instance?.RegisterSource(
                new SoundSource(transform.position, 1.5f, isPlayer: false));
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        SoundDetector.Instance?.DeregisterSource(_soundSource);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_soundSource != null)
            SoundDetector.Instance?.DeregisterSource(_soundSource);
    }
}
