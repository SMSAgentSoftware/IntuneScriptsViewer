using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Identity.Client;

namespace Endpoint_Manager_Scripts_Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            _clientApp = PublicClientApplicationBuilder.Create(ClientId)
                //.WithAuthority($"{Instance}{Tenant}")
                .WithDefaultRedirectUri()
                .Build();
            //TokenCacheHelper.EnableSerialization(_clientApp.UserTokenCache);
        }
        private static string ClientId = "14d82eec-204b-4c2f-b7e8-296a70dab67e"; //Microsoft Graph PowerShell (Preview) / Microsoft Graph Command Line Tools //"d1ddf0e4-d672-4dae-b554-9d5bdfd93547"; //Microsoft - Microsoft Intune PowerShell
        //private static string Tenant = "<tenantId here>";
        //private static string Instance = "https://login.microsoftonline.com/";
        private static IPublicClientApplication _clientApp;

        public static IPublicClientApplication PublicClientApp { get { return _clientApp; } }
    }
}
