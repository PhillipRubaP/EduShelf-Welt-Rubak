using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.AI;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace EduShelf.Api.Services
{
    public class TextEmbeddingGenerationServiceAdapter : IEmbeddingGenerator<string, Embedding<float>>
    {
        private readonly ITextEmbeddingGenerationService _textEmbeddingService;

        public TextEmbeddingGenerationServiceAdapter(ITextEmbeddingGenerationService textEmbeddingService)
        {
            _textEmbeddingService = textEmbeddingService;
        }

        public async Task<Embedding<float>> GenerateAsync(string data, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            var embedding = await _textEmbeddingService.GenerateEmbeddingAsync(data, cancellationToken: cancellationToken);
            return new Embedding<float>(embedding.ToArray());
        }

        public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> data, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            var embeddings = new List<Embedding<float>>();
            foreach (var item in data)
            {
                var embedding = await _textEmbeddingService.GenerateEmbeddingAsync(item, cancellationToken: cancellationToken);
                embeddings.Add(new Embedding<float>(embedding.ToArray()));
            }
            return new GeneratedEmbeddings<Embedding<float>>(embeddings);
        }

        public object? GetService(Type serviceType, object? key = null)
        {
            if (serviceType == typeof(ITextEmbeddingGenerationService))
            {
                return _textEmbeddingService;
            }
            return null;
        }

        public void Dispose()
        {
            // No unmanaged resources to dispose.
        }
    }
}