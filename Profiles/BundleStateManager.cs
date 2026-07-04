using System.Collections.Generic;

namespace Teuria.Profiles;

public class BundleStateManager
{
    public static BundleStateManager Instance { get; private set; } = null!;

    private Stack<BundleState> states = [];


    public BundleStateManager()
    {
        Instance = this;
    }

    public BundleState CreateBundle()
    {
        return new BundleState();
    }


    public void Push(BundleState state)
    {
        states.Push(state);
    }

    public BundleState Pop()
    {
        return states.Pop();
    }

    public BundleState Peek()
    {
        return states.Peek();
    }
}
