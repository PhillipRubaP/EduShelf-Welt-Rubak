# Use the official Ollama image as the base
FROM ollama/ollama

# Switch to root user to install curl
USER root

# Install curl using apt-get
RUN apt-get update && apt-get install -y curl

# The user 'ollama' may not exist in this context, so we'll run as root.

# The RUN command executes when the image is being built.
# We need to start the ollama server in the background, pull the models,
# and then stop the server. This is a common pattern for pre-loading data into an image.
RUN /bin/sh -c 'ollama serve & \
    sleep 5 && \
    ollama pull gemma:2b && \
    ollama pull nomic-embed-text'

COPY start.sh /start.sh
RUN chmod +x /start.sh

ENTRYPOINT ["/start.sh"]