using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rnd = UnityEngine.Random;

public class LengthStageManager : KugelblitzStageManager<KugelblitzLengthData>
{
    protected override KugelblitzLengthData GenerateTarget()
    {
        return new KugelblitzLengthData(Enumerable.Range(0, 7).Select(x => (byte)Rnd.Range(0, 7)).ToArray(), true);
    }

    public override KugelblitzColor GetBaseColor()
    {
        return KugelblitzColor.Green;
    }

    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        particle.UpdateParticleData(
            Current().GetFromIndex(index) != 0 ? KugelblitzColor.GetColorFromIndex(index) : KugelblitzColor.None,
            ParticleMovement.FromPeaks(Current().GetFromIndex(index) - 1));
    }

    protected override string StageType()
    {
        return "Green quirk";
    }
}

public class KugelblitzLengthData : KugelblitzData<KugelblitzLengthData>
{
    private byte[] _data = new byte[7];

    public KugelblitzLengthData(byte[] data)
    {
        _data = data;
        IsFinalResult = false;
    }

    public KugelblitzLengthData(byte[] data, bool isFinalResult)
    {
        _data = data;
        IsFinalResult = isFinalResult;
    }

    public override KugelblitzLengthData GetMerged(KugelblitzLengthData secondEntry)
    {
        if (IsFinalResult)
            throw new InvalidOperationException();
        return new KugelblitzLengthData(Enumerable.Range(0, 7).Select(x => (byte)((_data[x] + secondEntry._data[x]) % 7)).ToArray());
    }

    protected override KugelblitzLengthData EmptyStage()
    {
        return new KugelblitzLengthData(new byte[7]);
    }

    protected override KugelblitzLengthData GetBridgeFrom(KugelblitzLengthData currentState)
    {
        return new KugelblitzLengthData(Enumerable.Range(0, 7).Select(x => (byte)((_data[x] - currentState._data[x] + 7) % 7)).ToArray());
    }

    protected override KugelblitzLengthData RandomStage()
    {
        return new KugelblitzLengthData(Enumerable.Range(0, 7).Select(x => (byte)Rnd.Range(0, 7)).ToArray());
    }

    public byte GetFromIndex(int index)
    {
        return _data[index];
    }

    public override string ToString()
    {
        if (IsFinalResult)
            return "[Lengths: " + _data.Select(x => (x + 6) % 7 + 1).Join("-") + "]";
        else
            return "[" + _data.Join("") + "]";
    }
}
