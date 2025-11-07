#!/bin/sh

# Start Ollama in the background
/bin/ollama serve &

# Wait for the server to be ready
echo "Waiting for Ollama server to start..."
while ! wget -q -O - http://localhost:11434 > /dev/null 2>&1; do
    sleep 1
done
echo "Ollama server is running."

# Pull the models
echo "Pulling llava model..."
/bin/ollama pull llava

echo "Pulling nomic-embed-text model..."
/bin/ollama pull nomic-embed-text

echo "Model pulling complete. The server will continue to run."

# Wait indefinitely
wait