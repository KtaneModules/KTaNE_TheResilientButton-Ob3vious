using System.Collections;
using UnityEngine;

public class ResilientButtonScript : MonoBehaviour
{
    public const int TARGET_PRESS_DEPTH = 3;

    public KMSelectable Button;

    private int _pressDepth = 0;
    private bool _solved = false;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        Button.OnInteract += () =>
        {
            Button.AddInteractionPunch();
            _pressDepth++;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Button.transform);
            if (_solved)
                return false;

            Log("The button is pressed {0}!", _pressDepth > 3 ? (_pressDepth + " times") : new string[] { "once", "twice", "thrice" }[_pressDepth - 1]);

            if (_pressDepth >= TARGET_PRESS_DEPTH)
            {
                Log("The button has been pressed all the way. Module solved!");
                _solved = true;
                GetComponent<KMBombModule>().HandlePass();
            }
            return false;
        };

        Button.OnInteractEnded += () =>
        {
            if (!_solved)
                Log("The button has been released.");
            _pressDepth = 0;
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Button.transform);
        };
    }

    void Update()
    {
        float target = -0.0075f * Mathf.Min((float)_pressDepth / TARGET_PRESS_DEPTH, 1);
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
        Debug.LogFormat("[The Resilient Button #{0}] {1}", _moduleId, string.Format(text, args));
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
                if (_solved)
                {
                    yield return new WaitForSeconds(0.25f);
                    Button.OnInteractEnded();
                }
                break;
            case "release":
                if (_pressDepth <= 0)
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
            Button.OnInteract();
            yield return new WaitForSeconds(0.25f);
        }
        Button.OnInteractEnded();
        yield return null;
    }
}
