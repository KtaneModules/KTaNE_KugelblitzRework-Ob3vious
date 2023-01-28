using System;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class BaseStageManager : KugelblitzStageManager<KugelblitzBaseData>
{
    protected override KugelblitzBaseData GenerateTarget()
    {
        byte x = (byte)Rnd.Range(0, 7);
        byte y = (byte)Rnd.Range(0, 7);
        bool r = Rnd.Range(0, 2) == 1;
        KugelblitzBaseData target = new KugelblitzBaseData(x, y, r);
        return target;
    }

    public override KugelblitzColor GetBaseColor()
    {
        return KugelblitzColor.Black;
    }

    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        particle.UpdateParticleData(
            Current().GetFromIndex(index) ? KugelblitzColor.GetColorFromIndex(index) : KugelblitzColor.None, 
            ParticleMovement.FromRandom());
    }

    protected override string StageType()
    {
        return "Regular";
    }
}

public class KugelblitzBaseData : KugelblitzData<KugelblitzBaseData>
{
    private bool[] _data = new bool[7];

    public KugelblitzBaseData(byte x, byte y, bool r)
    {
        if (x >= 7 || y >= 7)
            throw new InvalidOperationException();

        for (int i = 0; i < 3; i++)
            _data[i] = ((x >> (2 - i)) & 1) == 1;
        for (int i = 0; i < 3; i++)
            _data[i + 3] = ((y >> (2 - i)) & 1) == 1;
        _data[6] = r;

        IsFinalResult = true;
    }

    private KugelblitzBaseData(bool[] data)
    {
        if (data.Length != 7)
            throw new InvalidOperationException();
        _data = data;

        IsFinalResult = false;
    }

    public bool GetFromIndex(int index)
    {
        if (IsFinalResult)
            throw new InvalidOperationException();

        return _data[index];
    }

    public byte GetX()
    {
        if (!IsFinalResult)
            throw new InvalidOperationException();

        byte x = 0;
        for (int i = 0; i < 3; i++)
            x |= (byte)((_data[i] ? 1 : 0) << (2 - i));

        return x;
    }

    public byte GetY()
    {
        if (!IsFinalResult)
            throw new InvalidOperationException();

        byte y = 0;
        for (int i = 0; i < 3; i++)
            y |= (byte)((_data[i + 3] ? 1 : 0) << (2 - i));

        return y;
    }

    public bool GetR()
    {
        if (!IsFinalResult)
            throw new InvalidOperationException();

        return _data[6];
    }

    public override KugelblitzBaseData GetMerged(KugelblitzBaseData secondEntry)
    {
        if (IsFinalResult)
            throw new InvalidOperationException();
        return new KugelblitzBaseData(Enumerable.Range(0, 7).Select(x => _data[x] ^ secondEntry._data[x]).ToArray());
    }

    protected override KugelblitzBaseData GetBridgeFrom(KugelblitzBaseData currentState)
    {
        return new KugelblitzBaseData(Enumerable.Range(0, 7).Select(x => _data[x] ^ currentState._data[x]).ToArray());
    }

    protected override KugelblitzBaseData EmptyStage()
    {
        return new KugelblitzBaseData(new bool[7]);
    }

    protected override KugelblitzBaseData RandomStage()
    {
        return new KugelblitzBaseData(Enumerable.Range(0, 7).Select(x => Rnd.Range(0, 2) == 1).ToArray());
    }

    public override string ToString()
    {
        if (IsFinalResult)
            return "[(X, Y)=(" + GetX() + ", " + GetY() + "); R=" + (GetR() ? 1 : 0) + "]";
        else
            return "[" + _data.Select(x => x ? 1 : 0).Join("") + "]";
    }
}

