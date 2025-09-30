using UnityEngine;
using System.Collections.Generic;

public class EditorModMenu : MonoBehaviour
{
    private Rect windowRect = new Rect(Screen.width-300, Screen.height - 20, 250, 300);
    private bool isMinimized = true;

    private List<(string label, System.Action action)> buttons;

    void Start()
    {
        buttons = new List<(string, System.Action)>
        {
            ("+100 Money", () => GameManager.Instance.AddMoney(100)),
            ("X2 Money", () => GameManager.Instance.SetPlayerMoney(GameManager.Instance.GetPlayerMoney() * 2)),
            ("Unlock All", () => {GameManager.Instance.unlockedBuildings = GameManager.Instance.allBuildings;}),
        };
    }

    void OnGUI()
    {
        // Dibuja la ventana
        windowRect = GUI.Window(0, windowRect, DrawWindow, "Mod Menu");
    }

    void DrawWindow(int windowID)
    {
        // Botón para minimizar
        if (GUI.Button(new Rect(windowRect.width - 25, 5, 20, 20), isMinimized ? "+" : "-"))
        {
            isMinimized = !isMinimized;
        }

        if (!isMinimized)
        {
            GUILayout.BeginVertical();

            foreach (var (label, action) in buttons)
            {
                if (GUILayout.Button(label))
                {
                    action.Invoke();
                }
            }
            windowRect.height = 300;
            windowRect.position = new Vector2(windowRect.x, Screen.height - 300);

            GUILayout.EndVertical();
        }
        else
        {
            windowRect.height = 50;
            windowRect.position = new Vector2(windowRect.x, Screen.height - 25);
        }

        // Hacer la ventana arrastrable
        // GUI.DragWindow(new Rect(0, 0, Screen.width, 20));
    }
}