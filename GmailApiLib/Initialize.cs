using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GmailApiLib
{
    public static class Initialize
    {
        private static readonly List<string> Scopes = new List<string> {GmailService.Scope.MailGoogleCom};
        private const string ApplicationName = "Gmail API .NET Checker";

        public static async Task StartDeleting(string user, string from, string search)
        {
            await Task.Run(() =>
            {
                try
                {
                    UserCredential credential;

                    using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                    {
                        var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                        credPath = Path.Combine(credPath, ".credentials\\gmail-checker");

                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            Scopes,
                            user,
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
                        Informer.RaiseOnResultReceived($"Credential file saved to: {credPath}");
                    }

                    // Create Gmail API service.
                    var service = new GmailService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = ApplicationName,
                    });

                    //Get messages
                    var messages = service.Users.Messages.List("me");
                    messages.Q =
                        $"in:inbox from:{from} before:{DateTime.Now.ToString("yyyy/MM/dd", new CultureInfo("En-us"))} '{search}'";
                    var msgForDel = messages.Execute().Messages;

                    if (msgForDel == null || msgForDel.Count == 0)
                    {
                        Informer.RaiseOnResultReceived("No message containing search pattern found");
                        return;
                    }

                    //Start deleting
                    foreach (var message in msgForDel)
                    {
                        try
                        {
                            service.Users.Messages.Trash("me", message.Id).Execute();
                            Informer.RaiseOnResultReceived($"Successfully moved to trash message with ID: {message.Id}");
                        }
                        catch (Exception ex)
                        {
                            Informer.RaiseOnResultReceived($"Can't move to trash message with ID: {message.Id}");
                            Informer.RaiseOnResultReceived(ex);
                        }
                    }
                    Informer.RaiseOnResultReceived($"{msgForDel.Count} messages successfully moved to trash");
                }
                catch (Exception ex)
                {
                    Informer.RaiseOnResultReceived(ex);
                }
            });
        }
    }
}