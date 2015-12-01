using SmartNotes.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class SettingsPage : Page
    {
        private AppSettings settings;
        private bool pinSet;
        private string pinCode;
        private string sortingOrder;

        public SettingsPage()
        {
            this.InitializeComponent();
            settings = MainPage.Current.Settings;
            VersionNumberTextBlock.Text = Info.ApplicationVersion;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            pinSet = settings.GetValueOrDefault("PinSetSetting", AppSettings.PinSetSettingDefault);
            pinCode = settings.GetValueOrDefault("PinCodeSetting", AppSettings.PinCodeSettingDefault);
            sortingOrder = settings.GetValueOrDefault("NotesOrderSetting", AppSettings.NotesOrderSettingDefault);
            
            if (sortingOrder == "ByDate")
            {
                ByDateRadioButton.IsChecked = true;
            }
            else if (sortingOrder == "ByTitle")
            {
                ByTitleRadioButton.IsChecked = true;
            }

            if (pinSet)
            {
                CurrentPinCodeTextBox.IsEnabled = true;
                SetNewPinButton.IsEnabled = false;
                NewPinCodeTextBox.Text = "";
                DefaultPinHintTextBlock.Text = loader.GetString("settings_changepincode_changehint");
                DefaultPinHintTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Magenta);
                NewPinCodeTextBox.IsEnabled = false;
            }
        }

        private void CurrentPinCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentPinCodeTextBox.Text.Length == 4)
            {
                if (CurrentPinCodeTextBox.Text == pinCode)
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

                    NewPinCodeTextBox.IsEnabled = true;
                    NewPinCodeTextBox.Focus(FocusState.Programmatic);
                    DefaultPinHintTextBlock.Text = loader.GetString("settings_changepincode_newcodehint");
                    DefaultPinHintTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Magenta);
                }
            }
        }

        private void NewPinCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var input = NewPinCodeTextBox.Text;

            if (input.Length == 4)
            {
                if (input != pinCode)
                {
                    SetNewPinButton.IsEnabled = true;
                }
            }
            else
            {
                SetNewPinButton.IsEnabled = false;
            }
        }

        private void SetNewPinButton_Click(object sender, RoutedEventArgs e)
        {
            var input = int.Parse(NewPinCodeTextBox.Text);
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            MainPage.Current.Settings.AddOrUpdateValue("PinCodeSetting", input.ToString());
            pinCode = input.ToString();
            CurrentPinCodeTextBox.IsEnabled = true;
            SetNewPinButton.IsEnabled = false;
            NewPinCodeTextBox.Text = "";
            CurrentPinCodeTextBox.Text = "";
            DefaultPinHintTextBlock.Text = loader.GetString("settings_changepincode_successhint");
            DefaultPinHintTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            NewPinCodeTextBox.IsEnabled = false;

            if (!pinSet)
            {
                MainPage.Current.Settings.AddOrUpdateValue("PinSetSetting", true);
                pinSet = true;
            }
        }

        private void NewPinCodeTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var inputString = NewPinCodeTextBox.Text;

                if (inputString.Length == 4)
                {
                    if (inputString != pinCode)
                    {
                        var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

                        MainPage.Current.Settings.AddOrUpdateValue("PinCodeSetting", inputString);
                        pinCode = inputString;
                        CurrentPinCodeTextBox.IsEnabled = true;
                        SetNewPinButton.IsEnabled = false;
                        NewPinCodeTextBox.Text = "";
                        CurrentPinCodeTextBox.Text = "";
                        DefaultPinHintTextBlock.Text = loader.GetString("settings_changepincode_successhint");
                        DefaultPinHintTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                        NewPinCodeTextBox.IsEnabled = false;

                        if (!pinSet)
                        {
                            MainPage.Current.Settings.AddOrUpdateValue("PinSetSetting", true);
                            pinSet = true;
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)ByDateRadioButton.IsChecked)
            {
                MainPage.Current.Settings.AddOrUpdateValue("NotesOrderSetting", "ByDate");
                sortingOrder = "ByDate";
            }
            else if((bool)ByTitleRadioButton.IsChecked)
            {
                MainPage.Current.Settings.AddOrUpdateValue("NotesOrderSetting", "ByTitle");
                sortingOrder = "ByTitle";
            }
        }

        private async void ContactButton_Click(object sender, RoutedEventArgs e)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string info = "\n" + Info.ApplicationName +
                        "\n" + Info.ApplicationVersion +
                        "\n---" +
                        "\n" + Info.SystemFamily + " " + Info.SystemArchitecture +
                        "\n" + Info.SystemVersion +
                        "\n---" +
                        "\n" + Info.DeviceManufacturer +
                        "\n" + Info.DeviceModel +
                        "\n******************************";

            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();

            emailMessage.Body = "\n\n\n\n\n\n\n" + loader.GetString("settings_mail_content") + "\n******************************" + info;
            emailMessage.Subject = loader.GetString("settings_mail_subject");

            var email = new Windows.ApplicationModel.Contacts.ContactEmail();
            email.Address = "hamza-dev@outlook.com";

            if (email != null)
            {
                var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient(email.Address);
                emailMessage.To.Add(emailRecipient);
            }

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private async void RateButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(
                string.Format("ms-windows-store:REVIEW?PFN={0}",
                Windows.ApplicationModel.Package.Current.Id.FamilyName
            )));
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += SettingsPage_DataRequested;
        }

        private void SettingsPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var shareText = loader.GetString("settings_share_text") +
                "\n\nhttps://www.microsoft.com/store/apps/" +
                //todo : replace the line below with the app product ID
                "converge/9wzdncrcwc95";

            args.Request.Data.SetText(shareText);
            args.Request.Data.Properties.Title = "Smart Notes";
        }
    }
}
