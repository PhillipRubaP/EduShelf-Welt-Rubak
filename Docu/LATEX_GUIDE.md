# LaTeX Crashkurs für deine Diplomarbeit

Hier ist eine kurze Übersicht, wie du in dieser Vorlage arbeitest.

## 1. Wo schreibe ich was?

LaTeX trennt **Inhalt** und **Design**.
- **`main.tex`**: Das "Skelett". Hier werden alle Kapitel mit `\input{...}` eingebunden.
- **`praeambel.tex`**: Einstellungen, Pakete, Design (hier musst du selten was ändern).
- **Deine Kapitel**: Erstelle für jeden Abschnitt eine eigene `.tex` Datei (z.B. `backend.tex`) und binde sie in der `main.tex` ein.

## 2. Grundlegende Befehle

### Überschriften
```latex
\chapter{Hauptkapitel}      % 1.
\section{Abschnitt}         % 1.1
\subsection{Unterabschnitt} % 1.1.1
```

### Textformatierung
- **Fett**: `\textbf{fetter Text}`
- *Kursiv*: `\textit{kursiver Text}`
- `Monospace` (für Code-Schnipsel im Text): `\texttt{code}`

### Listen
**Punkte:**
```latex
\begin{itemize}
    \item Erster Punkt
    \item Zweiter Punkt
\end{itemize}
```

**Nummeriert:**
```latex
\begin{enumerate}
    \item Erstens
    \item Zweitens
\end{enumerate}
```

## 3. Bilder einfügen

Bilder kommen in den Ordner `img`.

```latex
\begin{figure}[ht] % [ht] = here, top (versuche Bild hier oder oben zu platzieren)
    \centering
    % Bildname ohne Dateiendung. [width=0.8\textwidth] = 80% der Textbreite
    \includegraphics[width=0.8\textwidth]{img/mein_bild}
    \caption{Beschriftung unter dem Bild}
    \label{fig:mein_bild} % Label für Referenzen
\end{figure}
```
*Wichtig*: Erst wenn du das `[demo]` aus der `praeambel.tex` entfernst, siehst du die echten Bilder!

## 4. Code einfügen

Für Quellcode gibt es die `lstlisting`-Umgebung:

```latex
\begin{lstlisting}[language=CSharp, caption={Mein C#-Code}, label={code:beispiel}]
public void HelloWorld() {
    Console.WriteLine("Hello World");
}
\end{lstlisting}
```

## 5. Referenzen (Verweise)

In LaTeX schreibst du nie "siehe Abbildung 5", weil sich die Nummer ändern kann.
Stattdessen:
1. Setze ein Label beim Ziel: `\label{fig:mein_bild}`
2. Verweise im Text darauf: `Wie in Abbildung \ref{fig:mein_bild} zu sehen ist...`

## 6. Quellenverzeichnis

Quellen kommen in die Datei `literatur.bib`.
Format (BibTeX):
```bibtex
@online{website:google,
    author = {Google},
    title = {Google Startseite},
    url = {https://www.google.com},
    year = {2024},
    urldate = {2024-01-01}
}
```

Im Text zitieren:
```latex
Das haben wir gegoogelt \cite{website:google}.
```

## 7. Kompilieren (PDF erstellen)

Einfach in deinem Terminal (im `Docu`-Ordner):
```powershell
./compile_with_docker.ps1
```
Das erstellt die Datei `main.pdf`.
