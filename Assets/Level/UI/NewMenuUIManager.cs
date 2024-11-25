using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class NewMenuUIManager : MonoBehaviour
{
    Label title;
    Button startButton;
    Button firstTutorialButton;
    Button secondTutorialButton;
    Button thirdTutorialButton;
    Button exitButton;
    VisualElement menuContainer;


    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        title = root.Q<Label>("title");
        startButton = root.Q<Button>("start-button");
        firstTutorialButton = root.Q<Button>("first-tutorial-button");
        secondTutorialButton = root.Q<Button>("second-tutorial-button");
        thirdTutorialButton = root.Q<Button>("third-tutorial-button");
        exitButton = root.Q<Button>("exit-button");
        menuContainer = root.Q<VisualElement>("menu-container");
    }
    // Start is called before the first frame update
    void Start()
    {
        exitButton.clicked += Application.Quit;
        startButton.clicked += StartGame;
        firstTutorialButton.clicked += FirstTutorial;
        secondTutorialButton.clicked += SecondTutorial;
        thirdTutorialButton.clicked += ThirdTutorial;
    }
    void StartGame()
    {
        SceneManager.LoadScene("Main2");
    }
    void FirstTutorial()
    {
        SceneManager.LoadScene("ResourceTutorial1");
    }
    void SecondTutorial()
    {
        SceneManager.LoadScene("Main2");
    }
    void ThirdTutorial()
    {
        SceneManager.LoadScene("Main2");
    }


}
