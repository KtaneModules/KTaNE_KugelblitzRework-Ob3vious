using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rnd = UnityEngine.Random;

public class OffsetStageManager : KugelblitzStageManager<KugelblitzOffsetData>
{
    protected override KugelblitzOffsetData GenerateTarget()
    {
        return new KugelblitzOffsetData(Enumerable.Range(0, 7).Select(x => (byte)Rnd.Range(0, 7)).ToArray(), true);
    }

    public override KugelblitzColor GetBaseColor()
    {
        return KugelblitzColor.Red;
    }

    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        particle.UpdateParticleData(
            Current().GetFromIndex(index) != 0 ? KugelblitzColor.GetColorFromIndex(index) : KugelblitzColor.None,
            ParticleMovement.FromOrthogonal(Current().GetFromIndex(index) - 1));
    }

    protected override string StageType()
    {
        return "Red quirk";
    }
}

public class KugelblitzOffsetData : KugelblitzData<KugelblitzOffsetData>
{
    private byte[] _data = new byte[7];

    public KugelblitzOffsetData(byte[] data)
    {
        _data = data;
        IsFinalResult = false;
    }

    public KugelblitzOffsetData(byte[] data, bool isFinalResult)
    {
        _data = data;
        IsFinalResult = isFinalResult;
    }

    public override KugelblitzOffsetData GetMerged(KugelblitzOffsetData secondEntry)
    {
        if (IsFinalResult)
            throw new InvalidOperationException();
        return new KugelblitzOffsetData(Enumerable.Range(0, 7).Select(x => (byte)((_data[x] + secondEntry._data[x]) % 7)).ToArray());
    }

    protected override KugelblitzOffsetData EmptyStage()
    {
        return new KugelblitzOffsetData(new byte[7]);
    }

    protected override KugelblitzOffsetData GetBridgeFrom(KugelblitzOffsetData currentState)
    {
        return new KugelblitzOffsetData(Enumerable.Range(0, 7).Select(x => (byte)((_data[x] - currentState._data[x] + 7) % 7)).ToArray());
    }

    protected override KugelblitzOffsetData RandomStage()
    {
        return new KugelblitzOffsetData(Enumerable.Range(0, 7).Select(x => (byte)Rnd.Range(0, 7)).ToArray());
    }

    public byte GetFromIndex(int index)
    {
        return _data[index];
    }

    public override string ToString()
    {
        if (IsFinalResult)
            return "[Offsets: " + _data.Select(x => "+" + ((x + 6) % 7 + 1)).Join(", ") + "]";
        else
            return "[" + _data.Join("") + "]";
    }
}
