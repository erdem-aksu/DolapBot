using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bogus;
using DolapBot.Client;
using DolapBot.Client.Extensions;
using DolapBot.Client.Models;
using Newtonsoft.Json.Linq;
using RestSharp;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using TaskExtensions = DolapBot.Client.Extensions.TaskExtensions;

namespace DolapBot
{
    public partial class Form1 : Form
    {
        private const string ProxiesFileName = "proxies.txt";
        private const string ProductsFileName = "products.txt";
        private const string AccountFileName = "accounts.txt";
        private const string FollowsFileName = "follows.txt";

        private Dictionary<string, DateTime?> _follows = new Dictionary<string, DateTime?>();
        private DolapRestClient _loggedUserClient;

        public Form1()
        {
            InitializeComponent();
            Console.SetOut(new RichTextBoxWriter(richTextBox1));

            InitData();
        }

        private void InitData()
        {
            var basePath = AppContext.BaseDirectory;

            var followsPath = Path.Combine(basePath, FollowsFileName);

            if (File.Exists(followsPath))
            {
                foreach (var line in File.ReadAllLines(followsPath))
                {
                    var splits = line.Split("\t");

                    _follows.TryAdd(
                        splits[0],
                        string.IsNullOrEmpty(splits[1]) ? null : DateTime.Parse(splits[1])
                    );
                }
            }

            // var productsPath = Path.Combine(basePath, ProductsFileName);
            // var proxiessPath = Path.Combine(basePath, ProxiesFileName);
            //
            // if (File.Exists(accountsPath))
            // {
            //     foreach (var line in File.ReadAllLines(accountsPath).Take(1000))
            //     {
            //         accountBox.AppendText(line);
            //     }
            // }
            //
            // if (File.Exists(productsPath))
            // {
            //     foreach (var line in File.ReadAllLines(productsPath).Take(1000))
            //     {
            //         productListBox.AppendText(line);
            //     }
            // }
            //
            // if (File.Exists(proxiessPath))
            // {
            //     foreach (var line in File.ReadAllLines(proxiessPath).Take(1000))
            //     {
            //         proxyListBox.AppendText(line);
            //     }
            // }
        }

