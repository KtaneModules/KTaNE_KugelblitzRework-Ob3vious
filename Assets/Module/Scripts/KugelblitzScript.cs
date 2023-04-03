using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using KTMissionGetter;
using Rnd = UnityEngine.Random;
using System.Text.RegularExpressions;
using System.IO;

public class KugelblitzScript : MonoBehaviour
{

    public static KugelblitzScript LastInstance;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMBossModule BossProperties;
    public KMAudio Audio;
    public KMColorblindMode Colorblind;
    public KMModSettings Settings;

    public KugelblitzVoid Void;
    public KugelblitzParticle Particle;
    private List<KugelblitzParticle> _particles = new List<KugelblitzParticle>();

    public MeshRenderer Highlight;
    public VoidLinkage Linkage;

    private int _solves;
    private int _stagesAdvanced = 0;

    private KugelblitzLobby _lobby = null;

    private bool _solvable = false;

    private bool _solved = false;

    private bool _colorblind;

    public Material[] SolveMats;


    private string[] _ignoreList = { "Kugelblitz" };


    private static int _moduleIdCounter = 1;
    private int _moduleId;

    void Awake()
    {
        LastInstance = this;
        _moduleId = _moduleIdCounter++;
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
        Linkage.GetComponentInChildren<MeshRenderer>().enabled = false;

        AssignStage(new EmptyStage(KugelblitzColor.None));

        Module.OnActivate = () =>
        {
            _ignoreList = BossProperties.GetIgnoredModules("Kugelblitz", _ignoreList);

            if (RemainingSolves() < 2)
            {
                Log("Forced into a solve state due to lack of stages.");
                Module.HandlePass();
                AssignStage(new EmptyStage(KugelblitzColor.None));
                return;
            }

            _colorblind = Colorblind.ColorblindModeActive;
            UpdateColorblind();

            LobbyManager.Subscribe(this);
            StartCoroutine(WaitForLobby());
        };
    }

    public IEnumerator WaitForLobby()
    {
        yield return null;
        _lobby = LobbyManager.GetRandomLobby();
        _lobby.Subscribe(this);
        LobbyManager.Unsubscribe(this);
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
            _stagesAdvanced = solves - _solves;
            _solves = solves;
            return true;
        }
        return false;
    }

    private int CalculateSolveCount()
    {
        return Bomb.GetSolvedModuleNames().Count(x => !_ignoreList.Contains(x));
    }

    private int CalculateSolvableCount()
    {
        return Bomb.GetSolvableModuleNames().Count(x => !_ignoreList.Contains(x));
    }

    public int RemainingSolves()
    {
        return CalculateSolvableCount() - CalculateSolveCount();
    }

    public int StagesAdvanced()
    {
        return _stagesAdvanced;
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
        return Instantiate(Linkage, Void.transform.parent);
    }


    public void EnterSolvableState(float pacing)
    {
        _solvable = true;
        _particles.ForEach(x => x.SetSpeed(1 / (2 * pacing)));
        Void.SetSpeed(1 / pacing);
    }

    public void ExitSolvableState()
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


