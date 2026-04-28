# MafiaCardEngine
A simple Mafia game that refers to the original card game but with extended variations of roles.

## Project structure:
- GameLogic: Contains the pure core logic of the game, including role definitions, game flow, and win conditions. Built as a class library to be reusable across different platforms.
- GameLogic.Tests: Contains unit tests for the GameLogic project to ensure the correctness of the game mechanics.
- WebServer: A web application on *ASP.NET Core* that provides an API for clients to interact with the game.
- WebServer.Shared: Contains shared models and utilities used by both the WebServer and GameLogic projects.
- WPFClientShell: A desktop application built with *WPF* that serves as a client for players to connect to the WebServer.