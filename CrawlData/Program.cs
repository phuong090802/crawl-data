using Microsoft.Extensions.Configuration;
using PuppeteerSharp;

// const string ENVIRONMENT = "docker";
const string ENVIRONMENT = "development";

var builder = new ConfigurationBuilder();

if (ENVIRONMENT == "development")
{
    builder.SetBasePath(Environment.CurrentDirectory);
}

var configuration = builder
    .AddJsonFile("appsettings.json")
    .Build();

await CrawDataHandle(configuration);

static async Task CrawDataHandle(IConfigurationRoot configuration)
{
    var expires = DateTime.Parse(configuration.GetSection("AppSettings:Expires").Value!).Subtract(new DateTime(1970, 1, 1)).TotalMicroseconds;

    var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        // without UI
        Headless = true,
        // Args = new[] { "--no-sandbox" }
    });
    var page = await browser.NewPageAsync();

    var cookies = new CookieParam[]
    {
         new CookieParam
         {
             Name = "xs",
             Value = configuration.GetSection("AppSettings:Xs").Value!,
             Domain = configuration.GetSection("AppSettings:Domain").Value!,
             Path = configuration.GetSection("AppSettings:Path").Value!,
             Expires = expires,
             HttpOnly = true,
             Secure = true
         },
         new CookieParam
         {
             Name = "c_user",
             Value = configuration.GetSection("AppSettings:CUser").Value!,
             Domain = configuration.GetSection("AppSettings:Domain").Value!,
             Path = configuration.GetSection("AppSettings:Path").Value!,
             Expires = expires,
             HttpOnly = false,
             Secure = true
         }

    };
    await Console.Out.WriteLineAsync("starting...");
    await page.SetCookieAsync(cookies);
    await page.GoToAsync(configuration.GetSection("AppSettings:Target").Value!);
    await page.WaitForTimeoutAsync(2500);
    var hrefs = new List<string>();
    while (true)
    {
        var newHrefs = await ScrollAndGetHrefs(page);

        var uniqueNewHrefs = newHrefs.Where(href => !hrefs.Contains(href)).ToList();
        uniqueNewHrefs.ForEach(Console.WriteLine);

        if (uniqueNewHrefs.Count == 0)
        {
            await Console.Out.WriteLineAsync("ended");
            break;
        }
        hrefs.AddRange(uniqueNewHrefs);
    }

    // Close the browser
    await browser.CloseAsync();

}

static async Task<List<string>> ScrollAndGetHrefs(IPage page)
{
    await page.WaitForTimeoutAsync(5000);
    await page.EvaluateFunctionAsync(@"() => {
        window.scrollTo(0, document.body.scrollHeight);
    }");
    var hrefs = await page.EvaluateFunctionAsync<List<string>>(@"
        () => {
            const anchors = document.querySelectorAll('span.xt0psk2 > a');
            const hrefs = [];
            anchors.forEach(anchor => {
                const href = anchor.href
                if (!hrefs.includes(href)) {
                    hrefs.push(href);
                }
            });
            return hrefs;
        }
    ");
    return hrefs;
}