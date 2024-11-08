using System.IO;
using System.Threading;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;


namespace UPDL_Speed_Tracker.Google_API
{
    class GoogleSheetsHelper
    {
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Google Sheets API .NET Quickstart";
        private SheetsService service;

        public GoogleSheetsHelper()
        {
            InitializeService();
        }

        private void InitializeService()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("your-path: to\\Google Sheets API Credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        //
        //Add Data to Sheet from user input
        public void AddDataToSheet(string spreadsheetId, string range, IList<IList<object>> values)
        {
            var valueRange = new ValueRange
            {
                Values = values
            };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            appendRequest.Execute();
        }

        //
        //Check Row Has Data or not before store data
        public bool RowHasData(string spreadsheetId, string range)
        {
            SpreadsheetsResource.ValuesResource.GetRequest getRequest =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = getRequest.Execute();
            IList<IList<object>> values = response.Values;

            return values != null && values.Count > 0;
        }
    }
}