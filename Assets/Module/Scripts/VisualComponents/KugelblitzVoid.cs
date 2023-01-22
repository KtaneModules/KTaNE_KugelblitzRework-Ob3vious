using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Module.Scripts
{
    public class KugelblitzVoid : MonoBehaviour
    {
        public bool TextVisibility { get; set; }

        private MeshRenderer _renderer;
        private TextMesh _text;

        private KugelblitzColor _initColor = KugelblitzColor.None;
        private KugelblitzColor _targetColor = KugelblitzColor.None;
        private float _lerpCoefficient = 0;


        private float _speedMultiplier = 1;
        private float _newSpeedMultiplier = 1;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _text = GetComponentInChildren<TextMesh>();

            TextVisibility = false;
        }

        public void UpdateVoidData(KugelblitzColor c)
        {
            _initColor = GetColor(_lerpCoefficient);
            _targetColor = c;

            _lerpCoefficient = Mathf.Min(_lerpCoefficient, 1 - _lerpCoefficient);
        }

        void Update()
        {
            _lerpCoefficient = Mathf.Min(1, _lerpCoefficient + Time.deltaTime * _speedMultiplier);
            //_renderer.material.SetColor("_OutlineColor", GetColor(_lerpCoefficient).GetColor());
            _renderer.material.color = GetColor(_lerpCoefficient).GetColor();
            _renderer.transform.localScale = 0.6f * Vector3.one * GetOpacity(_lerpCoefficient);

            UpdateSpeed(_lerpCoefficient);

            _text.text = GetLabel(_lerpCoefficient);
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

        public void SetColor(KugelblitzColor color)
        {
            Color lerp = Color.Lerp(new Color(0.25f, 0.25f, 0.25f), color.GetColor(), 0.5f);
            UpdateVoidData(new KugelblitzColor(new Color(lerp.r, lerp.g, lerp.b, color.GetColor().a), color.GetName()));
        }

        public void Pulse()
        {
            UpdateVoidData(_targetColor);
            _lerpCoefficient = 0.875f;
        }


        private void UpdateSpeed(float lerp)
        {
            if (lerp > 0.5f && _speedMultiplier != _newSpeedMultiplier)
                _speedMultiplier = _newSpeedMultiplier;
        }

        public void SetSpeed(float speed)
        {
            _newSpeedMultiplier = speed;
        }
    }
}
