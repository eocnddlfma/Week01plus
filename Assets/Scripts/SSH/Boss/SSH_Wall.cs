using DG.Tweening;
using UnityEngine;

namespace SSH.Boss
{
    public class SSH_Wall : MonoBehaviour
    {
        [Header("Open Animation")]
        [SerializeField] private float _openDuration = 0.8f;
        [SerializeField] private float _moveUpAmount = 3f;

        [Header("Hit Effect")]
        [SerializeField] private float _flashDuration = 0.06f;

        private SpriteRenderer[] _renderers;
        private Collider2D[]     _colliders;
        private bool             _isOpening = false;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            _colliders = GetComponentsInChildren<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isOpening) return;
            Jaein_OrbitalWeapon orbital = other.GetComponent<Jaein_OrbitalWeapon>();
            if (orbital == null) return;

            Vector2 normal = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
            orbital.ReflectVelocity(normal);

            foreach (SpriteRenderer sr in _renderers)
            {
                Color orig = sr.color;
                sr.DOKill();
                sr.DOColor(Color.white, _flashDuration)
                  .SetLoops(2, LoopType.Yoyo)
                  .OnComplete(() => sr.color = orig);
            }
        }

        public void Open()
        {
            if (_isOpening) return;
            _isOpening = true;

            foreach (Collider2D col in _colliders)
                col.enabled = false;

            transform.DOKill();
            transform.DOMoveY(transform.position.y + _moveUpAmount, _openDuration)
                     .SetEase(Ease.InCubic);

            foreach (SpriteRenderer sr in _renderers)
                sr.DOFade(0f, _openDuration).SetEase(Ease.InCubic);

            Destroy(gameObject, _openDuration);
        }
    }
}
