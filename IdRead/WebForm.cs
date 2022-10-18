using Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace IdRead
{
    public class WebForm
    {
        public static void Fill()
        {
            try
            {
                Console.WriteLine("Enter password:");
                string pass = Email.ReadHidePassword();

                // add your chromedriver.exe path here
                ChromeDriver driver = new ChromeDriver(Environment.CurrentDirectory);

                //Maximize your browser size
                driver.Manage().Window.Maximize();

                //Add your MVC project URL
                var config = File.ReadAllText("..\\WeightNotes\\config.json");
                string host = System.Text.Json.JsonSerializer.Deserialize<ConfigWeightNotes>(config)?.SmtpServer.Host;
                string server = System.Text.Json.JsonSerializer.Deserialize<ConfigWeightNotes>(config)?.SmtpServer.Domain;
                driver.Navigate().GoToUrl("https://web" + host + "." + server);

                //Add your textbox id
                string account = System.Text.Json.JsonSerializer.Deserialize<ConfigWeightNotes>(config)?.User.Account;
                driver.FindElement(By.Id("rcmloginuser")).SendKeys(account);
                driver.FindElement(By.Id("rcmloginpwd")).SendKeys(pass);

                // //Add your checkbox id
                // driver.FindElement(By.Id("chckGamming")).Click();
                // driver.FindElement(By.Id("chckMusic")).Click();
                // driver.FindElement(By.Id("chckReading")).Click();

                // //Add your radio button id 
                // driver.FindElement(By.Id("rdMale")).Click();

                // //Add your select id which your have to select
                // var select = new SelectElement(driver.FindElement(By.Id("ddSelect")));
                // //Add your option text
                // select.SelectByText("New Jersey");

                // //Add your Date Picker id
                // var toDateBox = driver.FindElement(By.Id("dt"));
                // //This code is use for add date in Date Picker
                // toDateBox.SendKeys(DateTime.Now.ToString());

                //Add your button id 
                driver.FindElement(By.CssSelector(".mainaction")).Click();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}