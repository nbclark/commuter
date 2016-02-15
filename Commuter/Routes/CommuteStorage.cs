using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml;
using MobileSrc.Commuter.Shared;

namespace Commuter
{
    public class CommuteStorage
    {
        private const string _commuteStorage = @"\commutecache.xml";
        private const string _settingsStorage = @"\commutesettings.xml";
        private static XmlSerializer _serializer = new XmlSerializer(typeof(CommuteCollection));
        private static XmlSerializer _settingsSerializer = new XmlSerializer(typeof(Settings));

        public static CommuteCollection Load()
        {
            // return value
            CommuteCollection commutes = null;
            // deserialize weather information
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    using (Stream stream = store.OpenFile(_commuteStorage, FileMode.Open))
                    {
                        StreamReader sr = new StreamReader(stream);
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            commutes = (CommuteCollection)_serializer.Deserialize(reader);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // failed reading cached file
                    // assuming this is first launch : return fake data
                    commutes = new CommuteCollection();
                }
            }

            // return weather info
            return commutes;
        }

        internal static Settings LoadSettings()
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    using (Stream stream = store.OpenFile(_settingsStorage, FileMode.Open))
                    {
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            return (Settings)_settingsSerializer.Deserialize(reader);
                        }
                    }
                }
                catch
                {
                    // failed reading cached file
                    // assuming this is first launch : return fake data
                    return new Settings();
                }
            }
        }

        public static void Save()
        {
            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // create a new file (overwrite existing)
                    using (IsolatedStorageFileStream file = store.CreateFile(_commuteStorage))
                    {
                        _serializer.Serialize(file, DataContextManager.Commutes);
                    }
                    // create a new file (overwrite existing)
                    using (IsolatedStorageFileStream file = store.CreateFile(_settingsStorage))
                    {
                        _settingsSerializer.Serialize(file, DataContextManager.Settings);
                    }
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Error Saving Commute Information");
            }
        }
    }
}
