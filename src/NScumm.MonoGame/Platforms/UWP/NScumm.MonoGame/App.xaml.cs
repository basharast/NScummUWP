﻿using NScumm.Core;
using NScumm.Platform_UWP;
using System;
using System.Diagnostics;
using System.Xml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Navigation;

namespace NScumm.MonoGame
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                var y = MemoryManager.AppMemoryUsageLimit;
                try
                {
                    var u = MemoryManager.AppMemoryUsage;
                    if (y <= u || y < 100000000)
                    {
                        bool result = MemoryManager.TrySetAppMemoryUsageLimit(y + 100000000);
                    }
                }
                catch (Exception e)
                {
                }
            }
            catch
            {

            }

            InitializeComponent();

            UnhandledException += OnUnhandledException;
            Suspending += OnSuspending;
        }
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            ServiceLocator.Platform = new WindowsUAPPlatform();
            ServiceLocator.FileStorage = new FileStorage();
            ServiceLocator.TraceFatory = new TraceFactory("IO");
            ServiceLocator.SaveFileManager = new SaveFileManager(ServiceLocator.FileStorage);

#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;
            if (!isBackPressedReady)
            {
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                //SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
                isBackPressedReady = true;
            }
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(GameLibraryPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            //
            //Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        public static bool isGameStarted = false;
        bool isBackPressedReady = false;
        public static string CurrentGameID = "";
        public static string currentGameID
        {
            get
            {
                return CurrentGameID;
            }
            set
            {
                CurrentGameID = value;
                ServiceLocator.SaveFileManager.setID(CurrentGameID);
            }
        }
        private async void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (isGameStarted)
            {
                e.Handled = true;
                return;
            }
            try
            {
                Frame rootFrame = Window.Current.Content as Frame;
                var messageDialog = new MessageDialog("Do you want to exit?");
                messageDialog.Commands.Add(new UICommand("Exit", new UICommandInvokedHandler(this.CommandInvokedHandler)));
                messageDialog.Commands.Add(new UICommand("Dismiss"));
                await messageDialog.ShowAsync();
                e.Handled = true;
            }catch(Exception ex)
            {

            }
        }
        private void CommandInvokedHandler(IUICommand command)
        {
            // Display message showing the label of the command that was invoked
            CoreApplication.Exit();
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e != null)
            {
                Exception exception = e.Exception;
                if ((exception is XmlException || exception is NullReferenceException) && exception.ToString().ToUpper().Contains("INNERACTIVE"))
                {
                    Debug.WriteLine("Handled Inneractive exception {0}", exception);
                    e.Handled = true;
                    return;
                }
                else if (exception is NullReferenceException && exception.ToString().ToUpper().Contains("SOMA"))
                {
                    Debug.WriteLine("Handled Smaato null reference exception {0}", exception);
                    e.Handled = true;
                    return;
                }
                else if ((exception is System.IO.IOException || exception is NullReferenceException) && exception.ToString().ToUpper().Contains("GOOGLE"))
                {
                    Debug.WriteLine("Handled Google exception {0}", exception);
                    e.Handled = true;
                    return;
                }
                else if (exception is ObjectDisposedException && exception.ToString().ToUpper().Contains("MOBFOX"))
                {
                    Debug.WriteLine("Handled Mobfox exception {0}", exception);
                    e.Handled = true;
                    return;
                }
                else if ((exception is NullReferenceException || exception is XamlParseException || exception is TypeLoadException) && exception.ToString().ToUpper().Contains("MICROSOFT.ADVERTISING"))
                {
                    Debug.WriteLine("Handled Microsoft.Advertising exception {0}", exception);
                    e.Handled = true;
                    return;
                }
            }
            // APP SPECIFIC HANDLING HERE

            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                //Debugger.Break();
            }
            e.Handled = true;
        }
    }
}
