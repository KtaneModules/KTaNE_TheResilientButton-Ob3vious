using System.Collections;
using UnityEngine;

public class DefiantButtonScript : MonoBehaviour
{
    public const int BASE_PRESS_DEPTH = 1;

    public KMSelectable Button;

    private int _pressDepth = BASE_PRESS_DEPTH;
    private bool _solved = false;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        Button.OnInteract += () =>
        {
            Button.AddInteractionPunch();
            if (!_solved)
                Log("The button has been pressed.");
            _pressDepth = BASE_PRESS_DEPTH + 1;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Button.transform);
            return false;
        };

        Button.OnInteractEnded += () =>
        {
            _pressDepth--;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Button.transform);
            if (_solved)
                return;

            Log("The button is released {0}!", (BASE_PRESS_DEPTH - _pressDepth + 1) > 3 ? ((BASE_PRESS_DEPTH - _pressDepth + 1) + " times") : new string[] { "once", "twice", "thrice" }[BASE_PRESS_DEPTH - _pressDepth]);

            if (_pressDepth <= 0)
            {
                Log("The button has been released all the way. Module solved!");
                _solved = true;
                GetComponent<KMBombModule>().HandlePass();
            }
        };
    }

    void Update()
    {
        float target = -0.0075f * Mathf.Min((float)_pressDepth / (BASE_PRESS_DEPTH + 1), 1);
        float diff = Time.deltaTime * 0.02f;
        if (Mathf.Abs(target - Button.transform.localPosition.y) < diff)
            Button.transform.localPosition += new Vector3(0, target - Button.transform.localPosition.y, 0);
        else if (target > Button.transform.localPosition.y)
            Button.transform.localPosition += new Vector3(0, diff, 0);
        else
            Button.transform.localPosition -= new Vector3(0, diff, 0);
    }

    private void Log(string text, params object[] args)
    {
        Debug.LogFormat("[The Defiant Button #{0}] {1}", _moduleId, string.Format(text, args));
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "'!{0} hold' to hold the button. '!{0} release' to release the button.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        command = command.ToLowerInvariant();
        switch (command)
        {
            case "hold":
                Button.OnInteract();
                break;
            case "release":
                if (_pressDepth <= BASE_PRESS_DEPTH)
                {
                    yield return "sendtochaterror Please press the button before attempting to release it.";
                    yield break;
                }
                Button.OnInteractEnded();
                break;
            default:
                yield return "sendtochaterror Invalid command.";
                yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!_solved)
        {
            Button.OnInteractEnded();
            yield return new WaitForSeconds(0.25f);
        }
        yield return null;
    }
}
