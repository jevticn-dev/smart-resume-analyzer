package com.smartresumeanalyzer.base;

import io.github.bonigarcia.wdm.WebDriverManager;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxOptions;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.io.IOException;
import java.io.InputStream;
import java.time.Duration;
import java.util.Properties;

public class BaseTest {

    protected WebDriver driver;
    protected WebDriverWait wait;

    protected static final String BASE_URL;
    protected static final String TEST_EMAIL;
    protected static final String TEST_PASSWORD;
    protected static final String CV_PATH;
    protected static final String HR_EMAIL;
    protected static final int TIMEOUT = 10;

    static {
        Properties props = new Properties();
        try (InputStream in = BaseTest.class
                .getClassLoader()
                .getResourceAsStream("test.properties")) {
            if (in == null) {
                throw new RuntimeException(
                        "test.properties not found. Copy test.properties.example to " +
                                "test.properties and fill in your values."
                );
            }
            props.load(in);
        } catch (IOException e) {
            throw new RuntimeException("Failed to load test.properties", e);
        }

        BASE_URL      = props.getProperty("base.url", "http://localhost:4200");
        TEST_EMAIL    = props.getProperty("test.email");
        TEST_PASSWORD = props.getProperty("test.password");
        CV_PATH       = new java.io.File(
                props.getProperty("cv.path", "src/test/resources/CV_test.pdf")
        ).getAbsolutePath();
        HR_EMAIL = props.getProperty("hr.email");
    }

    @BeforeEach
    public void setUp() {
        WebDriverManager.firefoxdriver().setup();
        FirefoxOptions options = new FirefoxOptions();
        driver = new FirefoxDriver(options);
        driver.manage().window().maximize();
        driver.manage().timeouts().implicitlyWait(Duration.ofSeconds(TIMEOUT));
        wait = new WebDriverWait(driver, Duration.ofSeconds(TIMEOUT));
    }

    @AfterEach
    public void tearDown() {
        if (driver != null) {
            driver.quit();
        }
    }

    protected void navigateTo(String path) {
        driver.get(BASE_URL + path);
    }

    protected WebElement waitForVisible(By locator) {
        return wait.until(ExpectedConditions.visibilityOfElementLocated(locator));
    }

    protected WebElement waitForClickable(By locator) {
        return wait.until(ExpectedConditions.elementToBeClickable(locator));
    }

    protected void waitForUrlContains(String fragment) {
        wait.until(ExpectedConditions.urlContains(fragment));
    }

    protected void login() {
        navigateTo("/login");
        wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("email")))
                .sendKeys(TEST_EMAIL);
        wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("password")))
                .sendKeys(TEST_PASSWORD);
        wait.until(ExpectedConditions.elementToBeClickable(
                By.cssSelector("p-button button"))).click();
        waitForUrlContains("/dashboard");
    }
}