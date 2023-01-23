using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class VoidLinkage : MonoBehaviour
{
    public Transform Transform1;
    public Transform Transform2;

    public void Update()
    {
        SetPosition();
    }

    private Transform Smaller()
    {
        return Transform1.lossyScale.x < Transform2.lossyScale.x ? Transform1 : Transform2;
    }

    private Vector3 AveragePosition()
    {
        return Vector3.Lerp(Transform1.position, Transform2.position, 0.5f);
    }

    private Vector3 ToLocal(Vector3 globalPosition)
    {
        return Smaller().worldToLocalMatrix.MultiplyPoint3x4(globalPosition);
    }

    private void SetPosition()
    {
        transform.SetParent(Smaller());
        transform.localPosition = ToLocal(AveragePosition());
        transform.localScale = new Vector3(0.2f, 0.2f, ToLocal(AveragePosition()).magnitude);
        transform.LookAt(Smaller().position);
    }

    public void SetColor(KugelblitzColor color)
    {
        GetComponentInChildren<MeshRenderer>().material.color = color.GetColor();
    }

    public void SetMaterial(Material material)
    {
        GetComponentInChildren<MeshRenderer>().material = material;
    }
}