using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Mneme
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        bool editing = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Open XML document and populate dictionary, if it doesn't exist then create one
            if (!File.Exists("data.xml"))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                using (XmlWriter writer = XmlWriter.Create("data.xml", settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteComment("This is an auto generated file, please don't mess around with it unless you know what you are doing");

                    //Main Element
                    writer.WriteStartElement("DataSets");

                    //Sample
                    writer.WriteStartElement("Data");
                    writer.WriteAttributeString("Key", "(SAMPLE) Phone Number");
                    writer.WriteValue("(123)555-1234");
                    writer.WriteEndElement();

                    writer.WriteEndDocument();
                }
            }

            using (XmlReader reader = XmlReader.Create("data.xml"))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        string key = string.Empty;
                        string value = string.Empty;
                        switch (reader.Name)
                        {
                            case "Data":
                                key = reader["Key"];
                                if (reader.Read())
                                {
                                    value = reader.Value.Trim();
                                }
                                data.Add(key, value);
                                break;
                            default:
                                break;
                        }                        
                    }
                }
            }


            //populate ListBox
            foreach (KeyValuePair<string, string> input in data)
            {
                KeyList.Items.Add(input.Key);
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void KeyList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (KeyList.SelectedIndex != -1)
            {
                KeyBox.Text = "";
                ValueBox.Text = "";
                StatusLabel.Text = "Status: Nothing to report";
                if (KeyList.SelectedIndex == 0)
                {
                    EditButton.Enabled = false;
                    DeleteButton.Enabled = false;
                    ClipboardButton.Enabled = false;
                    SaveButton.Enabled = true;
                    KeyBox.ReadOnly = false;
                    ValueBox.ReadOnly = false;

                    KeyBox.Focus();
                }
                else
                {
                    try
                    {
                        KeyBox.Text = KeyList.Items[KeyList.SelectedIndex].ToString();
                        ValueBox.Text = data[KeyList.Items[KeyList.SelectedIndex].ToString()];
                        ValueBox.ReadOnly = true;
                        SaveButton.Enabled = false;
                        EditButton.Enabled = true;
                        DeleteButton.Enabled = true;
                        ClipboardButton.Enabled = true;
                        ClipboardButton.Focus();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void ClipboardButton_Click(object sender, EventArgs e)
        {
            if (KeyList.SelectedIndex != -1 || KeyList.SelectedIndex != 0)
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(data[KeyList.Items[KeyList.SelectedIndex].ToString()], TextDataFormat.Text);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (KeyBox.Text != "" || KeyBox.Text != null)
            {
                StatusLabel.Text = "Status: Saving...";
                try
                {
                    if (!editing)
                    {
                        data.Add(KeyBox.Text, ValueBox.Text);
                        KeyList.Items.Add(KeyBox.Text);
                        KeyList.SelectedIndex = KeyList.Items.Count - 1;
                    }
                    else
                    {
                        data[KeyList.Items[KeyList.SelectedIndex].ToString()] = ValueBox.Text;
                        editing = false;
                    }
                    saveXML();

                    StatusLabel.Text = "Status: Saved!";

                    EditButton.Enabled = true;
                    DeleteButton.Enabled = true;
                    ClipboardButton.Enabled = true;
                    SaveButton.Enabled = false;
                    KeyBox.ReadOnly = true;
                    ValueBox.ReadOnly = true;                    
                }
                catch (ArgumentException) { StatusLabel.Text = "Status: This name already exists, please use a unique name"; }
                catch (Exception ex) { StatusLabel.Text = "Status: " + ex.Message; }
            }
            else { StatusLabel.Text = "Status: Empty names are not allowed"; }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            ValueBox.ReadOnly = false;
            SaveButton.Enabled = true;
            EditButton.Enabled = false;

            editing = true;
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to remove \"" + KeyList.Items[KeyList.SelectedIndex].ToString() + "\"?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                try
                {
                    StatusLabel.Text = "Status: Removing...";
                    data.Remove(KeyList.Items[KeyList.SelectedIndex].ToString());
                    KeyList.Items.Remove(KeyList.Items[KeyList.SelectedIndex].ToString());
                    saveXML();
                    StatusLabel.Text = "Status: Removed!";
                    KeyList.SelectedIndex = KeyList.Items.Count - 1;
                }
                catch (Exception ex) { StatusLabel.Text = "Status: " + ex.Message; }
            }
        }

        private void saveXML()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create("data.xml", settings))
            {
                writer.WriteStartDocument();
                writer.WriteComment("This is an auto generated file, please don't mess around with it unless you know what you are doing");

                //Main Element
                writer.WriteStartElement("DataSets");

                foreach (KeyValuePair<string, string> input in data)
                {
                    writer.WriteStartElement("Data");
                    writer.WriteAttributeString("Key", input.Key);
                    writer.WriteValue(input.Value);
                    writer.WriteEndElement();
                }               

                writer.WriteEndDocument();
            }
        }

        private void ValueBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && ValueBox.Enabled == true)
            {
                SaveButton.PerformClick();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        
    }
}
