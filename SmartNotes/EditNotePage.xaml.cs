using NotificationsExtensions.Tiles;
using SmartNotes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class EditNotePage : Page
    {
        AppSettings settings = MainPage.Current.Settings;
        private Note editNote;
        List<ColorBox> ColorBoxes;
        ColorBox colorBoxToSave;
        ObservableCollection<Tag> TagsList;
        List<string> tagTexts;
        public static bool contentChanged = false;
        string SecondaryTileId;

        public EditNotePage()
        {
            this.InitializeComponent();

            ColorBoxes = SetColorBoxes();
            colorBoxToSave = new ColorBox();
            TagsList = new ObservableCollection<Tag>();

            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Showing += (s, args) =>
            {
                MainAppBar.Visibility = Visibility.Collapsed;
            };
            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Hiding += (s, args) =>
            {
                MainAppBar.Visibility = Visibility.Visible;
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            editNote = NoteManager.GetNoteById((int)e.Parameter);

            initTile();

            editNote.Tags.ForEach(p => TagsList.Add(p));
            tagTexts = new List<string>();
            editNote.Tags.ForEach(p => tagTexts.Add(p.Text));

            if (editNote.Id == 0)
            {
                int colorIndex = (new Random()).Next(ColorBoxes.Count - 1);
                editNote.Color = ColorBoxes[colorIndex].Color;
                editNote.StringColor = ColorBoxes[colorIndex].StringColor;
            }
            LockNoteButton.Label = editNote.LockNoteFlyoutText;
            LockGlyph.Visibility = editNote.Locked ? Visibility.Visible : Visibility.Collapsed;
            PinUnpinButton.Visibility = editNote.Locked || editNote.Id == 0 ? Visibility.Collapsed : Visibility.Visible;
            ShareButton.Visibility = editNote.Id == 0 ? Visibility.Collapsed : Visibility.Visible;

            TitleTextBox.Text = editNote.Title;
            ContentTextBox.Text = editNote.Text;
            colorBoxToSave.Color = editNote.Color;
            colorBoxToSave.StringColor = editNote.StringColor;
            ParentGrid.Background = new SolidColorBrush(editNote.Color);

            SaveNoteButton.IsEnabled = false;

            NoTagsTextBlock.Visibility = tagTexts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (editNote.Id == 0)
            {
                editNote.Id = NoteManager.NewId();
            }
            SecondaryTileId = editNote.Id + "NoteTile";
            contentChanged = false;
        }

        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            var tagToAdd = TagsTextBox.Text;
            tagToAdd = tagToAdd.ToLower().Replace(" ", "").Replace("#", "");
            if (tagToAdd.Length > 0)
            {
                tagToAdd = "#" + tagToAdd;
                if (!tagTexts.Contains(tagToAdd))
                {
                    TagsList.Add(NoteManager.SetTag(tagToAdd));
                    tagTexts.Add(tagToAdd);
                    TagsTextBox.Text = "";
                    NoTagsTextBlock.Visibility = Visibility.Collapsed;
                    if (TitleTextBox.Text != "" || ContentTextBox.Text != "")
                    {
                        SaveNoteButton.IsEnabled = true;
                        contentChanged = true;
                    }
                }
            }
        }

        private void saveTags()
        {
            editNote.Tags = TagsList.ToList();
        }

        private async void SaveNoteButton_Click(object sender, RoutedEventArgs e)
        {
            editNote.Text = ContentTextBox.Text;
            editNote.Title = TitleTextBox.Text;
            editNote.Date = DateTime.Now;
            editNote.EditDate = NoteManager.FormatDate(DateTime.Now);
            saveTags();
            saveColors();
            if (!NoteManager.AllNotesList.Contains(editNote))
            {
                NoteManager.AddNote(editNote);
            }
            else
            {
                NoteManager.UpdateNote(editNote);
            }
            await NoteManager.SaveNotes();

            Frame.GoBack();

            if (SecondaryTile.Exists(SecondaryTileId))
            {
                SecondaryTiles.UpdateTile(SecondaryTileId, editNote);
            }
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsGrid.Visibility = Visibility.Visible;
        }

        private void CloseOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsGrid.Visibility = Visibility.Collapsed;
        }

        private void ColorsGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var colorBox = (ColorBox)e.AddedItems.FirstOrDefault();
            ParentGrid.Background = new SolidColorBrush(colorBox.Color);
            colorBoxToSave = colorBox;
            if (TitleTextBox.Text != "" || ContentTextBox.Text != "")
            {
                SaveNoteButton.IsEnabled = true;
                contentChanged = true;
            }
        }

        private void saveColors()
        {
            editNote.Color = colorBoxToSave.Color;
            editNote.StringColor = colorBoxToSave.StringColor;
        }

        private async void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (editNote.Id != 0)
            {
                NoteManager.DeleteNote(editNote);
                await NoteManager.SaveNotes();
            }
            if (Frame.CanGoBack) Frame.GoBack();
            else Frame.Navigate(typeof(NotesListPage));
        }

        List<ColorBox> SetColorBoxes()
        {
            List<ColorBox> colorBoxes = new List<ColorBox>();
            colorBoxes.Add(new ColorBox { Color = Colors.White, StringColor = "White" });
            colorBoxes.Add(new ColorBox { Color = Colors.LightPink, StringColor = "lightPink" });
            colorBoxes.Add(new ColorBox { Color = Colors.LightYellow, StringColor = "lightYellow" });
            colorBoxes.Add(new ColorBox { Color = Colors.LightBlue, StringColor = "lightBlue" });
            colorBoxes.Add(new ColorBox { Color = Colors.LightGreen, StringColor = "lightGreen" });
            colorBoxes.Add(new ColorBox { Color = Colors.LightCyan, StringColor = "lightCyan" });
            colorBoxes.Add(new ColorBox { Color = Colors.LightSeaGreen, StringColor = "lightSeaGreen" });
            colorBoxes.Add(new ColorBox { Color = Colors.LightSalmon, StringColor = "lightSalmon" });
            colorBoxes.Add(new ColorBox { Color = Colors.LightSlateGray, StringColor = "lightSlateGray" });
            colorBoxes.Add(new ColorBox { Color = Colors.LightSteelBlue, StringColor = "lightSteelBlue" });
            colorBoxes.Add(new ColorBox { Color = Colors.Gainsboro, StringColor = "Gainsboro" });
            colorBoxes.Add(new ColorBox { Color = Colors.Pink, StringColor = "Pink" });
            colorBoxes.Add(new ColorBox { Color = Colors.BurlyWood, StringColor = "BurlyWood" });
            colorBoxes.Add(new ColorBox { Color = Colors.Moccasin, StringColor = "Moccasin" });
            colorBoxes.Add(new ColorBox { Color = Colors.Cyan, StringColor = "Cyan" });
            colorBoxes.Add(new ColorBox { Color = Colors.MediumSpringGreen, StringColor = "MediumSpringGreen" });

            return colorBoxes;
        }

        //delete tag button
        private void TagsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tag = (Tag)e.ClickedItem;

            TagsList.Remove(tag);
            tagTexts.Remove(tag.Text);
            NoTagsTextBlock.Visibility = tagTexts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (TitleTextBox.Text != "" || ContentTextBox.Text != "")
            {
                SaveNoteButton.IsEnabled = true;
                contentChanged = true;
            }
        }

        private void ContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ContentTextBox.Text != "")
            {
                SaveNoteButton.IsEnabled = true;
                contentChanged = true;
            }
        }

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TitleTextBox.Text != "")
            {
                SaveNoteButton.IsEnabled = true;
                contentChanged = true;
            }
        }

        private void TagsTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var tagToAdd = TagsTextBox.Text;
                tagToAdd = tagToAdd.ToLower().Replace(" ", "").Replace("#", "");
                if (tagToAdd.Length > 0)
                {
                    tagToAdd = "#" + tagToAdd;
                    if (!tagTexts.Contains(tagToAdd))
                    {
                        TagsList.Add(NoteManager.SetTag(tagToAdd));
                        tagTexts.Add(tagToAdd);
                        TagsTextBox.Text = "";
                        NoTagsTextBlock.Visibility = Visibility.Collapsed;
                        if (TitleTextBox.Text != "" || ContentTextBox.Text != "")
                        {
                            SaveNoteButton.IsEnabled = true;
                            contentChanged = true;
                        }
                    }
                }
            }
        }

        private async void Unpin(object sender)
        {
            // Unpin
            SecondaryTile secondaryTile = new SecondaryTile(SecondaryTileId);

            Windows.Foundation.Rect rect = MainPage.GetElementRect((FrameworkElement)sender);
            Windows.UI.Popups.Placement placement = Windows.UI.Popups.Placement.Above;

            bool isUnpinned = await secondaryTile.RequestDeleteForSelectionAsync(rect, placement);

            ToggleAppBarButton(isUnpinned);
        }

        private async void Pin(object sender)
        {
            Uri square150x150Logo = new Uri("ms-appx:///Assets/square150x150Tile-sdk.png");
            Uri wide310x150Logo = new Uri("ms-appx:///Assets/wide310x150Tile-sdk.png");
            Uri square310x310Logo = new Uri("ms-appx:///Assets/square310x310Tile-sdk.png");
            Uri square30x30Logo = new Uri("ms-appx:///Assets/square30x30Tile-sdk.png");

            string tileActivationArguments = editNote.Id.ToString();

            SecondaryTile secondaryTile = new SecondaryTile(SecondaryTileId,
                                                            editNote.Title,
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
                secondaryTile.VisualElements.ShowNameOnSquare310x310Logo = true;
            }

            secondaryTile.VisualElements.ShowNameOnWide310x150Logo = true;

            secondaryTile.VisualElements.ForegroundText = ForegroundText.Dark;
            secondaryTile.VisualElements.BackgroundColor = editNote.Color;

            secondaryTile.RoamingEnabled = false;

            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent(("Windows.Phone.UI.Input.HardwareButtons"))))
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

                bool isPinned = await secondaryTile.RequestCreateForSelectionAsync(MainPage.GetElementRect((FrameworkElement)sender), Windows.UI.Popups.Placement.Below);
                SecondaryTiles.UpdateTile(SecondaryTileId, editNote);

                if (isPinned)
                {
                    ToggleAppBarButton(!isPinned);
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
                ToggleAppBarButton(!await secondaryTile.RequestCreateAsync());
                SecondaryTiles.UpdateTile(SecondaryTileId, editNote);
            }
        }

        private void PinUnpinButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecondaryTile.Exists(SecondaryTileId))
            {
                Unpin(sender);
            }
            else
            {
                Pin(sender);
            }
        }

        private void ToggleAppBarButton(bool showPinButton)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            if (showPinButton)
            {
                PinUnpinButton.Label = loader.GetString("pin_text");
                editNote.PinText = loader.GetString("pin_text");
            }
            else
            {
                PinUnpinButton.Label = loader.GetString("unpin_text");
                editNote.PinText = loader.GetString("unpin_text");
            }
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += EditNotePage_DataRequested;
        }

        private void EditNotePage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (!string.IsNullOrEmpty(ContentTextBox.Text))
            {
                args.Request.Data.SetText(editNote.Text);
                args.Request.Data.Properties.Title = editNote.Title;
            }
            else
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                args.Request.FailWithDisplayText(loader.GetString("share_error"));
            }
        }

        void initTile()
        {
            SecondaryTileId = editNote.Id + "NoteTile";
            ToggleAppBarButton(!SecondaryTile.Exists(SecondaryTileId));
        }

        private async void LockNoteButton_Click(object sender, RoutedEventArgs e)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            if (!editNote.Locked)
            {
                editNote.Locked = true;
                PinUnpinButton.Visibility = Visibility.Collapsed;
                editNote.LockVisibility = "Visible";
                editNote.NoteVisibility = "Collapsed";
                editNote.LockNoteFlyoutText = loader.GetString("unlock_text");
                LockNoteButton.Label = loader.GetString("unlock_text");
                LockGlyph.Visibility = Visibility.Visible;
            }
            else
            {
                editNote.Locked = false;
                PinUnpinButton.Visibility = Visibility.Visible;
                editNote.LockVisibility = "Collapsed";
                editNote.NoteVisibility = "Visible";
                editNote.LockNoteFlyoutText = loader.GetString("lock_text");
                LockNoteButton.Label = loader.GetString("lock_text");
                LockGlyph.Visibility = Visibility.Collapsed;
            }
            await NoteManager.SaveNotes();
        }
    }

    public class ColorBox
    {
        public string StringColor { get; set; }
        public Color Color { get; set; }
    }
}
