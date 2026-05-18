package com.smartresumeanalyzer.tests;

import com.smartresumeanalyzer.base.BaseTest;
import com.smartresumeanalyzer.pages.LoginPage;
import com.smartresumeanalyzer.pages.RegisterPage;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

public class AuthTest extends BaseTest {

    @Test
    public void testSuccessfulLogin() {
        navigateTo("/login");
        LoginPage loginPage = new LoginPage(driver);
        loginPage.loginWith(TEST_EMAIL, TEST_PASSWORD);
        waitForUrlContains("/dashboard");
        assertTrue(driver.getCurrentUrl().contains("/dashboard"));
    }

    @Test
    public void testLoginWithWrongPassword() {
        navigateTo("/login");
        LoginPage loginPage = new LoginPage(driver);
        loginPage.loginWith(TEST_EMAIL, "wrongpassword");
        assertTrue(loginPage.isErrorDisplayed());
        assertFalse(driver.getCurrentUrl().contains("/dashboard"));
    }

    @Test
    public void testLoginWithWrongEmail() {
        navigateTo("/login");
        LoginPage loginPage = new LoginPage(driver);
        loginPage.loginWith("nonexistent@gmail.com", TEST_PASSWORD);
        assertTrue(loginPage.isErrorDisplayed());
    }

    @Test
    public void testRegisterPageLoads() {
        navigateTo("/register");
        RegisterPage registerPage = new RegisterPage(driver);
        assertTrue(driver.getCurrentUrl().contains("/register"));
    }

    @Test
    public void testRegisterWithExistingEmail() {
        navigateTo("/register");
        RegisterPage registerPage = new RegisterPage(driver);
        registerPage.registerWith("Test", "User", TEST_EMAIL, TEST_PASSWORD);
        assertTrue(registerPage.isErrorDisplayed());
    }

    @Test
    public void testAuthGuardRedirectsToLogin() {
        navigateTo("/projects");
        waitForUrlContains("/login");
        assertTrue(driver.getCurrentUrl().contains("/login"));
    }

    @Test
    public void testGuestGuardRedirectsLoggedInUser() {
        login();
        navigateTo("/login");
        waitForUrlContains("/dashboard");
        assertTrue(driver.getCurrentUrl().contains("/dashboard"));
    }
}