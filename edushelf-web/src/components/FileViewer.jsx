import React from 'react';

const FileViewer = ({ document }) => {
  const renderContent = () => {
    if (!document || !document.content) {
      return <p>No content to display.</p>;
    }

    // This is a simplified example.
    // In a real application, you would use libraries like react-pdf for PDFs
    // and a markdown renderer for markdown files.
    const fileType = document.name.split('.').pop().toLowerCase();

    switch (fileType) {
      case 'pdf':
        return <p>PDF preview not yet implemented. Would use a library like react-pdf.</p>;
      case 'md':
        return <pre>{document.content}</pre>; // Placeholder for markdown rendering
      case 'txt':
        return <pre>{document.content}</pre>;
      default:
        return <p>Unsupported file type.</p>;
    }
  };

  return (
    <div className="file-viewer p-4">
      <h3 className="text-xl font-bold mb-2">{document.name}</h3>
      <div className="content bg-gray-100 p-4 rounded">
        {renderContent()}
      </div>
    </div>
  );
};

export default FileViewer;