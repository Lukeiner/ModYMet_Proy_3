using UnityEngine;

public class Producto : MonoBehaviour
{
    public bool Disponible = true;
    public float TiempoReposicion = 8f;

    private SpriteRenderer m_Sprite;
    private Color m_ColorOriginal = Color.white;

    private void Awake()
    {
        m_Sprite = GetComponentInChildren<SpriteRenderer>();
        if (m_Sprite != null) m_ColorOriginal = m_Sprite.color;
    }

    public void Tomar()
    {
        if (!Disponible) return;

        Disponible = false;
        if (m_Sprite != null) m_Sprite.color = new Color(m_ColorOriginal.r, m_ColorOriginal.g, m_ColorOriginal.b, 0.25f);
        Invoke(nameof(Reponer), TiempoReposicion);
    }

    private void Reponer()
    {
        Disponible = true;
        if (m_Sprite != null) m_Sprite.color = m_ColorOriginal;
    }
}
