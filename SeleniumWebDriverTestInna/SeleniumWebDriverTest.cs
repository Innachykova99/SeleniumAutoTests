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

        IWebElement title = webDriver.FindElement(By.XPath($"//*[@class='page-title']/em[contains(text(), '{searchQuery}')]"));
        Assert.That(title.Displayed, Is.True, $"Page with search results for '{searchQuery}' not found.");
    }

    [Test]
    public void ValidateSearchResults()// check that all products from search query contain search query and a link in its' title ( href attribute in format 'https://...')
    {
        IWebElement search = webDriver.FindElement(By.XPath("//*[@id='searchform']/i"));
        Actions action = new Actions(webDriver);
        action.MoveToElement(search).Perform();

        IWebElement inputField = webDriver.FindElement(By.XPath("//*[@id='searchform-wrap']/form/input"));
        inputField.SendKeys("html" + Keys.Enter);

        var searchResults = webDriver.FindElements(By.CssSelector(".post-title a"))
             .Select(result => new
             {
                 ProductName = result.Text.ToLower(),
                 ProductLink = result.GetAttribute("href")
             });

        foreach (var result in searchResults)
        {
            Assert.That(result.ProductName.Contains("html"), $"Product name '{result.ProductName}' does not contain 'html'.");
            Assert.That(result.ProductLink.StartsWith("https://"), $"Product link '{result.ProductLink}' does not start with 'https://'.");
        }
    }

    [Test]
    public void ValidateThinkingInHTMLProductPage()
    {
        string productUrl = "https://practice.automationtesting.in/product/thinking-in-html/";
        webDriver.Navigate().GoToUrl(productUrl);

        IWebElement saleLabel = webDriver.FindElement(By.CssSelector("div span.onsale"));
        Assert.That(saleLabel.Displayed, "SALE label not displayed on product image.");

        IWebElement regularPrice = webDriver.FindElement(By.CssSelector("div ins span.woocommerce-Price-amount"));
        IWebElement discountedPrice = webDriver.FindElement(By.CssSelector("div del span.woocommerce-Price-amount"));

        Assert.That(regularPrice.Displayed, "Regular price not displayed.");
        Assert.That(discountedPrice.Displayed, "Discounted price not displayed.");
    }

    [Test]
    public void FindAndNavigateToRelatedProduct() //find in Related Products 'HTML5 WebApp Develpment' and navigate to it
    {
        string productUrl = "https://practice.automationtesting.in/product/thinking-in-html/";
        webDriver.Navigate().GoToUrl(productUrl);

        IWebElement RelatedProduct = webDriver.FindElements(By.CssSelector("a.woocommerce-LoopProduct-link h3")).Single(x => x.Text == "HTML5 WebApp Develpment");
        JavaScriptClick(RelatedProduct);

        WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
        wait.Until(driver => driver.Url.Contains("html5-webapp-develpment"));

        string expectedUrl = "https://practice.automationtesting.in/product/html5-webapp-develpment/";
        string actualUrl = webDriver.Url;
        Assert.That(expectedUrl, Is.EqualTo(actualUrl), $"Expected URL: {expectedUrl} is not equal to Actual URL: {actualUrl}");
    }

    private void JavaScriptClick(IWebElement element)
    {
        var javaScriptExecutor = (IJavaScriptExecutor)webDriver;
        javaScriptExecutor.ExecuteScript("arguments[0].click();", element);
    }

    [Test]
    public void AddProductToCartAndSaveDetails() // add product to cart and remember product name and price
    {
        string productUrl = "https://practice.automationtesting.in/product/html5-webapp-develpment/";
        webDriver.Navigate().GoToUrl(productUrl); // go to product link

        IWebElement AddToBasket = webDriver.FindElement(By.CssSelector("button.single_add_to_cart_button"));
        AddToBasket.Click(); // adding product to basket

        string ProductName = webDriver.FindElement(By.CssSelector(".product_title")).Text;
        string ProductPrice = webDriver.FindElement(By.CssSelector(".woocommerce-Price-amount")).Text; // find product name and price of an added product

        IWebElement ConfirmationMessage = webDriver.FindElement(By.CssSelector(".woocommerce-message")); //  check that confirmation message is displayed after adding to basket
        Assert.That(ConfirmationMessage.Displayed, "Product was not added to the cart successfully.");

        IWebElement ViewBasket = webDriver.FindElement(By.CssSelector(".woocommerce-message a")); // find View Basket button
        ViewBasket.Click(); // click on View Basket button > navigate to https://practice.automationtesting.in/basket/

        WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
        IWebElement ProductNameInCartElement = wait.Until(e => e.FindElement(By.CssSelector(".cart_item td.product-name a")));  // add explicit wait, anyway "no such element" exception is displayed
        IWebElement ProductPriceInCartElement = webDriver.FindElement(By.CssSelector(".product-price .woocommerce-Price-amount"));

        string productNameInCart = ProductNameInCartElement.Text;
        string productPriceInCart = ProductPriceInCartElement.Text; // remember product name and price from the cart

        Assert.That(ProductName, Is.EqualTo(productNameInCart), "Product name in cart doesn't match.");
        Assert.That(ProductPrice, Is.EqualTo(productPriceInCart), "Product price in cart doesn't match."); // check that product name and price from product page are equal to product name and price from the basker
    }


    [Test]
    public void ChangeNumberOfProduct()// change quantity of product to 3
    {
        string productUrl = "https://practice.automationtesting.in/product/html5-webapp-develpment/";
        webDriver.Navigate().GoToUrl(productUrl);

        IWebElement AddToBasket = webDriver.FindElement(By.CssSelector("button.single_add_to_cart_button"));
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

    [Test]
    public void ChangeNumberOfProductUsingKeyboardArrows()
    {
        string productUrl = "https://practice.automationtesting.in/product/html5-webapp-develpment/";
        webDriver.Navigate().GoToUrl(productUrl);

        IWebElement AddToBasket = webDriver.FindElement(By.CssSelector("button.single_add_to_cart_button"));
        AddToBasket.Click(); // duplicate of previous test (navigate to page and add to basket) - cuz need to add product to cart before changing the quantity

        string CartUrl = "https://practice.automationtesting.in/basket/";
        webDriver.Navigate().GoToUrl(CartUrl);

        IWebElement Quantity = webDriver.FindElement(By.CssSelector(".product-quantity input"));
        Actions action = new Actions(webDriver);
        action.MoveToElement(Quantity).Perform();
        Quantity.Click();

        action.SendKeys(Keys.ArrowUp).Perform();
        action.SendKeys(Keys.ArrowUp).Perform(); // or action.SendKeys(Keys.ArrowUp).SendKeys(Keys.ArrowUp).Perform();

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
