# EduShelf

EduShelf is a web-based platform for managing and interacting with educational content.

## Features
- **AI Integration:** Chat with your documents using the high-performance AI server.
- **Document Management:** Upload and organize PDF and text files.

## 🚀 How to Run

### Installation Guide

To get started, you will have to pull the repository, copy the example environment file (`cp .env.example .env.server`), and fill in the required values.

#### Commands

Start the server:
```bash
docker-compose -f docker-compose.server.yml --env-file .env.server up -d --build
```

Stop the server:
```bash
docker-compose -f docker-compose.server.yml --env-file .env.server down -v
```

Check the status of the model download:
```bash
docker logs -f edushelf-welt-rubak_ollama-init_1
```

Access the application:
http://172.16.17.15:5173/login