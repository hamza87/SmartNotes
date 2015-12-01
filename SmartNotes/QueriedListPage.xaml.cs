using NotificationsExtensions.Tiles;
using SmartNotes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SmartNotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueriedListPage : Page
    {
        AppSettings settings = MainPage.Current.Settings;
        bool localStorageMode;
        public ObservableCollection<Note> Notes;
        object par;

        public QueriedListPage()
        {
            this.InitializeComponent();
            Notes = new ObservableCollection<Note>();
        }

        private async void updateSecondaryTiles()
        {
            IReadOnlyList<SecondaryTile> tilelist = await SecondaryTile.FindAllAsync();

            if (tilelist.Count > 0)
            {
                foreach (var tile in tilelist)
                {
                    foreach (var note in Notes)
                    {
                        if (note.Id.ToString() == tile.Arguments)
                        {
                            note.PinText = "Unpin from Start";
                        }
                        else
                        {
                            note.PinText = "Pin to Start";
                        }
                    }
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            par = e.Parameter;

            UpdateNotes();
            updateSecondaryTiles();
        }

        void UpdateNotes()
        {
            List<Note> list = new List<Note>();

            if (par.GetType() == typeof(Tag))
            {
                list = NoteManager.GetNotesByTag((Tag)par);
            }
            else if (par.GetType() == typeof(string))
            {
                list = NoteManager.GetNotesByTerm((string)par);
            }

            if (list.Count == 0)
            {
                EmptyListTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyListTextBlock.Visibility = Visibility.Collapsed;
            }

            list = list.OrderByDescending(p => p.Date).ToList();
            Notes.Clear();
            list.ForEach(p => Notes.Add(p));
        }

        private void MainGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var note = (Note)e.ClickedItem;
            var id = note.Id;

            Frame.Navigate(typeof(EditNotePage), id);
        }

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EditNotePage), 0);
        }

        private void MainStackPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private void EditNoteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var note = (Note)(e.OriginalSource as FrameworkElement).DataContext;
            Frame.Navigate(typeof(EditNotePage), note.Id);
        }

        private async void DeleteNoteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var note = (Note)(e.OriginalSource as FrameworkElement).DataContext;

            NoteManager.DeleteNote(note);
            await NoteManager.SaveNotes();
            UpdateNotes();
        }

        private async void PinUnpinNoteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var pinFlyout = (MenuFlyoutItem)sender;
            var note = (Note)(e.OriginalSource as FrameworkElement).DataContext;
            string SecondaryTileId = note.Id + "NoteTile";

            if (SecondaryTile.Exists(SecondaryTileId))
            {
                // Unpin
                SecondaryTile secondaryTile = new SecondaryTile(SecondaryTileId);

                Windows.Foundation.Rect rect = MainPage.GetElementRect((FrameworkElement)sender);
                Windows.UI.Popups.Placement placement = Windows.UI.Popups.Placement.Above;

                bool isUnpinned = await secondaryTile.RequestDeleteForSelectionAsync(rect, placement);

                pinFlyout.Text = "Pin to Start";
                note.PinText = "Pin to Start";
            }
            else
            {
                //Pin
                Uri square150x150Logo = new Uri("ms-appx:///Assets/square150x150Tile-sdk.png");
                Uri wide310x150Logo = new Uri("ms-appx:///Assets/wide310x150Tile-sdk.png");
                Uri square310x310Logo = new Uri("ms-appx:///Assets/square310x310Tile-sdk.png");
                Uri square30x30Logo = new Uri("ms-appx:///Assets/square30x30Tile-sdk.png");

                string tileActivationArguments = note.Id.ToString();

                SecondaryTile secondaryTile = new SecondaryTile(SecondaryTileId,
                                                                note.Title,
                                                                tileActivationArguments,
                                                                square150x150Logo,
                                                                TileSize.Square150x150);

                if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent(("Windows.Phone.UI.Input.HardwareButtons"))))
                {
                    secondaryTile.VisualElements.Wide310x150Logo = wide310x150Logo;
                    secondaryTile.VisualElements.Square310x310Logo = square310x310Logo;
                }

                secondaryTile.VisualElements.Square30x30Logo = square30x30Logo;

                secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;

                if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent(("Windows.Phone.UI.Input.HardwareButtons"))))
                {
                    secondaryTile.VisualElements.ShowNameOnWide310x150Logo = true;
                    secondaryTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                }

                secondaryTile.VisualElements.ForegroundText = ForegroundText.Dark;
                secondaryTile.VisualElements.BackgroundColor = note.Color;

                secondaryTile.RoamingEnabled = false;

                if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent(("Windows.Phone.UI.Input.HardwareButtons"))))
                {
                    bool isPinned = await secondaryTile.RequestCreateForSelectionAsync(MainPage.GetElementRect((FrameworkElement)sender), Windows.UI.Popups.Placement.Below);

                    if (isPinned)
                    {
                        pinFlyout.Text = "Unpin from Start";
                        note.PinText = "Unpin from Start";
                    }
                    else
                    {
                        var dialog = new MessageDialog("The tile could not be created.", "Error");
                        dialog.Commands.Add(new UICommand("OK"));
                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    await secondaryTile.RequestCreateAsync();

                    note.PinText = "Unpin from Start";
                    pinFlyout.Text = "Unpin from Start";
                }
                SecondaryTiles.UpdateTile(SecondaryTileId, note);
            }
        }

        private void ShareNoteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var note = (Note)(e.OriginalSource as FrameworkElement).DataContext;

            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += (s, args) =>
            {
                args.Request.Data.SetText(note.Text);
                if (!String.IsNullOrEmpty(note.Title))
                {
                    args.Request.Data.Properties.Title = note.Title;
                }
            };
        }

        private void MainStackPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }
    }
}
