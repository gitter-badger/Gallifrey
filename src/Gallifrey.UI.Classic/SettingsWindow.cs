﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gallifrey.Exceptions.IntergrationPoints;

namespace Gallifrey.UI.Classic
{
    public partial class SettingsWindow : Form
    {
        private readonly IBackend gallifrey;

        public SettingsWindow(IBackend gallifrey)
        {
            this.gallifrey = gallifrey;
            InitializeComponent();

            if (gallifrey.Settings.JiraConnectionSettings.JiraUrl != null) txtJiraUrl.Text = gallifrey.Settings.JiraConnectionSettings.JiraUrl;
            if (gallifrey.Settings.JiraConnectionSettings.JiraUsername != null) txtJiraUsername.Text = gallifrey.Settings.JiraConnectionSettings.JiraUsername;
            if (gallifrey.Settings.JiraConnectionSettings.JiraPassword != null) txtJiraPassword.Text = gallifrey.Settings.JiraConnectionSettings.JiraPassword;

            chkAlert.Checked = gallifrey.Settings.AppSettings.AlertWhenNotRunning;
            chkAutoUpdate.Checked = gallifrey.Settings.AppSettings.AutoUpdate;
            txtAlertMins.Text = ((gallifrey.Settings.AppSettings.AlertTimeMilliseconds / 1000) / 60).ToString();
            txtTimerDays.Text = gallifrey.Settings.AppSettings.KeepTimersForDays.ToString();

            txtTargetHours.Text = gallifrey.Settings.AppSettings.TargetLogPerDay.Hours.ToString();
            txtTargetMins.Text = gallifrey.Settings.AppSettings.TargetLogPerDay.Minutes.ToString();

            for (var i = 0; i < chklstWorkingDays.Items.Count; i++)
            {
                var foundItem = gallifrey.Settings.AppSettings.ExportDays.Any(exportDay => exportDay.ToString().ToLower() == chklstWorkingDays.Items[i].ToString().ToLower());

                chklstWorkingDays.SetItemChecked(i, foundItem);
            }

            cmdWeekStart.Text = gallifrey.Settings.AppSettings.StartOfWeek.ToString();

            chkAlwaysTop.Checked = gallifrey.Settings.UiSettings.AlwaysOnTop;
        }

        private void btnCancelEditSettings_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraPassword))
            {
                MessageBox.Show("You have to populate the Jira Credentials!", "Missing Config");
                return;
            }
            CloseWindow();
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            TopMost = false;

            int keepTimerDays, alertTime;
            if (!int.TryParse(txtAlertMins.Text, out alertTime)) alertTime = 0;
            if (!int.TryParse(txtTimerDays.Text, out keepTimerDays)) keepTimerDays = 28;

            gallifrey.Settings.AppSettings.AlertWhenNotRunning = chkAlert.Checked;
            gallifrey.Settings.AppSettings.AlertTimeMilliseconds = (alertTime * 60) * 1000;
            gallifrey.Settings.AppSettings.KeepTimersForDays = keepTimerDays;
            gallifrey.Settings.AppSettings.AutoUpdate = chkAutoUpdate.Checked;
            gallifrey.Settings.UiSettings.AlwaysOnTop = chkAlwaysTop.Checked;

            int hours, minutes;

            if (!int.TryParse(txtTargetHours.Text, out hours)) { hours = 7; }
            if (!int.TryParse(txtTargetMins.Text, out minutes)) { minutes = 30; }

            gallifrey.Settings.AppSettings.TargetLogPerDay = new TimeSpan(hours, minutes, 0);

            gallifrey.Settings.AppSettings.ExportDays = (from object t in chklstWorkingDays.CheckedItems select (DayOfWeek) Enum.Parse(typeof (DayOfWeek), t.ToString(), true));
            gallifrey.Settings.AppSettings.StartOfWeek = (DayOfWeek) Enum.Parse(typeof (DayOfWeek), cmdWeekStart.Text, true);

            gallifrey.Settings.JiraConnectionSettings.JiraUrl = txtJiraUrl.Text;
            gallifrey.Settings.JiraConnectionSettings.JiraUsername = txtJiraUsername.Text;
            gallifrey.Settings.JiraConnectionSettings.JiraPassword = txtJiraPassword.Text;

            if (string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraUrl) ||
                    string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraUsername) ||
                    string.IsNullOrWhiteSpace(gallifrey.Settings.JiraConnectionSettings.JiraPassword))
            {
                MessageBox.Show("You have to populate the Jira Credentials!", "Missing Config");
                DialogResult = DialogResult.None;
                TopMost = true;
                return;
            }

            TopMost = true;
            CloseWindow();
        }

        private void CloseWindow()
        {
            TopMost = false;
            try
            {
                gallifrey.SaveSettings();
            }
            catch (JiraConnectionException)
            {
                MessageBox.Show("Unable to connect to Jira with these settings!", "Unable to connect");
                DialogResult = DialogResult.None;
                TopMost = true;
                return;
            }

            Close();
        }

        private void chkAlert_CheckedChanged(object sender, EventArgs e)
        {
            txtAlertMins.Enabled = chkAlert.Checked;
        }

    }
}
