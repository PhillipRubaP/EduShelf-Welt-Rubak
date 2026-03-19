# EduShelf
[English](#english) | [Deutsch](#deutsch)

---

<a name="english"></a>
## 🇬🇧 English

EduShelf is a web-based platform for managing and interacting with educational content.

### Features
- **AI Integration:** Chat with your documents using the high-performance AI server.
- **Document Management:** Upload and organize PDF and text files.

### 🚀 How to Run

#### Installation Guide

To get started, you will have to pull the repository, copy the example environment file:
```bash
cp .env.example .env.server
```
After that, fill in the required values.

##### Commands

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

#### Start the Project Locally
The API is configured to connect to the AI Server (172.16.17.15).
```bash
docker compose up
```

---

<a name="deutsch"></a>
## 🇩🇪 Deutsch

EduShelf ist eine webbasierte Plattform zur Verwaltung und Interaktion mit Bildungsinhalten.

### Funktionen
- **KI-Integration:** Chatten Sie mit Ihren Dokumenten über den leistungsstarken KI-Server.
- **Dokumentenverwaltung:** Hochladen und Organisieren von PDF- und Textdateien.

### 🚀 Ausführung

#### Installationsanleitung

Um zu beginnen, müssen Sie das Repository klonen und die Beispiel-Umgebungsdatei kopieren:
```bash
cp .env.example .env.server
```
Tragen Sie danach die erforderlichen Werte in die `.env.server` Datei ein.

##### Befehle

Server starten:
```bash
docker-compose -f docker-compose.server.yml --env-file .env.server up -d --build
```

Server stoppen und Volumes entfernen:
```bash
docker-compose -f docker-compose.server.yml --env-file .env.server down -v
```

Status des Modell-Downloads überprüfen:
```bash
docker logs -f edushelf-welt-rubak_ollama-init_1
```

Auf die Anwendung zugreifen:
http://172.16.17.15:5173/login

#### Projekt lokal starten
Die API ist so konfiguriert, dass sie sich mit dem KI-Server (172.16.17.15) verbindet.
```bash
docker compose up
```