using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IKugelblitzStageViewer
{
    void UpdateParticles(KugelblitzParticle[] particles);

    KugelblitzColor GetBaseColor();
}

public interface IKugelblitzStageManager
{
    void Generate(int stageCount);

    /// <summary>
    /// Advances one stage
    /// </summary>
    /// <returns>Whether the final stage has just been passed</returns>
    bool Advance();
    void Return();
    void CollapseStages();
}

public interface IKugelblitzData<T> where T : IKugelblitzData<T>
{
    IEnumerable<T> BuildStageSet(int stageCount);
    T GetMerged(T secondEntry);
}

public abstract class KugelblitzStageViewer : IKugelblitzStageViewer
{
    public void UpdateParticles(KugelblitzParticle[] particles)
    {
        for (int i = 0; i < particles.Length; i++)
            UpdateParticle(particles[i], i);
    }

    public abstract KugelblitzColor GetBaseColor();

    protected abstract void UpdateParticle(KugelblitzParticle particle, int index);
}

public abstract class KugelblitzStageManager<T> : KugelblitzStageViewer, IKugelblitzStageManager where T : KugelblitzData<T>
{
    private List<T> _stages = new List<T>();
    private int _index = 0;

    protected T Target;

    public void Return()
    {
        _index = 0;
    }

    public bool Advance()
    {
        _index++;
        if (_index >= _stages.Count)
        {
            //Return();
            return true;
        }
        return false;
    }

    protected T Current()
    {
        return _stages[_index];
    }

    public void CollapseStages()
    {
        List<T> newStages = new List<T>();
        for (int i = 0; i < _stages.Count; i += 2)
        {
            if (i + 1 == _stages.Count)
            {
                newStages.Add(_stages[i]);
                continue;
            }

            newStages.Add(_stages[i].GetMerged(_stages[i + 1]));
        }

        _stages = newStages;
    }

    public void Generate(int stageCount)
    {
        Target = GenerateTarget();
        _stages = Target.BuildStageSet(stageCount).ToList();
    }

    protected abstract T GenerateTarget();

    public T GetFinalData()
    {
        return Target;
    }

    protected abstract string StageType();

    public override string ToString()
    {
        return StageType() + ": [" + _stages.Join(", ") + "]";
    }
}

public abstract class KugelblitzData<T> : IKugelblitzData<T> where T : KugelblitzData<T>
{
    protected bool IsFinalResult;

    public IEnumerable<T> BuildStageSet(int stageCount)
    {
        if (!IsFinalResult)
            throw new InvalidOperationException();

        List<T> stages = new List<T>();

        T currentState = EmptyStage();

        for (int i = 0; i < stageCount - 1; i++)
        {
            T newStage = RandomStage();
            currentState = currentState.GetMerged(newStage);
            stages.Add(newStage);
        }
        stages.Add(GetBridgeFrom(currentState));

        //shuffling to make sure the final stage constraint bias isn't an issue
        return stages.Shuffle();
    }

    public abstract T GetMerged(T secondEntry);

    protected abstract T GetBridgeFrom(T currentState);

    protected abstract T RandomStage();

    protected abstract T EmptyStage();
}