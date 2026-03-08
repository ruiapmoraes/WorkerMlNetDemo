# WorkerMlNetDemo

Projeto de estudo em .NET com Worker Service + ML.NET para treinamento e previsão de tempo estimado de rota com dados simulados.

## Objetivo

Este projeto demonstra um pipeline simples de Machine Learning utilizando .NET, com foco em:

- execução em background com Worker Service
- geração de dataset fake em CSV
- treinamento de modelo com ML.NET
- persistência do modelo treinado em arquivo `.zip`
- carregamento do modelo para inferência
- execução contínua de previsões

## Tecnologias utilizadas

- .NET 8
- C#
- Worker Service
- ML.NET
- Options Pattern
- Logging nativo do .NET

## Estrutura do projeto

```text
Configurations/
Models/
Services/
Workers/
Program.cs
appsettings.json