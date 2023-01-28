using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rnd = UnityEngine.Random;

public class TurnStageManager : KugelblitzStageManager<KugelblitzTurnData>
{
    protected override KugelblitzTurnData GenerateTarget()
    {
        return new KugelblitzTurnData(Enumerable.Range(0, 6).Select(x => (byte)Rnd.Range(0, 3)).ToArray());
    }

    public override KugelblitzColor GetBaseColor()
    {
        return KugelblitzColor.Blue;
    }

    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        particle.UpdateParticleData(
            Current().GetFromIndex(index) != 0 ? KugelblitzColor.GetColorFromIndex(index) : KugelblitzColor.None,
            ParticleMovement.FromDiagonal(Current().GetFromIndex(index) == 2));
    }

    protected override string StageType()
    {
        return "Blue quirk";
    }
}

public class KugelblitzTurnData : KugelblitzData<KugelblitzTurnData>
{
    private byte[] _data = new byte[7];

    public KugelblitzTurnData(byte[] data)
    {
        IsFinalResult = data.Length == 6;

        if (IsFinalResult)
        {
            byte shift = (byte)Rnd.Range(0, 3);
            data = data.Select(x => (byte)((x - shift + 3) % 3)).ToArray();
            data = data.Take(4).Concat(new byte[] { shift }).Concat(data.Skip(4)).ToArray();
        }

        if (data.Length != 7)
            throw new InvalidOperationException();
        _data = data;
    }

    public override KugelblitzTurnData GetMerged(KugelblitzTurnData secondEntry)
    {
        if (IsFinalResult)
            throw new InvalidOperationException();
        return new KugelblitzTurnData(Enumerable.Range(0, 7).Select(x => (byte)((_data[x] + secondEntry._data[x]) % 3)).ToArray());
    }

    protected override KugelblitzTurnData EmptyStage()
    {
        return new KugelblitzTurnData(new byte[7]);
    }

    protected override KugelblitzTurnData GetBridgeFrom(KugelblitzTurnData currentState)
    {
        return new KugelblitzTurnData(Enumerable.Range(0, 7).Select(x => (byte)((_data[x] - currentState._data[x] + 3) % 3)).ToArray());
    }

    protected override KugelblitzTurnData RandomStage()
    {
        return new KugelblitzTurnData(Enumerable.Range(0, 7).Select(x => (byte)Rnd.Range(0, 3)).ToArray());
    }

    public byte GetFromIndex(int index)
    {
        if (IsFinalResult)
            return _data.Take(4).Concat(_data.Skip(5)).Select(x => (byte)((x + _data[5]) % 3)).ToArray()[index];

        return _data[index];
    }

    public override string ToString()
    {
        if (IsFinalResult)
            return "[Turns: " + Enumerable.Range(0, 6).Select(x => (GetFromIndex(x) + 2) % 3 + 1).Join("") + "]";
        else
            return "[" + _data.Join("") + "]";
    }
}
