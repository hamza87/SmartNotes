using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace SmartNotes.Models
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Tag> Tags { get; set; }
        public string Text { get; set; }
        public string StringColor { get; set; }
        public string EditDate { get; set; }
        public Color Color { get; set; }
        public DateTime Date { get; set; }
        public string PinText { get; set; }
        public bool Locked { get; set; }
        public string NoteVisibility { get; set; }
        public string LockVisibility { get; set; }
        public string LockNoteFlyoutText { get; set; }
    }

    public class NoteManager
    {
        public static List<Note> AllNotesList { get; private set; }
        public static List<Tag> AllTagsList { get; private set; }

        public static int NewId()
        {
            var random = new Random();
            int test = 0;
            List<int> ids = new List<int>();

            AllNotesList.ForEach(p => ids.Add(p.Id));

            do
            {
                test = random.Next(1, 10000);
            } while (ids.Contains(test));

            return test;
        }

        public static Note GetNoteById(int id)
        {
            if (id == 0) return createEmptyNote();

            var filteredNote = AllNotesList.Where(p => p.Id == id).FirstOrDefault();

            return filteredNote;
        }

        public static List<Note> GetNotesByTag(Tag tag)
        {
            var filteredNotes = new List<Note>();

            foreach (var note in AllNotesList)
            {
                foreach (var t in note.Tags)
                {
                    if (t.Text == tag.Text)
                    {
                        filteredNotes.Add(note);
                        break;
                    }
                }
            }

            return filteredNotes;
        }

        public static List<Note> GetNotesByTerm(string term)
        {
            var filteredNotes = new List<Note>();

            foreach(var note in AllNotesList)
            {
                string lowerText = note.Text.ToLower();
                if (lowerText.Contains(term.ToLower()))
                {
                    filteredNotes.Add(note);
                }
            }
            return filteredNotes;
        }

        public static void AddNote(Note note)
        {
            AllNotesList.Add(note);
        }

        public static void UpdateNote(Note note)
        {
            var index = AllNotesList.IndexOf(note);

            AllNotesList[index] = note;
        }

        public static void DeleteNote(Note note)
        {
            AllNotesList.Remove(note);
        }

        public static async Task<List<Note>> GetAllNotes()
        {
            if (AllNotesList == null || !AllNotesList.Any())
            {
                AllNotesList = new List<Note>();
                await LoadNotes("Notes");
            }
            return AllNotesList;
        }

        public static async Task<List<Tag>> GetAllTags()
        {
            if (AllNotesList == null || !AllNotesList.Any())
            {
                AllNotesList = new List<Note>();
                await LoadNotes("Notes");
            }

            List<Tag> tags = new List<Tag>();
            List<string> tagTexts = new List<string>();

            AllNotesList.ForEach(p => tags.AddRange(p.Tags));
            tags.ForEach(p => tagTexts.Add(p.Text));

            var filteredTags = new List<Tag>();
            var filteredTexts = new List<string>();
            foreach (var t in tags)
            {
                if (!filteredTexts.Contains(t.Text))
                {
                    filteredTags.Add(t);
                    filteredTexts.Add(t.Text);
                }
            }

            var sortedTags = new List<Tag>();
            sortedTags = filteredTags.OrderBy(p => p.Text).ToList();

            return sortedTags;
        }

        private static Note createEmptyNote()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            return new Note { Id = 0, Color = Colors.White, StringColor = "White", EditDate = "", Title = "", Tags = new List<Tag> { }, Text = "", PinText = loader.GetString("pin_text"), Locked = false, LockVisibility = "Collapsed", NoteVisibility = "Visible", LockNoteFlyoutText = loader.GetString("lock_text") };
        }

        public static string FormatDate(DateTime date)
        {
            string dateString = date.ToString("d", CultureInfo.CurrentUICulture);
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            if ((DateTime.Now - date).Minutes < 1)
            {
                return loader.GetString("now");
            }
            else if (date.Day == DateTime.Today.Day)
            {
                return date.ToString("t", CultureInfo.CurrentUICulture);
            }
            else if (date.Day == DateTime.Today.Day - 1)
            {
                return loader.GetString("yesterday");
            }
            else if (date.Year == DateTime.Today.Year)
            {
                return date.ToString("m", CultureInfo.CurrentUICulture);
            }
            else
            {
                return dateString;
            }
        }

        public static Tag SetTag(string text)
        {
            if (AllTagsList == null || !AllTagsList.Any())
            {
                AllTagsList = new List<Tag>();
                AllTagsList.Add(new Tag { Text = text });
                return AllTagsList[0];
            }
            var tag = AllTagsList.Where(p => p.Text == text).FirstOrDefault();
            if (tag == null)
            {
                var newTag = new Tag { Text = text };
                AllTagsList.Add(newTag);
                return newTag;
            }
            else
                return tag;
        }

        public static async Task SaveNotes()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile notesFile;
            
            try
            {
                notesFile = await folder.CreateFileAsync("Notes", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(notesFile, XmlSerialization.ToXml(AllNotesList));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task LoadNotes(string notesFileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile notesFile;

            try
            {
                notesFile = await folder.GetFileAsync(notesFileName);
            }
            catch (FileNotFoundException e)
            {
                notesFile = null;
            }
            if (notesFile != null)
            {
                string xmlText = await FileIO.ReadTextAsync(notesFile);
                AllNotesList = XmlSerialization.FromXml(xmlText);
                AllNotesList.ForEach(p => p.EditDate = FormatDate(p.Date));
            }
            else
            {
                return;
            }
        }
    }

    public class Tag
    {
        public string Text { get; set; }
    }
}
