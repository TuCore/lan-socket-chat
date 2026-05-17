# ChatSystem

[![CI/CD Pipeline](https://github.com/<OWNER>/<REPO>/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/<OWNER>/<REPO>/actions/workflows/ci-cd.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-Educational-green)](./README.md)

A real-time chat application built with **.NET 10** using raw **TCP Sockets**. Supports multiple clients chatting simultaneously through a central server.

---

## Overview

```
┌──────────────┐     TCP/IP      ┌──────────────┐
│ ChatConsole  │◄───────────────►│              │
│  (Console)   │                 │              │
└──────────────┘                 │              │
                                 │  ChatServer  │
┌──────────────┐     TCP/IP      │  (Port 9000) │
│ ChatConsole  │◄───────────────►│              │
│  (Console)   │                 │              │
└──────────────┘                 │              │
                                 │              │
┌──────────────┐     TCP/IP      │              │
│ ChatDesktop  │◄───────────────►│              │
│    (WPF)     │                 │              │
└──────────────┘                 └──────────────┘
```

## Project Structure

```
ChatSystem/
│
├── ChatSystem.slnx              # Solution file
│
├── ChatServer/                  # TCP Server (Console App)
│   ├── ChatServer.csproj
│   └── Program.cs               # Server logic: accept clients, broadcast messages
│
├── ChatConsole/                 # TCP Client (Console App)
│   ├── ChatConsole.csproj
│   └── Program.cs               # Console-based chat client
│
├── ChatDesktop/                 # TCP Client (WPF Desktop App)
│   ├── ChatDesktop.csproj
│   ├── App.xaml / App.xaml.cs
│   ├── MainWindow.xaml           # UI layout
│   ├── MainWindow.xaml.cs        # UI logic + TCP client
│   └── AssemblyInfo.cs
│
├── run-demo.bat                 # One-click demo: server + 5 clients
├── start-server.bat             # Start server only
├── start-client.bat             # Start one console client
├── start-wpf.bat                # Start one WPF client
├── stop-all.bat                 # Stop all running processes
└── .gitignore
```

## Features

- **Multi-client support** — Server handles unlimited concurrent connections using `async/await` and `ConcurrentDictionary`
- **Real-time messaging** — Messages are broadcast instantly to all connected clients
- **Two client types** — Console client for simplicity, WPF client for a graphical interface
- **Nickname system** — Each user sets a display name on connect
- **Join/Leave notifications** — Server announces when users connect or disconnect
- **Graceful disconnection** — Clients can disconnect cleanly with `/quit` or by closing the window
- **Custom port** — Server port is configurable via command-line argument

## Tech Stack

| Component | Technology |
|-----------|------------|
| Runtime | .NET 10 |
| Networking | `System.Net.Sockets` (TcpListener / TcpClient) |
| Concurrency | `async/await`, `Task.Run`, `ConcurrentDictionary` |
| Desktop UI | WPF (Windows Presentation Foundation) |
| Encoding | UTF-8 |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Windows 10/11 (required for WPF client)

Verify your installation:

```bash
dotnet --version
# Should output 10.x.x or higher
```

## Quick Start

### Option 1: One-click Demo

Double-click `run-demo.bat` or run:

```bash
run-demo.bat
```

This will automatically:
1. Build the entire solution
2. Start the server on port 9000
3. Open 3 console clients + 2 WPF clients (5 total)

### Option 2: Manual Setup

**Terminal 1 — Start the server:**

```bash
dotnet run --project ChatServer
```

**Terminal 2, 3, ... — Start clients:**

```bash
# Console client
dotnet run --project ChatConsole

# WPF client (graphical)
dotnet run --project ChatDesktop
```

## Usage

### Console Client

```
=== CHAT CLIENT ===

Enter server IP (default 127.0.0.1):
Enter server port (default 9000):
Enter your nickname: Alice
--------------------------------------------------
Connected! Type messages and press Enter to send.
Type /quit to disconnect.

Hello everyone!                     <- you type this
Bob: Hi Alice, welcome!             <- received from Bob
```

### WPF Client

1. Enter the server **IP** and **Port** (defaults: `127.0.0.1` : `9000`)
2. Enter your **Nickname**
3. Click **Connect**
4. Type messages in the input box and click **Send** or press **Enter**

### Server Console

```
=== CHAT SERVER ===
Server started on port 9000
Waiting for clients...

[+] Client connected: 127.0.0.1:52341 (Total: 1)
[*] 127.0.0.1:52341 set nickname: Alice
[+] Client connected: 127.0.0.1:52342 (Total: 2)
[*] 127.0.0.1:52342 set nickname: Bob
[MSG] Alice: Hello everyone!
[MSG] Bob: Hi Alice, welcome!
[-] Alice disconnected. (Total: 1)
```

## Scripts Reference

| Script | Description | Usage |
|--------|-------------|-------|
| `run-demo.bat` | Build + start server + 5 clients | `run-demo.bat` |
| `start-server.bat` | Start server (custom port optional) | `start-server.bat [port]` |
| `start-client.bat` | Start one console client | `start-client.bat` |
| `start-wpf.bat` | Start one WPF client | `start-wpf.bat` |
| `stop-all.bat` | Kill all chat processes | `stop-all.bat` |

## How It Works

### Server (`ChatServer`)

1. Creates a `TcpListener` on the specified port
2. Accepts incoming connections in an infinite loop
3. Each client is handled in a separate `Task` (concurrent processing)
4. When a client sends a message, the server **broadcasts** it to all other connected clients
5. Uses `ConcurrentDictionary` for thread-safe client management

### Client (`ChatConsole` / `ChatDesktop`)

1. Connects to the server using `TcpClient`
2. Sends nickname as the first message (protocol handshake)
3. Spawns a background `Task` to **receive** messages from the server
4. Main thread handles **sending** messages (user input)
5. WPF client uses `Dispatcher.Invoke` to update UI from background threads

## License

This project is for educational purposes.
"# lan-socket-chat" 
