using System;
using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using RanksApi;

namespace Ranks_FakeRank;

[MinimumApiVersion(260)]
public class RanksFakeRank : BasePlugin
{
    public override string ModuleAuthor => "thesamefabius - updated by kMagic";
    public override string ModuleName => "[Ranks] Fake Rank";
    public override string ModuleVersion => "v1.0.3";

    private Config _config = new();
    private IRanksApi? _api;
    private int _maxRank;
    private bool _guidelinesEnabled = false;
    private bool _hasLoggedWarning = false;
    private int _debugCounter = 0;

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _api = IRanksApi.Capability.Get();
        if (_api == null) return;

        _config = _api.LoadConfig<Config>("ranks_fakerank");
        _maxRank = _config.FakeRank.Max(f => f.Key);

        // Use a timer instead of OnTick for better performance
        AddTimer(0.5f, UpdateRanks, TimerFlags.REPEAT);

        AddCommand("css_fakerank_reload", "", (player, info) =>
        {
            if (player != null) return;

            _config = _api.LoadConfig<Config>("ranks_fakerank");
            _maxRank = _config.FakeRank.Max(f => f.Key);
        });
    }

    private void UpdateRanks()
    {
        if (_api == null) return;

        var players = Utilities.GetPlayers()
            .Where(u => u is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected, IsBot: false, IsHLTV: false })
            .ToList();

        if (players.Count == 0) return;

        _debugCounter++;
        bool shouldDebug = _debugCounter % 20 == 0; // Debug every 10 seconds (20 * 0.5s)

        foreach (var player in players)
        {
            try
            {
                sbyte rankType;
                int rankValue;
                int rank = 0;

                player.CompetitiveWins = 111;
                
                if (_config.Type is 0)
                {
                    rankType = 11;
                    rankValue = _api.GetPlayerExperience(player);
                }
                else
                {
                    rankType = 12;
                    rank = _api.GetPlayerRank(player);
                    rankValue = _config.FakeRank[rank > _maxRank ? _maxRank : rank <= 0 ? 1 : rank];
                }

                player.CompetitiveRankType = rankType;
                player.CompetitiveRanking = rankValue;

                if (shouldDebug)
                {
                    Server.PrintToConsole($"[FakeRank DEBUG] Player: {player.PlayerName} | Rank: {rank} | RankValue: {rankValue} | RankType: {rankType}");
                }
            }
            catch (Exception ex)
            {
                if (!_hasLoggedWarning && ex.Message.Contains("FollowCS2ServerGuidelines"))
                {
                    _guidelinesEnabled = true;
                    _hasLoggedWarning = true;
                    Server.PrintToConsole("======================================");
                    Server.PrintToConsole("[Ranks_FakeRank] ERROR: FollowCS2ServerGuidelines is enabled!");
                    Server.PrintToConsole("[Ranks_FakeRank] This plugin cannot work with this setting enabled.");
                    Server.PrintToConsole("[Ranks_FakeRank] Please set 'FollowCS2ServerGuidelines: false' in core.json");
                    Server.PrintToConsole("======================================");
                }
                else if (shouldDebug)
                {
                    Server.PrintToConsole($"[FakeRank DEBUG] Error updating player: {ex.Message}");
                }
                
                if (_guidelinesEnabled)
                {
                    return;
                }
            }
        }

        // Always send rank reveal to all players
        if (!_guidelinesEnabled)
        {
            try
            {
                var msg = UserMessage.FromPartialName("CCSUsrMsg_ServerRankRevealAll");
                if (msg != null)
                {
                    if (shouldDebug)
                    {
                        Server.PrintToConsole($"[FakeRank DEBUG] Sending rank reveal to {players.Count} players");
                    }
                    msg.Send();
                }
                else if (shouldDebug)
                {
                    Server.PrintToConsole("[FakeRank DEBUG] Failed to create ServerRankRevealAll message");
                }
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"[Ranks_FakeRank] Error sending rank reveal message: {ex.Message}");
            }
        }
    }
}

public class Config
{
    public int Type { get; set; } = 1;

    public Dictionary<int, int> FakeRank { get; set; } = new()
    {
        [1] = 1,
        [2] = 2,
        [3] = 3,
        [4] = 4,
        [5] = 5,
        [6] = 6,
        [7] = 7,
        [8] = 8,
        [9] = 9,
        [10] = 10,
        [11] = 11,
        [12] = 12,
        [13] = 13,
        [14] = 14,
        [15] = 15,
        [16] = 16,
        [17] = 17,
        [18] = 18,
    };
}