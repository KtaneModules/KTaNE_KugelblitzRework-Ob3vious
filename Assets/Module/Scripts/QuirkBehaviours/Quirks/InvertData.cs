using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rnd = UnityEngine.Random;

public class InvertStageManager : KugelblitzStageManager<KugelblitzInvertData>
{
    protected override KugelblitzInvertData GenerateTarget()
    {
        return new KugelblitzInvertData(Enumerable.Range(0, 7).Select(x => Rnd.Range(0, 2) == 1).ToArray(), true);
    }

    public override KugelblitzColor GetBaseColor()
    {
        return KugelblitzColor.Orange;
    }

    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        particle.UpdateParticleData(
            Current().GetFromIndex(index) ? KugelblitzColor.GetColorFromIndex(index) : KugelblitzColor.None,
            ParticleMovement.FromPassingAxis(0));
    }

    protected override string StageType()
    {
        return "Orange quirk";
    }
}

public class KugelblitzInvertData : KugelblitzData<KugelblitzInvertData>
{
    private bool[] _data = new bool[7];

    public KugelblitzInvertData(bool[] data)
    {
        _data = data;
        IsFinalResult = false;
    }

    public KugelblitzInvertData(bool[] data, bool isFinalResult)
    {
        _data = data;
        IsFinalResult = isFinalResult;
    }

    public override KugelblitzInvertData GetMerged(KugelblitzInvertData secondEntry)
    {
        if (IsFinalResult)
            throw new InvalidOperationException();
        return new KugelblitzInvertData(Enumerable.Range(0, 7).Select(x => _data[x] ^ secondEntry._data[x]).ToArray());
    }

    protected override KugelblitzInvertData EmptyStage()
    {
        return new KugelblitzInvertData(new bool[7]);
    }

    protected override KugelblitzInvertData GetBridgeFrom(KugelblitzInvertData currentState)
    {
        return new KugelblitzInvertData(Enumerable.Range(0, 7).Select(x => _data[x] ^ currentState._data[x]).ToArray());
    }

    protected override KugelblitzInvertData RandomStage()
    {
        return new KugelblitzInvertData(Enumerable.Range(0, 7).Select(x => Rnd.Range(0, 2) == 1).ToArray());
    }

    public bool GetFromIndex(int index)
    {
        return _data[index];
    }

    public override string ToString()
    {
        if (IsFinalResult)
            return "[Inverts: " + _data.Select(x => x ? '!' : '.').Join("") + "]";
        else
            return "[" + _data.Select(x => x ? 1 : 0).Join("") + "]";
    }
}
