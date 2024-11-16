﻿using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetterUpdates
{
    public class RegexResult
    {
        public bool Success { get; set; }
        public string Name { get; set; }
        public string OldVersion { get; set; }
        public string NewVersion { get; set; }
    }

    public class BetterUpdates : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private BetterUpdatesSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("3ddbdb43-d361-414c-876f-2b923ba6c21b");
        
        private IDialogsFactory Dialogs => PlayniteApi.Dialogs;
        private INotificationsAPI Notifs => PlayniteApi.Notifications;
        private IItemCollection<Game> GameDatabase => PlayniteApi.Database.Games;

        public BetterUpdates(IPlayniteAPI api) : base(api)
        {
            settings = new BetterUpdatesSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new BetterUpdatesSettingsView(this);
        }

        private RegexResult RegexCheck(NotificationMessage message)
        {
            // REGEX Patterns
            string patternName = @"Game update available: (.*?), link:";
            string patternVersion = @"Old Version:\s*([^,]+),\s*New Version:\s*([^)]+)";

            Match matchName = Regex.Match(message.Text, patternName);
            Match matchVersion = Regex.Match(message.Text, patternVersion);

            return new RegexResult {
                Success = matchName.Success && matchVersion.Success,
                Name = matchName.Success ? matchName.Groups[1].Value : null,
                OldVersion = matchVersion.Success ? matchVersion.Groups[1].Value : null,
                NewVersion = matchVersion.Success ? matchVersion.Groups[2].Value : null,
            };
        }

        private void UpdateGame(RegexResult regex, Guid newCompletionStatus)
        {
            // Find game in library
            Game searchGame = GameDatabase.FirstOrDefault(game => game.Name == regex.Name);

            // Edit Game
            searchGame.Version = regex.NewVersion;
            searchGame.Notes = $"Old version was : {regex.OldVersion}.\nOld completion was : {searchGame.CompletionStatus}.";
            searchGame.CompletionStatusId = newCompletionStatus;

            GameDatabase.Update(searchGame);
        }

        private void HandleNotifications()
        {
            int successCount = 0;

            foreach (var message in Notifs.Messages)
            {
                RegexResult regex = RegexCheck(message);

                if (regex.Success)
                {
                    // Updated GUID : f1e78913-617a-4cb0-8614-5351f0516e84
                    UpdateGame(regex, Guid.Parse("f1e78913-617a-4cb0-8614-5351f0516e84"));

                    // Remove notification
                    successCount++;
                }
                else
                {
                    Dialogs.ShowErrorMessage($"There was an error with this game:\n{message.Text}", "Error");
                }
            }

            Notifs.RemoveAll();
            Dialogs.ShowMessage($"{successCount} games were successfully updated. (Reload Playnite if you don't see any change)");
        }

        public void CheckNotifications()
        {
            List<MessageBoxOption> options = new List<MessageBoxOption>();
            options.Add(new MessageBoxOption("Yes"));
            options.Add(new MessageBoxOption("No"));

            MessageBoxOption response = Dialogs.ShowMessage("Did you setup the completion tag before?", "Confirm", System.Windows.MessageBoxImage.Question, options);

            if(response.Title == "Yes")
            {
                HandleNotifications();
            }
        }
    }
}