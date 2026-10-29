using System;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{

    public static readonly List<GrapplePoint> All = new List<GrapplePoint>();

    public enum GrappleType { Swing, Pull, Attach }

    [SerializeField] GrappleType type = GrappleType.Swing;
    [SerializeField] float ropeLength = 3f;
    [SerializeField] float maxRange = 6f;

    public GrappleType Type => type;
    public float RopeLength => ropeLength;
    public float MaxRange => maxRange;
    public Vector2 Position => transform.position;

    public SpriteRenderer Renderer => GetComponent<SpriteRenderer>();

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);       
    }
}
