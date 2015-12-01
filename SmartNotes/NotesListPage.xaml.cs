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
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
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
    public sealed partial class NotesListPage : Page
    {
        public ObservableCollection<Note> Notes;
        private Note lockedNote;
        private bool unlock;
        private AppSettings settings = MainPage.Current.Settings;
        private string sortingOrder;

        public NotesListPage()
        {
            this.InitializeComponent();
            Notes = new ObservableCollection<Note>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            sortingOrder = settings.GetValueOrDefault("NotesOrderSetting", AppSettings.NotesOrderSettingDefault);
            UpdateNotes();
            updateSecondaryTiles();
        }

        private async void updateSecondaryTiles()
        {
            IReadOnlyList<SecondaryTile> tilelist = await SecondaryTile.FindAllAsync();
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            if (tilelist.Count > 0)
            {
                foreach(var tile in tilelist)
                {
                    foreach(var note in Notes)
                    {
                        if (note.Id.ToString() == tile.Arguments)
                        {
                            note.PinText = loader.GetString("unpin_text");
                        }
                        else
                        {
                            note.PinText = loader.GetString("pin_text");
                        }
                    }
                }
            }
        }

        public async void UpdateNotes()
        {
            List<Note> list = new List<Note>();

            list = await NoteManager.GetAllNotes();

            if (list.Count == 0)
            {
                EmptyListTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyListTextBlock.Visibility = Visibility.Collapsed;
            }

            if (sortingOrder == "ByDate")
            {
                list = list.OrderByDescending(p => p.Date).ToList();
            }
            else if (sortingOrder == "ByTitle")
            {
                list = list.OrderByDescending(p => p.Title).ToList();
                list.Reverse();
            }
            list.ForEach(p => p.EditDate = NoteManager.FormatDate(p.Date));
            Notes.Clear();
            list.ForEach(p => Notes.Add(p));
        }

        private void MainGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var note = (Note)e.ClickedItem;
            var id = note.Id;

            if (!note.Locked)
            {
                Frame.Navigate(typeof(EditNotePage), id);
            }
            else
            {
                LockInputGrid.Visibility = Visibility.Visible;
                lockedNote = note;
            }
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

            var note = (Note)(e.OriginalSource as FrameworkElement).DataContext;

            lockedNote = note;
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
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            if (SecondaryTile.Exists(SecondaryTileId))
            {
                // Unpin
                SecondaryTile secondaryTile = new SecondaryTile(SecondaryTileId);

                Windows.Foundation.Rect rect = MainPage.GetElementRect((FrameworkElement)sender);
                Windows.UI.Popups.Placement placement = Windows.UI.Popups.Placement.Above;

                bool isUnpinned = await secondaryTile.RequestDeleteForSelectionAsync(rect, placement);

                pinFlyout.Text = loader.GetString("pin_text");
                note.PinText = loader.GetString("pin_text");
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
                    secondaryTile.VisualElements.Square310x310Logo = square310x310Logo;
                }

                secondaryTile.VisualElements.Wide310x150Logo = wide310x150Logo;

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
                    SecondaryTiles.UpdateTile(SecondaryTileId, note);

                    if (isPinned)
                    {
                        pinFlyout.Text = loader.GetString("unpin_text"); ;
                        note.PinText = loader.GetString("unpin_text"); ;
                    }
                    else
                    {
                        var dialog = new MessageDialog(loader.GetString("pin_error_text"), loader.GetString("pin_error_title"));
                        dialog.Commands.Add(new UICommand("OK"));
                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    await secondaryTile.RequestCreateAsync();
                    SecondaryTiles.UpdateTile(SecondaryTileId, note);

                    note.PinText = loader.GetString("unpin_text");
                    pinFlyout.Text = loader.GetString("unpin_text"); 
                }
            }
        }

        private void ShareNoteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var note = (Note)(e.OriginalSource as FrameworkElement).DataContext;

            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += (s, args) =>
            {
                if (!string.IsNullOrEmpty(note.Text))
                {
                    args.Request.Data.SetText(note.Text);
                    args.Request.Data.Properties.Title = note.Title;
                }
                else
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    args.Request.FailWithDisplayText(loader.GetString("share_error"));
                }
            };
        }

        private void MainStackPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);

            var note = (Note)(e.OriginalSource as FrameworkElement).DataContext;

            lockedNote = note;
        }

        private async void LockNoteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var note = (Note)(e.OriginalSource as FrameworkElement).DataContext;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            if (!note.Locked)
            {
                note.Locked = true;
                note.LockVisibility = "Visible";
                note.NoteVisibility = "Collapsed";
                note.LockNoteFlyoutText = loader.GetString("unlock_text");
                UpdateNotes();
                await NoteManager.SaveNotes();
            }
            else
            {
                LockInputGrid.Visibility = Visibility.Visible;
                unlock = true;
            }
        }

        private void CloseLockInputButton_Click(object sender, RoutedEventArgs e)
        {
            LockInputGrid.Visibility = Visibility.Collapsed;
            PinCodeTextBox.Text = "";
        }

        private async void PinCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox.Text.Length == 4)
            {
                if (textBox.Text == MainPage.Current.Settings.GetValueOrDefault<string>("PinCodeSetting", AppSettings.PinCodeSettingDefault))
                {
                    if (!unlock)
                    {
                        Frame.Navigate(typeof(EditNotePage), lockedNote.Id); 
                    }
                    //unlocking the note
                    else
                    {
                        var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

                        lockedNote.Locked = false;
                        lockedNote.LockVisibility = "Collapsed";
                        lockedNote.NoteVisibility = "Visible";
                        lockedNote.LockNoteFlyoutText = loader.GetString("lock_text");
                        textBox.Text = "";
                        unlock = false;
                        LockInputGrid.Visibility = Visibility.Collapsed;
                        await NoteManager.SaveNotes();
                        UpdateNotes();
                    }
                }
            }
        }

        private async void DuplicateNoteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var note = (Note)(e.OriginalSource as FrameworkElement).DataContext;

            Note newNote = new Note { Color = note.Color, Locked = note.Locked,
                                    LockNoteFlyoutText = note.LockNoteFlyoutText, LockVisibility = note.LockVisibility,
                                    NoteVisibility = note.NoteVisibility, PinText = note.PinText, StringColor = note.StringColor,
                                    Tags = note.Tags, Text = note.Text, Title = note.Title };

            newNote.Id = NoteManager.NewId();
            newNote.Date = DateTime.Now;
            newNote.EditDate = NoteManager.FormatDate(newNote.Date);
            Notes.Add(newNote);
            NoteManager.AddNote(newNote);
            await NoteManager.SaveNotes();
            UpdateNotes();
        }
    }
}
