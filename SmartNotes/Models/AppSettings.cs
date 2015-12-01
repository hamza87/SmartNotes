using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNotes.Models
{
    public class AppSettings
    {
        // Our settings
        public Windows.Storage.ApplicationDataContainer settings;

        // The key names of our settings
        const string PinCodeSettingKeyName = "PinCodeSetting";
        const string PinSetSettingKeyName = "PinSetSetting";
        const string NotesOrderSettingKeyName = "NotesOrderSetting";

        // The default value of our settings
        public static string PinCodeSettingDefault = "0000";
        public static bool PinSetSettingDefault = false;
        public static string NotesOrderSettingDefault = "ByDate";

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public AppSettings()
        {
            // Get the settings for this application.
            settings = Windows.Storage.ApplicationData.Current.LocalSettings;
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void AddOrUpdateValue(string Key, Object value)
        {
            // If the key exists
            if (settings.Values.Keys.Contains(Key))
            {
                // If the value has changed
                if (settings.Values[Key] != value)
                {
                    // Store the new value
                    settings.Values[Key] = value;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Values.Add(Key, value);
            }
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Values.Keys.Contains(Key))
            {
                value = (T)settings.Values[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }
    }
}
