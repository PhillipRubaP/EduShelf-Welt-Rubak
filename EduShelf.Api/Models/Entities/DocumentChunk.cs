using Pgvector;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities
{
    public class DocumentChunk
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public virtual Document Document { get; set; } = null!;
        public required string Content { get; set; }

        public int Page { get; set; }

        [Column(TypeName = "vector(768)")]
        public required Vector Embedding { get; set; }
    }
}