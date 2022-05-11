using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace Endpoint_Manager_Scripts_Editor
{
    public partial class MainWindow
    {
        string[] scopes = new string[] { "DeviceManagementConfiguration.ReadWrite.All" };
        private RootValue scripts;
        private string authToken;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Btn_ConnectIntune(object sender, RoutedEventArgs e)
        {
            AuthenticationResult authResult = null;
            var app = App.PublicClientApp;

            var accounts = await app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await app.AcquireTokenSilent(scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent. 
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await app.AcquireTokenInteractive(scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithParentActivityOrWindow(new WindowInteropHelper(this).Handle) // optional, used to center the browser on the window
                        .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
                        .ExecuteAsync();
                }
                catch (MsalException msalex)
                {
                    Status.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
                }
            }
            catch (Exception ex)
            {
                Status.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
                return;
            }

            if (authResult != null)
            {
                Tenant.Text = $"TenantId: {authResult.TenantId}";
                Account.Text = $"Account: {authResult.Account.Username}";
                Status.Text = "Retrieving list of scripts";
                authToken = authResult.AccessToken;
                string graphAPIEndpoint = "https://graph.microsoft.com/beta/deviceManagement/deviceManagementScripts";
                var result = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);

                scripts = JsonConvert.DeserializeObject<RootValue>(result);
                ArrayList scriptList = new ArrayList();
                foreach (Script script in scripts.value)
                {
                    scriptList.Add(script.displayName);
                };
                scriptList.Sort();
                ComboBox.ItemsSource = scriptList;
                Status.Text = "Select a script";
            }
        }

        public async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = scripts.value.FindIndex(x => x.displayName == ComboBox.SelectedItem);
            string id = scripts.value[index].id;
            string graphAPIEndpoint = "https://graph.microsoft.com/beta/deviceManagement/deviceManagementScripts" + "/" + id;
            var result = await GetHttpContentWithToken(graphAPIEndpoint, authToken);

            Script script = JsonConvert.DeserializeObject<Script>(result);
            var base64 = Convert.FromBase64String(script.scriptContent);
            var scriptText = Encoding.UTF8.GetString(base64);
            ScriptWindow.Text = scriptText;
            FileName.IsEnabled = true;
            FileName.Foreground = System.Windows.Media.Brushes.AntiqueWhite;
            FileName.Text = $"Filename: {script.fileName} |";
            RunasThirtyTwo.IsEnabled = true;
            RunasThirtyTwo.Text = $"Run as 32-bit: {script.runAs32Bit} |";
            RunasThirtyTwo.Foreground = System.Windows.Media.Brushes.AntiqueWhite;
            SignatureCheck.IsEnabled = true;
            SignatureCheck.Text = $"Enforce signature check: {script.enforceSignatureCheck} |";
            SignatureCheck.Foreground = System.Windows.Media.Brushes.AntiqueWhite;
            RunasAccount.IsEnabled = true;
            RunasAccount.Text = $"Run as account: {script.runAsAccount} |";
            RunasAccount.Foreground = System.Windows.Media.Brushes.AntiqueWhite;
            Created.IsEnabled = true;
            Created.Text = $"Created: {script.createdDateTime} |";
            Created.Foreground = System.Windows.Media.Brushes.AntiqueWhite;
            Modified.IsEnabled = true;
            Modified.Text = $"Modified: {script.lastModifiedDateTime} |";
            Modified.Foreground = System.Windows.Media.Brushes.AntiqueWhite;
            Description.IsEnabled = true;
            Description.Text = $"Description: {script.description}";
            Description.Foreground = System.Windows.Media.Brushes.AntiqueWhite;
        }

        private void MetroWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
    public class RootValue
    {
        public string odatametadata { get; set; }
        public List<Script> value;
    }
    public class Script
    {
        public bool enforceSignatureCheck { get; set; }
        public bool runAs32Bit { get; set; }
        public string id { get; set; }
        public string displayName { get; set; }
        public string scriptContent { get; set; }
        public string description { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string runAsAccount { get; set; }
        public string fileName { get; set; }
    }

}
