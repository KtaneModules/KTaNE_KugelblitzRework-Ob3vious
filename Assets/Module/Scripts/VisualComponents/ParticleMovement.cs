using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class ParticleMovement
{
    private const float LowerBoundCap = 0.4f;
    private const float UpperBoundCap = 1.4f;

    //these should at all times be equal
    private Vector3 _v1, _v2;
    private float _angle;
    private float _pseudoRadius;

    private ParticleMovement(Vector3 v1, Vector3 v2)
    {
        _pseudoRadius = 1;
        if (Vector3.Angle(v1, v2) > 90)
        {
            _v1 = v2.normalized;
            _v2 = -v1.normalized;
        }
        else
        {
            _v1 = v1.normalized;
            _v2 = v2.normalized;
        }
        
    }

    

    public Vector3 GetVector()
    {
        return _pseudoRadius * (Mathf.Cos(Mathf.PI * 2 * _angle) * _v1 + Mathf.Sin(Mathf.PI * 2 * _angle) * _v2 + EllipseCentre());
    }

    public void Elapse(float delta)
    {
        float f = Mathf.Pow(Mathf.Sqrt(2 / (LongAxis() * ShortAxis()) - 1), -Mathf.Cos(Vector3.Angle(GetVector(), EllipseCentre()) * Mathf.PI / 180) / 2);
        _angle += f * delta / Mathf.Pow(_pseudoRadius, 2);
    }



    public static ParticleMovement FromRandom()
    {
        while (true)
        {
            Vector3 v1 = new Vector3(Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f));
            Vector3 v2 = new Vector3(Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f));

            //equalising chances among angles
            if (v1.magnitude > 1 || v2.magnitude > 1)
                continue;

            ParticleMovement movement = new ParticleMovement(v1, v2);
            try { movement.RandomiseTrajectoryScale(); }
            catch { continue; }
            movement._angle = Rnd.Range(0f, 1f);

            return movement;
        }
    }

    public static ParticleMovement FromDiagonal(bool antidiagonal)
    {
        while (true)
        {
            float xz = Rnd.Range(-1f, 1f);
            Vector3 v1 = new Vector3(xz, Rnd.Range(-1f, 1f), (antidiagonal ? 1 : -1) * xz);
            xz = Rnd.Range(-1f, 1f);
            Vector3 v2 = new Vector3(xz, Rnd.Range(-1f, 1f), (antidiagonal ? 1 : -1) * xz);

            //equalising chances among angles
            if (v1.magnitude > 1 || v2.magnitude > 1)
                continue;

            ParticleMovement movement = new ParticleMovement(v1, v2);
            try { movement.RandomiseTrajectoryScale(); }
            catch { continue; }
            movement._angle = Rnd.Range(0f, 1f);

            return movement;
        }
    }

    public static ParticleMovement FromOrthogonal(int axis)
    {
        while (true)
        {
            Vector2 va = new Vector2(Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f));
            Vector2 vb = new Vector2(Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f));

            if ((((Mathf.Atan2(va.y, va.x) - Mathf.Atan2(vb.y, vb.x)) / (Mathf.PI * 2) + 1) % 1 >= 0.5f) == (axis % 2 == 1))
                vb *= -1;

            Vector3 v1;
            Vector3 v2;
            switch (axis)
            {
                case 0: 
                case 1:
                    v1 = new Vector3(va.x, va.y, 0);
                    v2 = new Vector3(vb.x, vb.y, 0);
                    break;
                case 2:
                case 3:
                    v1 = new Vector3(va.x, 0, va.y);
                    v2 = new Vector3(vb.x, 0, vb.y);
                    break;
                case 4:
                case 5:
                    v1 = new Vector3(0, va.x, va.y);
                    v2 = new Vector3(0, vb.x, vb.y);
                    break;
                default:
                    axis = 0;
                    continue;
            }

            //equalising chances among angles
            if (v1.magnitude > 1 || v2.magnitude > 1)
                continue;

            ParticleMovement movement = new ParticleMovement(v1, v2);
            try { movement.RandomiseTrajectoryScale(); }
            catch { continue; }
            movement._angle = Rnd.Range(0f, 1f);

            return movement;
        }
    }

    public static ParticleMovement FromPeaks(int dimension)
    {
        while (true)
        {
            Vector2 v = new Vector2(Mathf.Lerp(LowerBoundCap, UpperBoundCap, 1 / 2f), Mathf.Lerp(LowerBoundCap, UpperBoundCap, 4 / 7f));

            if (Rnd.Range(0, 2) == 1)
                v = new Vector2(-v.x, v.y);

            v /= Mathf.Sqrt(2);

            float angle = dimension * Mathf.PI / 3;

            Vector3 v1 = new Vector3(v.y * Mathf.Sin(angle), v.x, v.y * Mathf.Cos(angle));
            Vector3 v2 = new Vector3(v.y * Mathf.Sin(angle), -v.x, v.y * Mathf.Cos(angle));

            ParticleMovement movement = new ParticleMovement(v1, v2);

            try { movement.RandomiseTrajectoryScale(); }
            catch { break; }
            movement._angle = Rnd.Range(0f, 1f);

            return movement;
        }
        return null;
    }

    public static ParticleMovement FromPassingAxis(int axis)
    {
        while (true)
        {
            Vector3 v1;
            Vector3 v2 = new Vector3(Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f));

            switch (axis)
            {
                case 0:
                    v1 = new Vector3(0, 0, 1);
                    break;
                case 1:
                    v1 = new Vector3(1, 0, 0);
                    break;
                case 2:
                    v1 = new Vector3(0, 1, 0);
                    break;
                default:
                    axis = 0;
                    continue;
            }

            //equalising chances among angles
            if (v2.magnitude > 1)
                continue;

            ParticleMovement movement = new ParticleMovement(v1, v2);
            try { movement.RandomiseTrajectoryScale(); }
            catch { continue; }
            movement._angle = Rnd.Range(0f, 1f);

            return movement;
        }
    }

    public static ParticleMovement FromPlane(bool vertical)
    {
        while (true)
        {
            Vector2 va = new Vector2(Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f));
            Vector2 vb = new Vector2(Rnd.Range(-1f, 1f), Rnd.Range(-1f, 1f));

            Vector3 v1;
            Vector3 v2;

            if (vertical)
            {
                v1 = new Vector3(0, va.x, va.y);
                v2 = new Vector3(0, vb.x, vb.y);
            }
            else
            {
                v1 = new Vector3(va.x, va.y, 0);
                v2 = new Vector3(vb.x, vb.y, 0);
            }

            //equalising chances among angles
            if (v1.magnitude > 1 || v2.magnitude > 1)
                continue;

            ParticleMovement movement = new ParticleMovement(v1, v2);
            try { movement.RandomiseTrajectoryScale(); }
            catch { continue; }
            movement._angle = Rnd.Range(0f, 1f);

            return movement;
        }
    }

    public static ParticleMovement FromChase(ParticleMovement leader, float angleOffset)
    {
        ParticleMovement movement = new ParticleMovement(leader._v1, leader._v2);
        movement._pseudoRadius = leader._pseudoRadius;
        movement._angle = leader._angle - angleOffset;
        return movement;
    }

    private float LowerBound()
    {
        return LongAxis() - FocalDistance();
    }

    private float UpperBound()
    {
        return LongAxis() + FocalDistance();
    }

    private float ShortAxis()
    {
        return (_v1 - _v2).magnitude * _pseudoRadius / Mathf.Sqrt(2);
    }

    private float LongAxis()
    {
        return (_v1 + _v2).magnitude * _pseudoRadius / Mathf.Sqrt(2);
    }

    private float FocalDistance()
    {
        float a = LongAxis();
        float b = ShortAxis();
        return Mathf.Sqrt(a * a - b * b);
    }

    private Vector3 EllipseCentre()
    {
        Vector3 sum = _v1 + _v2;
        Vector3 difference = _v1 - _v2;

        Vector3 focalPoint;
        if (sum.magnitude > difference.magnitude)
            focalPoint = sum;
        else
            focalPoint = difference;

        return focalPoint.normalized * FocalDistance();
    }

    private void RandomiseTrajectoryScale()
    {
        //the ratio of bound ratios, expressed in a logarithmic scale 
        float upperRatio = Mathf.Log(UpperBoundCap / UpperBound());
        float lowerRatio = Mathf.Log(LowerBoundCap / LowerBound());

        //we exceed either bound regardless of radius
        if (upperRatio < lowerRatio)
            throw new InvalidOperationException();

        float radius = Mathf.Exp(Rnd.Range(lowerRatio, upperRatio));

        _pseudoRadius *= radius;

    }
}
