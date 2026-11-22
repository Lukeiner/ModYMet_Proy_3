using UnityEngine;

public class Producto : MonoBehaviour
{
    public bool Disponible = true;
    public float TiempoReposicion = 8f;

    private SpriteRenderer m_Sprite;

    private void Awake()
    {
        m_Sprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void Tomar()
    {
        if (!Disponible) return;

        Disponible = false;
        if (m_Sprite != null) m_Sprite.color = new Color(1f, 1f, 1f, 0.25f);
        Invoke(nameof(Reponer), TiempoReposicion);
    }

    private void Reponer()
    {
        Disponible = true;
        if (m_Sprite != null) m_Sprite.color = Color.white;
    }
}
