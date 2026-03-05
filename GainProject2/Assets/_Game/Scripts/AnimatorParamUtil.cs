using UnityEngine;

public static class AnimatorParamUtil
{
    public static bool HasParameter(Animator animator, string name, AnimatorControllerParameterType type)
    {
        if (animator == null) return false;
        if (string.IsNullOrEmpty(name)) return false;

        var ps = animator.parameters;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].type == type && ps[i].name == name) return true;
        }
        return false;
    }

    public static void TrySetTrigger(Animator animator, string name)
    {
        if (!HasParameter(animator, name, AnimatorControllerParameterType.Trigger)) return;
        animator.SetTrigger(name);
    }
}