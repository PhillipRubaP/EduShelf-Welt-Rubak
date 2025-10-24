#!/bin/sh
set -e

# Start ollama serve in the background
/bin/ollama serve &
# Get the process ID
pid=$!

# Wait for the server to be up
echo "Waiting for ollama server to start..."
sleep 5

# Pull the model
echo "Pulling moondream model..."
ollama pull llava:7b-v1.6-mistral-q2_K

# Wait for the background process to exit
wait $pid