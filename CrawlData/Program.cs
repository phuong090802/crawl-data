using Microsoft.Extensions.Configuration;
using PuppeteerSharp;

await CrawDataHandle();

static async Task CrawDataHandle()
{
    const string path = "/";
    const string domain = ".facebook.com";
    var expires = DateTime.Parse("2024-10-04T08:00:41.551Z").Subtract(new DateTime(1970, 1, 1)).TotalMicroseconds;

    #region for development
    // var configuration = new ConfigurationBuilder()
    //     .SetBasePath(Environment.CurrentDirectory)
    //     .AddJsonFile("appsettings.json")
    //     .Build();
    // using var browserFetcher = new BrowserFetcher();
    // if (!browserFetcher.GetInstalledBrowsers().Any())
    // {
    //     await browserFetcher.DownloadAsync();
    // }
    // await browserFetcher.DownloadAsync();
    #endregion

    #region  for Docker
    var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
    #endregion


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
             Value = "38%3ArNxproaxhqZNpw%3A2%3A1696492853%3A-1%3A-1",
             Domain = domain,
             Path = path,
             Expires = expires,
             HttpOnly = true,
             Secure = true
         },
         new CookieParam
         {
             Name = "c_user",
             Value = "61551762699177",
             Domain = domain,
             Path = path,
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
            await Console.Out.WriteLineAsync("end");
            break;

        }
        hrefs.AddRange(uniqueNewHrefs);
    }

    // Close the browser
    await browser.CloseAsync();

}

static async Task<List<string>> ScrollAndGetHrefs(IPage page)
{
    await page.EvaluateFunctionAsync(@"() => {
        window.scrollTo(0, document.body.scrollHeight);
    }");
    await page.WaitForTimeoutAsync(1500);
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