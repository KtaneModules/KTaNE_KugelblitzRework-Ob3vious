using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class WrapStageManager : KugelblitzStageManager<KugelblitzWrapData>
{
    protected override KugelblitzWrapData GenerateTarget()
    {
        //swapping axes has a 1/5 chance to equalise between all 20 transforms
        if (Rnd.Range(0, 5) == 4)
            return new KugelblitzWrapData(Rnd.Range(0, 2) == 1, Rnd.Range(0, 2) == 1);
        return new KugelblitzWrapData(Rnd.Range(0, 2) == 1, Rnd.Range(0, 2) == 1, Rnd.Range(0, 2) == 1, Rnd.Range(0, 2) == 1);
    }

    public override KugelblitzColor GetBaseColor()
    {
        return KugelblitzColor.Violet;
    }

    private ParticleMovement _pattern;
    private List<int> _indices;

    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        if (index == 0)
        {
            _pattern = ParticleMovement.FromRandom();
            _indices = Enumerable.Range(0, 12).ToList().Shuffle();
        }
        particle.UpdateParticleData(
            Current().GetFromIndex(index) ? KugelblitzColor.GetColorFromIndex(index) : KugelblitzColor.None, 
            ParticleMovement.FromChase(_pattern, _indices[index] / 12f));
    }
}

public class KugelblitzWrapData : KugelblitzData<KugelblitzWrapData>
{
    private bool[] _data = new bool[7];

    //swap X and Y
    public KugelblitzWrapData(bool flipX, bool flipY)
    {
        //select the only value pair that evaluates to true as bitwise AND
        for (int i = 0; i < 2; i++)
            _data[i] = true;

        //randomly pick either x and either y to put the parity bit in
        _data[Rnd.Range(0, 2) * 2 + 2] = flipX;
        _data[Rnd.Range(0, 2) * 2 + 3] = flipY;

        //randomly flip x and y pair as the xor stays the same
        for (int i = 0; i < 2; i++)
            if (Rnd.Range(0, 2) == 1)
                for (int j = 0; j < 2; j++)
                    _data[i + 2 + 2 * j] ^= true;

        //invert entire array; last bit indicates whether inverted
        if (Rnd.Range(0, 2) == 1)
            _data = _data.Select(x => !x).ToArray();

        IsFinalResult = true;
    }

    //no swap X and Y
    public KugelblitzWrapData(bool hFlipX, bool hFlipY, bool vFlipX, bool vFlipY)
    {
        //select a random value pair that evaluates to false as bitwise AND
        int randomFalse = Rnd.Range(0, 3);
        for (int i = 0; i < 2; i++)
            _data[i] = ((randomFalse >> (1 - i)) & 1) == 1;

        _data[2] = hFlipX;
        _data[3] = hFlipY;
        _data[4] = vFlipX;
        _data[5] = vFlipY;

        //invert entire array; last bit indicates whether inverted
        if (Rnd.Range(0, 2) == 1)
            _data = _data.Select(x => !x).ToArray();

        IsFinalResult = true;
    }

    private KugelblitzWrapData(bool[] data)
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

    public WrapTransform GetHWrap()
    {
        if (!IsFinalResult)
            throw new InvalidOperationException();

        bool[] paritylessData = _data.Take(6).Select(x => x ^ _data[6]).ToArray();
        if (paritylessData[0] && paritylessData[1])
            return new WrapTransform(paritylessData[2] ^ paritylessData[4], paritylessData[3] ^ paritylessData[5], true);
        return new WrapTransform(paritylessData[2], paritylessData[3], false);
    }

    public WrapTransform GetVWrap()
    {
        if (!IsFinalResult)
            throw new InvalidOperationException();

        bool[] paritylessData = _data.Take(6).Select(x => x ^ _data[6]).ToArray();
        if (paritylessData[0] && paritylessData[1])
            return new WrapTransform(paritylessData[2] ^ paritylessData[4], paritylessData[3] ^ paritylessData[5], true);
        return new WrapTransform(paritylessData[4], paritylessData[5], false);
    }

    public override KugelblitzWrapData GetMerged(KugelblitzWrapData secondEntry)
    {
        if (IsFinalResult)
            throw new InvalidOperationException();
        return new KugelblitzWrapData(Enumerable.Range(0, 7).Select(x => _data[x] ^ secondEntry._data[x]).ToArray());
    }

    protected override KugelblitzWrapData GetBridgeFrom(KugelblitzWrapData currentState)
    {
        return new KugelblitzWrapData(Enumerable.Range(0, 7).Select(x => _data[x] ^ currentState._data[x]).ToArray());
    }

    protected override KugelblitzWrapData EmptyStage()
    {
        return new KugelblitzWrapData(new bool[7]);
    }

    protected override KugelblitzWrapData RandomStage()
    {
        return new KugelblitzWrapData(Enumerable.Range(0, 7).Select(x => Rnd.Range(0, 2) == 1).ToArray());
    }

    public override string ToString()
    {
        if (IsFinalResult)
            return "[H-wrap=(" + GetHWrap() + "); V-wrap=(" + GetVWrap() + ")]";
        else
            return "[" + _data.Select(x => x ? 1 : 0).Join("") + "]";
    }
}

public class WrapTransform
{
    private bool _flipX;
    private bool _flipY;
    private bool _swapXY;

    public WrapTransform(bool flipX, bool flipY, bool swapXY)
    {
        _flipX = flipX;
        _flipY = flipY;
        _swapXY = swapXY;
    }

    public bool GetFlipX()
    {
        return _flipX;
    }

    public bool GetFlipY()
    {
        return _flipY;
    }

    public bool GetSwapXY()
    {
        return _swapXY;
    }

    public override string ToString()
    {
        return (_flipX ? "-" : "+") + (_flipY ? "-" : "+") + (_swapXY ? "\\" : "");
    }
}