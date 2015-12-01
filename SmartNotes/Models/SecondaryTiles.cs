using NotificationsExtensions.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace SmartNotes.Models
{
    public class SecondaryTiles
    {
        public static void UpdateTile(string tileId, Note note)
        {
            SecondaryTile secondaryTile = new SecondaryTile(tileId);

            var noteContent = note.Text;
            var noteTitle = note.Title;

            if (noteContent.Length > 200) noteContent = noteContent.Substring(0, 200);

            var tileContent = CreateTileTemplate();

            XmlDocument doc = tileContent.GetXml();

            XmlNodeList tileTextAttributes = doc.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = noteTitle;
            tileTextAttributes[1].InnerText = noteContent;
            tileTextAttributes[2].InnerText = noteTitle;
            tileTextAttributes[3].InnerText = noteContent;
            tileTextAttributes[4].InnerText = noteTitle;
            tileTextAttributes[5].InnerText = noteContent;
            tileTextAttributes[6].InnerText = noteTitle;
            tileTextAttributes[7].InnerText = noteContent;

            var tileNotification = new TileNotification(doc);

            TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId).Update(tileNotification);
        }

        public static TileContent CreateTileTemplate()
        {
            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,

                    DisplayName = "Smart Notes",

                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new TileText()
                                {
                                    Text = "Cars",
                                    Style = TileTextStyle.Caption
                                },
                                new TileText()
                                {
                                    Text = "Mercedes, Renault, Chevrolet, BMW, Volkswagen",
                                    Style = TileTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new TileText()
                                {
                                    Text = "Cars",
                                    Style = TileTextStyle.Caption,
                                },
                                new TileText()
                                {
                                    Text = "Mercedes, Renault, Chevrolet, BMW, Volkswagen",
                                    Style = TileTextStyle.CaptionSubtle,
                                    Wrap = true
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new TileText()
                                {
                                    Text = "Cars",
                                    Style = TileTextStyle.Caption,
                                },
                                new TileText()
                                {
                                    Text = "Mercedes, Renault, Chevrolet, BMW, Volkswagen",
                                    Style = TileTextStyle.CaptionSubtle,
                                    Wrap = true
                                }
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new TileText()
                                {
                                    Text = "Cars",
                                    Style = TileTextStyle.Body
                                },
                                new TileText()
                                {
                                    Text = "Mercedes, Renault, Chevrolet, BMW, Volkswagen",
                                    Style = TileTextStyle.BodySubtle,
                                    Wrap = true
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
