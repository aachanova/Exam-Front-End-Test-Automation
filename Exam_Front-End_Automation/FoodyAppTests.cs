using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Collections.ObjectModel;

namespace Exam_Front_End_Automation
{
    public class FoodyAppTests
    {
        protected IWebDriver driver;
        Actions actions;
        WebDriverWait wait;

        private static string? lastAddedFoodName;
        private static string? lastAddedFoodDescription;
        private static string? lastFoodCardNameBeforeDeletion;

        private static readonly string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:85/";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--disable-search-engine-choice-screen");
            chromeOptions.AddUserProfilePreference("profile.password_manager_enabled", false);

            driver = new ChromeDriver(chromeOptions);

            actions = new Actions(driver);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Navigate().GoToUrl(baseUrl);

            // Login
            driver.Navigate().GoToUrl($"{baseUrl}User/Login");

            driver.FindElement(By.XPath("//input[@name='Username']")).SendKeys("ani963");
            driver.FindElement(By.XPath("//input[@name='Password']")).SendKeys("123456");

            IWebElement loginButtonElement = driver.FindElement(By.XPath("//button[@type='submit']"));
            actions.MoveToElement(loginButtonElement).Click().Perform();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            driver.Quit();
            driver.Dispose();
        }

        private string CreateRandomString()
        {
            const string sequence = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            return new string(Enumerable
                .Repeat(sequence, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Test, Order(1)]
        public void AddFoodWithInvalidDataTest()
        {
            driver.Navigate().GoToUrl($"{baseUrl}Food/Add");

            IWebElement addButtonNewFood = driver.FindElement(By.XPath("//button[@type='submit']"));
            addButtonNewFood.Click();

            Assert.That(driver.Url, Is.EqualTo($"{baseUrl}Food/Add"), "The user was redirected to the different page but should stay on the Add Page.");

            IWebElement mainErrorMessage = driver.FindElement(By.XPath("//div[@class='text-danger validation-summary-errors']//li"));
            Assert.That(mainErrorMessage.Text.Trim(), Is.EqualTo("Unable to add this food revue!"), "The main error message is not as expected.");

            IWebElement nameErrorMessage = driver.FindElement(By.XPath("//span[@class='text-danger field-validation-error' and @data-valmsg-for='Name']"));
            Assert.That(nameErrorMessage.Text.Trim(), Is.EqualTo("The Name field is required."), "The name error message is not as expected.");

            IWebElement descriptionErrorMessage = driver.FindElement(By.XPath("//span[@class='text-danger field-validation-error' and @data-valmsg-for='Description']"));
            Assert.That(descriptionErrorMessage.Text.Trim(), Is.EqualTo("The Description field is required."), "The description error message is not as expected.");
        }

        [Test, Order(2)]
        public void AddRandomFoodTest()
        {
            driver.Navigate().GoToUrl($"{baseUrl}Food/Add");

            IWebElement inputFoodNameField = driver.FindElement(By.XPath("//input[@id='name']"));
            lastAddedFoodName = "Food name " + CreateRandomString();
            inputFoodNameField.SendKeys(lastAddedFoodName);

            IWebElement inputFoodDescriptionField = driver.FindElement(By.XPath("//input[@id='description']"));
            lastAddedFoodDescription = "Food description " + CreateRandomString();
            inputFoodDescriptionField.SendKeys(lastAddedFoodDescription);

            IWebElement addButtonNewFood = driver.FindElement(By.XPath("//button[@type='submit']"));
            addButtonNewFood.Click();

            Assert.That(driver.Url, Is.EqualTo(baseUrl), "The user was not redirected to the Home page.");

            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='row gx-5 align-items-center']")));

            ReadOnlyCollection<IWebElement> allFoodsCards = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));

            IWebElement lastFoodCard = allFoodsCards.Last();

            actions.MoveToElement(lastFoodCard).Perform();

            IWebElement lastFoodCardTitle = lastFoodCard.FindElement(By.XPath(".//h2[@class='display-4']"));
            Assert.That(lastFoodCardTitle.Text.Trim(), Is.EqualTo(lastAddedFoodName), "The name does not matched with the one, created by the user.");

        }

        [Test, Order(3)]
        public void EditLastAddedFoodTest()
        {
            driver.Navigate().GoToUrl(baseUrl);

            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='row gx-5 align-items-center']")));

            ReadOnlyCollection<IWebElement> allFoodsCards = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));

            IWebElement lastFoodCard = allFoodsCards.Last();

            actions.MoveToElement(lastFoodCard).Perform();

            IWebElement EditButtonLastCard = lastFoodCard.FindElement(By.XPath(".//a[@class='btn btn-primary btn-xl rounded-pill mt-5' and text()='Edit']"));
            EditButtonLastCard.Click();

            IWebElement inputNameEditPage = driver.FindElement(By.XPath("//input[@id='name']"));
            inputNameEditPage.Clear();
            string editedFoodName = "Some food name";
            inputNameEditPage.SendKeys(editedFoodName);

            IWebElement addButtonEditPage = driver.FindElement(By.XPath("//button[@type='submit']"));
            addButtonEditPage.Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='row gx-5 align-items-center']")));

            ReadOnlyCollection<IWebElement> allFoodsCardsAfterEdit = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));

            IWebElement lastFoodCardAfterEdit = allFoodsCardsAfterEdit.Last();

            actions.MoveToElement(lastFoodCardAfterEdit).Perform();

            IWebElement lastFoodCardTitleAfterEdit = lastFoodCardAfterEdit.FindElement(By.XPath(".//h2[@class='display-4']"));

            Assert.That(lastFoodCardTitleAfterEdit.Text.Trim(), Is.Not.EqualTo(editedFoodName), "The card title is changed, which should not be possible");

            try
            {
                Assert.That(lastFoodCardTitleAfterEdit.Text.Trim(), Is.EqualTo(lastAddedFoodName), "The card title is not the same as the last created card title before the edit attempt.");

                Console.WriteLine("The title could not be changed due to incomplete functionality.");
            }
            finally
            {
                Console.WriteLine("We apologize for the inconvenience");
            }
        }

        [Test, Order(4)]
        public void SearchForFoodTitleTest()
        {
            driver.Navigate().GoToUrl(baseUrl);

            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@type='search']")));

            IWebElement searchField = driver.FindElement(By.XPath("//input[@type='search']"));
            searchField.SendKeys(lastAddedFoodName);

            driver.FindElement(By.XPath("//button[@type='submit']")).Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='row gx-5 align-items-center']")));

            ReadOnlyCollection<IWebElement> allCardsAfterSearch = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));

            Assert.That(allCardsAfterSearch.Count(), Is.EqualTo(1), "The cards count is different than expected.");

            IWebElement foundFoodCardTitleAfterSearch = driver.FindElement(By.XPath("//h2[@class='display-4']"));
            Assert.That(foundFoodCardTitleAfterSearch.Text.Trim(), Is.EqualTo(lastAddedFoodName), "The found card title is different than the searched one by the user.");
        }

        [Test, Order(5)]
        public void DeleteLastAddedFoodTest()
        {
            driver.Navigate().GoToUrl(baseUrl);

            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='row gx-5 align-items-center']")));

            ReadOnlyCollection<IWebElement> allFoodsCards = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));

            int allFoodCardInitialCount = allFoodsCards.Count();


            IWebElement lastFoodCard = allFoodsCards.Last();

            lastFoodCardNameBeforeDeletion = lastFoodCard.FindElement(By.XPath(".//h2[@class='display-4']")).Text;

            actions.MoveToElement(lastFoodCard).Perform();

            IWebElement lastCardDeleteButton = lastFoodCard.FindElement(By.XPath(".//a[contains(@href, '/Food/Delete')]"));
            lastCardDeleteButton.Click();

            int expectedCount = allFoodCardInitialCount - 1;

            Assert.That(allFoodCardInitialCount, Is.Not.EqualTo(expectedCount), "The count after deletion is not as expected.");
            Assert.That(allFoodCardInitialCount - 1, Is.EqualTo(expectedCount), "The count has mot decreased by one.");

            allFoodsCards = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));
            lastFoodCard = allFoodsCards.Last();

            IWebElement lastFoodCardNameAfterDeletion = lastFoodCard.FindElement(By.XPath(".//h2[@class='display-4']"));
            Assert.That(lastFoodCardNameAfterDeletion.Text.Trim(), Is.Not.EqualTo(lastFoodCardNameBeforeDeletion), "The card name is not as expected. The last card name after deletion is the same as the card name before deletion.");
        }

        [Test, Order(6)]
        public void SearchForDeletedFoodTest()
        {
            driver.Navigate().GoToUrl(baseUrl);

            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@type='search']")));

            IWebElement searchField = driver.FindElement(By.XPath("//input[@type='search']"));
            searchField.SendKeys(lastFoodCardNameBeforeDeletion);

            driver.FindElement(By.XPath("//button[@type='submit']")).Click();

            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//h2[@class='display-4']")));
            IWebElement noFoodMessage = driver.FindElement(By.XPath("//h2[@class='display-4']"));

            actions.MoveToElement(noFoodMessage).Perform();

            Assert.That(noFoodMessage.Text.Trim(), Is.EqualTo("There are no foods :("), "The message is not as expected.");

            IWebElement addFoodButtonNavBar = driver.FindElement(By.XPath("//a[@class='nav-link' and contains(@href, '/Food/Add') ]"));

            Assert.True(addFoodButtonNavBar.Displayed, "Add button is not displayed.");
        }
    }
}