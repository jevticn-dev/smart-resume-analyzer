package com.smartresumeanalyzer.tests;

import com.smartresumeanalyzer.base.BaseTest;
import org.junit.jupiter.api.Test;
import org.openqa.selenium.By;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;

import java.io.File;
import java.time.Duration;

import static org.junit.jupiter.api.Assertions.*;

public class FullFlowTest extends BaseTest {

    private static final String CV_PATH = BaseTest.CV_PATH;
    private static final String HR_EMAIL = BaseTest.HR_EMAIL;


    private String registerUniqueUser() {
        String uniqueEmail = "testflow+" + System.currentTimeMillis() + "@gmail.com";

        navigateTo("/register");
        wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("firstName"))).sendKeys("Test");
        driver.findElement(By.id("lastName")).sendKeys("User");
        driver.findElement(By.id("email")).sendKeys(uniqueEmail);
        wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("password"))).sendKeys("TestUser1");
        wait.until(ExpectedConditions.elementToBeClickable(By.cssSelector("p-button button"))).click();
        waitForUrlContains("/dashboard");
        try { Thread.sleep(1000); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
        return uniqueEmail;
    }

    private void createProject() {
        navigateTo("/projects/create");

        wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("title"))).sendKeys("Senior Dev Application");
        driver.findElement(By.id("jobTitle")).sendKeys("Senior Software Engineer");
        driver.findElement(By.id("companyName")).sendKeys("TechCorp");
        driver.findElement(By.id("companyEmail")).sendKeys(HR_EMAIL);

        WebElement seniorityDropdown = wait.until(
                ExpectedConditions.elementToBeClickable(By.cssSelector("#seniorityLevel"))
        );
        seniorityDropdown.click();
        wait.until(ExpectedConditions.visibilityOfElementLocated(
                By.cssSelector(".p-select-list .p-select-option")
        )).click();

        driver.findElement(By.id("jobDescription")).sendKeys(
                "We are looking for a Senior Software Engineer with 5+ years of experience in Java and Angular. " +
                        "Strong knowledge of microservices, REST APIs, and cloud technologies required."
        );

        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//p-button[@label='Create Project']//button")
        )).click();

        waitForUrlContains("/projects/");
    }

    private void uploadCvVersion() {
        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//p-button[@label='Add CV Version']//button")
        )).click();

        wait.until(ExpectedConditions.visibilityOfElementLocated(
                By.cssSelector(".p-dialog")
        ));

        WebElement fileInput = wait.until(
                ExpectedConditions.presenceOfElementLocated(By.cssSelector("input[type='file']"))
        );
        fileInput.sendKeys(CV_PATH);

        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//p-button[@label='Upload & Analyze']//button")
        )).click();

        new org.openqa.selenium.support.ui.WebDriverWait(driver, Duration.ofSeconds(90))
                .until(ExpectedConditions.invisibilityOfElementLocated(
                        By.cssSelector(".p-dialog-mask")
                ));

        try { Thread.sleep(1000); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }
    }

    private org.openqa.selenium.support.ui.WebDriverWait WebDriverWaitLong() {
        return new org.openqa.selenium.support.ui.WebDriverWait(driver, Duration.ofSeconds(60));
    }

    private void navigateToFirstVersion() {
        wait.until(ExpectedConditions.elementToBeClickable(
                By.cssSelector(".flip-card")
        )).click();

        waitForUrlContains("/versions/");
    }


    @Test
    public void testRegisterAndDashboardLoads() {
        registerUniqueUser();
        assertTrue(driver.getCurrentUrl().contains("/dashboard"));
    }

    @Test
    public void testFullFlow_RegisterCreateProjectUploadCv() {
        registerUniqueUser();

        navigateTo("/projects");
        waitForUrlContains("/projects");

        createProject();

        assertTrue(driver.getCurrentUrl().matches(".*/projects/[^/]+$"));

        WebElement title = wait.until(ExpectedConditions.visibilityOfElementLocated(
                By.cssSelector(".detail-header h1")
        ));
        assertTrue(title.getText().contains("Senior Dev Application"));
    }

    @Test
    public void testFullFlow_RegisterCreateProjectUploadCvAndVerifyAnalysis() {
        registerUniqueUser();

        navigateTo("/projects");
        createProject();

        uploadCvVersion();

        assertTrue(driver.findElements(By.cssSelector(".flip-card")).size() > 0);

        navigateToFirstVersion();

        WebElement analysisPanel = wait.until(ExpectedConditions.visibilityOfElementLocated(
                By.cssSelector("app-analysis-result-panel")
        ));
        assertTrue(analysisPanel.isDisplayed());

        assertTrue(driver.findElements(By.cssSelector("pdf-viewer")).size() > 0);
    }

    @Test
    public void testFullFlow_SendAiGeneratedEmail() {
        registerUniqueUser();
        navigateTo("/projects");
        createProject();
        uploadCvVersion();
        navigateToFirstVersion();

        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//p-button[@label='Send Application']//button")
        )).click();

        wait.until(ExpectedConditions.visibilityOfElementLocated(
                By.cssSelector(".send-dialog, .p-dialog")
        ));

        wait.until(ExpectedConditions.elementToBeClickable(
                By.cssSelector(".method-card")
        )).click();

        new org.openqa.selenium.support.ui.WebDriverWait(driver, Duration.ofSeconds(30))
                .until(ExpectedConditions.visibilityOfElementLocated(
                        By.cssSelector(".send-field")
                ));

        WebElement toField = driver.findElement(By.cssSelector("input[placeholder='hr@company.com']"));
        WebElement subjectField = driver.findElement(By.cssSelector("input[placeholder='Application for...']"));

        assertEquals(HR_EMAIL, toField.getAttribute("value"));
        assertFalse(subjectField.getAttribute("value").isEmpty());

        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//p-button[@label='Send Email']//button")
        )).click();

        wait.until(ExpectedConditions.visibilityOfElementLocated(
                By.cssSelector(".p-toast-message-success")
        ));
    }

    @Test
    public void testFullFlow_ExportPdf() {
        registerUniqueUser();
        navigateTo("/projects");
        createProject();
        uploadCvVersion();
        navigateToFirstVersion();

        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//p-button[@label='Export PDF']//button")
        )).click();

        assertTrue(driver.getCurrentUrl().contains("/versions/"));

        assertTrue(driver.findElements(By.cssSelector(".p-toast-message-error")).isEmpty());
    }

    @Test
    public void testFullFlow_DeleteVersionAndProject() {
        registerUniqueUser();
        navigateTo("/projects");
        createProject();
        uploadCvVersion();

        String projectUrl = driver.getCurrentUrl();

        navigateToFirstVersion();

        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//p-button[@label='Delete Version']//button")
        )).click();

        wait.until(ExpectedConditions.visibilityOfElementLocated(
                By.xpath("//span[contains(@class,'p-dialog-title') and contains(text(),'Delete CV Version')]")
        ));

        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//p-button[@label='Delete']//button")
        )).click();

        waitForUrlContains("/projects/");
        assertTrue(driver.getCurrentUrl().matches(".*/projects/[^/]+$"));

        try { Thread.sleep(1000); } catch (InterruptedException e) { Thread.currentThread().interrupt(); }

        assertEquals(0, driver.findElements(By.cssSelector(".flip-card")).size());

        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//div[contains(@class,'header-actions')]//p-button[@label='Delete']//button")
        )).click();

        wait.until(ExpectedConditions.visibilityOfElementLocated(
                By.xpath("//span[contains(@class,'p-dialog-title') and contains(text(),'Delete Project')]")
        ));

        wait.until(ExpectedConditions.elementToBeClickable(
                By.xpath("//p-button[@label='Delete Project']//button")
        )).click();

        waitForUrlContains("/projects");
        assertTrue(driver.getCurrentUrl().contains("/projects"));
    }
}