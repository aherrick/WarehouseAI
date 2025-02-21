# Warehouse AI Assistant

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![GitHub Workflow Status](https://github.com/your-username/your-repo/actions/workflows/your-workflow.yml/badge.svg)

This project is a **Warehouse AI Assistant** using **Microsoft Semantic Kernel** and **Azure OpenAI**.

---

## 🚀 Configure User Secrets

Initialize User Secrets:
```sh
dotnet user-secrets init
```

Set Azure OpenAI Credentials:
```sh
dotnet user-secrets set "AzureOpenAI:DeploymentName" "your-deployment-name"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-openai-instance.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-secret-api-key"
```

Verify stored secrets:
```sh
dotnet user-secrets list
```

### Example `secrets.json`
```json
{
  "AzureOpenAI": {
    "DeploymentName": "your-deployment-name",
    "Endpoint": "https://your-openai-instance.openai.azure.com/",
    "ApiKey": "your-secret-api-key"
  }
}
```

---

Run the project:
```sh
dotnet run
```
