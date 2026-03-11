using UnityEngine;

public class WS_EffectParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private ParticleSystem _particleBoom;
    [SerializeField] private ParticleSystem _particleCharge;
    public void Play(float chargePercent)
    {
        if (_particle == null)
            return;

        PlayPartcle(_particle);
        PlayPartcle(_particleBoom);
        if(chargePercent == 1.0f)
            PlayPartcle(_particleCharge);
    }

    private void PlayPartcle(ParticleSystem system)
    {
        system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        system.Clear(false);
        system.Play(false);
    }
}