#pragma warning disable 414
    private string TwitchHelpMessage = "'!{0} colorblind' to toggle colorblind mode, !{0} highlight' to highlight the module and find its connections, !{0} disconnect to disconnect the module from its current group, !{0} id to retrieve the current group id, !{0} connect 1 to connect to the group with id 1, 'h'/'r'/'i' to hold, release or toggle interaction with the sphere (respectively). 'p'/'t' to wait for a pulse/tick from the module. Please do not execute stupid interactions like '!{0} rphh' or '!{0} iiiii'. Interactions can be chained (e.g. httrthr)";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (_lobby == null)
        {
            yield return "sendtochaterror {0}, please wait until the module has loaded.";
            yield break;
        }

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

            yield return "strike";
            yield return "solve";

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

        string command = (_lobby.IsSolvable() ? "" : "[.]") + _lobby.ExpectedInput;

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



    public class ModSettings
    {
        public string Preset = "???????";
        public float Pacing = 2.5f;
    }

    public string GetPreset()
    {
        try
        {
            ModSettings settings = JsonConvert.DeserializeObject<ModSettings>(Settings.Settings);
            if (settings.Preset.Length != 7 || settings.Preset.Any(x => !"+-?".Contains(x)))
            {
                settings.Preset = "???????";
                File.WriteAllText(Settings.SettingsPath, JsonConvert.SerializeObject(settings));
            }
            return settings.Preset;
        }
        catch
        {
            ModSettings settings = new ModSettings();
            File.WriteAllText(Settings.SettingsPath, JsonConvert.SerializeObject(settings));
            return "???????";
        }
    }

    public float GetPacing()
    {
        try
        {
            ModSettings settings = JsonConvert.DeserializeObject<ModSettings>(Settings.Settings);
            if (settings.Pacing > 10f || settings.Pacing < 0.25f)
            {
                settings.Pacing = Mathf.Clamp(settings.Pacing, 0.25f, 10f);
                File.WriteAllText(Settings.SettingsPath, JsonConvert.SerializeObject(settings));
            }
            return Mathf.Clamp(settings.Pacing, 0.25f, 10f);
        }
        catch
        {
            ModSettings settings = new ModSettings();
            File.WriteAllText(Settings.SettingsPath, JsonConvert.SerializeObject(settings));
            return 2.5f;
        }
    }



    public Queue<KugelblitzLobby.LobbyContentBuilder> GetQuirkSets()
    {
        Queue<KugelblitzLobby.LobbyContentBuilder> presets = new Queue<KugelblitzLobby.LobbyContentBuilder>();

        string missionDesc = Mission.Description;

        if (missionDesc == null)
            return new Queue<KugelblitzLobby.LobbyContentBuilder>();

        string pacingRegex = @"\d{1,2}(\.\d{1,3})?";
        string presetRegex = @"[1-8]{0,2}[+\-?]{7}";
        string poolRegex = "(" + pacingRegex + ";)?" + presetRegex;

        Regex regex = new Regex(@"\[Kugelblitz\]:(" + poolRegex + @",)*" + poolRegex);
        var match = regex.Match(missionDesc);
        if (!match.Success)
            return new Queue<KugelblitzLobby.LobbyContentBuilder>();

        string[] options = match.Value.Replace("[Kugelblitz]:", "").Split(',');

        foreach (string preset in options)
        {
            try
            {
                string[] segments = preset.Split('/');

                string presetCode = segments.Last();
                float time = 2.5f;
                if (segments.Length == 2)
                    time = float.Parse(segments[0]);

                if (time < 0.25f || time > 10)
                    throw new ArgumentOutOfRangeException();

                switch (presetCode.Length)
                {
                    case 7:
                        if (segments.Length == 2)
                            presets.Enqueue(new KugelblitzLobby.LobbyContentBuilder(1, 8, presetCode, time));
                        else
                            presets.Enqueue(new KugelblitzLobby.LobbyContentBuilder(1, 8, presetCode));
                        break;
                    case 8:
                        if (segments.Length == 2)
                            presets.Enqueue(new KugelblitzLobby.LobbyContentBuilder(presetCode[0] - '0', presetCode[0] - '0', presetCode.Substring(1), time));
                        else
                            presets.Enqueue(new KugelblitzLobby.LobbyContentBuilder(presetCode[0] - '0', presetCode[0] - '0', presetCode.Substring(1)));
                        break;
                    case 9:
                        if (segments.Length == 2)
                            presets.Enqueue(new KugelblitzLobby.LobbyContentBuilder(presetCode[0] - '0', presetCode[1] - '0', presetCode.Substring(2), time));
                        else
                            presets.Enqueue(new KugelblitzLobby.LobbyContentBuilder(presetCode[0] - '0', presetCode[1] - '0', presetCode.Substring(2)));
                        break;
                    default:
                        break;
                }
            }
            catch
            {
                return new Queue<KugelblitzLobby.LobbyContentBuilder>();
            }
        }
        return presets;
    }



    public override string ToString()
    {
        return "[Kugelblitz #" + _moduleId + "]";
    }

    public void Log(string line)
    {
        Debug.LogFormat("[Kugelblitz #{0}] {1}", _moduleId, line);
    }
}
