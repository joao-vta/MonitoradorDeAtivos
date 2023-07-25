using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

HttpClient httpClient = new HttpClient();

ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
IConfiguration configuration = configurationBuilder.AddUserSecrets<Program>().Build();

string API_KEY = configuration["api:key"];

string DEST_EMAIL = configuration["dest_email:email"];
string DEST_EMAIL_NAME = configuration["dest_email:name"];

string SOURCE_EMAIL = configuration["source_email:email"];
string SOURCE_EMAIL_NAME = configuration["source_email:name"];
string SOURCE_EMAIL_API_KEY = configuration["source_email:key"];


async Task<double> GetLastAssetPrice(string asset)
{
    string QUERY_URL = String.Format("https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={0}&interval=5min&apikey={1}",
        asset, API_KEY);
    string responseBody = await httpClient.GetStringAsync(QUERY_URL);

    dynamic parsedResponse = JsonConvert.DeserializeObject(responseBody);
    dynamic parsedData = parsedResponse["Time Series (5min)"];
    SortedDictionary<DateTime, Dictionary<string, double>> formattedData =
        JsonConvert.DeserializeObject<SortedDictionary<DateTime, Dictionary<string, double>>>(parsedData.ToString());
    double lastAssetPrice = formattedData.Last().Value["4. close"];

    return lastAssetPrice;
}


void SendEmail(string title, string body)
{
    var subject = title;
    var plainTextContent = body;

    var client = new SendGridClient(SOURCE_EMAIL_API_KEY);
    var from_email = new EmailAddress(SOURCE_EMAIL, SOURCE_EMAIL_NAME);
    var to_email = new EmailAddress(DEST_EMAIL, DEST_EMAIL_NAME);

    var msg = MailHelper.CreateSingleEmail(from_email, to_email, subject, plainTextContent, "");
    client.SendEmailAsync(msg);
}


string[] arguments = Environment.GetCommandLineArgs();
Console.WriteLine(arguments.Length);
if (arguments.Length < 4)
{
    Console.WriteLine("Error: invalid ammount of arguments. Correct usage: ./program ASSET SELL_PRICE PURCHASE_PRICE");
    throw new InvalidOperationException();
}

string asset = arguments[1];

int sellPrice = -1;
int purchasePrice = -1;
try
{
    sellPrice = Int32.Parse(arguments[2]);
    purchasePrice = Int32.Parse(arguments[3]);
}
catch (FormatException)
{
    Console.WriteLine("Error: invalid sell or purchase price");
    throw new InvalidOperationException();
}

Console.WriteLine("Tracking asset {0}. Advice set to sell above R${1} and to buy beneath R${2}", asset, sellPrice, purchasePrice);


while (true)
{
    double currentPrice = await GetLastAssetPrice(asset);
    Console.WriteLine("Asset {0} currently costs R${1}.", asset, currentPrice);

    if (currentPrice > sellPrice)
    {
        Console.WriteLine("Asset {0} surpassed R${1}. Advice is to sell.", asset, sellPrice);
        SendEmail("Advice to sell asset", String.Format("Hello! You should probably sell asset {0}. " +
            "It is currenlty worth {1}, more than the set price {2}.", asset, currentPrice, sellPrice));
    }

    if (currentPrice < purchasePrice)
    {
        Console.WriteLine("Asset {0} dropped bellow R${1}. Advice is to buy.", asset, purchasePrice);
        SendEmail("Advice to buy asset", String.Format("Hello! You should probably buy asset {0}. " +
            "It is currenlty worth {1}, more than the set price {2}.", asset, currentPrice, purchasePrice));
    }

    // Sleep set to 5 minutes due to API limitations
    Thread.Sleep(1000*60*5);
}
