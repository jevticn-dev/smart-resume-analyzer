package com.smartresumeanalyzer.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;
import java.util.List;

public class ProjectDetailPage {

    private final WebDriver driver;
    private final WebDriverWait wait;

    private final By deleteProjectButton = By.xpath("//p-button[@label='Delete']//button");
    private final By confirmDeleteButton = By.xpath("//p-button[@label='Delete Project']//button");
    private final By cancelDeleteButton = By.xpath("//p-button[@label='Cancel']//button");
    private final By deleteDialogHeader = By.xpath("//span[contains(@class,'p-dialog-title') and contains(text(),'Delete Project')]");
    private final By versionCards = By.cssSelector(".flip-card");
    private final By addCvVersionButton = By.xpath("//p-button[@label='Add CV Version']//button");
    private final By backButton = By.cssSelector(".back-btn");

    public ProjectDetailPage(WebDriver driver) {
        this.driver = driver;
        this.wait = new WebDriverWait(driver, Duration.ofSeconds(10));
    }

    public void clickDeleteProject() {
        wait.until(ExpectedConditions.elementToBeClickable(deleteProjectButton)).click();
    }

    public boolean isDeleteDialogVisible() {
        try {
            return wait.until(ExpectedConditions.visibilityOfElementLocated(deleteDialogHeader)).isDisplayed();
        } catch (Exception e) {
            return false;
        }
    }

    public void confirmDelete() {
        wait.until(ExpectedConditions.elementToBeClickable(confirmDeleteButton)).click();
    }

    public void cancelDelete() {
        wait.until(ExpectedConditions.elementToBeClickable(cancelDeleteButton)).click();
    }

    public List<WebElement> getVersionCards() {
        return driver.findElements(versionCards);
    }

    public int getVersionCount() {
        return getVersionCards().size();
    }

    public void clickVersionCard(int index) {
        List<WebElement> cards = getVersionCards();
        wait.until(ExpectedConditions.elementToBeClickable(cards.get(index))).click();
    }

    public void clickAddCvVersion() {
        wait.until(ExpectedConditions.elementToBeClickable(addCvVersionButton)).click();
    }

    public void goBack() {
        wait.until(ExpectedConditions.elementToBeClickable(backButton)).click();
    }

    public boolean isLoaded() {
        return driver.getCurrentUrl().matches(".*/projects/[^/]+$");
    }
}