        private void SaveData()
        {
            var basePath = AppContext.BaseDirectory;

            var accountsPath = Path.Combine(basePath, AccountFileName);
            var productsPath = Path.Combine(basePath, ProductsFileName);
            var proxiessPath = Path.Combine(basePath, ProxiesFileName);
            var followsPath = Path.Combine(basePath, FollowsFileName);

            using (var file = new StreamWriter(accountsPath, true))
            {
                file.Write(string.Join(Environment.NewLine, accountBox.Lines));
            }

            using (var file = new StreamWriter(productsPath, true))
            {
                file.Write(string.Join(Environment.NewLine, productListBox.Lines));
            }

            using (var file = new StreamWriter(proxiessPath, true))
            {
                file.Write(string.Join(Environment.NewLine, proxyListBox.Lines));
            }

            using (var file = new StreamWriter(followsPath, false))
            {
                file.Write(
                    string.Join(
                        Environment.NewLine,
                        _follows.Select(pair => $"{pair.Key}\t{pair.Value:g}")
                    )
                );
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            var accountCount = (int) accountCountBox.Value;
            var profilePage = textBox1.Text;
            var maxPage = (int) numericUpDown1.Value;

            var proxyList = proxyListBox.Lines;
            var accountList = accountBox.Lines.Take(1000);
            var commentLines = productListBox.Lines;

            Console.WriteLine($"{DateTime.Now:G} -> Hizmet başlatıldı.");

            var thread = new Thread(
                async () =>
                {
                    var getClient = new DolapRestClient();

                    var productsIds = new List<string>();

                    for (var i = 1; i <= maxPage; i++)
                    {
                        var productsGetReponse = await getClient.GetAsync<string>(
                            profilePage,
                            new
                            {
                                sayfa = i
                            }
                        );

                        var productMatches = Regex.Matches(
                                productsGetReponse.Content,
                                "data-product-id=\"((.(?<!(\"|\"\\/)))*)\""
                            )
                            .Select(m => m.Groups[1].Value);

                        productsIds.AddRange(productMatches);
                    }

                    productsIds = productsIds.Distinct().ToList();

                    var homeGetResponse = await getClient.GetAsync<string>("");

                    var matches = Regex.Matches(
                            homeGetResponse.Content,
                            "data-member-id=\"((.(?<!(\"|\"\\/)))*)\""
                        )
                        .Select(m => m.Groups[1].Value)
                        .Distinct()
                        .ToList();

                    for (int i = 0; i < accountCount; i++)
                    {
                        var client = new DolapRestClient();

                        var loginGetResponse =
                            await client.GetAsync<string>(DolapClientConstants.LoginUrl);

                        var captchaRestClient = new DolapRestClient();

                        var siteKey = Regex.Match(
                                loginGetResponse.Content,
                                "'sitekey': \'((.(?<!(\'|\'\\/)))*)'"
                            )
                            .Groups[1]
                            .Value;

                        if (string.IsNullOrEmpty(siteKey))
                        {
                            throw new Exception("Hcaptcha site key not found");
                        }

                        var captchaReq = new RestRequest(
                            DolapClientConstants.CaptchaApiRequestUrl,
                            Method.GET
                        );

                        captchaReq.AddQueryParameter("key", DolapClientConstants.CaptchaApiKey)
                            .AddQueryParameter("json", "1")
                            .AddQueryParameter("method", "userrecaptcha")
                            .AddQueryParameter("invisible", "1")
                            .AddQueryParameter("googlekey", siteKey)
                            .AddQueryParameter("pageurl", loginGetResponse.ResponseUri.ToString());

                        var captchaRes = await captchaRestClient.SendAsync<JToken>(captchaReq);

                        if (!captchaRes.IsSuccessful)
                        {
                            throw new Exception(captchaRes.ErrorMessage);
                        }

                        var captchaId = captchaRes.Data.Value<string>("request");

                        if (string.IsNullOrEmpty(captchaId))
                        {
                            throw new Exception("Captcha id not found.");
                        }

                        string captchaValue = null;

                        await TaskExtensions.WaitWhile(
                            async () =>
                            {
                                var captchaReq2 = new RestRequest(
                                    DolapClientConstants.CaptchaApiResponseUrl,
                                    Method.GET
                                );

                                captchaReq2.AddQueryParameter(
                                        "key",
                                        DolapClientConstants.CaptchaApiKey
                                    )
                                    .AddQueryParameter("json", "1")
                                    .AddQueryParameter("action", "get")
                                    .AddQueryParameter("id", captchaId);

                                var captchaRes2 =
                                    await captchaRestClient.SendAsync<JToken>(captchaReq2);

                                if (!captchaRes2.IsSuccessful
                                    || captchaRes2.Content.Contains("CAPCHA_NOT_READY"))
                                {
                                    return true;
                                }

                                captchaValue = captchaRes2.Data.Value<string>("request");

                                return string.IsNullOrEmpty(captchaValue);
                            },
                            20000,
                            100000
                        );

                        if (string.IsNullOrEmpty(captchaValue))
                        {
                            throw new Exception("Captcha response not found.");
                        }

                        // await client.PostAsync<JToken>(
                        //     DolapClientConstants.LoginUrl,
                        //     new
                        //     {
                        //         Username = "brittany1647@yahoo.com",
                        //         Password = "cSuyq37PR6SQ20"
                        //     },
                        //     options: new
                        //     {
                        //         captchaToken = captchaValue
                        //     }
                        // );

                        var faker = new Faker();

                        var gender = faker.Person.Gender;
                        var name = faker.Name.FirstName(gender);
                        var surname = faker.Name.LastName(gender);
                        var username =
                            (name.ToLowerInvariant() + surname.ToLowerInvariant()
                                                     + faker.Random.Number(99)).Replace(
                                " ",
                                string.Empty
                            );
                        var email = faker.Internet.Email(
                            name.ToLowerInvariant(),
                            surname.ToLowerInvariant(),
                            null,
                            faker.Random.Number(99).ToString()
                        );
                        var password = faker.Internet.Password(12) + faker.Random.Number(99);

                        var registerRequest = await client.PostAsync<JToken>(
                            DolapClientConstants.RegisterUrl,
                            new RegisterInput
                            {
                                Email = email,
                                NickName = username,
                                Password = password
                            },
                            options: new
                            {
                                captchaToken = captchaValue
                            }
                        );

                        // var accessToken = registerRequest.Data.Value<string>("accessToken");

                        accountBox.AppendText($"{email};{password}");

                        Console.WriteLine($"{DateTime.Now:G} -> Hesap oluşturuldu.");

                        var memberIds = new List<string>();

                        var rand = new Random().Next(7, 15);

                        while (memberIds.Count < rand && memberIds.Count < matches.Count)
                        {
                            var memberId = matches.Random();

                            if (!memberIds.Contains(memberId))
                            {
                                memberIds.Add(memberId);
                            }
                        }

                        foreach (var memberId in memberIds)
                        {
                            await client.PostAsync<JToken>(
                                DolapClientConstants.FollowUrl,
                                new { },
                                new Dictionary<string, string>() {{"id", memberId}}
                            );
                        }

                        Console.WriteLine($"{DateTime.Now:G} -> {rand} kişi takip edildi.");

                        foreach (var productId in productsIds.OrderBy(s => Guid.NewGuid()).Take(40))
                        {
                            await client.PostAsync<JToken>(
                                DolapClientConstants.LikeUrl,
                                new { },
                                new Dictionary<string, string>() {{"id", productId}}
                            );

                            var comment = commentLines.Random();

                            if (!string.IsNullOrEmpty(comment))
                            {
                                await client.PostAsync<JToken>(
                                    DolapClientConstants.CommentUrl,
                                    new
                                    {
                                        commentBody = comment,
                                        productId = productId,
                                    }
                                );
                            }
                        }

                        Console.WriteLine($"{DateTime.Now:G} -> Ürünler işlendi.");
                    }

                    button1.Enabled = true;
                }
            );
            /*
            var thread = new Thread(
                async () =>
                {
                    var client = new DolapRestClient();

                    var registerResponse =
                        await client.GetAsync<string>(DolapClientConstants.LoginUrl);

                    var captchaRestClient = new DolapRestClient();

                    var siteKey = Regex.Match(
                            registerResponse.Content,
                            "'sitekey': \'((.(?<!(\'|\'\\/)))*)'"
                        )
                        .Groups[1]
                        .Value;

                    if (string.IsNullOrEmpty(siteKey))
                    {
                        throw new Exception("Hcaptcha site key not found");
                    }

                    var captchaReq = new RestRequest(
                        DolapClientConstants.CaptchaApiRequestUrl,
                        Method.GET
                    );

                    captchaReq.AddQueryParameter("key", DolapClientConstants.CaptchaApiKey)
                        .AddQueryParameter("json", "1")
                        .AddQueryParameter("method", "userrecaptcha")
                        .AddQueryParameter("invisible", "1")
                        .AddQueryParameter("googlekey", siteKey)
                        .AddQueryParameter("pageurl", registerResponse.ResponseUri.ToString());

                    var captchaRes = await captchaRestClient.SendAsync<JToken>(captchaReq);

                    if (!captchaRes.IsSuccessful)
                    {
                        throw new Exception(captchaRes.ErrorMessage);
                    }

                    var captchaId = captchaRes.Data.Value<string>("request");

                    if (string.IsNullOrEmpty(captchaId))
                    {
                        throw new Exception("Captcha id not found.");
                    }

                    string captchaValue = null;

                    await TaskExtensions.WaitWhile(
                        async () =>
                        {
                            var captchaReq2 = new RestRequest(
                                DolapClientConstants.CaptchaApiResponseUrl,
                                Method.GET
                            );

                            captchaReq2.AddQueryParameter("key", DolapClientConstants.CaptchaApiKey)
                                .AddQueryParameter("json", "1")
                                .AddQueryParameter("action", "get")
                                .AddQueryParameter("id", captchaId);

                            var captchaRes2 =
                                await captchaRestClient.SendAsync<JToken>(captchaReq2);

                            if (!captchaRes2.IsSuccessful
                                || captchaRes2.Content.Contains("CAPCHA_NOT_READY"))
                            {
                                return true;
                            }

                            captchaValue = captchaRes2.Data.Value<string>("request");

                            return string.IsNullOrEmpty(captchaValue);
                        },
                        20000,
                        100000
                    );

                    if (string.IsNullOrEmpty(captchaValue))
                    {
                        throw new Exception("Captcha response not found.");
                    }

                    var faker = new Faker();

                    var gender = faker.Person.Gender;
                    var name = faker.Name.FirstName(gender);
                    var surname = faker.Name.LastName(gender);
                    var username =
                        (name.ToLowerInvariant() + surname.ToLowerInvariant()
                         + faker.Random.Number(99)).Replace(
                            " ",
                            string.Empty
                        );
                    var email = faker.Internet.Email(
                        name.ToLowerInvariant(),
                        surname.ToLowerInvariant(),
                        null,
                        faker.Random.Number(99).ToString()
                    );
                    var password = faker.Internet.Password(12) + faker.Random.Number(99);

                    var registerRequest = await client.PostAsync<JToken>(
                        DolapClientConstants.RegisterUrl,
                        new RegisterInput
                        {
                            Email = email,
                            NickName = username,
                            Password = password
                        },
                        options: new
                        {
                            captchaToken = captchaValue
                        }
                    );

                    var accessToken = registerRequest.Data.Value<string>("accessToken");
                    
                    accountBox.AppendText($"{email};{password}");
                    
                    button1.Enabled = true;
                }
            );
            */

            thread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;

            var accountCount = (int) accountCountBox.Value;
            var username = usernameBox.Text;
            var password = passwordBox.Text;

            if (!(accountCount > 0))
            {
                throw new Exception("Hesap sayısı 0 dan büyük olmalı.");
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(
                    nameof(username),
                    "Kullanıcı adı veya şifre boş olamaz."
                );
            }

            var commentLines = productListBox.Lines;

            Console.WriteLine($"{DateTime.Now:G} -> Hizmet başlatıldı.");

            var thread = new Thread(
                async () =>
                {
                    var getClient = new DolapRestClient();

                    var homeGetResponse = await getClient.GetAsync<string>("");

                    var html = new HtmlDocument();

                    html.LoadHtml(homeGetResponse.Content);

                    var navlinks = html.DocumentNode
                        .SelectNodes("//ul[contains(@class, \"inner-links\")]/li/a")
                        ?.Select(n => n.GetAttributeValue("href", null))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList() ?? new List<string>();

                    while (_follows.Count(f => !f.Value.HasValue) < accountCount)
                    {
                        var membersResponse = await getClient.GetAsync<string>(navlinks.Random());

                        var matches = Regex.Matches(
                                membersResponse.Content,
                                "data-member-id=\"((.(?<!(\"|\"\\/)))*)\""
                            )
                            .Select(m => m.Groups[1].Value)
                            .Distinct()
                            .ToList();

                        foreach (var match in matches)
                        {
                            if (!_follows.ContainsKey(match))
                            {
                                _follows.Add(match, null);
                            }
                        }
                    }

                    if (_loggedUserClient == null)
                    {
                        _loggedUserClient = new DolapRestClient();

                        var loginGetResponse =
                            await _loggedUserClient.GetAsync<string>(DolapClientConstants.LoginUrl);

                        var captchaRestClient = new DolapRestClient();

                        var siteKey = Regex.Match(
                                loginGetResponse.Content,
                                "'sitekey': \'((.(?<!(\'|\'\\/)))*)'"
                            )
                            .Groups[1]
                            .Value;

                        if (string.IsNullOrEmpty(siteKey))
                        {
                            throw new Exception("Hcaptcha site key not found");
                        }

                        var captchaReq = new RestRequest(
                            DolapClientConstants.CaptchaApiRequestUrl,
                            Method.GET
                        );

                        captchaReq.AddQueryParameter("key", DolapClientConstants.CaptchaApiKey)
                            .AddQueryParameter("json", "1")
                            .AddQueryParameter("method", "userrecaptcha")
                            .AddQueryParameter("invisible", "1")
                            .AddQueryParameter("googlekey", siteKey)
                            .AddQueryParameter("pageurl", loginGetResponse.ResponseUri.ToString());

                        var captchaRes = await captchaRestClient.SendAsync<JToken>(captchaReq);

                        if (!captchaRes.IsSuccessful)
                        {
                            throw new Exception(captchaRes.ErrorMessage);
                        }

                        var captchaId = captchaRes.Data.Value<string>("request");

                        if (string.IsNullOrEmpty(captchaId))
                        {
                            throw new Exception("Captcha id not found.");
                        }

                        string captchaValue = null;

                        await TaskExtensions.WaitWhile(
                            async () =>
                            {
                                var captchaReq2 = new RestRequest(
                                    DolapClientConstants.CaptchaApiResponseUrl,
                                    Method.GET
                                );

                                captchaReq2.AddQueryParameter(
                                        "key",
                                        DolapClientConstants.CaptchaApiKey
                                    )
                                    .AddQueryParameter("json", "1")
                                    .AddQueryParameter("action", "get")
                                    .AddQueryParameter("id", captchaId);

                                var captchaRes2 =
                                    await captchaRestClient.SendAsync<JToken>(captchaReq2);

                                if (!captchaRes2.IsSuccessful
                                    || captchaRes2.Content.Contains("CAPCHA_NOT_READY"))
                                {
                                    return true;
                                }

                                captchaValue = captchaRes2.Data.Value<string>("request");

                                return string.IsNullOrEmpty(captchaValue);
                            },
                            20000,
                            100000
                        );

                        if (string.IsNullOrEmpty(captchaValue))
                        {
                            throw new Exception("Captcha response not found.");
                        }

                        await _loggedUserClient.PostAsync<JToken>(
                            DolapClientConstants.LoginUrl,
                            new
                            {
                                Username = username,
                                Password = password
                            },
                            options: new
                            {
                                captchaToken = captchaValue
                            }
                        );

                        Console.WriteLine($"{DateTime.Now:G} -> Giriş yapıldı.");
                    }

                    var followedAccounts = _follows.Where(pair => !pair.Value.HasValue)
                        .Take(accountCount)
                        .ToList();

                    foreach (var pair in followedAccounts)
                    {
                        await _loggedUserClient.PostAsync<JToken>(
                            DolapClientConstants.FollowUrl,
                            new { },
                            new Dictionary<string, string>() {{"id", pair.Key}}
                        );

                        _follows[pair.Key] = DateTime.Now;

                        Console.WriteLine(
                            $"{DateTime.Now:G} -> {pair.Key} takip edildi."
                        );

                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    Console.WriteLine(
                        $"{DateTime.Now:G} -> {followedAccounts.Count} kişi takip edildi."
                    );

                    button2.Enabled = true;
                }
            );

            thread.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;

            var username = usernameBox.Text;
            var password = passwordBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(
                    nameof(username),
                    "Kullanıcı adı veya şifre boş olamaz."
                );
            }

            Console.WriteLine($"{DateTime.Now:G} -> Hizmet başlatıldı.");

            var thread = new Thread(
                async () =>
                {
                    var client = new DolapRestClient();

                    var loginGetResponse =
                        await client.GetAsync<string>(DolapClientConstants.LoginUrl);

                    var captchaRestClient = new DolapRestClient();

                    var siteKey = Regex.Match(
                            loginGetResponse.Content,
                            "'sitekey': \'((.(?<!(\'|\'\\/)))*)'"
                        )
                        .Groups[1]
                        .Value;

                    if (string.IsNullOrEmpty(siteKey))
                    {
                        throw new Exception("Hcaptcha site key not found");
                    }

                    var captchaReq = new RestRequest(
                        DolapClientConstants.CaptchaApiRequestUrl,
                        Method.GET
                    );

                    captchaReq.AddQueryParameter("key", DolapClientConstants.CaptchaApiKey)
                        .AddQueryParameter("json", "1")
                        .AddQueryParameter("method", "userrecaptcha")
                        .AddQueryParameter("invisible", "1")
                        .AddQueryParameter("googlekey", siteKey)
                        .AddQueryParameter("pageurl", loginGetResponse.ResponseUri.ToString());

                    var captchaRes = await captchaRestClient.SendAsync<JToken>(captchaReq);

                    if (!captchaRes.IsSuccessful)
                    {
                        throw new Exception(captchaRes.ErrorMessage);
                    }

                    var captchaId = captchaRes.Data.Value<string>("request");

                    if (string.IsNullOrEmpty(captchaId))
                    {
                        throw new Exception("Captcha id not found.");
                    }

                    string captchaValue = null;

                    await TaskExtensions.WaitWhile(
                        async () =>
                        {
                            var captchaReq2 = new RestRequest(
                                DolapClientConstants.CaptchaApiResponseUrl,
                                Method.GET
                            );

                            captchaReq2.AddQueryParameter(
                                    "key",
                                    DolapClientConstants.CaptchaApiKey
                                )
                                .AddQueryParameter("json", "1")
                                .AddQueryParameter("action", "get")
                                .AddQueryParameter("id", captchaId);

                            var captchaRes2 =
                                await captchaRestClient.SendAsync<JToken>(captchaReq2);

                            if (!captchaRes2.IsSuccessful
                                || captchaRes2.Content.Contains("CAPCHA_NOT_READY"))
                            {
                                return true;
                            }

                            captchaValue = captchaRes2.Data.Value<string>("request");

                            return string.IsNullOrEmpty(captchaValue);
                        },
                        20000,
                        100000
                    );

                    if (string.IsNullOrEmpty(captchaValue))
                    {
                        throw new Exception("Captcha response not found.");
                    }

                    await client.PostAsync<JToken>(
                        DolapClientConstants.LoginUrl,
                        new
                        {
                            Username = username,
                            Password = password
                        },
                        options: new
                        {
                            captchaToken = captchaValue
                        }
                    );

                    Console.WriteLine($"{DateTime.Now:G} -> Giriş yapıldı.");

                    var unfollowedAccounts = _follows.Where(pair => pair.Value.HasValue)
                        .Where(pair => pair.Value.Value.AddDays(1) <= DateTime.Now)
                        .ToList();

                    foreach (var pair in unfollowedAccounts)
                    {
                        await client.PostAsync<JToken>(
                            DolapClientConstants.UnFollowUrl,
                            new { },
                            new Dictionary<string, string>() {{"id", pair.Key}}
                        );

                        _follows.Remove(pair.Key);

                        Console.WriteLine(
                            $"{DateTime.Now:G} -> {pair.Key} takipten çıkıldı."
                        );

                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    Console.WriteLine(
                        $"{DateTime.Now:G} -> {unfollowedAccounts.Count} kişi takipten çıkıldı."
                    );

                    button3.Enabled = true;
                }
            );

            thread.Start();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        // private void toolStripButton3_Click(object sender, EventArgs e)
        // {
        //     var frm = new RepinForm {Location = this.Location, StartPosition = FormStartPosition.Manual};
        //     frm.FormClosing += delegate
        //     {
        //         this.Show();
        //         Console.SetOut(new RichTextBoxWriter(this.richTextBox1));
        //     };
        //     frm.Show();
        //     Console.SetOut(new RichTextBoxWriter(frm.richTextBox1));
        //     Hide();
        // }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveData();
        }

        private void button2_EnabledChanged(object sender, EventArgs e)
        {
            if (button2.Enabled)
            {
                button2.PerformClick();
            }
        }
    }
}