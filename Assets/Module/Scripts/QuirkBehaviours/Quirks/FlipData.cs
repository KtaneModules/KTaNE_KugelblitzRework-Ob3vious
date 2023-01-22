using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class FlipStageManager : KugelblitzStageManager<KugelblitzFlipData>
{
    protected override KugelblitzFlipData GenerateTarget()
    {
        return new KugelblitzFlipData(Enumerable.Range(0, 6).Select(x => Rnd.Range(0, 2) == 1).ToArray());
    }

    public override KugelblitzColor GetBaseColor()
    {
        return KugelblitzColor.Indigo;
    }
    
    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        particle.UpdateParticleData(
            Current().GetFromIndex(index) ? KugelblitzColor.GetColorFromIndex(index) : KugelblitzColor.None,
            ParticleMovement.FromPassingAxis(2));
    }
}

public class KugelblitzFlipData : KugelblitzData<KugelblitzFlipData>
{
    private bool[] _data = new bool[7];

    public KugelblitzFlipData(bool[] data)
    {
        IsFinalResult = data.Length == 6;

        if (IsFinalResult)
        {
            bool invert = Rnd.Range(0, 2) == 1;
            data = data.Select(x => x ^ invert).ToArray();
            data = data.Take(5).Concat(new bool[] { invert }).Concat(data.Skip(5)).ToArray();
        }

        if (data.Length != 7)
            throw new InvalidOperationException();
        _data = data;
    }

    public bool GetFromIndex(int index)
    {
        if (IsFinalResult)
            return _data.Take(5).Concat(_data.Skip(6)).Select(x => x ^ _data[5]).ToArray()[index];

        return _data[index];
    }

    public override KugelblitzFlipData GetMerged(KugelblitzFlipData secondEntry)
    {
        if (IsFinalResult)
            throw new InvalidOperationException();
        return new KugelblitzFlipData(Enumerable.Range(0, 7).Select(x => _data[x] ^ secondEntry._data[x]).ToArray());
    }

    protected override KugelblitzFlipData GetBridgeFrom(KugelblitzFlipData currentState)
    {
        return new KugelblitzFlipData(Enumerable.Range(0, 7).Select(x => _data[x] ^ currentState._data[x]).ToArray());
    }

    protected override KugelblitzFlipData EmptyStage()
    {
        return new KugelblitzFlipData(new bool[7]);
    }

    protected override KugelblitzFlipData RandomStage()
    {
        return new KugelblitzFlipData(Enumerable.Range(0, 7).Select(x => Rnd.Range(0, 2) == 1).ToArray());
    }

    public override string ToString()
    {
        if (IsFinalResult)
            return "[Flips: " + Enumerable.Range(0, 6).Select(x => GetFromIndex(x) ? '!' : '.').Join("") + "]";
        else
            return "[" + _data.Select(x => x ? 1 : 0).Join("") + "]";
    }
}