﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YoutubeGameBarWidget;
using YoutubeGameBarWidget.WebServer;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace YoutubeGameBarOverlay {
    /// <summary>
    /// The main page of the overlay. It exists to the user input and validate the URL, invoke webserver and redirect it to the webpage.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string mediaURL;
        private WebServer ws;

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Cleans the app variables as soon as frame navigates to MainPage.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ws = null;
            this.mediaURL = "";

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Handles the click on Play Button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private async void HandlePlayButton(object sender, RoutedEventArgs eventArgs)
        {
            PrepareToPlay();
        }

        /// <summary>
        /// Handles the keypresses on inputUrl TextBox.
        /// In case Enter/Return is pressed, executes the same funciton as clicking the play button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keyArgs"></param>
        private async void HandleEnterPress(object sender, KeyRoutedEventArgs keyArgs)
        {
            if (keyArgs.Key == Windows.System.VirtualKey.Enter)
            {
                PrepareToPlay();
            }
        }

        /// <summary>
        /// Performs the necessary checks for validating the media URL, setting the XAML View states depending on the code worflow.
        /// </summary>
        private void PrepareToPlay()
        {
            InLoadingState(true);
            SetInputAsMediaURL();

            if (IsMediaURLValid() == true)
            {
                PrepareVideoUI();
            }
            else
            {
                InLoadingState(false);
                ShowErrorMessage("Invalid URL!");
            }
        }

        /// <summary>
        /// Shows an Error Message in the ErrorMessage TextBox XAML Element.
        /// </summary>
        /// <param name="errorMessage"></param>
        public void ShowErrorMessage(string errorMessage)
        {
            ErrorMessage.Text = errorMessage;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Sets a "Loading State" for the objects in XAML View, enabling and disabling functionality based on the passed value.
        /// </summary>
        /// <param name="value"></param>
        private void InLoadingState(bool value)
        {
            if (value == true)
            {
                LoadingRing.IsActive = true;
                ErrorMessage.Visibility = Visibility.Collapsed;
                inputUrlTextBox.IsEnabled = false;
                PlayButton.IsEnabled = false;
            }
            else
            {
                LoadingRing.IsActive = false;
                inputUrlTextBox.IsEnabled = true;
                PlayButton.IsEnabled = true;
            }
            
        }

        /// <summary>
        /// Sets the MediaURL as the current string on the TextBox.
        /// </summary>
        private void SetInputAsMediaURL()
        {
            mediaURL = inputUrlTextBox.Text;
        }

        /// <summary>
        /// Checks if the MediaURL is a valid Youtube URL.
        /// </summary>
        private bool IsMediaURLValid()
        {
            if (mediaURL.Length <= 32)
            {
                return false;
            }
            else
            {
                string youtubeBaseURLInput = mediaURL.Substring(0, 24);
                string youtubeBaseURLExpected = "https://www.youtube.com/";
                
                if (youtubeBaseURLInput == youtubeBaseURLExpected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Prepares the necessary elements to compose the video UI.
        /// These elements are:
        /// 1 - The webserver with VideoUI
        /// 2 - Make the Webpage go to the Video URL.
        /// </summary>
        private void PrepareVideoUI()
        {
            InitializeWebServer();

            string baseUri = "http://localhost:54523/?mediaUrl=";
            Uri videoUri = new Uri(baseUri + GetMediaId());
            this.Frame.Navigate(typeof(Webpage), videoUri);
        }

        /// <summary>
        /// Starts the Webserver by calling its constructor.
        /// </summary>
        private void InitializeWebServer()
        {
            this.ws = new WebServer();

            //Just to be sure that GC will not collect our WebServer.
            GC.KeepAlive(this.ws);
        }

        /// <summary>
        /// Gets and returns the Video ID from the Media URL.
        /// </summary>
        /// <returns></returns>
        private string GetMediaId()
        {
            char argumentSeparator = '&';
            string mediaId = "";
            string videoSeparator = "v=";
            string playlistSeparator = "list=";

            try
            {
                mediaId = mediaURL.Split(playlistSeparator)[1].Split(argumentSeparator).First();
                return mediaId;
            } 
            catch (IndexOutOfRangeException)
            {   
                mediaId = mediaURL.Split(videoSeparator)[1].Substring(0,11);
                return mediaId;
            }
        }
    }
}
