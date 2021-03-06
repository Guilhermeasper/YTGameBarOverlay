﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using YoutubeGameBarWidget;
using YoutubeGameBarWidget.Pages;
using YoutubeGameBarWidget.Pages.PageObjects;
using YoutubeGameBarWidget.Search;
using YoutubeGameBarWidget.Utilities;

namespace YoutubeGameBarOverlay
{
    /// <summary>
    /// The main page of the overlay.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Search search;
        private bool inLoadingState;
        public string mediaURL;

        public MainPage()
        {
            this.search = new Search();
            this.search.FinishedFetchingResults += PresentResults;
            this.search.FailedFetchingResults += PresentSearchError;
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        /// <summary>
        /// Cleans the page variables as soon as frame navigates to MainPage.
        /// </summary>
        /// <param name="e">The navigation arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.mediaURL = Constants.Common.EmptyString;
            this.inputBox.Text = Constants.Common.EmptyString;
            this.InLoadingState(false);

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Handles the click on Play Button.
        /// 
        /// In case of no video has been searched, show an error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HandlePlayButton(object sender, RoutedEventArgs eventArgs)
        {
            ListItems list = (ListItems)this.inputBox.ItemsSource;

            if (list != null && list.Count > 0)
            {
                SetAsMediaURL(list.ElementAt(0).MediaUrl);
                PrepareToPlay();
            }
            else
            {
                ShowErrorMessage(Constants.Error.VideoNotSelected);
            }
        }

        /// <summary>
        /// Handles the click at the Feedback Page Button navigating to it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HandleFeedbackButton(object sender, RoutedEventArgs eventArgs)
        {
            this.Frame.Navigate(typeof(FeedbackPage));
        }

        /// <summary>
        /// Handles the click at the Changelog Page Button navigating to it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HandleChangelogButton(object sender, RoutedEventArgs eventArgs)
        {
            this.Frame.Navigate(typeof(ChangelogPage));
        }

        /// <summary>
        /// Prepares the App to play the video by validating the Media URL and setting the page elements states depending on the URL Validity.
        /// </summary>
        private void PrepareToPlay()
        {
            InLoadingState(true);

            if (Validator.IsMediaURLValid(this.mediaURL) == true)
            {
                StartPlayback();
            }
            else
            {
                InLoadingState(false);
                ShowErrorMessage(Constants.Error.URLNotValid);
            }
        }

        /// <summary>
        /// Set the instance MediaURL attribute as the given string.
        /// </summary>
        /// <param name="input">The string to be set as MediaURL</param>
        private void SetAsMediaURL(string input)
        {
            this.mediaURL = input;
        }

        /// <summary>
        /// Prepares the necessary elements to start the playback on the WebPage.
        /// 
        /// These elements are:
        /// 1 - The Information payload to be passed to Webpage.
        /// 2 - Navigate to Webpage.
        /// </summary>
        private void StartPlayback()
        {
            InformationPayload information = new InformationPayload(Utils.GetProperVideoUIUri(this.mediaURL));

            this.Frame.Navigate(typeof(Webpage), information);
        }

        /// <summary>
        /// Handles the text changes on the Search bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void inputBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (inputBox.Text == Constants.Common.EmptyString)
            {
                inputBox.IsSuggestionListOpen = false;
                sender.ItemsSource = new ListItems();
                Painter.RunUIUpdateByMethod(FalseLoading);
            }
            else if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (inputBox.Text.Length >= 8)
                {
                    string inputStart = inputBox.Text.Substring(0, 8);

                    if (Regex.IsMatch(inputStart, Validator.RegexPatterns.HTTPBaseExpected))
                    {
                        SetAsMediaURL(inputBox.Text);
                        PrepareToPlay();
                    }
                    else
                    {
                        await DoSearch();
                    }
                }
                else
                {
                    await DoSearch();
                }                
            }
        }

        /// <summary>
        /// Handles the app behavior based on the selected suggestion.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void inputBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                InLoadingState(true);
                ListItem chosenItem = (ListItem)args.ChosenSuggestion;
                SetAsMediaURL(chosenItem.MediaUrl);

                StartPlayback();
            }
        }

        /// <summary>
        /// Executes the search using the text available on inputBox.
        /// In case of a unkown exception is raised, shows a error message accordingly.
        /// </summary>
        private async Task DoSearch()
        {
            if (this.inLoadingState == false)
            {
                this.inLoadingState = true;
                Painter.RunUIUpdateByMethod(WeakLoading);
            }

            try
            {
                await this.search.ByTerm(inputBox.Text);
            }
            catch (Exception ex)
            {
                if (!(ex is NotSupportedException))
                {
                    PresentSearchError(null, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Presents the results on AutoSuggestBox when the results are ready.
        /// This function is triggered by the Search's FinishedFetchingResults event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PresentResults(Object sender, EventArgs e)
        {
            if (this.inLoadingState == true)
            {
                InLoadingState(false);
            }

            inputBox.ItemsSource = this.search.Retreive();
            inputBox.IsSuggestionListOpen = true;
        }

        /// <summary>
        /// Presents a seach error when something went wrong with the request to server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PresentSearchError(Object sender, EventArgs e)
        {
            InLoadingState(false);
            ShowErrorMessage(Constants.Error.SearchNotAvailable);
        }

        /// <summary>
        /// Shows an Error Message in the ErrorMessage element.
        /// </summary>
        /// <param name="errorMessage">The error message to be displayed.</param>
        private async void ShowErrorMessage(string errorMessage)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {

                        ErrorMessage.Text = errorMessage;
                        ErrorMessage.Visibility = Visibility.Visible;
                    }
                );
        }

        /// <summary>
        /// Sets a "Loading State" for the objects in the page, enabling and disabling functionality based on the given value.
        /// </summary>
        /// <param name="value">The value indicating if it is in a loading state or not.</param>
        private void InLoadingState(bool value)
        {
            if (value == true)
            {
                this.inLoadingState = true;
                Painter.RunUIUpdateByMethod(TrueLoading);
            }
            else
            {
                this.inLoadingState = false;
                Painter.RunUIUpdateByMethod(FalseLoading);
            }
        }

        /// <summary>
        /// Auxiliary method to asynchronously update UI on a InLoadingState(true) ocasion.
        /// </summary>
        private async void TrueLoading()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        LoadingRing.IsActive = true;
                        ErrorMessage.Visibility = Visibility.Collapsed;
                        inputBox.IsEnabled = false;
                        PlayButton.IsEnabled = false;
                    }
                );
        }

        /// <summary>
        /// Auxiliary method to asynchronously update UI on a InLoadingState(false) ocasion.
        /// </summary>
        private async void FalseLoading()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        LoadingRing.IsActive = false;
                        inputBox.IsEnabled = true;
                        PlayButton.IsEnabled = true;

                        if (ErrorMessage.Text != Constants.Common.EmptyString)
                        {
                            ErrorMessage.Visibility = Visibility.Visible;
                        }
                    }
                );
        }

        /// <summary>
        /// Auxiliary method to asynchronously update UI on to a weak loading state, without locking all page elements.
        /// </summary>
        private async void WeakLoading()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        LoadingRing.IsActive = true;
                        inputBox.IsSuggestionListOpen = false;
                        ErrorMessage.Visibility = Visibility.Collapsed;
                    }
                );
        }
    }
}
