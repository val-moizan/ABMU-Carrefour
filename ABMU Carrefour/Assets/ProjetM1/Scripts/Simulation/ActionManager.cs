using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager
{
    private List<Action> actions;
    public ActionManager()
    {
        actions = new List<Action>();
    }

    public void addAction(Action action)
    {
        actions.Add(action);
    }

    public List<Action> getActions()
    {
        return this.actions;
    }

    public Action getFirstAction()
    {
        return actions.Count == 0 ? null : actions[0];
    }
    public void removeFirstAction()
    {
        this.actions.RemoveAt(0);
    }
    public void clearAllActions()
    {
        this.actions.Clear();
    }

}
