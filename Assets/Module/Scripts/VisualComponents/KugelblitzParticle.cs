using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KugelblitzParticle : MonoBehaviour {
    public bool TextVisibility { get; set; }

    private MeshRenderer _renderer;
    private TextMesh _text;

    private KugelblitzColor _initColor = KugelblitzColor.None;
    private KugelblitzColor _targetColor = KugelblitzColor.None;
    private float _lerpCoefficient = 0;

    private ParticleMovement _movement;
    private ParticleMovement _newMovement;

    private float _speedMultiplier = 1;
    private float _newSpeedMultiplier = 1;

    void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _text = GetComponentInChildren<TextMesh>();
        _movement = ParticleMovement.FromRandom();
        _newMovement = _movement;

        TextVisibility = false;
    }

    public void UpdateParticleData(KugelblitzColor c, ParticleMovement m)
    {
        _initColor = GetColor(_lerpCoefficient);
        _targetColor = c;

        _newMovement = m;

        _lerpCoefficient = Mathf.Min(_lerpCoefficient, 1 - _lerpCoefficient);
    }

    public void Pulse()
    {
        UpdateParticleData(_targetColor, _newMovement);
        _lerpCoefficient = 0;
    }

    private void UpdateMovement(float lerp)
    {
        if (lerp > 0.5f && _movement != _newMovement)
            _movement = _newMovement;
    }

    private void UpdateSpeed(float lerp)
    {
        if (lerp > 0.5f && _speedMultiplier != _newSpeedMultiplier)
            _speedMultiplier = _newSpeedMultiplier;
    }

    void Update () {
        _lerpCoefficient = Mathf.Min(1, _lerpCoefficient + Time.deltaTime * _speedMultiplier);
        //_renderer.material.SetColor("_OutlineColor", GetColor(_lerpCoefficient).GetColor());
        _renderer.material.color = GetColor(_lerpCoefficient).GetColor();
        _renderer.transform.localScale = 0.2f * Vector3.one * GetOpacity(_lerpCoefficient);
        _text.text = GetLabel(_lerpCoefficient);

        UpdateMovement(_lerpCoefficient);
        UpdateSpeed(_lerpCoefficient);
        _movement.Elapse(Time.deltaTime / 2 * _speedMultiplier * (2 - GetOpacity(_lerpCoefficient)));
        transform.localPosition = _movement.GetVector();
    }

    public void SetSpeed(float speed)
    {
        _newSpeedMultiplier = speed;
    }

    private KugelblitzColor GetColor(float lerp)
    {
        //regular lerp gives weird results
        if (lerp < 0.5f)
            return _initColor;
        return _targetColor;
    }

    private string GetLabel(float lerp)
    {
        if (!TextVisibility)
            return "";
        return GetColor(lerp).GetName();
    }

    private float GetOpacity(float lerp)
    {
        return (Mathf.Cos(lerp * Mathf.PI * 2) + 1) / 2 * GetColor(lerp).GetColor().a;
    }

    public float GetLerp()
    {
        return _lerpCoefficient;
    }
}
