# WebGPT

This is a Blazor Server Application - AI Chat. It combines OpenAI's GPT-3.5 Turbo model and Brave Search API. 

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
