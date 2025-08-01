using Pgvector;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities
{
    public class DocumentChunk
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public Document Document { get; set; }
        public string Content { get; set; }

        [Column(TypeName = "vector(768)")]
        public Vector Embedding { get; set; }
    }
}