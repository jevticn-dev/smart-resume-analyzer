package com.smartresumeanalyzer.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;

public class VersionDetailPage {

    private final WebDriver driver;
    private final WebDriverWait wait;

    private final By deleteVersionButton = By.xpath("//p-button[@label='Delete Version']//button");
    private final By confirmDeleteButton = By.xpath("//p-button[@label='Delete']//button[not(ancestor::p-button[@label='Delete Project'])]");
    private final By cancelDeleteButton = By.xpath("//p-button[@label='Cancel']//button");
    private final By deleteDialogHeader = By.xpath("//span[contains(@class,'p-dialog-title') and contains(text(),'Delete CV Version')]");
    private final By sendApplicationButton = By.xpath("//p-button[@label='Send Application']//button");
    private final By exportPdfButton = By.xpath("//p-button[@label='Export PDF']//button");
    private final By backButton = By.cssSelector(".back-btn");
    private final By analysisPanel = By.cssSelector("app-analysis-result-panel");
    private final By pdfViewer = By.cssSelector("pdf-viewer");

    public VersionDetailPage(WebDriver driver) {
        this.driver = driver;
        this.wait = new WebDriverWait(driver, Duration.ofSeconds(10));
    }

    public void clickDeleteVersion() {
        wait.until(ExpectedConditions.elementToBeClickable(deleteVersionButton)).click();
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

    public void clickSendApplication() {
        wait.until(ExpectedConditions.elementToBeClickable(sendApplicationButton)).click();
    }

    public void clickExportPdf() {
        wait.until(ExpectedConditions.elementToBeClickable(exportPdfButton)).click();
    }

    public void goBack() {
        wait.until(ExpectedConditions.elementToBeClickable(backButton)).click();
    }

    public boolean isAnalysisPanelVisible() {
        try {
            return driver.findElement(analysisPanel).isDisplayed();
        } catch (Exception e) {
            return false;
        }
    }

    public boolean isPdfViewerVisible() {
        try {
            return driver.findElement(pdfViewer).isDisplayed();
        } catch (Exception e) {
            return false;
        }
    }

    public boolean isLoaded() {
        return driver.getCurrentUrl().matches(".*/projects/[^/]+/versions/[^/]+$");
    }
}