#!/bin/sh
set -e

MODEL_NAME="llava:latest"

# Start ollama serve in the background to check for the model
/bin/ollama serve &
pid=$!

# Wait for the server to be up
echo "Waiting for ollama server to start..."
while ! curl -s http://localhost:11434 > /dev/null; do
    sleep 1
done
echo "Ollama server started."

# Check if the model already exists
if ! ollama list | grep -q "$MODEL_NAME"; then
  echo "Model '$MODEL_NAME' not found. Pulling (this may take a while)..."
  
  # Pull the model
  ollama pull "$MODEL_NAME"
  echo "Model pulled successfully."
fi

# Stop the temporary server
kill $pid
wait $pid 2>/dev/null

# Start the main ollama server process
echo "Starting main ollama server..."
/bin/ollama serve