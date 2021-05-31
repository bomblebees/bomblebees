using UnityEngine;

public class ChangeAllParticleColors : MonoBehaviour
{
    [SerializeField] private Color color = new Color(1f, 0.65f, 0f);
    private ParticleSystem[] _particleSystems;

    private void Awake()
    {
        _particleSystems = GetComponentsInChildren<ParticleSystem>();

        foreach (var system in _particleSystems)
        {
            var systemMain = system.main;
            systemMain.startColor = color;
        }
    }
}