using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EduShelf.Api.Services
{
    public class ImageProcessingService
    {
        private readonly InferenceSession _session;
        private readonly string[] _modelLabels;

        public ImageProcessingService(string modelPath, string[] modelLabels)
        {
            _session = new InferenceSession(modelPath);
            _modelLabels = modelLabels;
        }

        public async Task<IEnumerable<string>> ProcessImageAsync(Stream imageStream)
        {
            using var image = await Image.LoadAsync<Rgb24>(imageStream);
            
            // Pre-process the image (example for a 416x416 model)
            var inputSize = 416;
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(inputSize, inputSize),
                Mode = ResizeMode.Pad
            }));

            var inputTensor = new DenseTensor<float>(new[] { 1, 3, inputSize, inputSize });
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image[x, y];
                    inputTensor[0, 0, y, x] = pixel.R / 255.0f;
                    inputTensor[0, 1, y, x] = pixel.G / 255.0f;
                    inputTensor[0, 2, y, x] = pixel.B / 255.0f;
                }
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            // Run inference
            using var results = _session.Run(inputs);

            // Post-process the results (this is a placeholder)
            // The actual implementation depends on the model's output format.
            // This example assumes the model outputs a list of detected class indices.
            var output = results.FirstOrDefault();
            if (output?.Value is not DenseTensor<float> outputTensor)
            {
                return new[] { "Could not process model output." };
            }

            // Placeholder: return the top 5 labels with highest scores
            var topResults = outputTensor.Buffer.ToArray()
                .Select((score, index) => new { score, index })
                .OrderByDescending(item => item.score)
                .Take(5)
                .Select(item => _modelLabels[item.index])
                .ToList();

            return topResults;
        }
    }
}