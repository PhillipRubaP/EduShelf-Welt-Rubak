# Use the official Ollama image as a base
FROM ollama/ollama

# Copy the startup script into the container
COPY start.sh /start.sh

# Make the script executable
RUN chmod +x /start.sh

# Set the startup script as the entrypoint
ENTRYPOINT ["/start.sh"]