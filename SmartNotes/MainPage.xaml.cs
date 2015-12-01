using SmartNotes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartNotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        public AppSettings Settings;
        ObservableCollection<Tag> Tags;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            Settings = new AppSettings();
            
            Tags = new ObservableCollection<Tag>();

            // Register a handler for BackRequested events
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainFrame.Navigate(typeof(NotesListPage));

            if (e.Parameter != null)
            {
                string arguments = (string)e.Parameter;
                if (!String.IsNullOrEmpty(arguments))
                {
                    if (NoteManager.AllNotesList.Contains(NoteManager.GetNoteById(int.Parse(arguments))))
                    {
                        MainFrame.Navigate(typeof(EditNotePage), int.Parse(arguments));
                    }
                }
            }
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
            if (MainSplitView.IsPaneOpen)
            {
                SettingsButton.Focus(FocusState.Programmatic);
            }
        }

        private async void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            List<Tag> tagsList = new List<Tag>();

            tagsList = await NoteManager.GetAllTags();
            Tags.Clear();
            tagsList.ForEach(p => Tags.Add(p));

            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private async void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            
            if (MainFrame.CanGoBack)
            {
                if (EditNotePage.contentChanged)
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    var dialog = new MessageDialog(loader.GetString("dialog_ignore_content"), loader.GetString("dialog_ignore_title"));
                    dialog.Commands.Add(new UICommand { Label = loader.GetString("dialog_button_yes"), Id = 0 });
                    dialog.Commands.Add(new UICommand { Label = loader.GetString("dialog_button_no"), Id = 1 });
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;
                    var result = await dialog.ShowAsync();
                    if ((int)result.Id == 0)
                    {
                        MainFrame.GoBack();
                    }
                }
                else
                {
                    MainFrame.GoBack();
                }
            }
            else
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var dialog = new MessageDialog(loader.GetString("dialog_close_content"), loader.GetString("dialog_close_title"));
                dialog.Commands.Add(new UICommand { Label = loader.GetString("dialog_button_yes"), Id = 0 });
                dialog.Commands.Add(new UICommand { Label = loader.GetString("dialog_button_no"), Id = 1 });
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    Application.Current.Exit();
                }
            }
        }

        private void TagsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tag = (Tag)e.ClickedItem;
            MainFrame.Navigate(typeof(QueriedListPage), tag);
            MainSplitView.IsPaneOpen = false;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(SettingsPage));

            MainSplitView.IsPaneOpen = false;
        }
        
        private void SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            MainFrame.Navigate(typeof(QueriedListPage), args.QueryText);
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(NotesListPage));
            MainSplitView.IsPaneOpen = false;
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
