using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ValueStage : KugelblitzStageViewer
{
    private KugelblitzColor _color;
    private byte _count;

    public ValueStage(KugelblitzColor color, byte count)
    {
        _color = color;
        _count = count;
    }

    public override KugelblitzColor GetBaseColor()
    {
        return _color;
    }

    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        particle.UpdateParticleData(
            index < _count ? KugelblitzColor.White : KugelblitzColor.Black,
            ParticleMovement.FromRandom());
    }
}

public class EmptyStage : KugelblitzStageViewer
{
    private KugelblitzColor _color;

    public EmptyStage(KugelblitzColor color)
    {
        _color = color;
    }

    public override KugelblitzColor GetBaseColor()
    {
        return _color;
    }

    protected override void UpdateParticle(KugelblitzParticle particle, int index)
    {
        particle.UpdateParticleData(
            KugelblitzColor.None,
            ParticleMovement.FromRandom());
    }
}