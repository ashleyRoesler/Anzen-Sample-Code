using ReimEnt.Cloud;
using System.Collections.Generic;
using UnityEngine.UI;
using ReimEnt.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class MainMenuUIController : UIController {

    private readonly Logger _logger = new Logger("Main Menu");

    [Required]
    public Button CreateGameButton;
    public Transform ContextMenuLocation;

    [Space]
    public Button GameMenuButton;

    [Required]
    public GameModeSelectionUIController GameModeSelector;

    [Space]
    [Required]
    public Text GameModeCategoryText;
    [Required]
    public Text SubHeaderText;

    [Space]
    public string SelectedCategory = "Campaign";
    public GameMode SelectedGameMode;
    public Level SelectedLevel;

    private static string _lastCategory;
    private static GameMode _lastGameMode = null;
    private static Level _lastLevel = null;

    private void Awake() {
        GameMenuButton.onClick.AddListener(() => {
            FindObjectOfType<GameMenuController>().ToggleShow();
        });

        CreateGameButton.onClick.AddListener(CreateGameButton_Clicked);

        GameModeSelector.GameModeSelected += GameMode_Selected;

        // set to default or set to last game mode chosen (right now it defaults to the hidden sanctum)
        if (_lastGameMode)
            GameMode_Selected(_lastCategory, _lastGameMode, _lastLevel);
        else
            GameMode_Selected(SelectedCategory, SelectedGameMode, SelectedLevel);
    }

    private void OnDisable() {
        GameModeSelector.GameModeSelected -= GameMode_Selected;
    }

    private void CreateGameButton_Clicked() {
        List<ContextMenuItem> gameTypes = new List<ContextMenuItem> {
            new ContextMenuItem("Offline", StartLocalButton_Clicked),
            new ContextMenuItem("Online", StartCloudButton_Clicked)
        };

        if (ContextMenuLocation)
            ContextMenus.Show(ContextMenuLocation.position, gameTypes.ToArray());
        else
            ContextMenus.Show(gameTypes.ToArray());
    }

    private async void StartLocalButton_Clicked() {

        if (!SelectedGameMode || !SelectedLevel) 
            return;

        await Network.Start(NetworkMode.Host);
        Room.Start(SelectedGameMode, SelectedLevel);
    }

    private void StartCloudButton_Clicked() {

        if (!SelectedGameMode || !SelectedLevel) 
            return;

        StartCloud(SelectedGameMode, SelectedLevel);
    }

    private async void StartCloud(GameMode gameMode, Level level) {
        LoadingOperation operation = Loading.Start("Requesting cloud server...");
        try {

            var builds = await Cloud.Admin.FetchBuilds();
            builds.RemoveAll(b => b.BuildId == null);

            if (builds.Count == 0) 
                throw new CloudException("There are no server builds available on the cloud");

            var response = await Cloud.RequestServer(builds[0].BuildId, gameMode, level);
            _logger.Log($"Received server request confirmation. Address: {response.IPV4Address}, Port: {response.Ports[0].Num}");

            Cloud.IsInACloudGame = true;

            await Network.Connect(response.IPV4Address, (ushort)response.Ports[0].Num);

        } catch (CloudException ex) {
            Dialogs.Request(ex.Message);
            Cloud.IsInACloudGame = false;
        } finally {
            Loading.End(operation);
        }
    }

    private void GameMode_Selected(string category, GameMode gameMode, Level level) {

        _lastCategory = category;
        _lastGameMode = gameMode;
        _lastLevel = level;

        SelectedCategory = category;
        SelectedGameMode = gameMode;
        SelectedLevel = level;

        GameModeCategoryText.text = category;

        if (category == "Arena") {
            SubHeaderText.text = gameMode.DisplayName;
        }
        else {
            SubHeaderText.text = level.name;
        }
    }
}