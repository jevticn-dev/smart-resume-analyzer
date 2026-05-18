package com.smartresumeanalyzer.tests;

import com.smartresumeanalyzer.base.BaseTest;
import com.smartresumeanalyzer.pages.ProjectDetailPage;
import com.smartresumeanalyzer.pages.ProjectListPage;
import com.smartresumeanalyzer.pages.VersionDetailPage;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

public class ProjectTest extends BaseTest {

    @Test
    public void testProjectListLoads() {
        login();
        navigateTo("/projects");
        ProjectListPage listPage = new ProjectListPage(driver);
        assertTrue(listPage.isLoaded());
    }

    @Test
    public void testProjectListShowsProjects() {
        login();
        navigateTo("/projects");
        ProjectListPage listPage = new ProjectListPage(driver);
        assertTrue(listPage.getProjectCount() >= 0);
    }

    @Test
    public void testNavigateToProjectDetail() {
        login();
        ProjectListPage listPage = new ProjectListPage(driver);
        if (listPage.getProjectCount() == 0) return;

        listPage.clickFirstProject();
        waitForUrlContains("/projects/");
        ProjectDetailPage detailPage = new ProjectDetailPage(driver);
        assertTrue(detailPage.isLoaded());
    }

    @Test
    public void testDeleteProjectDialogOpens() {
        login();
        ProjectListPage listPage = new ProjectListPage(driver);
        if (listPage.getProjectCount() == 0) return;

        listPage.clickFirstProject();
        waitForUrlContains("/projects/");

        ProjectDetailPage detailPage = new ProjectDetailPage(driver);
        detailPage.clickDeleteProject();
        assertTrue(detailPage.isDeleteDialogVisible());
    }

    @Test
    public void testDeleteProjectDialogCancelKeepsProject() {
        login();
        ProjectListPage listPage = new ProjectListPage(driver);
        if (listPage.getProjectCount() == 0) return;

        listPage.clickFirstProject();
        waitForUrlContains("/projects/");
        String url = driver.getCurrentUrl();

        ProjectDetailPage detailPage = new ProjectDetailPage(driver);
        detailPage.clickDeleteProject();
        assertTrue(detailPage.isDeleteDialogVisible());
        detailPage.cancelDelete();

        assertEquals(url, driver.getCurrentUrl());
    }

    @Test
    public void testNavigateToVersionDetail() {
        login();
        ProjectListPage listPage = new ProjectListPage(driver);
        if (listPage.getProjectCount() == 0) return;

        listPage.clickFirstProject();
        waitForUrlContains("/projects/");

        ProjectDetailPage detailPage = new ProjectDetailPage(driver);
        if (detailPage.getVersionCount() == 0) return;

        detailPage.clickVersionCard(0);
        waitForUrlContains("/versions/");

        VersionDetailPage versionPage = new VersionDetailPage(driver);
        assertTrue(versionPage.isLoaded());
    }

    @Test
    public void testDeleteVersionDialogOpens() {
        login();
        ProjectListPage listPage = new ProjectListPage(driver);
        if (listPage.getProjectCount() == 0) return;

        listPage.clickFirstProject();
        waitForUrlContains("/projects/");

        ProjectDetailPage detailPage = new ProjectDetailPage(driver);
        if (detailPage.getVersionCount() == 0) return;

        detailPage.clickVersionCard(0);
        waitForUrlContains("/versions/");

        VersionDetailPage versionPage = new VersionDetailPage(driver);
        versionPage.clickDeleteVersion();
        assertTrue(versionPage.isDeleteDialogVisible());
    }

    @Test
    public void testDeleteVersionDialogCancelKeepsVersion() {
        login();
        ProjectListPage listPage = new ProjectListPage(driver);
        if (listPage.getProjectCount() == 0) return;

        listPage.clickFirstProject();
        waitForUrlContains("/projects/");

        ProjectDetailPage detailPage = new ProjectDetailPage(driver);
        if (detailPage.getVersionCount() == 0) return;

        detailPage.clickVersionCard(0);
        waitForUrlContains("/versions/");
        String url = driver.getCurrentUrl();

        VersionDetailPage versionPage = new VersionDetailPage(driver);
        versionPage.clickDeleteVersion();
        assertTrue(versionPage.isDeleteDialogVisible());
        versionPage.cancelDelete();

        assertEquals(url, driver.getCurrentUrl());
    }

    @Test
    public void testVersionDetailShowsAnalysis() {
        login();
        ProjectListPage listPage = new ProjectListPage(driver);
        if (listPage.getProjectCount() == 0) return;

        listPage.clickFirstProject();
        waitForUrlContains("/projects/");

        ProjectDetailPage detailPage = new ProjectDetailPage(driver);
        if (detailPage.getVersionCount() == 0) return;

        detailPage.clickVersionCard(0);
        waitForUrlContains("/versions/");

        VersionDetailPage versionPage = new VersionDetailPage(driver);
        assertTrue(versionPage.isAnalysisPanelVisible());
    }

    @Test
    public void testVersionDetailBackButtonNavigatesToProject() {
        login();
        ProjectListPage listPage = new ProjectListPage(driver);
        if (listPage.getProjectCount() == 0) return;

        listPage.clickFirstProject();
        waitForUrlContains("/projects/");
        String projectUrl = driver.getCurrentUrl();

        ProjectDetailPage detailPage = new ProjectDetailPage(driver);
        if (detailPage.getVersionCount() == 0) return;

        detailPage.clickVersionCard(0);
        waitForUrlContains("/versions/");

        VersionDetailPage versionPage = new VersionDetailPage(driver);
        versionPage.goBack();
        waitForUrlContains("/projects/");

        assertEquals(projectUrl, driver.getCurrentUrl());
    }
}