package com.smartresumeanalyzer.pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;
import java.util.List;

public class ProjectListPage {

    private final WebDriver driver;
    private final WebDriverWait wait;
    private final By projectRows = By.cssSelector(".project-row");
    private final By newProjectButton = By.cssSelector("p-button button");

    public ProjectListPage(WebDriver driver) {
        this.driver = driver;
        this.wait = new WebDriverWait(driver, Duration.ofSeconds(10));
    }

    public List<WebElement> getProjectRows() {
        return driver.findElements(projectRows);
    }

    public int getProjectCount() {
        return getProjectRows().size();
    }

    public void clickFirstProject() {
        wait.until(ExpectedConditions.elementToBeClickable(projectRows)).click();
    }

    public void clickNewProject() {
        wait.until(ExpectedConditions.elementToBeClickable(newProjectButton)).click();
    }

    public boolean isLoaded() {
        return driver.getCurrentUrl().contains("/projects");
    }
}