using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnityEngine;


public static class LobbyManager
{
    private static List<KugelblitzLobby> _currentLobbies = new List<KugelblitzLobby>();
    private static Queue<KugelblitzLobby.LobbyContentBuilder> _presets = new Queue<KugelblitzLobby.LobbyContentBuilder>();
    private static bool _hasCheckedForMission = false;

    private static List<KugelblitzScript> _interestedModules = new List<KugelblitzScript>();


    public static void Subscribe(KugelblitzScript kugelblitz)
    {
        _interestedModules.Add(kugelblitz);
        if (GetAvailableLobbies().Sum(x => x.OpenSlots()) < _interestedModules.Count)
            GetNewLobby();
    }

    //TP no mission only
    public static void Unsubscribe(KugelblitzScript kugelblitz)
    {
        _interestedModules.Remove(kugelblitz);
    }



    public static void Reset()
    {
        _currentLobbies.Clear();
        _presets.Clear();
        _hasCheckedForMission = false;
    }

    public static KugelblitzLobby GetNewLobby()
    {
        KugelblitzLobby lobby = new KugelblitzLobby(GetContent(), GetNewId());
        _currentLobbies.Add(lobby);
        return lobby;
    }

    public static KugelblitzLobby GetLobbyFromId(string id)
    {
        try
        {
            return GetAvailableLobbies().First(x => x.GetId() == id);
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<KugelblitzLobby> GetRequestingLobbies()
    {
        return _currentLobbies.Where(x => x.IsAvailable());
    }

    private static IEnumerable<KugelblitzLobby> GetAvailableLobbies()
    {
        return _currentLobbies.Where(x => x.IsAvailable());
    }

    public static KugelblitzLobby GetRandomLobby()
    {
        IEnumerable<KugelblitzLobby> availableLobbies = GetRequestingLobbies();
        if (availableLobbies.Count() > 0)
            return availableLobbies.PickRandom();

        availableLobbies = GetAvailableLobbies();
        if (availableLobbies.Count() == 0)
            return GetNewLobby();

        List<KugelblitzLobby> lobbiesByDensity = new List<KugelblitzLobby>();
        foreach (KugelblitzLobby lobby in availableLobbies)
            for (int i = 0; i < lobby.OpenSlots(); i++)
                lobbiesByDensity.Add(lobby);
        return lobbiesByDensity.PickRandom();
    }




    private static KugelblitzLobby.LobbyContentBuilder GetContent()
    {
        CheckPresets();
        return _presets.Count > 0 ? _presets.Dequeue() : new KugelblitzLobby.LobbyContentBuilder();
    }

    private static string GetNewId()
    {
        for (int i = 0; i < _currentLobbies.Count + 1; i++)
            if (_currentLobbies.All(x => x.GetId() != (i + 1).ToString()))
                return (i + 1).ToString();
        return "id";
    }

    private static void CheckPresets()
    {
        if (_hasCheckedForMission)
            return;

        _presets = KugelblitzScript.LastInstance.GetQuirkSets();
        _hasCheckedForMission = true;
    }
}
