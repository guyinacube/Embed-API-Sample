# Embed-API-Sample
A sample application to use the Power BI APIs for embedding. This makes use of the Power BI .NET SDK which in turn calls the Power BI REST API.

## Register Azure AD App

You can register your Azure AD app by following the steps in the following [document](https://powerbi.microsoft.com/documentation/powerbi-developer-register-app/).

## Update Password and Client ID

After you create your Azure AD App, you will want to update the Password and Client ID within secrets.cs.

```
// Update Power BI Account Password
public static string Password = "<PASSWORD>";

// The Azure AD App - Application ID/Client ID
public static string ClientID = "<CLIENT ID>";
```
