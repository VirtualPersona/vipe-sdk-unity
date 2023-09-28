using System;
using System.Threading;
using System.Threading.Tasks;

public class MainThreadDispatcher
{
    private static MainThreadDispatcher instance;
    private static SynchronizationContext mainSyncContext;

    private MainThreadDispatcher()
    {
        mainSyncContext = SynchronizationContext.Current;
    }

    public static MainThreadDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MainThreadDispatcher();
            }
            return instance;
        }
    }

    public static void RunAsyncTask(Task task, Action continuation)
    {
        task.ContinueWith(t =>
        {
            mainSyncContext.Post(_ => continuation(), null);
        });
    }

    public static void RunOnMainThread(Action action)
    {
        if (instance == null)
        {
            throw new Exception("No MainThreadDispatcher instance found. Please initialize one.");
        }
        mainSyncContext.Post(_ => action(), null);
    }
}
