using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace InnaAutoTests;
public class SeleniumWebDriverTest
{
    private IWebDriver webDriver;

    [SetUp]
    public void SetUp()
    {
        webDriver = new ChromeDriver();
        webDriver.Manage().Window.Maximize();
        webDriver.Navigate().GoToUrl("https://practice.automationtesting.in/shop/");
    }

    [Test]
    public void SearchInput() // enter "html" and perform search
    {
        IWebElement search = webDriver.FindElement(By.XPath("//*[@id=\"searchform\"]/i"));
        Actions action = new Actions(webDriver);
        action.MoveToElement(search).Perform();

        IWebElement inputField = webDriver.FindElement(By.XPath("//*[@id=\"searchform-wrap\"]/form/input"));
        inputField.SendKeys("html" + Keys.Enter);
    }

    [Test]
    public void CompareTitle() // check that page with search results is displayed, title "HTML"
    {
        string searchQuery = "html";
        string searchUrl = $"https://practice.automationtesting.in/?s={searchQuery}";
        webDriver.Navigate().GoToUrl(searchUrl);

        IWebElement element = webDriver.FindElement(By.XPath($"//*[@class=\"page-title\"]/em[contains(text(), '{searchQuery}')]"));
        Assert.That(element.Displayed, Is.True, $"Page with search results for '{searchQuery}' not found.");
    }

    [Test]
    public void ValidateSearchResults()// check that all products from search query contain search query and a link in its' title ( href attribute in format 'https://...')
    {
        string searchQuery = "html";
        string searchUrl = $"https://practice.automationtesting.in/?s={searchQuery}";
        webDriver.Navigate().GoToUrl(searchUrl);
        IReadOnlyCollection<IWebElement> searchResults = webDriver.FindElements(By.CssSelector(".post-title a"));

        foreach (var result in searchResults)
        {
            string productName = result.Text.ToLower();
            string productLink = result.FindElement(By.XPath("//*[@id=\"loops-wrapper\"]/article/a")).GetAttribute("href");

            Assert.That(productName.Contains(searchQuery), $"Product name '{productName}' does not contain '{searchQuery}'.");
            Assert.That(productLink.StartsWith("https://"), $"Product link '{productLink}' does not start with 'https://'.");
        }
    }


    [Test]
    public void ValidateThinkingInHTMLProductPage()
    {
        string productUrl = "https://practice.automationtesting.in/product/thinking-in-html/";
        webDriver.Navigate().GoToUrl(productUrl);

        IWebElement saleLabel = webDriver.FindElement(By.CssSelector("#product-163 span"));
        Assert.That(saleLabel.Displayed, "SALE label not displayed on product image.");

        IWebElement regularPrice = webDriver.FindElement(By.CssSelector("#product-163 p ins span"));
        IWebElement discountedPrice = webDriver.FindElement(By.CssSelector("#product-163 del span"));

        Assert.That(regularPrice.Displayed, "Regular price not displayed.");
        Assert.That(discountedPrice.Displayed, "Discounted price not displayed.");
    }

    [Test]
    public void FindAndNavigateToRelatedProduct() //find in Related Products 'HTML5 WebApp Develpment' and navigate to it
    {
        string productUrl = "https://practice.automationtesting.in/product/thinking-in-html/";
        webDriver.Navigate().GoToUrl(productUrl);

        IWebElement RelatedProduct = webDriver.FindElement(By.CssSelector("li.post-182 a.woocommerce-LoopProduct-link"));
        RelatedProduct.Click();
        string expectedUrl = "https://practice.automationtesting.in/product/html5-webapp-development/";
        string actualUrl = webDriver.Url;
        Assert.That(expectedUrl, Is.EqualTo(actualUrl), $"Expected URL: {expectedUrl} is not equal to Actual URL: {actualUrl}");

        //Assert.AreEqual(expectedUrl, actualUrl, $"Expected URL: {expectedUrl}. Actual URL: {actualUrl}"); // AreEqual can't be used here
    }

    [Test]
    public void AddProductToCartAndSaveDetails() // add product to cart and remember product name and price
    {
        string productUrl = "https://practice.automationtesting.in/product/html5-webapp-develpment/";
        webDriver.Navigate().GoToUrl(productUrl); // go to product link

        IWebElement AddToBasket = webDriver.FindElement(By.CssSelector("#product-182 button.single_add_to_cart_button"));
        AddToBasket.Click(); // adding product to basket

        IWebElement ProductNameElement = webDriver.FindElement(By.CssSelector(".product_title"));
        IWebElement ProductPriceElement = webDriver.FindElement(By.CssSelector(".woocommerce-Price-amount")); // find product name and price of an added product

        string productName = ProductNameElement.Text;
        string productPrice = ProductPriceElement.Text; // remember the name and price of a product (on product page)

        IWebElement ConfirmationMessage = webDriver.FindElement(By.CssSelector(".woocommerce-message")); //  check that confirmation message is displayed after adding to basket
        Assert.That(ConfirmationMessage.Displayed, "Product was not added to the cart successfully.");

        IWebElement ViewBasket = webDriver.FindElement(By.CssSelector(".woocommerce-message a")); // find View Basket button
        ViewBasket.Click(); // click on View Basket button > navigate to https://practice.automationtesting.in/basket/

        WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(60));
        IWebElement ProductNameInCartElement = wait.Until(e => e.FindElement(By.CssSelector(".cart_item td.product-name a")));  // add explicit wait, anyway "no such element" exception is displayed
        IWebElement ProductPriceInCartElement = webDriver.FindElement(By.CssSelector(".product-price .woocommerce-Price-amount"));

        string productNameInCart = ProductNameInCartElement.Text;
        string productPriceInCart = ProductPriceInCartElement.Text; // remember product name and price from the cart

        Assert.That(productName, Is.EqualTo(productNameInCart), "Product name in cart doesn't match.");
        Assert.That(productPrice, Is.EqualTo(productPriceInCart), "Product price in cart doesn't match."); // check that product name and price from product page are equal to product name and price from the basker
    }


    [Test]
    public void ChangeNumberOfProduct()// change quantity of product to 3
    {
        string productUrl = "https://practice.automationtesting.in/product/html5-webapp-develpment/";
        webDriver.Navigate().GoToUrl(productUrl);

        IWebElement AddToBasket = webDriver.FindElement(By.CssSelector("#product-182 button.single_add_to_cart_button"));
        AddToBasket.Click(); // duplicate of previous test (navigate to page and add to basket) - cuz need to add product to cart before changing the quantity

        string CartUrl = "https://practice.automationtesting.in/basket/";
        webDriver.Navigate().GoToUrl(CartUrl);

        IWebElement Quantity = webDriver.FindElement(By.CssSelector(".product-quantity input"));
        Actions action = new Actions(webDriver);
        action.MoveToElement(Quantity).Perform();
        Quantity.Clear(); //this solution was applies as buttons for changing quantity are not presented in devtools
        Quantity.Click();
        Quantity.SendKeys("3");

        IWebElement quantityValue = webDriver.FindElement(By.CssSelector(".product-quantity input")); // realized logic for comparing number of product in the cart (maybe separate it into the new test?)
        string actualQuantity = quantityValue.GetAttribute("value");
        Assert.That(actualQuantity, Is.EqualTo("3"), $"The quantity of the product in the cart: ({actualQuantity}) is not 3.");
    }

    [TearDown]
    public void TearDown()
    {
        webDriver.Close();
    }
}
