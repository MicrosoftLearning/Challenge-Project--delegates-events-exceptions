﻿using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using UI.ImageRendering;
using LangtonsAnt;
using Json;
using UI;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameBuffer buffer;
        DispatcherTimer gameTimer;
        PlayUIMode playUiState;

        string rule = "LR";
        const int imagesPerSecond = 25;
        const int nGenerations = 1000;
        const int generationsPerStep = 1;

        public MainWindow()
        {
            InitializeComponent();
            buffer = CreateGameBuffer(null, nGenerations, rule);
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(1000 / imagesPerSecond);
            gameTimer.Tick += (sender, args) =>
            {
                if (buffer.MoveNext(generationsPerStep))
                    UpdateGameView(buffer.Current!);
                else
                { 
                    MessageBox.Show("Game Over. We no longer have any ants.");
                    PlayUIState = PlayUIMode.Stopped;
                }
            };
            PlayUIState = PlayUIMode.Stopped;
        }

        private IGame CreateGame(string initialRule = "LR")
        {
            IGame newGame = null;
            newGame = new Game(128, null);
            newGame.Ants = new List<IAnt>(new IAnt[] {
              new GeneralizedAnt(i: newGame.Size / 2 + 1, j: newGame.Size / 2 + 1, direction: 0) { Rule = initialRule }
            });
            return newGame;
        }

        private GameBuffer CreateGameBuffer(IGame? initialState = null, int nGenerations = 100, string initialRule = "LR")
        {
            if (initialState == null)
            {
                initialState = CreateGame(initialRule);
            }
            return new GameBuffer(initialState, nGenerations);
        }

        #region Properties

        PlayUIMode PlayUIState
        {
            get { return playUiState; }
            set
            {
                switch (value)
                {
                    case PlayUIMode.Playing:
                        gameTimer.Start();
                        btnPlay.Visibility = Visibility.Collapsed;
                        btnPause.Visibility = Visibility.Visible;
                        break;
                    case PlayUIMode.Paused:
                        gameTimer.Stop();
                        btnPause.Visibility = Visibility.Collapsed;
                        btnPlay.Visibility = Visibility.Visible;
                        break;
                    case PlayUIMode.Stopped:
                        gameTimer.Stop();
                        buffer = CreateGameBuffer(null, nGenerations, rule);
                        UpdateGameView(buffer.Current!);
                        btnPlay.Visibility = Visibility.Visible;
                        btnPause.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
                playUiState = value;
            }
        }

        public int MaxColor
        {
            get { return (rule?.Length ?? 0) - 1; }
        }

        #endregion

        #region UI

        private void UpdateGameView(IGame gameState)
        {
            ImageSource source;
            source = GameImageRenderer.GetGenerationImageSourceX2(gameState);
            imgGame.Source = source;

            lblGenerationN.Text = gameState.GenerationN.ToString();
        }

        #endregion

        #region Event Handlers

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            PlayUIState = PlayUIMode.Playing;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            PlayUIState = PlayUIMode.Paused;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            PlayUIState = PlayUIMode.Stopped;
        }

        private void btnStepBackward_Click(object sender, RoutedEventArgs e)
        {
            PlayUIState = PlayUIMode.Paused;

            if (!buffer.MovePrevious())
                MessageBox.Show($"Cannot move back. We only store a limited number of previous generations of the game.");
            UpdateGameView(buffer.Current!);
        }

        private void btnStepForward_Click(object sender, RoutedEventArgs e)
        {
            PlayUIState = PlayUIMode.Paused;

            if (!buffer.MoveNext())
                MessageBox.Show("Game Over. We no longer have any ants.");
            UpdateGameView(buffer.Current!);
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (buffer.Current == null)
                throw new InvalidOperationException("Cannot save the game when current game state is null");

            PlayUIState = PlayUIMode.Paused;

            var saveFileDialog = new SaveFileDialog() { Filter = "JSON Document(*.json)|*.json" };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = GameJSONSerializer.ToJson(buffer.Current);
                    await File.WriteAllTextAsync(saveFileDialog.FileName, jsonString);
                }
                catch (JSONSerializationException jsex)
                {
                    MessageBox.Show(jsex.Message);
                    return;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Could not save JSON file to disk. {ex}");
                    return;
                }
            }
        }

        private async void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            PlayUIState = PlayUIMode.Paused;

            string fileName;
            var openFileDialog = new OpenFileDialog() { Filter = "JSON Document(*.json)|*.json" };
            if (openFileDialog.ShowDialog() == true)
            {
                fileName = openFileDialog.FileName;
                try
                {
                    string json = await File.ReadAllTextAsync(fileName);
                    IGame loadedGame = GameJSONSerializer.FromJson(json);
                    buffer = CreateGameBuffer(loadedGame, nGenerations, rule);

                    UpdateGameView(buffer.Current);
                }
                catch (JSONSerializationException jsex)
                {
                    MessageBox.Show(jsex.Message);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Could not load JSON file from disk. {ex}");
                }
            }
        }

        private void btnEditRuleStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditRuleApply_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditRuleCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditAnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditCellColor_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBackToGame_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}