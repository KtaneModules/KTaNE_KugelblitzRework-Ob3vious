using Assets.Module.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class KugelblitzScript : MonoBehaviour
{

    public static KugelblitzScript LastInstance;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMBossModule BossProperties;
    public KMAudio Audio;
    public KMColorblindMode Colorblind;

    public KugelblitzVoid Void;
    public KugelblitzParticle Particle;
    private List<KugelblitzParticle> _particles = new List<KugelblitzParticle>();

    public MeshRenderer Highlight;
    public VoidLinkage Linkage;

    private int _solves;

    private KugelblitzLobby _lobby = null;

    private bool _solvable = false;

    private bool _solved = false;

    private bool _colorblind;

    public Material[] SolveMats;

    void Awake()
    {
        LastInstance = this;
    }

    void Start()
    {
        for (int i = 0; i < 7; i++)
        {
            KugelblitzParticle particle = Instantiate(Particle, Particle.transform.parent);
            _particles.Add(particle);
        }
        Particle.GetComponent<MeshRenderer>().enabled = false;

        Highlight.enabled = false;
        Debug.Log(Linkage);
        Linkage = GetComponentInChildren<VoidLinkage>();
        Debug.Log(Linkage);
        Linkage.GetComponentInChildren<MeshRenderer>().enabled = false;

        AssignStage(new EmptyStage(KugelblitzColor.None));

        Module.OnActivate = () =>
        {
            _lobby = LobbyManager.GetRandomLobby();
            _lobby.Subscribe(this);
            _colorblind = Colorblind.ColorblindModeActive;
            UpdateColorblind();
        };
    }

    public void AssignStage(IKugelblitzStageViewer viewer)
    {
        viewer.UpdateParticles(_particles.ToArray());
        Void.SetColor(viewer.GetBaseColor());
    }

    void Update()
    {
        if (_lobby != null)
            _lobby.UpdateLobby();
    }

    public bool UpdateSolves()
    {
        int solves = CalculateSolveCount();
        if (solves != _solves)
        {
            _solves = solves;
            return true;
        }
        return false;
    }

    private int CalculateSolveCount()
    {
        return Bomb.GetSolvedModuleNames().Count(x => x != "Kugelblitz");
    }

    private int CalculateSolvableCount()
    {
        return Bomb.GetSolvableModuleNames().Count(x => x != "Kugelblitz");
    }

    public int RemainingSolves()
    {
        return CalculateSolvableCount() - CalculateSolveCount();
    }

    public void HighlightVoid()
    {
        Highlight.enabled = true;
    }

    public void EndHighlightVoid()
    {
        Highlight.enabled = false;
    }

    public VoidLinkage GetLinkCopy()
    {
        Debug.Log(Linkage);
        Debug.Log(this);
        Debug.Log(_lobby);
        return Instantiate(Linkage, Void.transform.parent);
    }


    public void EnterSolvableState(float pacing)
    {
        _solvable = true;
        _particles.ForEach(x => x.SetSpeed(1 / (2 * pacing)));
        Void.SetSpeed(1 / pacing);
    }

    public void ExitSolvablestate()
    {
        _solvable = false;
        _particles.ForEach(x => x.SetSpeed(1));
        Void.SetSpeed(1);
    }

    public bool IsSolvable()
    {
        return _solvable;
    }

    public void Pulse()
    {
        _particles.ForEach(x => x.Pulse());
    }

    public void PlaySound(string clipName)
    {
        Audio.PlaySoundAtTransform(clipName, transform.root);
    }

    public KMAudio.KMAudioRef PlayRefSound(string clipName)
    {
        return Audio.PlaySoundAtTransformWithRef(clipName, transform.root);
    }

    public void Solve()
    {
        Module.HandlePass();
        _solved = true;
    }

    public void UpdateColorblind()
    {
        _particles.ForEach(x => x.TextVisibility = _colorblind);
        Void.TextVisibility = _colorblind;
    }

    void OnDestroy()
    {
        _lobby.StopSound();
        LobbyManager.Reset();
    }


    public Queue<KugelblitzLobby.LobbyContentBuilder> GetQuirkSets()
    {
        return new Queue<KugelblitzLobby.LobbyContentBuilder>();
    }



#pragma warning disable 414
    private string TwitchHelpMessage = "'!{0} colorblind' to toggle colorblind mode, !{0} highlight' to highlight the module and find its connections, 'h'/'r'/'i' to hold, release or toggle interaction with the sphere (respectively). 'p'/'t' to wait for a pulse/tick from the module. Please do not execute stupid interactions like '!{0} rphh' or '!{0} iiiii'. Commands can be chained for the sake of solvability.";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if (command == "colorblind")
        {
            _colorblind = !_colorblind;
            UpdateColorblind();
        }
        else if (command == "highlight")
        {
            _lobby.Highlight();
            yield return new WaitForSeconds(1);
            _lobby.EndHighlight();
        }
        else if (command == "disconnect")
        {
            if (_lobby.Unsubscribe(this))
            {
                _lobby = LobbyManager.GetNewLobby();
                _lobby.Subscribe(this);
            }
            else
            {
                yield return "sendtochaterror {0}, unable to disconnect.";
                yield break;
            }
        }
        else if (command == "id")
        {
            yield return "sendtochat {0}, ID is " + _lobby.GetId();
        }
        else if (command.Split(' ')[0] == "connect" && command.Split(' ').Length == 2)
        {
            KugelblitzLobby newLobby = LobbyManager.GetLobbyFromId(command.Split(' ')[1]);

            if (newLobby == null)
            {
                yield return "sendtochaterror {0}, no such available id found.";
                yield break;
            }

            if (_lobby.Unsubscribe(this))
            {
                _lobby = newLobby;
                _lobby.Subscribe(this);
            }
            else
            {
                yield return "sendtochaterror {0}, unable to disconnect.";
                yield break;
            }
        }
        else
        {
            if (!_solvable)
            {
                yield return "sendtochaterror {0}, you might want to wait until it's actually ready.";
                yield break;
            }
            string validCommands = "ih[r]pt.";
            bool holding = false;
            for (int i = 0; i < command.Length; i++)
            {
                if (("h[".Contains(command[i]) && holding) || ("r]".Contains(command[i]) && !holding))
                {
                    yield return "sendtochaterror {0}, you messed up somewhere and made it invalid. Try again.";
                    yield break;
                }
                else
                {
                    if ("h[".Contains(command[i]))
                        holding = true;
                    else if ("r]".Contains(command[i]))
                        holding = false;
                    else if (command[i] == 'i')
                        holding = !holding;
                }
                if (!validCommands.Contains(command[i]))
                {
                    yield return "sendtochaterror {0}, " + command[i] + " is not a valid command.";
                    yield break;
                }
            }
            if (holding)
            {
                yield return "sendtochaterror {0}, you shouldn't be holding this thing forever. It's not safe. Mind checking those rules?";
                yield break;
            }

            KMSelectable sphere = Void.GetComponent<KMSelectable>();
            while (_particles[0].GetLerp() >= 0.125f) { yield return new WaitForSeconds(0.25f); }
            for (int i = 0; i < command.Length; i++)
            {
                yield return null;
                while (_particles[0].GetLerp() < 0.125f) { yield return null; }

                switch (command[i])
                {
                    case 'i':
                        if (!holding)
                            sphere.OnInteract();
                        else
                            sphere.OnInteractEnded();
                        holding = !holding;
                        break;

                    case 'h':
                    case '[':
                        sphere.OnInteract();
                        holding = true;
                        break;

                    case 'r':
                    case ']':
                        sphere.OnInteractEnded();
                        holding = false;
                        break;

                    case 'p':
                    case 't':
                    case '.':
                        while (_particles[0].GetLerp() >= 0.125f)
                            yield return null;
                        break;

                    default:
                        break;
                }
                yield return null;
            }
            yield return "strike";
            yield return "solve";
        }
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!_solvable)
            yield return true;
        yield return null;

        if (_lobby.IsAutosolveClaimed())
        {
            while (!_solved)
                yield return true;
            yield break;
        }

        _lobby.ClaimAutosolve();

        string command = (_lobby.IsSolvable() ? "" : "[.]") + KugelblitzCalculation.Calculate(_lobby);

        KMSelectable sphere = Void.GetComponent<KMSelectable>();
        while (_particles[0].GetLerp() >= 0.125f) { yield return new WaitForSeconds(0.25f); }
        for (int i = 0; i < command.Length; i++)
        {
            yield return null;
            while (_particles[0].GetLerp() < 0.125f) { yield return null; }

            switch (command[i])
            {
                case '[':
                    sphere.OnInteract();
                    break;

                case ']':
                    sphere.OnInteractEnded();
                    break;

                case '.':
                    while (_particles[0].GetLerp() >= 0.125f)
                        yield return null;
                    break;

                default:
                    break;
            }
            yield return null;
        }
    }

}
