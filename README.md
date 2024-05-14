# WebGPT

This is a Blazor Server Application - AI Chat. This project is a proof of concept of a system that combines the power of GPT models and search engines. The system can answer questions and assist in various tasks using up-to-date information from the web.

## Technologies

- .NET 7
- Blazor Server App
- Azure.AI.OpenAI NuGet package
- OpenAI GPT-4-1106-preview API
- Brave Search API for AI inference
- GitLab Markdown API
- Bootstrap styles

## Startup

1. Open Visual Studio 2022 or later and clone this project.
2. Add `appsettings.json` file to the `WebGPT` project. This file should be as follows:
```json
{
  "ApiOptions": {
    "GitLab": {
      "BaseUrl": "https://gitlab.com/api/v4",
      "ApiKey": "[your api key]"
    },
    "OpenAi": {
      "ApiKey": "[your api key]"
    },
    "Brave": {
      "BaseUrl": "https://api.search.brave.com/res/v1",
      "ApiKey": "[your api key]"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

3. Run the app (`Ctrl+F5`).
