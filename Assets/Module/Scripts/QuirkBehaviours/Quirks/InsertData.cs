using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rnd = UnityEngine.Random;

public class InsertStageManager : KugelblitzStageManager<KugelblitzInsertData>
{
    protected override KugelblitzInsertData GenerateTarget()
    {
        return new KugelblitzInsertData(Enumerable.Range(0, 7).Select(x => Rnd.Range(0, 2) == 1).ToArray(), true);
    }

    public override KugelblitzColor GetBaseColor()
    {
        return KugelblitzColor.Yellow;
    }

    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        particle.UpdateParticleData(
            Current().GetFromIndex(index) ? KugelblitzColor.GetColorFromIndex(index) : KugelblitzColor.None,
            ParticleMovement.FromPassingAxis(1));
    }
}

public class KugelblitzInsertData : KugelblitzData<KugelblitzInsertData>
{
    private bool[] _data = new bool[7];

    public KugelblitzInsertData(bool[] data)
    {
        _data = data;
        IsFinalResult = false;
    }

    public KugelblitzInsertData(bool[] data, bool isFinalResult)
    {
        _data = data;
        IsFinalResult = isFinalResult;
    }

    public override KugelblitzInsertData GetMerged(KugelblitzInsertData secondEntry)
    {
        if (IsFinalResult)
            throw new InvalidOperationException();
        return new KugelblitzInsertData(Enumerable.Range(0, 7).Select(x => _data[x] ^ secondEntry._data[x]).ToArray());
    }

    protected override KugelblitzInsertData EmptyStage()
    {
        return new KugelblitzInsertData(new bool[7]);
    }

    protected override KugelblitzInsertData GetBridgeFrom(KugelblitzInsertData currentState)
    {
        return new KugelblitzInsertData(Enumerable.Range(0, 7).Select(x => _data[x] ^ currentState._data[x]).ToArray());
    }

    protected override KugelblitzInsertData RandomStage()
    {
        return new KugelblitzInsertData(Enumerable.Range(0, 7).Select(x => Rnd.Range(0, 2) == 1).ToArray());
    }

    public bool GetFromIndex(int index)
    {
        return _data[index];
    }

    public override string ToString()
    {
        if (IsFinalResult)
            return "[Inserts: " + _data.Select(x => (x ? 1 : 0) + "|").Join(", ") + "]";
        else
            return "[" + _data.Select(x => x ? 1 : 0).Join("") + "]";
    }
}
