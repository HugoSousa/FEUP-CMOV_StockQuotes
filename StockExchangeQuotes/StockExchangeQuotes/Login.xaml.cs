using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace StockExchangeQuotes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page, OnApiRequestCompleted
    {
        public Login()
        {
            this.InitializeComponent();
        }


        private void LoginClick(object sender, RoutedEventArgs e)
        {
            if (UsernameField.Text == "" || PasswordField.Password == "")
            {
                ErrorField.Text = "All fields are mandatory.";
                return;
            }

            ErrorField.Text = "";

            APIRequest request = new APIRequest(APIRequest.POST, this, APIRequest.requestCodeType.Login, "login");

            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"username", UsernameField.Text},
                {"password", PasswordField.Password}
            };
            var serializer = new DataContractJsonSerializer(dict.GetType(), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true
            });
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, dict);
            byte[] bytes = stream.ToArray();
            string content = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            
            request.Execute(null, content);
        }

        public void onTaskCompleted(string result, APIRequest.requestCodeType requestCode)
        {
            if (result != null)
            {
                if (requestCode == APIRequest.requestCodeType.Login)
                {
                    JsonObject json = JsonObject.Parse(result);
                    if (!json.ContainsKey("error"))
                    {
                        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                        JsonObject user = json.GetNamedObject("user");

                        localSettings.Values["token"] = json.GetNamedString("token");
                        if (user.GetNamedValue("main_share").ValueType != JsonValueType.Null)
                            localSettings.Values["main_share"] = user.GetNamedNumber("main_share");
                        localSettings.Values["username"] = user.GetNamedString("login");

                        Frame.Navigate(typeof (MainPage));
                        Frame.BackStack.Clear();
                    }
                    else
                    {
                        ErrorField.Text = json.GetNamedString("error");
                    }
                }
            }
            else
            {
                var toastXmlContent = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

                var txtNodes = toastXmlContent.GetElementsByTagName("text");
                txtNodes[0].AppendChild(toastXmlContent.CreateTextNode("Server request failed."));
                txtNodes[1].AppendChild(toastXmlContent.CreateTextNode("Server is down or you lost internet connection."));

                var toast = new ToastNotification(toastXmlContent);
                var toastNotifier = ToastNotificationManager.CreateToastNotifier();
                toastNotifier.Show(toast);
            }
        }

        private void RegisterClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (Register));
        }
    }
}
