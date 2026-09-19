using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BotPlayer.Client;
using BotPlayer.Windows;

namespace BotPlayer.Classes;

public class SessionManager
{
    public static readonly SessionManager Instance = new();

    private readonly Stack<Session> _sessions = [];

    public async Task InitSession(IProgress<int> progress)
    {
        var totalAccounts = WindowExecution.Instance.Accounts.Length;

        if (totalAccounts == 0)
        {
            progress.Report(0);
            return;
        }

        for (var i = 0; i < totalAccounts; i++)
        {
            try
            {
                var account = WindowExecution.Instance.Accounts[i];
                var session = new Session(account);
                _sessions.Push(session);
                progress.Report(i + 1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi tạo phiên: {ex.Message}");
            }

            await Task.Delay(100);
        }
    }

    public async Task Dispose()
    {
        var sessionsToDispose = new List<Session>();
        while (_sessions.TryPop(out var session)) sessionsToDispose.Add(session);
        foreach (var session in sessionsToDispose)
        {
            try
            {
                session.CleanNetwork();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi đóng phiên: {ex.Message}");
            }

            await Task.Delay(50);
        }
    }

    public async Task Dispose(IProgress<int> progress)
    {
        var sessionCount = _sessions.Count;

        if (sessionCount == 0)
        {
            progress.Report(0);
            return;
        }

        var sessionsToDispose = new List<Session>(sessionCount);
        while (_sessions.TryPop(out var session)) sessionsToDispose.Add(session);
        for (var i = 0; i < sessionsToDispose.Count; i++)
        {
            try
            {
                sessionsToDispose[i].CleanNetwork();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi đóng phiên: {ex.Message}");
            }

            progress.Report(i + 1);
            await Task.Delay(50);
        }

        progress.Report(sessionCount);
    }
}