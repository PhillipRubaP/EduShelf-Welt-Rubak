-- Users Table
CREATE TABLE Users (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Email TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role VARCHAR(50) NOT NULL
);

-- Tags Table
CREATE TABLE Tags (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) UNIQUE NOT NULL
);


-- Documents Table
CREATE TABLE Documents (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    Title VARCHAR(255) NOT NULL,
    Path TEXT NOT NULL,
    FileType VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- DocumentTag Junction Table
CREATE TABLE DocumentTag (
    Id SERIAL PRIMARY KEY,
    DocumentId INTEGER NOT NULL,
    TagId INTEGER NOT NULL,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE,
    UNIQUE (DocumentId, TagId)
);


-- Favourites Table
CREATE TABLE Favourites (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    DocumentId INTEGER NOT NULL,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    UNIQUE (UserId, DocumentId)
);

-- AccessLogs Table
CREATE TABLE AccessLogs (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    DocumentId INTEGER NOT NULL,
    ViewedAt TIMESTAMPTZ DEFAULT NOW(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE
);

-- ChatMessages Table
CREATE TABLE ChatMessages (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    DocumentId INTEGER NOT NULL,
    Message TEXT NOT NULL,
    Response TEXT,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE
);

-- Flashcards Table
CREATE TABLE Flashcards (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    DocumentId INTEGER NOT NULL,
    Question TEXT NOT NULL,
    Answer TEXT NOT NULL,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE
);

-- Quiz Table
CREATE TABLE Quiz (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    DocumentId INTEGER NOT NULL,
    Score INTEGER NOT NULL,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE
);

-- Indexes for performance
CREATE INDEX idx_documents_userid ON Documents(UserId);
CREATE INDEX idx_documenttag_documentid ON DocumentTag(DocumentId);
CREATE INDEX idx_documenttag_tagid ON DocumentTag(TagId);
CREATE INDEX idx_favourites_userid ON Favourites(UserId);
CREATE INDEX idx_favourites_documentid ON Favourites(DocumentId);
CREATE INDEX idx_accesslogs_userid ON AccessLogs(UserId);
CREATE INDEX idx_accesslogs_documentid ON AccessLogs(DocumentId);
CREATE INDEX idx_chatmessages_userid ON ChatMessages(UserId);
CREATE INDEX idx_chatmessages_documentid ON ChatMessages(DocumentId);
CREATE INDEX idx_flashcards_userid ON Flashcards(UserId);
CREATE INDEX idx_flashcards_documentid ON Flashcards(DocumentId);
CREATE INDEX idx_quiz_userid ON Quiz(UserId);
CREATE INDEX idx_quiz_documentid ON Quiz(DocumentId);