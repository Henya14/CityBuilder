using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Xml.Linq;
using UnityEditor;
using System.Diagnostics.Tracing;

public class MenuUIManager : MonoBehaviour
{
    Label gameNameLabel;
    Button startButton;
    Button settingsButton;
    Button quitButton;
    Button backToMainMenuButton;
    VisualElement mainMenuContainer;
    VisualElement settingsContainer;

    private void OnEnable() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        gameNameLabel = root.Q<Label>("game-title-text");
        startButton = root.Q<Button>("start-game-button");
        settingsButton = root.Q<Button>("settings-button");
        quitButton = root.Q<Button>("quit-button");
        backToMainMenuButton = root.Q<Button>("back-to-mainmenu-button");
        mainMenuContainer = root.Q<VisualElement>("main-menu-container");
        settingsContainer = root.Q<VisualElement>("settings-container");

    }
    // Start is called before the first frame update
    void Start()
    {
        startButton.clicked += StartButton;
        settingsButton.clicked += SettingsButton;
        quitButton.clicked += QuitButton;
        backToMainMenuButton.clicked += MainMenuButton;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartButton() {
        SceneManager.UnloadSceneAsync(0);
        SceneManager.LoadScene(1);
    }
    void SettingsButton() {
        mainMenuContainer.style.display = DisplayStyle.None;
        settingsContainer.style.display = DisplayStyle.Flex;
    }
    void QuitButton() {
        Application.Quit();
    }
    void MainMenuButton() {
        settingsContainer.style.display = DisplayStyle.None;
        mainMenuContainer.style.display = DisplayStyle.Flex;
    }
}
