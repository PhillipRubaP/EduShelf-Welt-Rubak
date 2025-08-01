# Use the official Ollama image as the base
FROM ollama/ollama

# The RUN command executes when the image is being built.
# We need to start the ollama server in the background, pull the models,
# and then stop the server. This is a common pattern for pre-loading data into an image.
RUN /bin/sh -c 'ollama serve & \
    sleep 5 && \
    ollama pull llama3 && \
    ollama pull nomic-embed-text'